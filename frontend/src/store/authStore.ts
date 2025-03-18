import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import AuthService, { UserProfile } from '../api/auth';
import axios from "axios";

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
  login: (email: string, password: string) => Promise<void>;
  register: (email: string, password: string, name: string, surname: string) => Promise<void>;
  logout: () => void;
  fetchUserProfile: () => Promise<void>;
  clearError: () => void;
}

const useAuthStore = create<AuthState>()(
  persist(
    (set, get) => ({
      user: null,
      isAuthenticated: !!localStorage.getItem('token'),
      isLoading: false,
      error: null,

      login: async (email, password) => {
        set({ isLoading: true, error: null });
        try {
          await AuthService.login({ email, password });
          await get().fetchUserProfile();
          set({ isAuthenticated: true, isLoading: false });
        } catch (error) {
          console.error('Login failed:', error);
          let errorMessage = 'Login failed. Please check your credentials.';

          if (axios.isAxiosError(error) && error.response?.data?.message) {
            errorMessage = error.response.data.message;
          }

          set({ error: errorMessage, isLoading: false, isAuthenticated: false });
          throw new Error(errorMessage);
        }
      },

      register: async (email, password, name, surname) => {
        set({ isLoading: true, error: null });
        try {
          await AuthService.register({ email, password, name, surname });
          set({ isLoading: false });
        } catch (error) {
          console.error('Registration failed:', error);
          let errorMessage = 'Registration failed. Please try again.';

          if (axios.isAxiosError(error) && error.response?.data?.message) {
            errorMessage = error.response.data.message;
          }

          set({ error: errorMessage, isLoading: false });
          throw new Error(errorMessage);
        }
      },

      logout: () => {
        AuthService.logout();
        set({ user: null, isAuthenticated: false });
      },

      fetchUserProfile: async () => {
        set({ isLoading: true });
        try {
          const user = await AuthService.getCurrentUser();
          set({ user, isLoading: false });
        } catch (error) {
          console.error('Failed to fetch user profile:', error);
          set({ user: null, isLoading: false });

          // If we can't fetch the user profile, it means the token is invalid
          if (axios.isAxiosError(error) && error.response?.status === 401) {
            set({ isAuthenticated: false });
            AuthService.logout();
          }
        }
      },

      clearError: () => set({ error: null }),
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({ user: state.user, isAuthenticated: state.isAuthenticated }),
    }
  )
);

export default useAuthStore;