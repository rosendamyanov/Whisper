import { create } from 'zustand';
import { chatApi } from '../services/api/chat';
import { mapChatToPreview } from '../utils/mappers/sidebarMapper';
import { UiChatPreview } from '../types/models/sidebar';

interface ChatState {
  chats: UiChatPreview[];
  selectedChatId: string | null; // <--- NEW
  isLoading: boolean;
  error: string | null;

  fetchChats: (currentUserId: string) => Promise<void>;
  setSelectedChat: (id: string) => void; // <--- NEW
}

export const useChatStore = create<ChatState>((set) => ({
  chats: [],
  selectedChatId: null,
  isLoading: false,
  error: null,

  fetchChats: async (currentUserId: string) => {
    set({ isLoading: true, error: null });
    try {
      const apiResponse = await chatApi.getMyChats();
      if (apiResponse && apiResponse.data) {
        const cleanChats = apiResponse.data.map((chat) => 
          mapChatToPreview(chat, currentUserId)
        );
        set({ chats: cleanChats });
      } else {
        set({ error: 'Failed to load chats' });
      }
    } catch (err) {
      console.error(err);
      set({ error: 'An unexpected error occurred' });
    } finally {
      set({ isLoading: false });
    }
  },

  // Simple action to update selection
  setSelectedChat: (id) => set({ selectedChatId: id }) 
}));