import { useState } from 'react';
import { useFriendStore } from '../../stores/friendshipStore';
import { X, Search, UserPlus, Check } from 'lucide-react';

export const AddFriendModal = ({ onClose }: { onClose: () => void }) => {
  const [query, setQuery] = useState('');
  // Track locally which IDs we've sent requests to (for visual feedback)
  const [sentIds, setSentIds] = useState<string[]>([]);
  
  const { searchUsers, searchResults, sendFriendRequest, isLoading, clearSearch } = useFriendStore();

  const handleSearch = (e: React.FormEvent) => {
    e.preventDefault();
    searchUsers(query);
  };

  const handleSend = async (id: string) => {
    await sendFriendRequest(id);
    setSentIds((prev) => [...prev, id]);
  };

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-md bg-[#121215] border border-white/[0.08] rounded-2xl shadow-2xl overflow-hidden flex flex-col max-h-[80vh]">
        
        {/* Header */}
        <div className="p-4 border-b border-white/[0.05] flex justify-between items-center bg-[#18181b]">
          <h2 className="text-white font-bold text-lg">Add Friend</h2>
          <button onClick={() => { clearSearch(); onClose(); }} className="text-gray-400 hover:text-white transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        {/* Search Input */}
        <div className="p-4">
          <form onSubmit={handleSearch} className="relative">
            <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-500" />
            <input 
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="Search by username (min 3 chars)..." 
              className="w-full bg-black/30 border border-white/[0.1] rounded-xl py-3 pl-10 pr-4 text-white placeholder:text-gray-600 focus:outline-none focus:border-[#00b4ff]/50 transition-all"
              autoFocus
            />
          </form>
        </div>

        {/* Results List */}
        <div className="flex-1 overflow-y-auto p-4 pt-0 space-y-2 custom-scrollbar">
          {isLoading ? (
             <div className="text-center text-gray-500 py-8 text-sm">Searching...</div>
          ) : searchResults.length > 0 ? (
            searchResults.map(user => (
              <div key={user.id} className="flex items-center justify-between p-3 rounded-xl bg-white/[0.03] hover:bg-white/[0.06] transition-colors border border-transparent hover:border-white/[0.05]">
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-slate-700 flex items-center justify-center font-bold text-white text-sm overflow-hidden">
                    {user.avatar ? (
                        <img src={user.avatar} alt={user.username} className="w-full h-full object-cover"/>
                    ) : (
                        user.username.substring(0, 2).toUpperCase()
                    )}
                  </div>
                  <span className="text-gray-200 font-medium text-sm">{user.username}</span>
                </div>
                
                <button 
                  disabled={sentIds.includes(user.id)}
                  onClick={() => handleSend(user.id)}
                  className={`p-2 rounded-lg transition-all ${
                    sentIds.includes(user.id) 
                        ? 'bg-green-500/10 text-green-400 cursor-default' 
                        : 'bg-[#00b4ff]/10 text-[#00b4ff] hover:bg-[#00b4ff]/20'
                  }`}
                >
                  {sentIds.includes(user.id) ? <Check className="w-4 h-4" /> : <UserPlus className="w-4 h-4" />}
                </button>
              </div>
            ))
          ) : query.length >= 3 ? (
            <div className="text-center text-gray-600 py-8 text-sm">No users found</div>
          ) : null}
        </div>
      </div>
    </div>
  );
};