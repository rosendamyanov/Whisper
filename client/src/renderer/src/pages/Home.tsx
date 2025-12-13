import { useState } from 'react'

// Types
interface Member {
  id: string
  name: string
  avatar: string
  status: 'online' | 'away' | 'dnd' | 'offline'
  role?: 'admin' | 'member'
}

interface ChatItem {
  id: string
  name: string
  type: 'direct' | 'group'
  status?: 'online' | 'away' | 'dnd' | 'offline'
  members?: number
  memberList?: Member[]
  avatar: string
  lastMessage: string
  lastMessageTime: string
  unread?: number
  isLive?: boolean
}

interface Message {
  id: string
  senderId: string
  senderName?: string
  content: string
  timestamp: string
  isMe: boolean
  avatar?: string
}

// Mock Data
const mockMembers: Member[] = [
  { id: '1', name: 'Alex Chen', avatar: 'AC', status: 'online', role: 'admin' },
  { id: '2', name: 'Sarah Miller', avatar: 'SM', status: 'away', role: 'member' },
  { id: '3', name: 'Jordan Lee', avatar: 'JL', status: 'offline', role: 'member' },
  { id: '4', name: 'Me', avatar: 'Me', status: 'online', role: 'member' },
]

const mockChats: ChatItem[] = [
  { id: '1', name: 'Alex Chen', type: 'direct', status: 'online', avatar: 'AC', lastMessage: 'Hey! Are you free tonight?', lastMessageTime: '2m', unread: 2 },
  { 
    id: 'group-1', 
    name: 'Weekend Project', 
    type: 'group', 
    members: 4, 
    memberList: mockMembers,
    avatar: '#', 
    lastMessage: 'Sarah: I just pushed the changes.', 
    lastMessageTime: '5m', 
    isLive: true 
  },
  { id: '2', name: 'Sarah Miller', type: 'direct', status: 'away', avatar: 'SM', lastMessage: 'That sounds great!', lastMessageTime: '15m' },
  { id: '3', name: 'Jordan Lee', type: 'direct', status: 'dnd', avatar: 'JL', lastMessage: 'Let me check...', lastMessageTime: '1h' },
]

const mockMessages: Message[] = [
  { id: '1', senderId: '2', senderName: 'Sarah', avatar: 'SM', content: 'Guys, check out the new design update.', timestamp: '12:00 PM', isMe: false },
  { id: '2', senderId: 'me', content: 'Looking good! I love the new color palette.', timestamp: '12:02 PM', isMe: true },
  { id: '3', senderId: '1', senderName: 'Alex', avatar: 'AC', content: 'I think we should make the sidebar a bit darker though.', timestamp: '12:03 PM', isMe: false },
  { id: '4', senderId: '1', senderName: 'Alex', avatar: 'AC', content: 'Im going to start a stream to show you what I mean.', timestamp: '12:05 PM', isMe: false },
]

const statusColors = {
  online: 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.8)]',
  away: 'bg-amber-400 shadow-[0_0_8px_rgba(251,191,36,0.8)]',
  dnd: 'bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.8)]',
  offline: 'bg-slate-600'
}

const statusText = {
  online: 'Online',
  away: 'Away',
  dnd: 'Do Not Disturb',
  offline: 'Offline'
}

export default function Home() {
  const [chats] = useState<ChatItem[]>(mockChats)
  const [selectedChat, setSelectedChat] = useState<ChatItem | null>(mockChats[1]) 
  const [messages] = useState<Message[]>(mockMessages)
  const [newMessage, setNewMessage] = useState('')
  const [searchQuery, setSearchQuery] = useState('')
  const [isStreamExpanded, setIsStreamExpanded] = useState(true)
  const [showParticipants, setShowParticipants] = useState(true)
  const [showAddFriend, setShowAddFriend] = useState(false)

  const filteredChats = chats.filter(c => 
    c.name.toLowerCase().includes(searchQuery.toLowerCase())
  )

  const onlineCount = chats.filter(c => c.type === 'direct' && c.status === 'online').length

  return (
    <div className="h-screen w-screen relative overflow-hidden bg-[#0a0a0c] text-white selection:bg-[#00b4ff]/30 font-sans">
      
      {/* Background */}
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

      {/* Main Container */}
      <div className="relative z-10 h-full p-4 md:p-6 flex gap-6">
        
        {/* LEFT PANEL: Sidebar */}
        <div className="w-80 flex-shrink-0 flex flex-col gap-4">
          
          {/* User Profile Card */}
          <div className="bg-white/[0.02] backdrop-blur-2xl border border-white/[0.05] p-5 rounded-3xl flex items-center justify-between shadow-xl">
            <div className="flex items-center gap-3">
              <div className="relative">
                <div className="w-11 h-11 rounded-xl bg-gradient-to-br from-[#00b4ff] to-[#0078ff] flex items-center justify-center font-bold text-base shadow-lg shadow-[#00b4ff]/25">
                  Me
                </div>
                <div className={`absolute -bottom-1 -right-1 w-3.5 h-3.5 rounded-full border-[2.5px] border-[#0a0a0c] ${statusColors.online}`} />
              </div>
              <div>
                <h2 className="font-semibold text-sm text-white">Username</h2>
                <p className="text-[11px] text-emerald-400 font-medium">Online</p>
              </div>
            </div>
            <button className="p-2.5 rounded-xl text-gray-400 hover:text-white hover:bg-white/[0.05] transition-all">
              <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
              </svg>
            </button>
          </div>

          {/* Chat List Card */}
          <div className="flex-1 bg-white/[0.02] backdrop-blur-2xl border border-white/[0.05] rounded-[2rem] flex flex-col shadow-2xl overflow-hidden">
            
            {/* Search */}
            <div className="p-4 pb-3">
              <div className="relative">
                <svg className="absolute left-3.5 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
                </svg>
                <input 
                  type="text" 
                  placeholder="Search conversations..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="w-full bg-black/30 border border-white/[0.05] rounded-xl py-3 pl-10 pr-4 text-sm text-gray-200 focus:outline-none focus:bg-black/40 focus:border-[#00b4ff]/30 transition-all placeholder:text-gray-600"
                />
              </div>
              
              {/* Online indicator */}
              <div className="flex items-center gap-2 mt-3 px-1">
                <div className={`w-2 h-2 rounded-full ${statusColors.online}`} />
                <span className="text-xs text-gray-400">{onlineCount} friends online</span>
              </div>
            </div>

            {/* Chat List */}
            <div className="flex-1 overflow-y-auto px-2 pb-2 space-y-1">
              {filteredChats.map(chat => (
                <button
                  key={chat.id}
                  onClick={() => setSelectedChat(chat)}
                  className={`w-full p-3 rounded-2xl flex items-center gap-3 transition-all duration-200 group ${
                    selectedChat?.id === chat.id 
                    ? 'bg-gradient-to-r from-[#00b4ff]/15 to-[#00b4ff]/5 border border-[#00b4ff]/20' 
                    : 'hover:bg-white/[0.04] border border-transparent'
                  }`}
                >
                  <div className="relative flex-shrink-0">
                    <div className={`w-12 h-12 rounded-2xl flex items-center justify-center text-sm font-bold transition-all ${
                      selectedChat?.id === chat.id 
                        ? 'bg-gradient-to-br from-[#00b4ff] to-[#0078ff] text-white shadow-lg shadow-[#00b4ff]/25' 
                        : 'bg-slate-800 text-gray-300 group-hover:bg-slate-700'
                    }`}>
                      {chat.type === 'group' ? (
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                        </svg>
                      ) : (
                        chat.avatar
                      )}
                    </div>
                    {chat.type === 'direct' && chat.status && (
                      <div className={`absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 rounded-full border-[2.5px] border-[#121215] ${statusColors[chat.status]}`} />
                    )}
                    {chat.isLive && (
                      <div className="absolute -top-1.5 -right-1.5 flex items-center gap-1 bg-red-500 text-[9px] font-bold px-1.5 py-0.5 rounded-md border-2 border-[#121215]">
                        <span className="w-1.5 h-1.5 bg-white rounded-full animate-pulse" />
                        LIVE
                      </div>
                    )}
                  </div>
                  
                  <div className="flex-1 min-w-0 text-left">
                    <div className="flex justify-between items-center gap-2">
                      <span className={`font-semibold text-sm truncate ${selectedChat?.id === chat.id ? 'text-white' : 'text-gray-200 group-hover:text-white'}`}>
                        {chat.name}
                      </span>
                      <span className="text-[10px] text-gray-500 flex-shrink-0">{chat.lastMessageTime}</span>
                    </div>
                    <div className="flex items-center justify-between gap-2 mt-0.5">
                      <p className={`text-xs truncate ${selectedChat?.id === chat.id ? 'text-gray-300' : 'text-gray-500'}`}>
                        {chat.lastMessage}
                      </p>
                      {chat.unread && chat.unread > 0 && (
                        <span className="flex-shrink-0 w-5 h-5 rounded-full bg-[#00b4ff] text-[10px] font-bold flex items-center justify-center">
                          {chat.unread}
                        </span>
                      )}
                    </div>
                  </div>
                </button>
              ))}
            </div>
            
            {/* Action Buttons */}
            <div className="p-3 border-t border-white/[0.05] bg-black/20">
              <div className="flex gap-2">
                <button 
                  onClick={() => setShowAddFriend(true)}
                  className="flex-1 py-3 rounded-xl bg-[#00b4ff] hover:bg-[#00a0e6] text-white text-xs font-bold transition-all flex items-center justify-center gap-2 shadow-lg shadow-[#00b4ff]/20"
                >
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                  </svg>
                  Add Friend
                </button>
                <button className="px-4 py-3 rounded-xl bg-white/[0.05] hover:bg-white/[0.08] text-gray-300 hover:text-white transition-all" title="Create Group">
                  <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                  </svg>
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* RIGHT PANEL: Chat Area */}
        <div className="flex-1 bg-white/[0.02] backdrop-blur-3xl border border-white/[0.05] rounded-[2.5rem] flex flex-col relative overflow-hidden shadow-2xl">
          
          {selectedChat ? (
            <>
              {/* Header */}
              <div className="h-20 px-6 flex items-center justify-between border-b border-white/[0.05]">
                <div className="flex items-center gap-4">
                  <div className={`w-12 h-12 rounded-2xl flex items-center justify-center font-bold text-lg border ${
                    selectedChat.type === 'group' 
                      ? 'bg-[#00b4ff]/10 text-[#00b4ff] border-[#00b4ff]/20' 
                      : 'bg-slate-800 text-gray-200 border-white/[0.05]'
                  }`}>
                    {selectedChat.type === 'group' ? '#' : selectedChat.avatar}
                  </div>
                  <div>
                    <h1 className="text-lg font-bold text-white">{selectedChat.name}</h1>
                    <div className="flex items-center gap-2 mt-0.5">
                      {selectedChat.isLive ? (
                        <span className="flex items-center gap-1.5 text-red-400 text-xs font-medium">
                          <span className="w-2 h-2 bg-red-500 rounded-full animate-pulse" />
                          Live now
                        </span>
                      ) : selectedChat.type === 'group' ? (
                        <span className="text-xs text-gray-400">{selectedChat.members} members</span>
                      ) : (
                        <span className={`text-xs font-medium ${selectedChat.status === 'online' ? 'text-emerald-400' : 'text-gray-500'}`}>
                          {statusText[selectedChat.status || 'offline']}
                        </span>
                      )}
                    </div>
                  </div>
                </div>
                 
                {/* Header Actions */}
                <div className="flex items-center gap-2">
                  <button className="h-10 w-10 rounded-xl bg-white/[0.05] hover:bg-white/[0.08] flex items-center justify-center text-gray-400 hover:text-white transition-all" title="Voice Call">
                    <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
                    </svg>
                  </button>
                  <button 
                    onClick={() => setIsStreamExpanded(!isStreamExpanded)}
                    className={`h-10 px-4 rounded-xl flex items-center gap-2 transition-all font-medium text-xs ${
                      selectedChat.isLive 
                        ? 'bg-red-500 text-white shadow-lg shadow-red-500/25' 
                        : 'bg-white/[0.05] text-gray-400 hover:bg-white/[0.08] hover:text-white'
                    }`}
                    title="Video/Stream"
                  >
                    <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                      <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15 10l4.553-2.276A1 1 0 0121 8.618v6.764a1 1 0 01-1.447.894L15 14M5 18h8a2 2 0 002-2V8a2 2 0 00-2-2H5a2 2 0 00-2 2v8a2 2 0 002 2z" />
                    </svg>
                    <span className="hidden sm:inline">{selectedChat.isLive ? 'Watching' : 'Stream'}</span>
                  </button>
                  {selectedChat.type === 'group' && (
                    <button 
                      onClick={() => setShowParticipants(!showParticipants)}
                      className={`h-10 w-10 rounded-xl flex items-center justify-center transition-all ${
                        showParticipants ? 'bg-white/[0.1] text-white' : 'bg-white/[0.05] text-gray-400 hover:bg-white/[0.08] hover:text-white'
                      }`}
                      title="Toggle Members"
                    >
                      <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M12 4.354a4 4 0 110 5.292M15 21H3v-1a6 6 0 0112 0v1zm0 0h6v-1a6 6 0 00-9-5.197M13 7a4 4 0 11-8 0 4 4 0 018 0z" />
                      </svg>
                    </button>
                  )}
                </div>
              </div>

              {/* Main Content Area */}
              <div className="flex-1 flex overflow-hidden">
                
                {/* Center: Stream + Messages */}
                <div className="flex-1 flex flex-col min-w-0">
                  
                  {/* Live Stream Area */}
                  {selectedChat.isLive && isStreamExpanded && (
                    <div className="m-4 mb-0 h-52 flex-shrink-0 bg-gradient-to-br from-slate-900 to-slate-800 rounded-2xl overflow-hidden relative border border-white/[0.08] shadow-xl">
                      <div className="absolute inset-0 flex items-center justify-center">
                        <div className="text-center">
                          <div className="w-16 h-16 mx-auto rounded-2xl border-2 border-[#00b4ff]/50 p-1 mb-3">
                            <div className="w-full h-full rounded-xl bg-slate-700 flex items-center justify-center text-sm font-bold text-gray-300">
                              AC
                            </div>
                          </div>
                          <p className="text-sm text-gray-400">Alex Chen is sharing screen</p>
                          <p className="text-xs text-gray-600 mt-1">Click to expand</p>
                        </div>
                      </div>
                      {/* Stream controls overlay */}
                      <div className="absolute bottom-3 left-3 right-3 flex items-center justify-between">
                        <span className="flex items-center gap-1.5 px-2 py-1 rounded-lg bg-black/50 text-red-400 text-xs font-medium">
                          <span className="w-1.5 h-1.5 bg-red-500 rounded-full animate-pulse" />
                          LIVE
                        </span>
                        <div className="flex gap-1">
                          <button className="p-2 rounded-lg bg-black/50 text-gray-300 hover:text-white transition-colors">
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M15.536 8.464a5 5 0 010 7.072m2.828-9.9a9 9 0 010 12.728M5.586 15.536a5 5 0 001.414 1.414" />
                            </svg>
                          </button>
                          <button className="p-2 rounded-lg bg-black/50 text-gray-300 hover:text-white transition-colors">
                            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M4 8V4m0 0h4M4 4l5 5m11-1V4m0 0h-4m4 0l-5 5M4 16v4m0 0h4m-4 0l5-5m11 5l-5-5m5 5v-4m0 4h-4" />
                            </svg>
                          </button>
                        </div>
                      </div>
                    </div>
                  )}

                  {/* Messages */}
                  <div className={`flex-1 overflow-y-auto px-6 space-y-5 pb-28 ${selectedChat.isLive && isStreamExpanded ? 'pt-4' : 'pt-6'}`}>
                    {/* Date Divider */}
                    <div className="flex items-center justify-center py-2">
                      <div className="px-4 py-1.5 rounded-full bg-white/[0.04] border border-white/[0.05]">
                        <span className="text-xs text-gray-500 font-medium">Today</span>
                      </div>
                    </div>

                    {messages.map((msg) => (
                      <div key={msg.id} className={`flex ${msg.isMe ? 'justify-end' : 'justify-start'}`}>
                        <div className={`flex gap-3 max-w-[75%] ${msg.isMe ? 'flex-row-reverse' : ''}`}>
                          {/* Avatar for group messages */}
                          {!msg.isMe && selectedChat.type === 'group' && (
                            <div className="flex-shrink-0 self-end mb-5">
                              <div className="w-8 h-8 rounded-xl bg-slate-800 flex items-center justify-center text-xs font-bold text-gray-400 border border-white/[0.05]">
                                {msg.avatar}
                              </div>
                            </div>
                          )}

                          <div className={`flex flex-col ${msg.isMe ? 'items-end' : 'items-start'}`}>
                            {/* Sender name for group messages */}
                            {!msg.isMe && selectedChat.type === 'group' && (
                              <span className="text-xs text-gray-500 mb-1 ml-1 font-medium">{msg.senderName}</span>
                            )}
                            
                            {/* Message bubble */}
                            <div className={`
                              px-4 py-3 text-sm leading-relaxed shadow-lg
                              ${msg.isMe 
                                ? 'bg-gradient-to-br from-[#00b4ff] to-[#0088dd] text-white rounded-2xl rounded-br-sm' 
                                : 'bg-slate-800/80 text-gray-100 rounded-2xl rounded-bl-sm border border-white/[0.05]'
                              }
                            `}>
                              {msg.content}
                            </div>
                            
                            {/* Timestamp - always visible but subtle */}
                            <span className={`text-[10px] text-gray-600 mt-1.5 ${msg.isMe ? 'mr-1' : 'ml-1'}`}>
                              {msg.timestamp}
                            </span>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>

                  {/* Message Input */}
                  <div className="absolute bottom-4 left-4 right-4 z-20">
                    <div className="bg-[#0d0d0f]/90 backdrop-blur-xl border border-white/[0.08] rounded-2xl flex items-end shadow-2xl shadow-black/50">
                      <button className="p-4 text-gray-500 hover:text-[#00b4ff] transition-colors" title="Attach">
                        <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                          <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M12 6v6m0 0v6m0-6h6m-6 0H6" />
                        </svg>
                      </button>
                      <textarea 
                        value={newMessage}
                        onChange={(e) => setNewMessage(e.target.value)}
                        placeholder={`Message ${selectedChat.type === 'group' ? selectedChat.name : selectedChat.name}...`}
                        className="flex-1 bg-transparent text-white placeholder-gray-600 focus:outline-none resize-none py-4 text-sm"
                        rows={1}
                        style={{ minHeight: '52px', maxHeight: '120px' }}
                      />
                      <div className="flex items-center gap-1 p-2">
                        <button className="p-2 text-gray-500 hover:text-gray-300 transition-colors rounded-lg hover:bg-white/[0.05]" title="Emoji">
                          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M14.828 14.828a4 4 0 01-5.656 0M9 10h.01M15 10h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                          </svg>
                        </button>
                        <button 
                          className="p-2.5 bg-[#00b4ff] hover:bg-[#00a0e6] text-white rounded-xl transition-all shadow-lg shadow-[#00b4ff]/25 disabled:opacity-50 disabled:cursor-not-allowed"
                          disabled={!newMessage.trim()}
                          title="Send"
                        >
                          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M5 12h14M12 5l7 7-7 7" />
                          </svg>
                        </button>
                      </div>
                    </div>
                  </div>
                </div>

                {/* Right: Participants Panel */}
                {selectedChat.type === 'group' && showParticipants && (
                  <div className="w-56 border-l border-white/[0.05] overflow-y-auto bg-black/20">
                    <div className="p-4">
                      <h3 className="text-xs font-bold text-gray-500 uppercase tracking-wider mb-4">
                        Members — {selectedChat.memberList?.length}
                      </h3>
                      
                      <div className="space-y-1">
                        {selectedChat.memberList?.map(member => (
                          <button 
                            key={member.id} 
                            className="w-full flex items-center gap-3 p-2 rounded-xl hover:bg-white/[0.04] transition-colors group"
                          >
                            <div className="relative flex-shrink-0">
                              <div className="w-9 h-9 rounded-xl bg-slate-800 flex items-center justify-center text-xs font-bold text-gray-400 group-hover:text-gray-300 transition-colors">
                                {member.avatar}
                              </div>
                              <div className={`absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-[#0d0d0f] ${statusColors[member.status]}`} />
                            </div>
                            <div className="flex-1 min-w-0 text-left">
                              <div className="flex items-center gap-1.5">
                                <span className={`text-sm font-medium truncate ${member.status !== 'offline' ? 'text-gray-200' : 'text-gray-500'}`}>
                                  {member.name}
                                </span>
                                {member.role === 'admin' && (
                                  <svg className="w-3.5 h-3.5 text-amber-400 flex-shrink-0" fill="currentColor" viewBox="0 0 20 20">
                                    <path d="M9.049 2.927c.3-.921 1.603-.921 1.902 0l1.07 3.292a1 1 0 00.95.69h3.462c.969 0 1.371 1.24.588 1.81l-2.8 2.034a1 1 0 00-.364 1.118l1.07 3.292c.3.921-.755 1.688-1.54 1.118l-2.8-2.034a1 1 0 00-1.175 0l-2.8 2.034c-.784.57-1.838-.197-1.539-1.118l1.07-3.292a1 1 0 00-.364-1.118L2.98 8.72c-.783-.57-.38-1.81.588-1.81h3.461a1 1 0 00.951-.69l1.07-3.292z" />
                                  </svg>
                                )}
                              </div>
                              <p className="text-[10px] text-gray-600 capitalize">{statusText[member.status]}</p>
                            </div>
                          </button>
                        ))}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </>
          ) : (
            /* Empty State - Polished */
            <div className="flex-1 flex flex-col items-center justify-center p-8">
              <div className="w-20 h-20 rounded-3xl bg-gradient-to-br from-[#00b4ff]/20 to-[#00b4ff]/5 border border-[#00b4ff]/20 flex items-center justify-center mb-6">
                <svg className="w-10 h-10 text-[#00b4ff]" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
              </div>
              <h3 className="text-xl font-bold text-white mb-2">Welcome to Whisper</h3>
              <p className="text-gray-500 text-sm text-center max-w-xs mb-6">
                Select a conversation from the sidebar or start a new chat with a friend
              </p>
              <button 
                onClick={() => setShowAddFriend(true)}
                className="px-6 py-3 rounded-xl bg-[#00b4ff] hover:bg-[#00a0e6] text-white text-sm font-semibold transition-all shadow-lg shadow-[#00b4ff]/25 flex items-center gap-2"
              >
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                </svg>
                Add a Friend
              </button>
            </div>
          )}
        </div>
      </div>

      {/* Add Friend Modal */}
      {showAddFriend && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-black/70 backdrop-blur-sm" onClick={() => setShowAddFriend(false)} />
          <div className="relative w-full max-w-md bg-[#121215] border border-white/[0.08] rounded-3xl shadow-2xl overflow-hidden">
            {/* Modal top glow */}
            <div className="absolute top-0 left-8 right-8 h-px bg-gradient-to-r from-transparent via-[#00b4ff]/40 to-transparent" />
            
            <div className="p-6">
              <div className="flex items-center justify-between mb-6">
                <h2 className="text-xl font-bold text-white">Add Friend</h2>
                <button 
                  onClick={() => setShowAddFriend(false)}
                  className="p-2 rounded-xl hover:bg-white/[0.05] text-gray-400 hover:text-white transition-all"
                >
                  <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
                  </svg>
                </button>
              </div>
              
              <p className="text-gray-400 text-sm mb-4">
                Enter your friend's username to send them a friend request.
              </p>
              
              <input 
                type="text" 
                placeholder="Username" 
                className="w-full bg-white/[0.04] border border-white/[0.08] rounded-xl px-4 py-4 mb-4 text-white placeholder-gray-600 focus:outline-none focus:border-[#00b4ff]/40 transition-all" 
              />
              
              <button 
                onClick={() => setShowAddFriend(false)} 
                className="w-full py-4 bg-[#00b4ff] hover:bg-[#00a0e6] rounded-xl text-white font-bold transition-all shadow-lg shadow-[#00b4ff]/25"
              >
                Send Friend Request
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  )
}