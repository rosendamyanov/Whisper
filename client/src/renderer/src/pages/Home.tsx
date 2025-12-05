import { useState } from 'react'

export default function Home() {
  const [message, setMessage] = useState('')

  return (
    <div className='h-screen flex bg-dark-950 text-white'>
      {/* Server List Sidebar */}
      <div className='w-16 bg-dark-950 flex flex-col items-center py-3 space-y-2 border-r border-dark-900'>
        <button className='w-12 h-12 rounded-full bg-primary-500 flex items-center justify-center hover:rounded-2xl transition-all duration-200'>
          <span className='text-xl font-bold'>W</span>
        </button>
        <div className='w-8 h-0.5 bg-dark-800 rounded' />
        <button className='w-12 h-12 rounded-full bg-dark-800 flex items-center justify-center hover:bg-primary-500 hover:rounded-2xl transition-all duration-200'>
          <span className='text-2xl'>+</span>
        </button>
      </div>

      {/* Channels Sidebar */}
      <div className='w-60 bg-dark-900 flex flex-col'>
        <div className='h-12 px-4 flex items-center border-b border-dark-800 shadow-md'>
          <h2 className='font-semibold text-white'>Whisper Server</h2>
        </div>
        
        <div className='flex-1 overflow-y-auto p-2'>
          <div className='mb-4'>
            <h3 className='px-2 mb-2 text-xs font-semibold text-gray-400 uppercase'>Text Channels</h3>
            <button className='w-full px-2 py-1.5 rounded hover:bg-dark-800 text-gray-300 hover:text-white text-left transition flex items-center'>
              <span className='mr-1.5'>#</span>
              <span>general</span>
            </button>
            <button className='w-full px-2 py-1.5 rounded hover:bg-dark-800 text-gray-300 hover:text-white text-left transition flex items-center'>
              <span className='mr-1.5'>#</span>
              <span>random</span>
            </button>
          </div>

          <div>
            <h3 className='px-2 mb-2 text-xs font-semibold text-gray-400 uppercase'>Voice Channels</h3>
            <button className='w-full px-2 py-1.5 rounded hover:bg-dark-800 text-gray-300 hover:text-white text-left transition flex items-center'>
              <span className='mr-1.5'>🔊</span>
              <span>General</span>
            </button>
          </div>
        </div>

        <div className='h-14 bg-dark-950 px-2 flex items-center border-t border-dark-800'>
          <div className='w-8 h-8 rounded-full bg-primary-500 flex items-center justify-center mr-2'>
            <span className='text-sm font-bold'>U</span>
          </div>
          <div className='flex-1'>
            <p className='text-sm font-medium'>Username</p>
            <p className='text-xs text-gray-400'>#0000</p>
          </div>
        </div>
      </div>

      {/* Main Chat Area */}
      <div className='flex-1 flex flex-col'>
        <div className='h-12 px-4 flex items-center border-b border-dark-800 shadow-md'>
          <span className='text-gray-400 mr-2'>#</span>
          <h2 className='font-semibold text-white'>general</h2>
        </div>

        <div className='flex-1 overflow-y-auto p-4 space-y-4'>
          <div className='hover:bg-dark-900 p-2 rounded'>
            <div className='flex items-start'>
              <div className='w-10 h-10 rounded-full bg-primary-500 flex items-center justify-center mr-3'>
                <span className='font-bold'>W</span>
              </div>
              <div>
                <div className='flex items-center'>
                  <span className='font-semibold mr-2'>Whisper Bot</span>
                  <span className='text-xs text-gray-400'>12:00 PM</span>
                </div>
                <p className='text-gray-300'>Welcome to Whisper! Start chatting with your friends.</p>
              </div>
            </div>
          </div>
        </div>

        <div className='p-4'>
          <div className='bg-dark-800 rounded-lg'>
            <input
              type='text'
              value={message}
              onChange={(e) => setMessage(e.target.value)}
              placeholder='Message #general'
              className='w-full bg-transparent px-4 py-3 text-white placeholder-gray-500 focus:outline-none'
            />
          </div>
        </div>
      </div>

      {/* Members Sidebar */}
      <div className='w-60 bg-dark-900 p-3'>
        <h3 className='mb-3 text-xs font-semibold text-gray-400 uppercase'>Online — 1</h3>
        <div className='space-y-2'>
          <div className='flex items-center px-2 py-1.5 rounded hover:bg-dark-800 cursor-pointer transition'>
            <div className='relative'>
              <div className='w-8 h-8 rounded-full bg-primary-500 flex items-center justify-center mr-3'>
                <span className='text-sm font-bold'>U</span>
              </div>
              <div className='absolute bottom-0 right-2 w-3 h-3 bg-green-500 rounded-full border-2 border-dark-900' />
            </div>
            <span className='text-sm text-gray-300'>Username</span>
          </div>
        </div>
      </div>
    </div>
  )
}