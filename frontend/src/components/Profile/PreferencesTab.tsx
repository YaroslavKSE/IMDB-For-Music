import { useState, useEffect } from 'react';
import { Music, Disc, User, Plus, Loader, AlertCircle, ChevronRight } from 'lucide-react';
import UserPreferencesService, { UserPreferencesResponse } from '../../api/preferences';
import UsersService from '../../api/users';
import SearchModal from './SearchModal';
import ArtistCard from '../Search/ArtistCard';
import AlbumCard from '../Search/AlbumCard';
import TrackRow from '../Search/TrackRow';
import CatalogService, { ArtistSummary, AlbumSummary, TrackSummary } from '../../api/catalog';
import axios from 'axios';

type PreferenceType = 'artists' | 'albums' | 'tracks';

interface PreferencesTabProps {
  userId?: string;        // Optional: If provided, shows public user preferences
  username?: string;      // Optional: Alternative to userId for public preferences
  isOwnProfile?: boolean; // Whether this is the current user's profile
}

// Define more specific types
type ArtistItem = ArtistSummary;
type AlbumItem = AlbumSummary;
type TrackItem = TrackSummary;

const PreferencesTab = ({ userId, username, isOwnProfile = false }: PreferencesTabProps) => {
  const [preferences, setPreferences] = useState<UserPreferencesResponse | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [searchModalOpen, setSearchModalOpen] = useState(false);
  const [currentSearchType, setCurrentSearchType] = useState<PreferenceType>('artists');

  // Properly defined with specific types
  const [artistItems, setArtistItems] = useState<ArtistItem[]>([]);
  const [albumItems, setAlbumItems] = useState<AlbumItem[]>([]);
  const [trackItems, setTrackItems] = useState<TrackItem[]>([]);

  // Loading states for each type
  const [artistsLoading, setArtistsLoading] = useState(false);
  const [albumsLoading, setAlbumsLoading] = useState(false);
  const [tracksLoading, setTracksLoading] = useState(false);
  const [hasPreferences, setHasPreferences] = useState(true);

  useEffect(() => {
    fetchPreferences();
  }, [userId, username]);

  // Fetch preferences based on whether it's the current user or a public user
  const fetchPreferences = async () => {
    try {
      setLoading(true);
      setError(null);

      // Reset item states
      setArtistItems([]);
      setAlbumItems([]);
      setTrackItems([]);

      let response: UserPreferencesResponse;

      // Determine which API to call based on the provided props
      if (isOwnProfile) {
        // Current user preferences
        response = await UserPreferencesService.getUserPreferences();
      } else if (userId) {
        // Public user preferences by ID
        response = await UsersService.getUserPreferencesById(userId);
      } else if (username) {
        // Public user preferences by username
        response = await UsersService.getUserPreferencesByUsername(username);
      } else {
        throw new Error('Either userId, username, or isOwnProfile must be provided');
      }

      setPreferences(response);

      // Determine if the user has any preferences
      const hasAnyPreferences =
        (response.artists && response.artists.length > 0) ||
        (response.albums && response.albums.length > 0) ||
        (response.tracks && response.tracks.length > 0);

      setHasPreferences(hasAnyPreferences);

      // Fetch details for each type of preference
      if (response.artists.length > 0) {
        const artistDetails = await fetchItemDetails(response.artists, 'artists');
        setArtistItems(artistDetails as ArtistItem[]);
      }

      if (response.albums.length > 0) {
        const albumDetails = await fetchItemDetails(response.albums, 'albums');
        setAlbumItems(albumDetails as AlbumItem[]);
      }

      if (response.tracks.length > 0) {
        const trackDetails = await fetchItemDetails(response.tracks, 'tracks');
        setTrackItems(trackDetails as TrackItem[]);
      }
    } catch (err) {
      console.error('Error fetching preferences:', err);

      // Handle 404 as no preferences available rather than an error
      if (axios.isAxiosError(err) && err.response?.status === 404) {
        // This means the user has no preferences - this is not an error state
        setHasPreferences(false);
        setArtistItems([]);
        setAlbumItems([]);
        setTrackItems([]);
        setPreferences({ artists: [], albums: [], tracks: [] });
      } else {
        // For other errors, set the error state
        const userDisplayName = isOwnProfile ? 'your' : (username ? `${username}'s` : "user's");
        setError(`Failed to load ${userDisplayName} preferences. Please try again later.`);
      }
    } finally {
      setLoading(false);
    }
  };

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
        let fetchedAlbums: AlbumItem[] = [];

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
        let fetchedTracks: TrackItem[] = [];

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

  const handleAddPreference = (type: PreferenceType) => {
    setCurrentSearchType(type);
    setSearchModalOpen(true);
  };

  const handlePreferenceAdded = () => {
    // Refresh preferences after adding new ones
    fetchPreferences();
    setSearchModalOpen(false);
  };

  // Render section for a specific preference type (artists, albums, tracks)
  const renderPreferenceSection = (type: PreferenceType, items: ArtistItem[] | AlbumItem[] | TrackItem[]) => {
    const isLoading =
      type === 'artists' ? artistsLoading :
      type === 'albums' ? albumsLoading :
      tracksLoading;

    const title =
      type === 'artists' ? 'Favorite Artists' :
      type === 'albums' ? 'Favorite Albums' :
      'Favorite Tracks';

    const icon =
      type === 'artists' ? <User className="h-5 w-5 mr-2 text-indigo-600" /> :
      type === 'albums' ? <Disc className="h-5 w-5 mr-2 text-purple-600" /> :
      <Music className="h-5 w-5 mr-2 text-pink-600" />;

    const emptyIcon =
      type === 'artists' ? <User className="h-10 w-10 mx-auto text-gray-400 mb-2" /> :
      type === 'albums' ? <Disc className="h-10 w-10 mx-auto text-gray-400 mb-2" /> :
      <Music className="h-10 w-10 mx-auto text-gray-400 mb-2" />;

    const emptyText = isOwnProfile
      ? `You haven't added any favorite ${type} yet.`
      : `${username || 'This user'} hasn't added any favorite ${type} yet.`;

    // Loading skeleton UI
    const renderLoadingSkeleton = () => {
      if (type === 'artists') {
        return (
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
        );
      } else if (type === 'albums') {
        return (
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
        );
      } else {
        return (
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
        );
      }
    };

    // Render items based on type
    const renderItems = () => {
      if (items.length === 0) {
        return (
          <div className="text-center py-6 bg-gray-50 rounded-lg border border-gray-200">
            {emptyIcon}
            <p className="text-gray-500">{emptyText}</p>
            {isOwnProfile && (
              <button
                onClick={() => handleAddPreference(type)}
                className="mt-3 inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
              >
                <Plus className="h-4 w-4 mr-1" />
                Add {type === 'artists' ? 'Artists' : type === 'albums' ? 'Albums' : 'Tracks'}
              </button>
            )}
          </div>
        );
      }

      if (type === 'artists') {
        return (
          <div>
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
              {items.slice(0, 4).map(artist => (
                <ArtistCard key={artist.spotifyId} artist={artist} />
              ))}
            </div>
            {items.length > 4 && (
              <div className="mt-4 text-right">
                <button
                  onClick={() => console.log(`View all ${type}`)}
                  className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium ml-auto"
                >
                  View all {type} <ChevronRight className="h-4 w-4 ml-1" />
                </button>
              </div>
            )}
          </div>
        );
      } else if (type === 'albums') {
        return (
          <div>
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
              {items.slice(0, 4).map(album => (
                <AlbumCard key={album.spotifyId} album={album} />
              ))}
            </div>
            {items.length > 4 && (
              <div className="mt-4 text-right">
                <button
                  onClick={() => console.log(`View all ${type}`)}
                  className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium ml-auto"
                >
                  View all {type} <ChevronRight className="h-4 w-4 ml-1" />
                </button>
              </div>
            )}
          </div>
        );
      } else {
        return (
          <div>
            <div className="bg-white rounded-lg shadow overflow-hidden">
              {items.slice(0, 4).map((track, index) => (
                <TrackRow key={track.spotifyId} track={track} index={index} />
              ))}
            </div>
            {items.length > 4 && (
              <div className="mt-4 text-right">
                <button
                  onClick={() => console.log(`View all ${type}`)}
                  className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium ml-auto"
                >
                  View all {type} <ChevronRight className="h-4 w-4 ml-1" />
                </button>
              </div>
            )}
          </div>
        );
      }
    };

    return (
      <div>
        <div className="flex justify-between items-center mb-4">
          <h4 className="text-md font-medium text-gray-800 flex items-center">
            {icon}
            {title}
          </h4>
          {isOwnProfile && (
            <button
              onClick={() => handleAddPreference(type)}
              className="flex items-center text-sm text-primary-600 hover:text-primary-800"
            >
              <Plus className="h-4 w-4 mr-1" />
              Add {type === 'artists' ? 'Artist' : type === 'albums' ? 'Album' : 'Track'}
            </button>
          )}
        </div>

        {isLoading ? renderLoadingSkeleton() : renderItems()}
      </div>
    );
  };

  if (loading && artistItems.length === 0 && albumItems.length === 0 && trackItems.length === 0) {
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
          {isOwnProfile ? 'Music Preferences' : `${username || 'User'}'s Music Preferences`}
        </h3>
      </div>

      <div className="p-6">
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6 flex items-center">
            <AlertCircle className="h-5 w-5 mr-2" />
            <span>{error}</span>
          </div>
        )}

        {isOwnProfile && (
          <p className="text-gray-600 mb-6">
            Add your favorite artists, albums, and tracks to personalize your experience and help us recommend music you'll love.
          </p>
        )}

        {!hasPreferences && !loading && (
          <div className="text-center py-6 bg-gray-50 rounded-lg border border-gray-200">
            <Music className="h-16 w-16 mx-auto text-gray-400 mb-2" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No Public Preferences</h3>
            <p className="text-gray-500">
              {isOwnProfile
                ? "You haven't added any music preferences yet."
                : `${username ? `${username} hasn't` : "This user hasn't"} shared any public music preferences yet.`}
            </p>
            {isOwnProfile && (
              <button
                onClick={() => handleAddPreference('artists')}
                className="mt-4 inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50"
              >
                <Plus className="h-4 w-4 mr-1" />
                Add Preferences
              </button>
            )}
          </div>
        )}

        {hasPreferences && (
          <div className="space-y-8">
            {/* Artists Section */}
            {(artistsLoading || artistItems.length > 0) && (
              renderPreferenceSection('artists', artistItems)
            )}

            {/* Albums Section */}
            {(albumsLoading || albumItems.length > 0) && (
              renderPreferenceSection('albums', albumItems)
            )}

            {/* Tracks Section */}
            {(tracksLoading || trackItems.length > 0) && (
              renderPreferenceSection('tracks', trackItems)
            )}
          </div>
        )}
      </div>

      {/* Search Modal (only for own profile) */}
      {isOwnProfile && searchModalOpen && (
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

export default PreferencesTab;