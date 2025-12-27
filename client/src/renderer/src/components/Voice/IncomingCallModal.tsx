import { useVoiceStore } from '../../stores/voiceStore';
import { Phone, PhoneOff } from 'lucide-react';

export const IncomingCallModal = () => {
    const { incomingCall, acceptCall, rejectCall } = useVoiceStore();

    if (!incomingCall) return null;

    return (
        <div className="fixed inset-0 z-[100] flex items-center justify-center bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
            <div className="flex flex-col items-center gap-6 p-8 bg-[#18181b] border border-white/[0.1] rounded-3xl shadow-2xl w-80">
                
                {/* Avatar / Pulse */}
                <div className="relative">
                    <div className="w-24 h-24 rounded-full bg-slate-800 border-4 border-[#18181b] overflow-hidden flex items-center justify-center z-10 relative">
                         {/* If you have avatar url, put img here */}
                         <span className="text-3xl font-bold text-gray-400">
                            {incomingCall.callerName.substring(0,2).toUpperCase()}
                         </span>
                    </div>
                    {/* Ringing Animation */}
                    <div className="absolute inset-0 rounded-full bg-emerald-500/30 animate-ping" />
                    <div className="absolute inset-0 rounded-full bg-emerald-500/20 animate-ping delay-75" />
                </div>

                <div className="text-center space-y-1">
                    <h2 className="text-2xl font-bold text-white">{incomingCall.callerName}</h2>
                    <p className="text-emerald-400 font-medium animate-pulse">Incoming Voice Call...</p>
                </div>

                <div className="flex items-center gap-6 w-full justify-center">
                    {/* Decline */}
                    <button 
                        onClick={rejectCall}
                        className="w-16 h-16 rounded-full bg-red-500 hover:bg-red-600 flex items-center justify-center text-white transition-transform hover:scale-110 shadow-lg shadow-red-500/25"
                    >
                        <PhoneOff className="w-8 h-8" />
                    </button>

                    {/* Accept */}
                    <button 
                        onClick={acceptCall}
                        className="w-16 h-16 rounded-full bg-emerald-500 hover:bg-emerald-600 flex items-center justify-center text-white transition-transform hover:scale-110 shadow-lg shadow-emerald-500/25"
                    >
                        <Phone className="w-8 h-8" />
                    </button>
                </div>
            </div>
        </div>
    );
};