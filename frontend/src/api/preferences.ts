// src/api/preferences.ts
import { createApiClient } from '../utils/axios-factory';

// Create the API client specifically for user preferences
const preferencesApi = createApiClient('/users/preferences');

export interface UserPreferencesResponse {
  artists: string[];
  albums: string[];
  tracks: string[];
}

export interface PreferenceOperationResponse {
  success: boolean;
  message: string;
}

export interface AddPreferenceRequest {
  itemType: string; // 'artist', 'album', or 'track'
  spotifyId: string;
}

export interface BulkAddPreferencesRequest {
  artists: string[];
  albums: string[];
  tracks: string[];
}

const UserPreferencesService = {
  // Get all user preferences
  getUserPreferences: async (): Promise<UserPreferencesResponse> => {
    const response = await preferencesApi.get('');
    return response.data;
  },

  // Add a single preference
  addPreference: async (itemType: string, spotifyId: string): Promise<PreferenceOperationResponse> => {
    const request: AddPreferenceRequest = {
      itemType,
      spotifyId
    };
    const response = await preferencesApi.post('', request);
    return response.data;
  },

  // Remove a single preference
  removePreference: async (itemType: string, spotifyId: string): Promise<PreferenceOperationResponse> => {
    const request: AddPreferenceRequest = {
      itemType,
      spotifyId
    };
    const response = await preferencesApi.delete('', { data: request });
    return response.data;
  },

  // Add multiple preferences at once
  bulkAddPreferences: async (
    artists: string[] = [],
    albums: string[] = [],
    tracks: string[] = []
  ): Promise<PreferenceOperationResponse> => {
    const request: BulkAddPreferencesRequest = {
      artists,
      albums,
      tracks
    };
    const response = await preferencesApi.post('/bulk', request);
    return response.data;
  },

  // Clear all preferences of a specific type
  clearPreferences: async (type: string): Promise<PreferenceOperationResponse> => {
    const response = await preferencesApi.delete('/clear', {
      params: { type }
    });
    return response.data;
  }
};

export default UserPreferencesService;