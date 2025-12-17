// src/renderer/src/stores/authStore.ts
import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { User } from '../types/response/user/User';

interface AuthState {
  user: User | null;
  isAuthenticated: boolean;
  
  // Actions
  setSession: (user: User) => void;
  logout: () => void;
}

export const useAuthStore = create<AuthState>()(
  persist(
    (set) => ({
      user: null,
      isAuthenticated: false,

      setSession: (user) => set({ user, isAuthenticated: true }),
      
      logout: () => set({ user: null, isAuthenticated: false }),
    }),
    {
      name: 'whisper-auth-storage', // unique name for localStorage
    }
  )
);