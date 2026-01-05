import { JSX, useEffect, useState } from 'react'
import { useFriendStore } from '../../stores/friendshipStore'
import { useChatStore } from '../../stores/chatStore'
import { useAuthStore } from '../../stores/authStore'
import { X, Check } from 'lucide-react'
import { chatApi } from '../../services/api/chat'

export const CreateGroupModal = ({ onClose }: { onClose: () => void }): JSX.Element => {
  const { friends, fetchFriends, isLoading } = useFriendStore()
  const { fetchChats } = useChatStore()
  const { user } = useAuthStore()

  const [groupName, setGroupName] = useState('')
  const [selectedIds, setSelectedIds] = useState<string[]>([])
  const [isSubmitting, setIsSubmitting] = useState(false)

  useEffect(() => {
    fetchFriends()
  }, [fetchFriends])

  const toggleUser = (id: string): void => {
    setSelectedIds((prev) => (prev.includes(id) ? prev.filter((x) => x !== id) : [...prev, id]))
  }

  const handleCreate = async (): Promise<void> => {
    if (!groupName || selectedIds.length === 0) return

    setIsSubmitting(true)
    try {
      await chatApi.createGroupChat(groupName, selectedIds)

      if (user?.id) fetchChats(user.id)

      onClose()
    } catch (e) {
      console.error('Failed to create group:', e)
    } finally {
      setIsSubmitting(false)
    }
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-md bg-[#121215] border border-white/[0.08] rounded-2xl shadow-2xl flex flex-col max-h-[80vh]">
        <div className="p-4 border-b border-white/[0.05] flex justify-between items-center bg-[#18181b]">
          <h2 className="text-white font-bold text-lg">Create Group</h2>
          <button onClick={onClose} className="text-gray-400 hover:text-white transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="p-4 space-y-4 flex-1 overflow-hidden flex flex-col">
          <div>
            <label className="text-xs text-gray-500 uppercase font-bold mb-1.5 block ml-1">
              Group Name
            </label>
            <input
              value={groupName}
              onChange={(e) => setGroupName(e.target.value)}
              className="w-full bg-black/30 border border-white/[0.1] rounded-xl p-3 text-white focus:outline-none focus:border-[#00b4ff]/50 transition-all text-sm"
              placeholder="e.g. Weekend Project"
              autoFocus
            />
          </div>

          <div className="flex-1 overflow-y-auto min-h-0 custom-scrollbar">
            <label className="text-xs text-gray-500 uppercase font-bold mb-2 block ml-1 sticky top-0 bg-[#121215] py-1 z-10">
              Select Members ({selectedIds.length})
            </label>

            {isLoading ? (
              <div className="text-center text-gray-500 text-xs py-4">Loading friends...</div>
            ) : friends.length === 0 ? (
              <div className="text-center text-gray-500 text-xs py-4">
                No friends found. Add some friends first!
              </div>
            ) : (
              <div className="space-y-1">
                {friends.map((friend) => {
                  const isSelected = selectedIds.includes(friend.id)
                  return (
                    <div
                      key={friend.id}
                      onClick={() => toggleUser(friend.id)}
                      className={`flex items-center gap-3 p-2.5 rounded-xl cursor-pointer transition-all border ${
                        isSelected
                          ? 'bg-[#00b4ff]/10 border-[#00b4ff]/30'
                          : 'bg-transparent border-transparent hover:bg-white/[0.03]'
                      }`}
                    >
                      <div
                        className={`w-5 h-5 rounded-md border flex items-center justify-center transition-colors flex-shrink-0 ${
                          isSelected ? 'bg-[#00b4ff] border-[#00b4ff]' : 'border-gray-600'
                        }`}
                      >
                        {isSelected && <Check className="w-3.5 h-3.5 text-white" />}
                      </div>

                      <div className="w-8 h-8 rounded-full bg-slate-700 flex items-center justify-center text-xs font-bold text-gray-300">
                        {friend.username.substring(0, 2).toUpperCase()}
                      </div>

                      <span
                        className={`text-sm font-medium ${isSelected ? 'text-white' : 'text-gray-400'}`}
                      >
                        {friend.username}
                      </span>
                    </div>
                  )
                })}
              </div>
            )}
          </div>

          <button
            onClick={handleCreate}
            disabled={!groupName || selectedIds.length === 0 || isSubmitting}
            className="w-full py-3 bg-[#00b4ff] hover:bg-[#00a0e6] text-white rounded-xl font-bold transition-all disabled:opacity-50 disabled:cursor-not-allowed text-sm shadow-lg shadow-[#00b4ff]/20"
          >
            {isSubmitting ? 'Creating...' : 'Create Group'}
          </button>
        </div>
      </div>
    </div>
  )
}
