import { JSX, useEffect, useRef, useState, useCallback } from 'react'
import { useChatStore } from '../../stores/chatStore'
import { useMessageStore } from '../../stores/messageStore'
import { useAuthStore } from '../../stores/authStore'
import { useVoiceStore } from '../../stores/voiceStore'
import { ActiveCallBanner } from '../Voice/ActiveCallBanner'
import { StreamPlayer } from '../Stream/StreamPlayer'
import { useStreamStore } from '../../stores/streamStore'

export const ChatArea = (): JSX.Element => {
  const { user } = useAuthStore()
  const { chats, selectedChatId } = useChatStore()
  const { messages, fetchMessages, sendMessage, isLoading } = useMessageStore()
  const { isInCall, callStatus, startCall } = useVoiceStore()

  // STREAM STORE
  const { startSharing, joinStream, isStreaming, isWatching, availableStreams, monitorChat } =
    useStreamStore()

  const [newMessage, setNewMessage] = useState('')
  const [showParticipants, setShowParticipants] = useState(false)

  // RESIZE STATE
  const [streamHeight, setStreamHeight] = useState(320)
  const [isResizing, setIsResizing] = useState(false)
  const chatContainerRef = useRef<HTMLDivElement>(null)

  const messagesEndRef = useRef<HTMLDivElement>(null)
  const selectedChat = chats.find((c) => c.id === selectedChatId)

  // LOGIC: Show stream pane if I am hosting OR a stream is available in this chat
  // We show it even if I haven't joined yet (so I can see the "Join" preview)
  const showStreamPane =
    selectedChatId && (isStreaming || isWatching || !!availableStreams[selectedChatId])

  // Helper to check if we can join (Stream exists + Not hosting + Not watching)
  const canJoin =
    selectedChatId && !!availableStreams[selectedChatId] && !isStreaming && !isWatching

  useEffect(() => {
    if (selectedChatId) {
      fetchMessages(selectedChatId)
    }
  }, [selectedChatId, fetchMessages])

  useEffect(() => {
    if (selectedChatId) {
      monitorChat(selectedChatId)
    }
  }, [selectedChatId, monitorChat])

  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }, [messages])

  const handleSend = async (): Promise<void> => {
    if (!newMessage.trim() || !selectedChatId) return

    const content = newMessage
    setNewMessage('')

    await sendMessage(selectedChatId, content)
  }

  const handleKeyPress = (e: React.KeyboardEvent): void => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      handleSend()
    }
  }

  const handleStartCall = (): void => {
    if (selectedChatId && !isInCall && selectedChat) {
      const friendName = selectedChat.type === 'direct' ? selectedChat.title : 'Group Call'

      const friendId =
        selectedChat.type === 'direct'
          ? selectedChat.members?.find((m) => m.id !== user?.id)?.id
          : 'GROUP'

      if (friendId) {
        startCall(selectedChatId, friendId, friendName)
      } else {
        console.warn('Could not determine friend ID for call')
      }
    }
  }

  // --- RESIZE LOGIC ---
  const startResizing = useCallback(() => {
    setIsResizing(true)
  }, [])

  const stopResizing = useCallback(() => {
    setIsResizing(false)
  }, [])

  const resize = useCallback(
    (mouseMoveEvent: MouseEvent) => {
      if (isResizing && chatContainerRef.current) {
        const containerRect = chatContainerRef.current.getBoundingClientRect()
        // Calculate height relative to the container top, subtracting the header height (~80px)
        const newHeight = mouseMoveEvent.clientY - containerRect.top - 80

        // Clamp height: Min 150px, Max 70% of viewport
        if (newHeight > 150 && newHeight < window.innerHeight * 0.7) {
          setStreamHeight(newHeight)
        }
      }
    },
    [isResizing]
  )

  useEffect(() => {
    if (isResizing) {
      window.addEventListener('mousemove', resize)
      window.addEventListener('mouseup', stopResizing)
    }
    return () => {
      window.removeEventListener('mousemove', resize)
      window.removeEventListener('mouseup', stopResizing)
    }
  }, [isResizing, resize, stopResizing])

  if (!selectedChat) {
    return (
      <div className="flex-1 bg-white/[0.02] backdrop-blur-3xl border border-white/[0.05] rounded-[2.5rem] flex items-center justify-center shadow-2xl">
        <div className="text-center text-gray-500">
          <p className="text-lg font-medium">No chat selected</p>
          <p className="text-sm">Choose a conversation from the sidebar</p>
        </div>
      </div>
    )
  }

  return (
    <div
      ref={chatContainerRef}
      className="flex-1 bg-white/[0.02] backdrop-blur-3xl border border-white/[0.05] rounded-[2.5rem] flex flex-col relative overflow-hidden shadow-2xl"
    >
      {/* --- HEADER --- */}
      <div className="h-20 px-6 flex items-center justify-between border-b border-white/[0.05] bg-black/10 shrink-0">
        <div className="flex items-center gap-4">
          <div className="w-12 h-12 rounded-2xl flex items-center justify-center overflow-hidden bg-slate-800 text-gray-200 border border-white/[0.05]">
            <img
              src={selectedChat.avatarUrl}
              alt={selectedChat.title}
              className="w-full h-full object-cover"
              onError={(e) => {
                ;(e.target as HTMLImageElement).style.display = 'none'
                ;(e.target as HTMLImageElement).parentElement!.innerText = selectedChat.title
                  .substring(0, 2)
                  .toUpperCase()
              }}
            />
          </div>
          <div>
            <h1 className="text-lg font-bold text-white">{selectedChat.title}</h1>
            <div className="flex items-center gap-2 mt-0.5">
              <span
                className={`text-xs font-medium ${selectedChat.status === 'online' ? 'text-emerald-400' : 'text-gray-500'}`}
              >
                {selectedChat.type === 'group'
                  ? `${selectedChat.members?.length || 0} members`
                  : selectedChat.status || 'Offline'}
              </span>
            </div>
          </div>
        </div>

        {/* ACTION BUTTONS */}
        <div className="flex items-center gap-2">
          {/* 1. JOIN STREAM BUTTON */}
          {canJoin && (
            <button
              onClick={() => selectedChatId && joinStream(selectedChatId)}
              className="h-10 px-4 rounded-xl flex items-center gap-2 bg-emerald-500/10 text-emerald-400 hover:bg-emerald-500/20 transition-all border border-emerald-500/20 animate-pulse"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                viewBox="0 0 24 24"
                fill="currentColor"
                className="w-4 h-4"
              >
                <path d="M4.5 4.5a3 3 0 00-3 3v9a3 3 0 003 3h8.25a3 3 0 003-3v-9a3 3 0 00-3-3H4.5zM19.94 18.75l-2.69-2.69V7.94l2.69-2.69c.944-.945 2.56-.276 2.56 1.06v11.38c0 1.336-1.616 2.005-2.56 1.06z" />
              </svg>
              <span className="text-xs font-bold">Join Stream</span>
            </button>
          )}

          {/* 2. SHARE SCREEN BUTTON */}
          {/* Hide this if we are watching OR if we can join someone else */}
          {!isWatching && !canJoin && (
            <button
              onClick={() => selectedChatId && startSharing(selectedChatId)}
              disabled={isStreaming}
              className={`h-10 w-10 rounded-xl flex items-center justify-center transition-all ${
                isStreaming
                  ? 'text-red-500 bg-red-500/10 cursor-default border border-red-500/20'
                  : 'bg-white/[0.05] hover:bg-white/[0.08] text-gray-400 hover:text-white'
              }`}
              title="Share Screen"
            >
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                className="w-5 h-5"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M9 17.25v1.007a3 3 0 01-.879 2.122L7.5 21h9l-.621-.621A3 3 0 0115 18.257V17.25m6-12V15a2.25 2.25 0 01-2.25 2.25H5.25A2.25 2.25 0 013 15V5.25m18 0A2.25 2.25 0 0018.75 3H5.25A2.25 2.25 0 003 5.25m18 0V12a2.25 2.25 0 01-2.25 2.25H5.25"
                />
              </svg>
            </button>
          )}

          {/* 3. VOICE CALL BUTTON */}
          <button
            onClick={handleStartCall}
            disabled={isInCall || callStatus === 'outgoing'}
            className={`h-10 w-10 rounded-xl flex items-center justify-center transition-all ${
              isInCall || callStatus === 'outgoing'
                ? 'text-emerald-500 bg-emerald-500/10 cursor-default shadow-glow-sm'
                : 'bg-white/[0.05] hover:bg-white/[0.08] text-gray-400 hover:text-white'
            }`}
            title={callStatus === 'outgoing' ? 'Calling...' : 'Start Voice Call'}
          >
            {callStatus === 'outgoing' && (
              <span className="animate-ping absolute inline-flex h-6 w-6 rounded-full bg-emerald-400 opacity-20"></span>
            )}

            <svg className="w-5 h-5 relative" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1.5}
                d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z"
              />
            </svg>
          </button>

          {selectedChat.type === 'group' && (
            <button
              onClick={() => setShowParticipants(!showParticipants)}
              className={`h-10 w-10 rounded-xl flex items-center justify-center transition-all ${
                showParticipants
                  ? 'bg-white/[0.1] text-white'
                  : 'bg-white/[0.05] text-gray-400 hover:bg-white/[0.08] hover:text-white'
              }`}
            >
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={1.5}
                  d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z"
                />
              </svg>
            </button>
          )}
        </div>
      </div>

      {isInCall && <ActiveCallBanner />}

      {/* --- RESIZABLE STREAM PANE --- */}
      {showStreamPane && selectedChatId && (
        <div className="relative bg-black shrink-0 flex flex-col" style={{ height: streamHeight }}>
          <div className="flex-1 min-h-0 relative">
            <StreamPlayer chatId={selectedChatId} />
          </div>

          {/* RESIZE HANDLE */}
          <div
            className="h-2 bg-[#18181b] hover:bg-[#00b4ff] cursor-row-resize flex items-center justify-center transition-colors shrink-0 z-40 border-t border-b border-white/[0.05]"
            onMouseDown={startResizing}
          >
            <div className="w-10 h-1 rounded-full bg-white/20" />
          </div>
        </div>
      )}

      {/* --- MESSAGES AREA --- */}
      <div className="flex-1 flex overflow-hidden">
        <div className="flex-1 flex flex-col min-w-0">
          <div className="flex-1 overflow-y-auto px-6 space-y-6 pb-28 pt-6 custom-scrollbar">
            {messages.length === 0 && !isLoading && (
              <div className="flex items-center justify-center h-full text-gray-500 text-sm">
                Say hello! 👋
              </div>
            )}

            {messages.map((msg, index) => {
              const isMe = msg.isMe

              const senderId = msg.senderId
              const senderName = msg.senderName || 'Unknown'

              const showAvatar = !isMe && (index === 0 || messages[index - 1].senderId !== senderId)

              const initials = msg.senderInitials || senderName.substring(0, 2).toUpperCase()
              const timeString =
                msg.displayTime ||
                new Date(msg.sentAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })

              return (
                <div
                  key={msg.id}
                  className={`flex ${isMe ? 'justify-end' : 'justify-start'} group animate-in slide-in-from-bottom-2 duration-300`}
                >
                  <div className={`flex gap-3 max-w-[75%] ${isMe ? 'flex-row-reverse' : ''}`}>
                    <div className="w-8 flex-shrink-0 flex flex-col justify-end">
                      {showAvatar ? (
                        <div className="w-8 h-8 rounded-xl bg-slate-800 flex items-center justify-center text-xs font-bold text-gray-400 border border-white/[0.05]">
                          {initials}
                        </div>
                      ) : (
                        !isMe && <div className="w-8" />
                      )}
                    </div>

                    <div className={`flex flex-col ${isMe ? 'items-end' : 'items-start'}`}>
                      {!isMe && selectedChat.type === 'group' && showAvatar && (
                        <span className="text-[10px] text-gray-400 mb-1 ml-1 font-medium">
                          {senderName}
                        </span>
                      )}

                      <div
                        className={`
                          px-4 py-3 text-sm leading-relaxed shadow-lg break-words max-w-full text-left
                          ${
                            isMe
                              ? 'bg-gradient-to-br from-[#00b4ff] to-[#0088dd] text-white rounded-2xl rounded-br-sm'
                              : 'bg-[#18181b] text-gray-100 rounded-2xl rounded-bl-sm border border-white/[0.05]'
                          }
                        `}
                      >
                        {msg.content}
                      </div>

                      <span
                        className={`text-[10px] text-gray-600 mt-1 opacity-0 group-hover:opacity-100 transition-opacity ${isMe ? 'mr-1' : 'ml-1'}`}
                      >
                        {timeString}
                      </span>
                    </div>
                  </div>
                </div>
              )
            })}
            <div ref={messagesEndRef} />
          </div>

          <div className="absolute bottom-4 left-4 right-4 z-20">
            <div className="bg-[#0d0d0f]/90 backdrop-blur-xl border border-white/[0.08] rounded-2xl flex items-end shadow-2xl shadow-black/50">
              <textarea
                value={newMessage}
                onChange={(e) => setNewMessage(e.target.value)}
                onKeyDown={handleKeyPress}
                placeholder={`Message ${selectedChat.title}...`}
                className="flex-1 bg-transparent text-white placeholder-gray-600 focus:outline-none resize-none py-4 px-5 text-sm"
                rows={1}
                style={{ minHeight: '56px', maxHeight: '120px' }}
              />
              <div className="flex items-center gap-1 p-2">
                <button
                  onClick={handleSend}
                  className="p-3 bg-[#00b4ff] hover:bg-[#00a0e6] text-white rounded-xl transition-all shadow-lg shadow-[#00b4ff]/25 disabled:opacity-50 disabled:cursor-not-allowed"
                  disabled={!newMessage.trim()}
                >
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path
                      strokeLinecap="round"
                      strokeLinejoin="round"
                      strokeWidth={2}
                      d="M5 12h14M12 5l7 7-7 7"
                    />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        {selectedChat.type === 'group' && showParticipants && (
          <div className="w-56 border-l border-white/[0.05] bg-black/20 p-4 hidden md:block">
            <h3 className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-4">
              Members
            </h3>
            <div className="space-y-2 overflow-y-auto max-h-full">
              {selectedChat.members?.map((m) => (
                <div
                  key={m.id}
                  className="flex items-center gap-2 text-sm text-gray-300 p-2 hover:bg-white/[0.05] rounded-lg transition-colors cursor-default"
                >
                  <div className="w-6 h-6 rounded-full bg-slate-700 flex items-center justify-center text-[10px] font-bold">
                    {(m.username || '?').substring(0, 2).toUpperCase()}
                  </div>
                  <span className="truncate">{m.username}</span>
                </div>
              ))}
            </div>
          </div>
        )}
      </div>
    </div>
  )
}
