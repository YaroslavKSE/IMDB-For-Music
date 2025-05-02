import { useState, useEffect } from 'react';
import { Music, Disc, User, Plus, Loader, AlertCircle, ChevronRight } from 'lucide-react';
import UserPreferencesService, { UserPreferencesResponse } from '../../api/preferences';
import SearchModal from './SearchModal.tsx';
import ArtistCard from '../Search/ArtistCard';
import AlbumCard from '../Search/AlbumCard';
import TrackRow from '../Search/TrackRow';
import CatalogService from '../../api/catalog';

type PreferenceType = 'artists' | 'albums' | 'tracks';

const ProfilePreferencesTab = () => {
  const [preferences, setPreferences] = useState<UserPreferencesResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchModalOpen, setSearchModalOpen] = useState(false);
  const [currentSearchType, setCurrentSearchType] = useState<PreferenceType>('artists');
  const [removeLoading, setRemoveLoading] = useState<Record<string, boolean>>({});

  // State for storing the fetched items
  const [artistItems, setArtistItems] = useState<any[]>([]);
  const [albumItems, setAlbumItems] = useState<any[]>([]);
  const [trackItems, setTrackItems] = useState<any[]>([]);

  // Loading states for each type
  const [artistsLoading, setArtistsLoading] = useState(false);
  const [albumsLoading, setAlbumsLoading] = useState(false);
  const [tracksLoading, setTracksLoading] = useState(false);

  useEffect(() => {
    fetchPreferences();
  }, []);

  // Fetch details for a specific item type
  const fetchItemDetails = async (ids: string[], type: PreferenceType) => {
    if (ids.length === 0) return [];

    try {
      const loadingStateSetter =
        type === 'artists' ? setArtistsLoading :
        type === 'albums' ? setAlbumsLoading :
        setTracksLoading;

      loadingStateSetter(true);

      // Limit the number of requests by using batch endpoints when possible
      if (type === 'albums' && ids.length > 0) {
        // BatchAlbums supports up to 20 IDs at once
        const batchSize = 20;
        let fetchedAlbums = [];

        for (let i = 0; i < ids.length; i += batchSize) {
          const batchIds = ids.slice(i, i + batchSize);
          const response = await CatalogService.getBatchAlbums(batchIds);
          if (response.albums) {
            fetchedAlbums = [...fetchedAlbums, ...response.albums];
          }
        }

        return fetchedAlbums;
      } else if (type === 'tracks' && ids.length > 0) {
        // BatchTracks supports up to 20 IDs at once
        const batchSize = 20;
        let fetchedTracks = [];

        for (let i = 0; i < ids.length; i += batchSize) {
          const batchIds = ids.slice(i, i + batchSize);
          const response = await CatalogService.getBatchTracks(batchIds);
          if (response.tracks) {
            fetchedTracks = [...fetchedTracks, ...response.tracks];
          }
        }

        return fetchedTracks;
      } else {
        // For artists, we need to fetch them one by one (no batch endpoint available)
        return await Promise.all(
          ids.map(async (id) => {
            try {
              if (type === 'artists') return await CatalogService.getArtist(id);
              return null; // Should not reach here
            } catch (err) {
              console.error(`Error fetching ${type} with ID ${id}:`, err);
              return null;
            }
          })
        ).then(results => results.filter(item => item !== null));
      }
    } catch (err) {
      console.error(`Error fetching ${type} details:`, err);
      return [];
    } finally {
      const loadingStateSetter =
        type === 'artists' ? setArtistsLoading :
        type === 'albums' ? setAlbumsLoading :
        setTracksLoading;

      loadingStateSetter(false);
    }
  };

  const fetchPreferences = async () => {
    try {
      setLoading(true);
      setError(null);

      // Reset item states
      setArtistItems([]);
      setAlbumItems([]);
      setTrackItems([]);

      const response = await UserPreferencesService.getUserPreferences();
      setPreferences(response);

      // Fetch details for each type of preference
      if (response.artists.length > 0) {
        const artistDetails = await fetchItemDetails(response.artists, 'artists');
        setArtistItems(artistDetails);
      }

      if (response.albums.length > 0) {
        const albumDetails = await fetchItemDetails(response.albums, 'albums');
        setAlbumItems(albumDetails);
      }

      if (response.tracks.length > 0) {
        const trackDetails = await fetchItemDetails(response.tracks, 'tracks');
        setTrackItems(trackDetails);
      }
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
      if (type === 'artists') {
        setArtistItems(prev => prev.filter(item => item.spotifyId !== spotifyId));
      } else if (type === 'albums') {
        setAlbumItems(prev => prev.filter(item => item.spotifyId !== spotifyId));
      } else if (type === 'tracks') {
        setTrackItems(prev => prev.filter(item => item.spotifyId !== spotifyId));
      }

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

            {artistsLoading ? (
              <div className="animate-pulse grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="bg-white rounded-lg shadow overflow-hidden">
                    <div className="aspect-square w-full overflow-hidden bg-gray-200 rounded-full mx-auto p-2"></div>
                    <div className="p-3 text-center">
                      <div className="h-4 bg-gray-200 rounded w-3/4 mx-auto mb-2"></div>
                      <div className="h-3 bg-gray-200 rounded w-1/2 mx-auto"></div>
                    </div>
                  </div>
                ))}
              </div>
            ) : preferences && artistItems.length > 0 ? (
              <div>
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                  {artistItems.slice(0, 4).map(artist => (
                    <ArtistCard key={artist.spotifyId} artist={artist} />
                  ))}
                </div>

                {/* "View all" link if there are more than 4 items */}
                {artistItems.length > 4 && (
                  <div className="mt-4 text-right">
                    <button
                      onClick={() => /* Handle view all action */ {}}
                      className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium ml-auto"
                    >
                      View all artists <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  </div>
                )}
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

            {albumsLoading ? (
              <div className="animate-pulse grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="bg-white rounded-lg shadow overflow-hidden">
                    <div className="bg-gray-200 aspect-square w-full"></div>
                    <div className="p-3">
                      <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                      <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                    </div>
                  </div>
                ))}
              </div>
            ) : preferences && albumItems.length > 0 ? (
              <div>
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                  {albumItems.slice(0, 4).map(album => (
                    <AlbumCard key={album.spotifyId} album={album} />
                  ))}
                </div>

                {/* "View all" link if there are more than 4 items */}
                {albumItems.length > 4 && (
                  <div className="mt-4 text-right">
                    <button
                      onClick={() => /* Handle view all action */ {}}
                      className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium ml-auto"
                    >
                      View all albums <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  </div>
                )}
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

            {tracksLoading ? (
              <div className="animate-pulse space-y-4 bg-white rounded-lg shadow overflow-hidden">
                {[...Array(4)].map((_, i) => (
                  <div key={i} className="flex items-center px-6 py-4">
                    <div className="w-12 h-12 bg-gray-200 mr-4"></div>
                    <div className="flex-1">
                      <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                      <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                    </div>
                  </div>
                ))}
              </div>
            ) : preferences && trackItems.length > 0 ? (
              <div>
                <div className="bg-white rounded-lg shadow overflow-hidden">
                  {trackItems.slice(0, 4).map((track, index) => (
                    <TrackRow key={track.spotifyId} track={track} index={index} />
                  ))}
                </div>

                {/* "View all" link if there are more than 4 items */}
                {trackItems.length > 4 && (
                  <div className="mt-4 text-right">
                    <button
                      onClick={() => /* Handle view all action */ {}}
                      className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium ml-auto"
                    >
                      View all tracks <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  </div>
                )}
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

export default ProfilePreferencesTab;