import { JSX, useEffect } from 'react'
import { Sidebar } from '../components/Sidebar/Sidebar'
import { ChatArea } from '../components/Chat/ChatArea'
import { signalRService } from '../services/signalR'
import { useMessageStore } from '../stores/messageStore'
import { useVoiceStore } from '../stores/voiceStore'
import { IncomingCallModal } from '@renderer/components/Voice/IncomingCallModal'

export const Home = (): JSX.Element => {
  const { initializeListeners } = useMessageStore()
  const { initVoiceListeners } = useVoiceStore()

  useEffect(() => {
    const initApp = async (): Promise<void> => {
      await signalRService.connect()
      initializeListeners()

      await initVoiceListeners()
    }
    initApp()
  }, [initializeListeners, initVoiceListeners])

  return (
    <div className="h-screen w-screen relative overflow-hidden bg-[#0a0a0c] text-white selection:bg-[#00b4ff]/30 font-sans">
      {/* Background Effects */}
      <div
        className="absolute inset-0 z-0 pointer-events-none"
        style={{
          background: `
            radial-gradient(circle at 15% 50%, rgba(0, 180, 255, 0.08), transparent 40%),
            radial-gradient(circle at 85% 80%, rgba(0, 180, 255, 0.04), transparent 40%),
            radial-gradient(circle at 50% 0%, rgba(6, 182, 212, 0.05), transparent 40%)
          `
        }}
      />

      {/* Main Layout */}
      <div className="relative z-10 h-full p-4 md:p-6 flex gap-6">
        <Sidebar />
        <ChatArea />
      </div>
      <IncomingCallModal />
    </div>
  )
}
