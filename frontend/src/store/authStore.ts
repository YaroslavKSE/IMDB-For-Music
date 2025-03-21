// authStore.ts
import { create } from 'zustand';
import AuthService, { UserProfile } from '../api/auth';
import { handleAuth0Logout } from '../utils/auth0-config';

interface AuthState {
  user: UserProfile | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;

  // Actions
  login: (email: string, password: string) => Promise<void>;
  socialLogin: (accessToken: string, provider: string) => Promise<void>;
  register: (email: string, password: string, name: string, surname: string) => Promise<void>;
  logout: () => Promise<void>;
  clearError: () => void;
  setUser: (user: UserProfile | null) => void;
  fetchUserProfile: () => Promise<void>;
}

const useAuthStore = create<AuthState>((set, get) => ({
  user: null,
  isAuthenticated: AuthService.isAuthenticated(),
  isLoading: false,
  error: null,

  login: async (email: string, password: string) => {
    try {
      set({ isLoading: true, error: null });
      await AuthService.login({ email, password });
      await get().fetchUserProfile();
      set({ isAuthenticated: true, isLoading: false });
    } catch (error) {
      console.error('Login error:', error);
      set({
        isLoading: false,
        error: error instanceof Error ? error.message : 'Failed to login. Please try again.'
      });
      throw error;
    }
  },

  socialLogin: async (accessToken: string, provider: string) => {
    try {
      set({ isLoading: true, error: null });
      await AuthService.socialLogin({ accessToken, provider });
      await get().fetchUserProfile();
      set({ isAuthenticated: true, isLoading: false });
    } catch (error) {
      console.error('Social login error:', error);
      set({
        isLoading: false,
        error: error instanceof Error ? error.message : `Failed to login with ${provider}. Please try again.`
      });
      throw error;
    }
  },

  register: async (email: string, password: string, name: string, surname: string) => {
    try {
      set({ isLoading: true, error: null });
      await AuthService.register({ email, password, name, surname });
      set({ isLoading: false });
    } catch (error) {
      console.error('Registration error:', error);
      set({
        isLoading: false,
        error: error instanceof Error ? error.message : 'Failed to register. Please try again.'
      });
      throw error;
    }
  },

  logout: async () => {
    try {
      set({ isLoading: true });
      // First, log out from Auth0
      await handleAuth0Logout();
      // Clear local state
      set({
        user: null,
        isAuthenticated: false,
        isLoading: false
      });
    } catch (error) {
      console.error('Logout error:', error);
      // Even if there's an error, we should reset the auth state
      set({
        user: null,
        isAuthenticated: false,
        isLoading: false
      });
    }
  },

  clearError: () => set({ error: null }),

  setUser: (user) => set({ user }),

  fetchUserProfile: async () => {
    if (!AuthService.isAuthenticated()) {
      return;
    }

    try {
      set({ isLoading: true });
      const userProfile = await AuthService.getCurrentUser();
      set({ user: userProfile, isLoading: false });
    } catch (error) {
      console.error('Error fetching user profile:', error);
      set({
        isLoading: false,
        // Don't set an error message here to avoid interrupting the user experience
      });
    }
  }
}));

export default useAuthStore;