import { JSX, useEffect } from 'react'
import { useFriendStore } from '../../stores/friendshipStore'
import { X, Check, X as XIcon, Clock } from 'lucide-react'

export const FriendRequestsModal = ({ onClose }: { onClose: () => void }): JSX.Element => {
  const { pendingRequests, fetchPendingRequests, acceptRequest, declineRequest } = useFriendStore()

  useEffect(() => {
    fetchPendingRequests()
  }, [fetchPendingRequests])

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/60 backdrop-blur-sm p-4 animate-in fade-in duration-200">
      <div className="w-full max-w-md bg-[#121215] border border-white/[0.08] rounded-2xl shadow-2xl flex flex-col max-h-[80vh]">
        <div className="p-4 border-b border-white/[0.05] flex justify-between items-center bg-[#18181b]">
          <h2 className="text-white font-bold text-lg flex items-center gap-2">
            Friend Requests
            <span className="bg-[#00b4ff] text-white text-[10px] px-2 py-0.5 rounded-full">
              {pendingRequests.length}
            </span>
          </h2>
          <button onClick={onClose} className="text-gray-400 hover:text-white transition-colors">
            <X className="w-5 h-5" />
          </button>
        </div>

        <div className="flex-1 overflow-y-auto p-4 space-y-2 custom-scrollbar">
          {pendingRequests.length === 0 ? (
            <div className="flex flex-col items-center justify-center py-10 text-gray-500 gap-3">
              <div className="w-12 h-12 rounded-full bg-white/[0.03] flex items-center justify-center">
                <Clock className="w-6 h-6 opacity-50" />
              </div>
              <p className="text-sm">No pending requests</p>
            </div>
          ) : (
            pendingRequests.map((req) => (
              <div
                key={req.friendshipId}
                className="flex items-center justify-between p-3 rounded-xl bg-white/[0.03] hover:bg-white/[0.05] transition-colors border border-white/[0.02]"
              >
                <div className="flex items-center gap-3">
                  <div className="w-10 h-10 rounded-full bg-slate-800 flex items-center justify-center text-sm font-bold text-gray-300">
                    {req.username.substring(0, 2).toUpperCase()}
                  </div>
                  <div>
                    <h4 className="text-sm font-semibold text-gray-200">{req.username}</h4>
                    <span className="text-[10px] text-gray-500">StringDate</span>
                  </div>
                </div>

                <div className="flex items-center gap-2">
                  <button
                    onClick={() => acceptRequest(req.friendshipId)}
                    title="Accept"
                    className="p-2 rounded-lg bg-emerald-500/10 text-emerald-500 hover:bg-emerald-500/20 transition-colors"
                  >
                    <Check className="w-4 h-4" />
                  </button>
                  <button
                    onClick={() => declineRequest(req.friendshipId)}
                    title="Decline"
                    className="p-2 rounded-lg bg-red-500/10 text-red-500 hover:bg-red-500/20 transition-colors"
                  >
                    <XIcon className="w-4 h-4" />
                  </button>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  )
}
