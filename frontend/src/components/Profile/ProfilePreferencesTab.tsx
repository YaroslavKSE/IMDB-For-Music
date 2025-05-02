import { useState, useEffect } from 'react';
import { Music, Disc, User, Plus, Loader, AlertCircle } from 'lucide-react';
import UserPreferencesService, { UserPreferencesResponse } from '../../api/preferences';
import SearchModal from './SearchModal.tsx';
import PreferenceItem from './PreferenceItem';

type PreferenceType = 'artists' | 'albums' | 'tracks';

const ProfilePreferencesTab = () => {
  const [preferences, setPreferences] = useState<UserPreferencesResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchModalOpen, setSearchModalOpen] = useState(false);
  const [currentSearchType, setCurrentSearchType] = useState<PreferenceType>('artists');
  const [removeLoading, setRemoveLoading] = useState<Record<string, boolean>>({});

  useEffect(() => {
    fetchPreferences();
  }, []);

  const fetchPreferences = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await UserPreferencesService.getUserPreferences();
      setPreferences(response);
    } catch (err) {
      console.error('Error fetching preferences:', err);
      setError('Failed to load your preferences. Please try again later.');
    } finally {
      setLoading(false);
    }
  };

  const handleAddPreference = (type: PreferenceType) => {
    setCurrentSearchType(type);
    setSearchModalOpen(true);
  };

  const handleRemovePreference = async (type: PreferenceType, spotifyId: string) => {
    try {
      setRemoveLoading(prev => ({ ...prev, [spotifyId]: true }));

      // Convert plural type to singular for API call
      const itemType = type === 'artists' ? 'artist' :
                      type === 'albums' ? 'album' : 'track';

      await UserPreferencesService.removePreference(itemType, spotifyId);

      // Update local state after successful removal
      setPreferences(prev => {
        if (!prev) return null;

        return {
          ...prev,
          [type]: prev[type].filter(id => id !== spotifyId)
        };
      });
    } catch (err) {
      console.error(`Error removing ${type} preference:`, err);
      setError(`Failed to remove preference. Please try again.`);
    } finally {
      setRemoveLoading(prev => ({ ...prev, [spotifyId]: false }));
    }
  };

  const handlePreferenceAdded = () => {
    // Refresh preferences after adding new ones
    fetchPreferences();
    setSearchModalOpen(false);
  };

  if (loading) {
    return (
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="p-6">
          <div className="flex justify-center items-center py-12">
            <Loader className="h-8 w-8 text-primary-600 animate-spin mr-3" />
            <span className="text-gray-600">Loading preferences...</span>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-6 py-4 bg-primary-50 border-b border-primary-100">
        <h3 className="text-lg font-medium text-primary-800 flex items-center">
          <Music className="h-5 w-5 mr-2" />
          Music Preferences
        </h3>
      </div>

      <div className="p-6">
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6 flex items-center">
            <AlertCircle className="h-5 w-5 mr-2" />
            <span>{error}</span>
          </div>
        )}

        <p className="text-gray-600 mb-6">
          Add your favorite artists, albums, and tracks to personalize your experience and help us recommend music you'll love.
        </p>

        {/* Preferences Sections */}
        <div className="space-y-8">
          {/* Artists Section */}
          <div>
            <div className="flex justify-between items-center mb-4">
              <h4 className="text-md font-medium text-gray-800 flex items-center">
                <User className="h-5 w-5 mr-2 text-indigo-600" />
                Favorite Artists
              </h4>
              <button
                onClick={() => handleAddPreference('artists')}
                className="flex items-center text-sm text-primary-600 hover:text-primary-800"
              >
                <Plus className="h-4 w-4 mr-1" />
                Add Artist
              </button>
            </div>

            {preferences && preferences.artists.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                {preferences.artists.map(artistId => (
                  <PreferenceItem
                    key={artistId}
                    id={artistId}
                    type="artists"
                    onRemove={() => handleRemovePreference('artists', artistId)}
                    isLoading={removeLoading[artistId] || false}
                  />
                ))}
              </div>
            ) : (
              <div className="text-center py-6 bg-gray-50 rounded-lg border border-gray-200">
                <User className="h-10 w-10 mx-auto text-gray-400 mb-2" />
                <p className="text-gray-500">You haven't added any favorite artists yet.</p>
                <button
                  onClick={() => handleAddPreference('artists')}
                  className="mt-3 inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Add Artists
                </button>
              </div>
            )}
          </div>

          {/* Albums Section */}
          <div>
            <div className="flex justify-between items-center mb-4">
              <h4 className="text-md font-medium text-gray-800 flex items-center">
                <Disc className="h-5 w-5 mr-2 text-purple-600" />
                Favorite Albums
              </h4>
              <button
                onClick={() => handleAddPreference('albums')}
                className="flex items-center text-sm text-primary-600 hover:text-primary-800"
              >
                <Plus className="h-4 w-4 mr-1" />
                Add Album
              </button>
            </div>

            {preferences && preferences.albums.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                {preferences.albums.map(albumId => (
                  <PreferenceItem
                    key={albumId}
                    id={albumId}
                    type="albums"
                    onRemove={() => handleRemovePreference('albums', albumId)}
                    isLoading={removeLoading[albumId] || false}
                  />
                ))}
              </div>
            ) : (
              <div className="text-center py-6 bg-gray-50 rounded-lg border border-gray-200">
                <Disc className="h-10 w-10 mx-auto text-gray-400 mb-2" />
                <p className="text-gray-500">You haven't added any favorite albums yet.</p>
                <button
                  onClick={() => handleAddPreference('albums')}
                  className="mt-3 inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Add Albums
                </button>
              </div>
            )}
          </div>

          {/* Tracks Section */}
          <div>
            <div className="flex justify-between items-center mb-4">
              <h4 className="text-md font-medium text-gray-800 flex items-center">
                <Music className="h-5 w-5 mr-2 text-pink-600" />
                Favorite Tracks
              </h4>
              <button
                onClick={() => handleAddPreference('tracks')}
                className="flex items-center text-sm text-primary-600 hover:text-primary-800"
              >
                <Plus className="h-4 w-4 mr-1" />
                Add Track
              </button>
            </div>

            {preferences && preferences.tracks.length > 0 ? (
              <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-3">
                {preferences.tracks.map(trackId => (
                  <PreferenceItem
                    key={trackId}
                    id={trackId}
                    type="tracks"
                    onRemove={() => handleRemovePreference('tracks', trackId)}
                    isLoading={removeLoading[trackId] || false}
                  />
                ))}
              </div>
            ) : (
              <div className="text-center py-6 bg-gray-50 rounded-lg border border-gray-200">
                <Music className="h-10 w-10 mx-auto text-gray-400 mb-2" />
                <p className="text-gray-500">You haven't added any favorite tracks yet.</p>
                <button
                  onClick={() => handleAddPreference('tracks')}
                  className="mt-3 inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
                >
                  <Plus className="h-4 w-4 mr-1" />
                  Add Tracks
                </button>
              </div>
            )}
          </div>
        </div>
      </div>

      {searchModalOpen && (
        <SearchModal
          isOpen={searchModalOpen}
          onClose={() => setSearchModalOpen(false)}
          type={currentSearchType}
          onPreferenceAdded={handlePreferenceAdded}
        />
      )}
    </div>
  );
};

// Using the imported PreferenceItem component instead of an inline one

export default ProfilePreferencesTab;