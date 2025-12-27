import { create } from 'zustand';
import { messageApi } from '../services/api/message';
import { signalRService } from '../services/signalR'; 
import { MessageResponse } from '../types/api/response/chat/MessageResponse';
// 1. Import the mapper function
import { UiMessage, mapMessageToUi } from '../utils/mappers/messageMapper'; 
import { useAuthStore } from './authStore'; // 2. Import auth store to get current user ID

interface MessageState {
    messages: UiMessage[];
    isLoading: boolean;
    
    fetchMessages: (chatId: string) => Promise<void>;
    sendMessage: (chatId: string, content: string) => Promise<void>;
    // Action receives raw API response, but will convert it internally
    addMessage: (message: MessageResponse) => void; 
    
    initializeListeners: () => void;
}

export const useMessageStore = create<MessageState>((set, get) => ({
    messages: [],
    isLoading: false,

    fetchMessages: async (chatId) => {
        set({ isLoading: true, messages: [] });
        try {
            const res = await messageApi.getChatMessages(chatId);
            if (res.isSuccess && res.data) {
                // Handle DTO vs Array structure
                const data: any = res.data;
                const rawList: MessageResponse[] = Array.isArray(data) ? data : (data.messages || []);
                
                // 3. Get Current User ID
                const currentUserId = useAuthStore.getState().user?.id;

                // 4. MAP the raw messages to UI messages
                const uiList = rawList.map(msg => mapMessageToUi(msg, currentUserId));
                
                // Sort by time
                const sorted = uiList.sort((a, b) => 
                    new Date(a.sentAt).getTime() - new Date(b.sentAt).getTime()
                );
                
                set({ messages: sorted });
            }
            signalRService.joinChat(chatId);
        } catch (error) {
            console.error(error);
        } finally {
            set({ isLoading: false });
        }
    },

    sendMessage: async (chatId, content) => {
        try {
            await messageApi.sendMessage(chatId, content);
        } catch (error) {
            console.error(error);
        }
    },

    addMessage: (message: MessageResponse) => {
        set((state) => {
            // Check duplicates
            if (state.messages.some(m => m.id === message.id)) {
                return state; 
            }

            // 5. MAP the single incoming message before adding
            const currentUserId = useAuthStore.getState().user?.id;
            const uiMessage = mapMessageToUi(message, currentUserId);

            return { messages: [...state.messages, uiMessage] };
        });
    },

    initializeListeners: () => {
        signalRService.on('ReceiveMessage', (msg: MessageResponse) => {
            // Log to confirm raw data
            // console.log("New Message Received:", msg);
            get().addMessage(msg);
        });
    }
}));