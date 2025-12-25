import { useEffect, useState } from 'react';
import { useChatStore } from '../../stores/chatStore';
import { useAuthStore } from '@renderer/stores/authStore';
import { useFriendStore } from '@renderer/stores/friendshipStore';
import { AddFriendModal } from '../Modals/AddFriendModal';
import { CreateGroupModal } from '../Modals/CreateGroupChatModal';
import { FriendRequestsModal } from '../Modals/FriendRequestsModal';

const STATUS_COLORS = {
  online: 'bg-emerald-500 shadow-[0_0_8px_rgba(16,185,129,0.8)]',
  away: 'bg-amber-400 shadow-[0_0_8px_rgba(251,191,36,0.8)]',
  dnd: 'bg-rose-500 shadow-[0_0_8px_rgba(244,63,94,0.8)]',
  offline: 'bg-slate-600'
};

export const Sidebar = () => {
  const { chats, isLoading, fetchChats, selectedChatId, setSelectedChat } = useChatStore();
  const [searchQuery, setSearchQuery] = useState('');
  // NEW: Get the logged-in user
  const { user } = useAuthStore();
  const [showAddFriend, setShowAddFriend] = useState(false);
  const [showCreateGroup, setShowCreateGroup] = useState(false);
  const { pendingRequests, fetchPendingRequests } = useFriendStore();
  const [showRequests, setShowRequests] = useState(false);
  
  // Hardcoded for now until we have AuthStore connected fully
  const myStatus = 'online'; 

  useEffect(() => {
    // 3. Only fetch if we have a user (prevents 401s on logout)
    if (user?.id) {
        fetchChats(user.id);
        fetchPendingRequests(); // <--- FIX: Pass real ID, not 'current-user-id'
    }
  }, [fetchChats, user?.id]); // <--- Add user.id to dependency array

  const filteredChats = chats.filter(c => 
    c.title.toLowerCase().includes(searchQuery.toLowerCase())
  );

  const onlineCount = chats.filter(c => c.type === 'direct' && c.status === 'online').length;

  return (
    <div className="w-80 flex-shrink-0 flex flex-col gap-4">
      
      {/* 1. User Profile Card */}
      <div className="bg-white/[0.02] backdrop-blur-2xl border border-white/[0.05] p-5 rounded-3xl flex items-center justify-between shadow-xl">
        <div className="flex items-center gap-3">
          <div className="relative">
            <div className="w-11 h-11 rounded-xl bg-gradient-to-br from-[#00b4ff] to-[#0078ff] flex items-center justify-center font-bold text-base shadow-lg shadow-[#00b4ff]/25 text-white">
              Me
            </div>
            <div className={`absolute -bottom-1 -right-1 w-3.5 h-3.5 rounded-full border-[2.5px] border-[#0a0a0c] ${STATUS_COLORS[myStatus]}`} />
          </div>
          <div>
            <h2 className="font-semibold text-sm text-white">
              {user?.username || 'My Account'}
            </h2>
            <p className="text-[11px] text-emerald-400 font-medium capitalize">{myStatus}</p>
          </div>
          <div className="flex items-center gap-1">
            <button 
                onClick={() => setShowRequests(true)}
                className="relative p-2.5 rounded-xl text-gray-400 hover:text-white hover:bg-white/[0.05] transition-all"
            >
                <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                    <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9" />
                </svg>
                
                {/* Red Dot Badge */}
                {pendingRequests.length > 0 && (
                    <span className="absolute top-2 right-2.5 w-2 h-2 rounded-full bg-red-500 border border-[#1e1e21]" />
                )}
            </button>
        </div>
        </div>
        
        <button className="p-2.5 rounded-xl text-gray-400 hover:text-white hover:bg-white/[0.05] transition-all">
          <svg className="w-5 h-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M10.325 4.317c.426-1.756 2.924-1.756 3.35 0a1.724 1.724 0 002.573 1.066c1.543-.94 3.31.826 2.37 2.37a1.724 1.724 0 001.065 2.572c1.756.426 1.756 2.924 0 3.35a1.724 1.724 0 00-1.066 2.573c.94 1.543-.826 3.31-2.37 2.37a1.724 1.724 0 00-2.572 1.065c-.426 1.756-2.924 1.756-3.35 0a1.724 1.724 0 00-2.573-1.066c-1.543.94-3.31-.826-2.37-2.37a1.724 1.724 0 00-1.065-2.572c-1.756-.426-1.756-2.924 0-3.35a1.724 1.724 0 001.066-2.573c-.94-1.543.826-3.31 2.37-2.37.996.608 2.296.07 2.572-1.065z" />
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={1.5} d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
          </svg>
        </button>
      </div>

      {/* 2. Chat List Card */}
      <div className="flex-1 bg-white/[0.02] backdrop-blur-2xl border border-white/[0.05] rounded-[2rem] flex flex-col shadow-2xl overflow-hidden">
        
        {/* Search Header */}
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
          <div className="flex items-center gap-2 mt-3 px-1">
            <div className={`w-2 h-2 rounded-full ${STATUS_COLORS.online}`} />
            <span className="text-xs text-gray-400">{onlineCount} friends online</span>
          </div>
        </div>

        {/* Chat List */}
        <div className="flex-1 overflow-y-auto px-2 pb-2 space-y-1 custom-scrollbar">
          {isLoading ? (
            <div className="p-8 text-center text-gray-500 text-xs">Loading chats...</div>
          ) : filteredChats.map(chat => {
            const isSelected = selectedChatId === chat.id;
            
            return (
              <button
                key={chat.id}
                onClick={() => setSelectedChat(chat.id)}
                className={`w-full p-3 rounded-2xl flex items-center gap-3 transition-all duration-200 group ${
                  isSelected 
                  ? 'bg-gradient-to-r from-[#00b4ff]/15 to-[#00b4ff]/5 border border-[#00b4ff]/20' 
                  : 'hover:bg-white/[0.04] border border-transparent'
                }`}
              >
                {/* Avatar with Glow Effect */}
                <div className="relative flex-shrink-0">
                  <div className={`w-12 h-12 rounded-2xl flex items-center justify-center overflow-hidden font-bold transition-all ${
                    isSelected
                      ? 'bg-gradient-to-br from-[#00b4ff] to-[#0078ff] text-white shadow-lg shadow-[#00b4ff]/25' 
                      : 'bg-slate-800 text-gray-300 group-hover:bg-slate-700'
                  }`}>
                    {/* Use image if available, otherwise show initials or icon */}
                    <img src={chat.avatarUrl} alt={chat.title} className="w-full h-full object-cover" />
                  </div>
                  
                  {chat.type === 'direct' && chat.status && (
                    <div className={`absolute -bottom-0.5 -right-0.5 w-3.5 h-3.5 rounded-full border-[2.5px] border-[#121215] ${STATUS_COLORS[chat.status] || STATUS_COLORS.offline}`} />
                  )}
                </div>
                
                {/* Text Content */}
                <div className="flex-1 min-w-0 text-left">
                  <div className="flex justify-between items-center gap-2">
                    <span className={`font-semibold text-sm truncate transition-colors ${
                      isSelected ? 'text-white' : 'text-gray-200 group-hover:text-white'
                    }`}>
                      {chat.title}
                    </span>
                    {chat.lastMessage && (
                      <span className="text-[10px] text-gray-500 flex-shrink-0">{chat.lastMessage.time}</span>
                    )}
                  </div>
                  
                  <div className="flex items-center justify-between gap-2 mt-0.5">
                    <p className={`text-xs truncate transition-colors ${
                      isSelected ? 'text-gray-300' : 'text-gray-500'
                    }`}>
                       {chat.lastMessage?.isMe && <span className="text-[#00b4ff]">You: </span>}
                       {chat.lastMessage?.content}
                    </p>
                    
                    {chat.unreadCount > 0 && (
                      <span className="flex-shrink-0 w-5 h-5 rounded-full bg-[#00b4ff] text-[10px] font-bold flex items-center justify-center text-white">
                        {chat.unreadCount}
                      </span>
                    )}
                  </div>
                </div>
              </button>
            );
          })}
        </div>

        {/* Action Buttons */}
                <div className="p-3 border-t border-white/[0.05] bg-black/20">
                  <div className="flex gap-2">
                    
                    {/* --- CHANGE 1: Add onClick to Add Friend Button --- */}
                    <button 
                      onClick={() => setShowAddFriend(true)} 
                      className="flex-1 py-3 rounded-xl bg-[#00b4ff] hover:bg-[#00a0e6] text-white text-xs font-bold transition-all flex items-center justify-center gap-2 shadow-lg shadow-[#00b4ff]/20"
                    >
                      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M18 9v3m0 0v3m0-3h3m-3 0h-3m-2-5a4 4 0 11-8 0 4 4 0 018 0zM3 20a6 6 0 0112 0v1H3v-1z" />
                      </svg>
                      Add Friend
                    </button>

                    {/* --- CHANGE 2: Add onClick to Create Group Button --- */}
                    <button 
                      onClick={() => setShowCreateGroup(true)}
                      className="px-4 py-3 rounded-xl bg-white/[0.05] hover:bg-white/[0.08] text-gray-300 hover:text-white transition-all" 
                      title="Create Group"
                    >
                      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                        <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z" />
                      </svg>
                    </button>
                  </div>
                </div>

      </div>
      {/* --- CHANGE 3: Render the Modals here at the bottom --- */}
      {showAddFriend && <AddFriendModal onClose={() => setShowAddFriend(false)} />}
      {showCreateGroup && <CreateGroupModal onClose={() => setShowCreateGroup(false)} />}
      {showRequests && <FriendRequestsModal onClose={() => setShowRequests(false)} />}
    </div>
  );
};