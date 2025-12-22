// src/renderer/src/pages/Home.tsx
import { useState } from 'react';
import { Sidebar } from '../components/Sidebar/Sidebar'; // <--- The new component

// --- KEEPING YOUR MOCK DATA FOR THE RIGHT PANEL (VISUAL REF) ---

interface Member {
  id: string;
  name: string;
  avatar: string;
  status: 'online' | 'away' | 'dnd' | 'offline';
  role?: 'admin' | 'member';
}

interface ChatItem {
  id: string;
  name: string;
  type: 'direct' | 'group';
  status?: 'online' | 'away' | 'dnd' | 'offline';
  members?: number;
  memberList?: Member[];
  avatar: string;
  lastMessage: string;
  lastMessageTime: string;
  unread?: number;
  isLive?: boolean;
}

interface Message {
  id: string;
  senderId: string;
  senderName?: string;
  content: string;
  timestamp: string;
  isMe: boolean;
  avatar?: string;
}

const mockMembers: Member[] = [
  { id: '1', name: 'Alex Chen', avatar: 'AC', status: 'online', role: 'admin' },
  { id: '2', name: 'Sarah Miller', avatar: 'SM', status: 'away', role: 'member' },
  { id: '3', name: 'Jordan Lee', avatar: 'JL', status: 'offline', role: 'member' },
  { id: '4', name: 'Me', avatar: 'Me', status: 'online', role: 'member' },
];

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
];

const mockMessages: Message[] = [
  { id: '1', senderId: '2', senderName: 'Sarah', avatar: 'SM', content: 'Guys, check out the new design update.', timestamp: '12:00 PM', isMe: false },
  { id: '2', senderId: 'me', content: 'Looking good! I love the new color palette.', timestamp: '12:02 PM', isMe: true },
  { id: '3', senderId: '1', senderName: 'Alex', avatar: 'AC', content: 'I think we should make the sidebar a bit darker though.', timestamp: '12:03 PM', isMe: false },
  { id: '4', senderId: '1', senderName: 'Alex', avatar: 'AC', content: 'Im going to start a stream to show you what I mean.', timestamp: '12:05 PM', isMe: false },
];

const statusColors = {
  online: 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.8)]',
  away: 'bg-amber-400 shadow-[0_0_8px_rgba(251,191,36,0.8)]',
  dnd: 'bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.8)]',
  offline: 'bg-slate-600'
};

const statusText = {
  online: 'Online',
  away: 'Away',
  dnd: 'Do Not Disturb',
  offline: 'Offline'
};

export const Home = () => {
  // Using Mock Data ONLY for the Right Panel
  const [selectedChat] = useState<ChatItem | null>(mockChats[1]); 
  const [messages] = useState<Message[]>(mockMessages);
  const [newMessage, setNewMessage] = useState('');
  const [isStreamExpanded, setIsStreamExpanded] = useState(true);
  const [showParticipants, setShowParticipants] = useState(true);
  const [showAddFriend, setShowAddFriend] = useState(false);

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
        
        {/* --- LEFT PANEL: REPLACED WITH REAL COMPONENT --- */}
        <Sidebar />

        {/* --- RIGHT PANEL: ORIGINAL MOCK DESIGN --- */}
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
                        </div>
                      </div>
                      <div className="absolute bottom-3 left-3 right-3 flex items-center justify-between">
                        <span className="flex items-center gap-1.5 px-2 py-1 rounded-lg bg-black/50 text-red-400 text-xs font-medium">
                          <span className="w-1.5 h-1.5 bg-red-500 rounded-full animate-pulse" />
                          LIVE
                        </span>
                      </div>
                    </div>
                  )}

                  {/* Messages */}
                  <div className={`flex-1 overflow-y-auto px-6 space-y-5 pb-28 ${selectedChat.isLive && isStreamExpanded ? 'pt-4' : 'pt-6'}`}>
                    <div className="flex items-center justify-center py-2">
                      <div className="px-4 py-1.5 rounded-full bg-white/[0.04] border border-white/[0.05]">
                        <span className="text-xs text-gray-500 font-medium">Today</span>
                      </div>
                    </div>

                    {messages.map((msg) => (
                      <div key={msg.id} className={`flex ${msg.isMe ? 'justify-end' : 'justify-start'}`}>
                        <div className={`flex gap-3 max-w-[75%] ${msg.isMe ? 'flex-row-reverse' : ''}`}>
                          {!msg.isMe && selectedChat.type === 'group' && (
                            <div className="flex-shrink-0 self-end mb-5">
                              <div className="w-8 h-8 rounded-xl bg-slate-800 flex items-center justify-center text-xs font-bold text-gray-400 border border-white/[0.05]">
                                {msg.avatar}
                              </div>
                            </div>
                          )}

                          <div className={`flex flex-col ${msg.isMe ? 'items-end' : 'items-start'}`}>
                            {!msg.isMe && selectedChat.type === 'group' && (
                              <span className="text-xs text-gray-500 mb-1 ml-1 font-medium">{msg.senderName}</span>
                            )}
                            <div className={`
                              px-4 py-3 text-sm leading-relaxed shadow-lg
                              ${msg.isMe 
                                ? 'bg-gradient-to-br from-[#00b4ff] to-[#0088dd] text-white rounded-2xl rounded-br-sm' 
                                : 'bg-slate-800/80 text-gray-100 rounded-2xl rounded-bl-sm border border-white/[0.05]'
                              }
                            `}>
                              {msg.content}
                            </div>
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
                        <button 
                          className="p-2.5 bg-[#00b4ff] hover:bg-[#00a0e6] text-white rounded-xl transition-all shadow-lg shadow-[#00b4ff]/25 disabled:opacity-50 disabled:cursor-not-allowed"
                          disabled={!newMessage.trim()}
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
                          <div key={member.id} className="w-full flex items-center gap-3 p-2 rounded-xl hover:bg-white/[0.04] transition-colors group cursor-pointer">
                            <div className="relative flex-shrink-0">
                              <div className="w-9 h-9 rounded-xl bg-slate-800 flex items-center justify-center text-xs font-bold text-gray-400 group-hover:text-gray-300 transition-colors">
                                {member.avatar}
                              </div>
                              <div className={`absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-[#0d0d0f] ${statusColors[member.status]}`} />
                            </div>
                            <div className="flex-1 min-w-0 text-left">
                              <span className={`text-sm font-medium truncate ${member.status !== 'offline' ? 'text-gray-200' : 'text-gray-500'}`}>
                                {member.name}
                              </span>
                              <p className="text-[10px] text-gray-600 capitalize">{statusText[member.status]}</p>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                )}
              </div>
            </>
          ) : (
             <div className="flex-1 flex items-center justify-center text-gray-500">Select a chat</div>
          )}
        </div>
      </div>

      {/* Add Friend Modal (Only visual for now) */}
      {showAddFriend && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4">
          <div className="absolute inset-0 bg-black/70 backdrop-blur-sm" onClick={() => setShowAddFriend(false)} />
          <div className="relative w-full max-w-md bg-[#121215] border border-white/[0.08] rounded-3xl shadow-2xl overflow-hidden p-6">
            <h2 className="text-xl font-bold text-white mb-4">Add Friend</h2>
            <button onClick={() => setShowAddFriend(false)} className="w-full py-4 bg-[#00b4ff] rounded-xl text-white font-bold">Close</button>
          </div>
        </div>
      )}
    </div>
  );
};