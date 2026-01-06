import { JSX, useRef, useState } from 'react'
import { useStreamStore } from '../../stores/streamStore'
import { signalRService } from '../../services/signalR'
import { useStream } from '../../hooks/useStream'

export const StreamPlayer = ({ chatId }: { chatId: string }): JSX.Element | null => {
  const {
    isStreaming,
    isWatching,
    remoteStream,
    stopSharing,
    leaveStream,
    joinStream,
    setQuality,
    availableStreams
  } = useStreamStore()

  const containerRef = useRef<HTMLDivElement>(null)
  const [isFullscreen, setIsFullscreen] = useState(false)

  // 1. Determine State
  const localStream = signalRService.streamManager?.getLocalStream() || null
  const streamInfo = availableStreams[chatId]
  const hasActiveStream = !!streamInfo

  // 2. Decide what to render
  const streamToShow = isWatching ? remoteStream : isStreaming ? localStream : null

  const videoRef = useStream(streamToShow)

  // 3. Fullscreen Logic
  const toggleFullscreen = (): void => {
    if (!containerRef.current) return

    if (!document.fullscreenElement) {
      containerRef.current.requestFullscreen().catch((err) => {
        console.error(`Error attempting to enable fullscreen: ${err.message}`)
      })
      setIsFullscreen(true)
    } else {
      document.exitFullscreen()
      setIsFullscreen(false)
    }
  }

  // If no stream exists at all, render nothing
  if (!isStreaming && !hasActiveStream) return null

  return (
    <div ref={containerRef} className="relative w-full h-full bg-black overflow-hidden group">
      {/* --- VIDEO LAYER --- */}
      {(isStreaming || isWatching) && (
        <video
          ref={videoRef}
          className="w-full h-full object-contain pointer-events-none"
          autoPlay
          playsInline
          muted={isStreaming}
        />
      )}

      {/* --- PREVIEW / JOIN LAYER --- */}
      {!isStreaming && !isWatching && hasActiveStream && (
        <div className="absolute inset-0 z-20 flex flex-col items-center justify-center bg-gray-900/50 backdrop-blur-md">
          <div className="text-center space-y-4">
            <div className="w-16 h-16 rounded-full bg-emerald-500/20 text-emerald-400 flex items-center justify-center mx-auto animate-pulse">
              <svg
                xmlns="http://www.w3.org/2000/svg"
                fill="none"
                viewBox="0 0 24 24"
                strokeWidth={1.5}
                stroke="currentColor"
                className="w-8 h-8"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  d="M5.25 5.653c0-.856.917-1.398 1.667-.986l11.54 6.348a1.125 1.125 0 010 1.971l-11.54 6.347a1.125 1.125 0 01-1.667-.985V5.653z"
                />
              </svg>
            </div>
            <div>
              <h3 className="text-white font-bold text-lg">{streamInfo.hostUsername} is Live</h3>
              <p className="text-gray-400 text-sm">Click to watch the stream</p>
            </div>
            <button
              onClick={() => joinStream(chatId)}
              className="px-6 py-2 bg-emerald-500 hover:bg-emerald-600 text-white font-bold rounded-full transition-all shadow-lg shadow-emerald-500/20"
            >
              Join Stream
            </button>
          </div>
        </div>
      )}

      {/* --- CONTROLS OVERLAY --- */}
      {(isStreaming || isWatching) && (
        <div className="absolute bottom-0 left-0 right-0 p-4 bg-gradient-to-t from-black/90 via-black/50 to-transparent opacity-0 group-hover:opacity-100 transition-opacity flex justify-between items-center z-30">
          <div className="flex items-center gap-2">
            <div className="w-2 h-2 rounded-full bg-red-500 animate-pulse" />
            <span className="text-white text-xs font-bold uppercase tracking-wider shadow-black drop-shadow-md">
              {isStreaming ? 'You are Live' : 'Live'}
            </span>
          </div>

          <div className="flex items-center gap-3">
            {/* Quality Selector (Host Only) */}
            {isStreaming && (
              <select
                className="bg-black/40 backdrop-blur-md text-white text-[10px] py-1 px-2 rounded border border-white/10 outline-none cursor-pointer hover:bg-black/60"
                onChange={(e) => setQuality(e.target.value)}
                defaultValue="1080p_60"
              >
                <option value="720p_30">720p 30</option>
                <option value="1080p_60">1080p 60</option>
                <option value="4k_60">4K 60</option>
              </select>
            )}

            {/* Fullscreen Button */}
            <button
              onClick={toggleFullscreen}
              className="text-white/80 hover:text-white transition-colors p-1"
              title={isFullscreen ? 'Exit Fullscreen' : 'Fullscreen'}
            >
              {isFullscreen ? (
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
                    d="M9 9V4.5M9 9H4.5M9 9L3.75 3.75M9 15v4.5M9 15H4.5M9 15l-5.25 5.25M15 9h4.5M15 9V4.5M15 9l5.25-5.25M15 15h4.5M15 15v4.5M15 15l5.25 5.25"
                  />
                </svg>
              ) : (
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
                    d="M3.75 3.75v4.5m0-4.5h4.5m-4.5 0L9 9M3.75 20.25v-4.5m0 4.5h4.5m-4.5 0L9 15M20.25 3.75h-4.5m4.5 0v4.5m0-4.5L15 9m5.25 11.25h-4.5m4.5 0v-4.5m0 4.5L15 15"
                  />
                </svg>
              )}
            </button>

            {/* End/Leave Button */}
            <button
              onClick={() => (isStreaming ? stopSharing() : leaveStream())}
              className="bg-red-500/80 hover:bg-red-600 text-white px-3 py-1 rounded text-xs font-bold transition-colors backdrop-blur-sm"
            >
              {isStreaming ? 'End' : 'Leave'}
            </button>
          </div>
        </div>
      )}
    </div>
  )
}
