import { JSX, useEffect, useRef } from 'react'
import { useVoiceStore } from '../../stores/voiceStore'
import { useAuthStore } from '../../stores/authStore'
import { Mic, MicOff, PhoneOff, PhoneOutgoing, Headphones, VolumeX } from 'lucide-react'

const AudioPlayer = ({ stream }: { stream: MediaStream }): JSX.Element => {
  const audioRef = useRef<HTMLAudioElement>(null)
  useEffect(() => {
    const el = audioRef.current
    if (el && stream) {
      el.srcObject = stream
      el.play().catch((e) => {
        if (e.name !== 'AbortError') console.error(e)
      })
    }
  }, [stream])
  return <audio ref={audioRef} autoPlay controls={false} />
}

export const ActiveCallBanner = (): JSX.Element => {
  const { user } = useAuthStore()
  const {
    participants,
    endCall,
    toggleMute,
    toggleDeafen,
    audioStreams,
    callStatus,
    outgoingCall
  } = useVoiceStore()

  const me = participants.find((p) => p.userId === user?.id)
  const isMuted = me?.isMuted || false
  const isDeafened = me?.isDeafened || false

  const isRinging = callStatus === 'outgoing'
  const statusText = isRinging ? `Calling ${outgoingCall?.callerName}...` : 'Voice Connected'

  return (
    <div className="bg-[#121214] border-b border-white/[0.05] px-4 py-2 flex items-center justify-between shrink-0 animate-in slide-in-from-top-2">
      {Array.from(audioStreams.entries()).map(([uid, s]) => (
        <AudioPlayer key={uid} stream={s} />
      ))}

      {/* Status & Participants */}
      <div className="flex items-center gap-4 overflow-hidden">
        <div
          className={`flex items-center gap-2 font-bold text-[10px] uppercase tracking-wider px-2 border-r border-white/[0.1] ${isRinging ? 'text-yellow-400' : 'text-emerald-400'}`}
        >
          {isRinging ? (
            <PhoneOutgoing className="w-3 h-3 animate-pulse" />
          ) : (
            <span className="w-1.5 h-1.5 rounded-full bg-emerald-500 animate-pulse" />
          )}
          {statusText}
        </div>

        {!isRinging && (
          <div className="flex items-center gap-2">
            {participants.map((p) => (
              <div key={p.userId} className="relative group cursor-default" title={p.username}>
                {/* Avatar with Speaking Glow */}
                <div
                  className={`
                                    w-8 h-8 rounded-full flex items-center justify-center text-[10px] font-bold border-2 transition-all
                                    ${
                                      p.isSpeaking
                                        ? 'border-emerald-500 shadow-[0_0_10px_rgba(16,185,129,0.6)] scale-105'
                                        : 'border-transparent bg-slate-700 text-gray-300'
                                    }
                                `}
                >
                  {p.username.substring(0, 2).toUpperCase()}
                </div>

                {/* Status Icons Overlay */}
                <div className="absolute -bottom-1 -right-1 flex gap-0.5">
                  {p.isDeafened ? (
                    <div className="bg-red-500 rounded-full p-0.5 border border-black">
                      <VolumeX className="w-2 h-2 text-white" />
                    </div>
                  ) : p.isMuted ? (
                    <div className="bg-[#121214] rounded-full p-0.5 border border-black">
                      <MicOff className="w-2 h-2 text-red-500" />
                    </div>
                  ) : null}
                </div>
              </div>
            ))}
          </div>
        )}
      </div>

      {/* Controls */}
      <div className="flex items-center gap-2">
        {!isRinging && (
          <>
            <button
              onClick={toggleMute}
              className={`p-2 rounded-lg transition-colors ${isMuted ? 'bg-red-500/20 text-red-500' : 'hover:bg-white/[0.05] text-gray-400'}`}
              title="Toggle Mute"
            >
              {isMuted ? <MicOff className="w-4 h-4" /> : <Mic className="w-4 h-4" />}
            </button>
            <button
              onClick={toggleDeafen}
              className={`p-2 rounded-lg transition-colors ${isDeafened ? 'bg-red-500/20 text-red-500' : 'hover:bg-white/[0.05] text-gray-400'}`}
              title="Toggle Deafen"
            >
              {isDeafened ? <VolumeX className="w-4 h-4" /> : <Headphones className="w-4 h-4" />}
            </button>
          </>
        )}

        <button
          onClick={endCall}
          className="p-2 rounded-lg bg-red-500/10 text-red-500 hover:bg-red-500 hover:text-white transition-all"
        >
          <PhoneOff className="w-4 h-4" />
        </button>
      </div>
    </div>
  )
}
