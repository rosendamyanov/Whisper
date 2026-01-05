import { create } from 'zustand'
import * as signalR from '@microsoft/signalr'
import { VoiceManager } from '../core/VoiceManager'
import { useAuthStore } from './authStore'
import type { StoreApi } from 'zustand'

export interface VoiceParticipant {
  userId: string
  username: string
  isMuted: boolean
  isDeafened: boolean
  isSpeaking: boolean
}

interface CallInfo {
  chatId: string
  callerId: string
  callerName: string
  callerAvatar?: string
}

interface VoiceState {
  isInCall: boolean
  callStatus: 'idle' | 'outgoing' | 'incoming' | 'connected'

  participants: VoiceParticipant[]
  audioStreams: Map<string, MediaStream>

  incomingCall: CallInfo | null
  outgoingCall: CallInfo | null

  initVoiceListeners: () => Promise<void>
  startCall: (chatId: string, friendId: string, friendName: string) => Promise<void>
  acceptCall: () => Promise<void>
  rejectCall: () => void
  endCall: () => void
  toggleMute: () => void
  toggleDeafen: () => void
  setParticipants: (p: VoiceParticipant[]) => void
}

type VoiceSet = StoreApi<VoiceState>['setState']
type VoiceGet = StoreApi<VoiceState>['getState']

let voiceManager: VoiceManager | null = null
let voiceConnection: signalR.HubConnection | null = null
let ringtoneAudio: HTMLAudioElement | null = null

const stopRingtone = (): void => {
  if (ringtoneAudio) {
    ringtoneAudio.pause()
    ringtoneAudio.currentTime = 0
    ringtoneAudio = null
  }
}

const ensureConnection = async (
  set: VoiceSet,
  get: VoiceGet
): Promise<signalR.HubConnection | null> => {
  const token =
    localStorage.getItem('whisper-token') ||
    JSON.parse(localStorage.getItem('whisper-auth-storage') || '{}')?.state?.user?.token

  if (!voiceConnection) {
    const apiUrl = import.meta.env.VITE_API_URL || 'https://localhost:7126/api'
    const rootUrl = apiUrl.replace(/\/api$/, '')
    const hubUrl = `${rootUrl}/hubs/voice`

    voiceConnection = new signalR.HubConnectionBuilder()
      .withUrl(hubUrl, {
        accessTokenFactory: () => token || '',
        withCredentials: true
      })
      .withAutomaticReconnect()
      .build()

    voiceConnection.on('IncomingCall', (data) => {
      if (get().isInCall) return
      stopRingtone()
      set({ callStatus: 'incoming', incomingCall: data })
      try {
        ringtoneAudio = new Audio(
          'https://assets.mixkit.co/active_storage/sfx/1359/1359-preview.mp3'
        )
        ringtoneAudio.loop = true
        ringtoneAudio.play().catch(() => {})
      } catch {
        // Ignore audio play errors
      }
    })

    voiceConnection.on('CallRejected', () => {
      stopRingtone()
      get().endCall()
      alert('User busy or declined.')
    })

    voiceConnection.on('CallCanceled', () => {
      stopRingtone()
      set({ callStatus: 'idle', incomingCall: null })
    })

    voiceConnection.on(
      'ParticipantMuteChanged',
      (data: { userId: string; isMuted?: boolean; isDeafened?: boolean }) => {
        set((state: VoiceState) => ({
          participants: state.participants.map((p) =>
            p.userId === data.userId
              ? {
                  ...p,
                  isMuted: data.isMuted ?? p.isMuted,
                  isDeafened: data.isDeafened ?? p.isDeafened
                }
              : p
          )
        }))
      }
    )

    voiceConnection.on('ParticipantStartedSpeaking', (data: { userId: string }) => {
      set((state: VoiceState) => ({
        participants: state.participants.map((p) =>
          p.userId === data.userId ? { ...p, isSpeaking: true } : p
        )
      }))
    })

    voiceConnection.on('ParticipantStoppedSpeaking', (data: { userId: string }) => {
      set((state: VoiceState) => ({
        participants: state.participants.map((p) =>
          p.userId === data.userId ? { ...p, isSpeaking: false } : p
        )
      }))
    })

    voiceConnection.on('ParticipantJoined', (participant: VoiceParticipant) => {
      set((state: VoiceState) => {
        const exists = state.participants.find((p) => p.userId === participant.userId)
        if (exists) return {}
        return { participants: [...state.participants, participant] }
      })

      if (get().callStatus === 'outgoing') {
        set({ callStatus: 'connected' })
      }
    })

    voiceConnection.on('SessionTimeoutStarted', (data) => {
      console.warn(`Session timeout started. Ending in ${data.timeoutMinutes} mins.`)
    })

    voiceConnection.on('SessionTimeoutExpired', () => {
      alert('Call ended due to inactivity.')
      get().endCall()
    })
  }

  if (voiceConnection.state === signalR.HubConnectionState.Connected) return voiceConnection
  if (voiceConnection.state === signalR.HubConnectionState.Disconnected) {
    try {
      await voiceConnection.start()
      return voiceConnection
    } catch (err) {
      console.error('Voice Hub Connection Failed:', err)
      return null
    }
  }
  return voiceConnection
}

export const useVoiceStore = create<VoiceState>((set, get) => ({
  isInCall: false,
  callStatus: 'idle',
  participants: [],
  audioStreams: new Map(),
  incomingCall: null,
  outgoingCall: null,

  initVoiceListeners: async () => {
    await ensureConnection(set, get)
  },

  startCall: async (chatId, friendId, friendName) => {
    const user = useAuthStore.getState().user
    if (!user) return
    set({
      callStatus: 'outgoing',
      outgoingCall: { chatId, callerId: friendId, callerName: friendName }
    })

    const conn = await ensureConnection(set, get)
    if (!conn) {
      get().endCall()
      return
    }

    await internalJoinSession(chatId, user.id, set, get)
    conn.invoke('CallUser', friendId, chatId).catch(() => get().endCall())
  },

  acceptCall: async () => {
    stopRingtone()
    const { incomingCall } = get()
    const userId = useAuthStore.getState().user?.id
    if (!incomingCall || !userId) return

    set({ callStatus: 'connected', incomingCall: null })
    await ensureConnection(set, get)
    await internalJoinSession(incomingCall.chatId, userId, set, get)
  },

  rejectCall: () => {
    stopRingtone()
    const { incomingCall } = get()
    if (voiceConnection && incomingCall)
      voiceConnection.invoke('RejectCall', incomingCall.callerId).catch(() => {})
    set({ callStatus: 'idle', incomingCall: null })
  },

  endCall: () => {
    stopRingtone()
    const { callStatus, outgoingCall, incomingCall } = get()

    set({
      isInCall: false,
      callStatus: 'idle',
      participants: [],
      audioStreams: new Map(),
      incomingCall: null,
      outgoingCall: null
    })

    if (voiceManager) {
      voiceManager.cleanup()
      voiceManager = null
    }

    if (voiceConnection?.state === signalR.HubConnectionState.Connected) {
      if (callStatus === 'outgoing' && outgoingCall)
        voiceConnection.invoke('CancelCall', outgoingCall.callerId).catch(() => {})
      if (callStatus === 'incoming' && incomingCall)
        voiceConnection.invoke('RejectCall', incomingCall.callerId).catch(() => {})
    }
  },

  toggleMute: () => {
    const userId = useAuthStore.getState().user?.id
    const me = get().participants.find((p) => p.userId === userId)
    if (me) {
      voiceManager?.toggleMute(!me.isMuted)
    }
  },

  toggleDeafen: () => {
    const userId = useAuthStore.getState().user?.id
    const me = get().participants.find((p) => p.userId === userId)
    if (me) {
      voiceManager?.toggleDeafen(!me.isDeafened)
    }
  },

  setParticipants: (p) => set({ participants: p })
}))

async function internalJoinSession(
  chatId: string,
  userId: string,
  set: VoiceSet,
  get: VoiceGet
): Promise<void> {
  if (!voiceConnection) return
  if (!voiceManager) {
    voiceManager = new VoiceManager(voiceConnection)
    voiceManager.onRemoteStream = (uid, stream) => {
      set((state: VoiceState) => {
        const newStreams = new Map(state.audioStreams)
        newStreams.set(uid, stream)
        return {
          audioStreams: newStreams,
          callStatus: state.callStatus === 'outgoing' ? 'connected' : state.callStatus
        }
      })
    }
    voiceManager.onParticipantLeft = (uid) => {
      set((state: VoiceState) => {
        const newStreams = new Map(state.audioStreams)
        newStreams.delete(uid)
        return {
          audioStreams: newStreams,
          participants: state.participants.filter((p) => p.userId !== uid)
        }
      })
    }
  }

  try {
    const session = await voiceManager.joinSession(chatId, userId)
    if (session && session.participants) {
      const mappedParticipants: VoiceParticipant[] = Object.values(session.participants).map(
        (p) => ({
          userId: p.userId,
          username: p.username ?? 'Unknown',
          isMuted: p.isMuted ?? false,
          isDeafened: p.isDeafened ?? false,
          isSpeaking: false
        })
      )
      set({ isInCall: true, participants: mappedParticipants })
      if (get().callStatus === 'outgoing' && mappedParticipants.length > 1) {
        set({ callStatus: 'connected' })
      }
    }
  } catch {
    get().endCall()
  }
}
