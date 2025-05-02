import { useState, useEffect } from 'react';
import { Music, Disc, User, Loader, AlertCircle, ChevronRight } from 'lucide-react';
import CatalogService from '../../api/catalog';
import UsersService, { UserPreferencesResponse } from '../../api/users';
import ArtistCard from '../Search/ArtistCard';
import AlbumCard from '../Search/AlbumCard';
import TrackRow from '../Search/TrackRow';
import axios from 'axios';

interface PublicProfilePreferencesTabProps {
  userId: string;
  username?: string;
}

const PublicProfilePreferencesTab = ({ userId, username }: PublicProfilePreferencesTabProps) => {
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // States for different preference types
  const [artistIds, setArtistIds] = useState<string[]>([]);
  const [albumIds, setAlbumIds] = useState<string[]>([]);
  const [trackIds, setTrackIds] = useState<string[]>([]);

  // State for storing the fetched items
  const [artistItems, setArtistItems] = useState<any[]>([]);
  const [albumItems, setAlbumItems] = useState<any[]>([]);
  const [trackItems, setTrackItems] = useState<any[]>([]);

  // Loading states for each type
  const [artistsLoading, setArtistsLoading] = useState(false);
  const [albumsLoading, setAlbumsLoading] = useState(false);
  const [tracksLoading, setTracksLoading] = useState(false);
  const [hasPreferences, setHasPreferences] = useState(true);

  // Fetch preferences
  useEffect(() => {
    const fetchUserPreferences = async () => {
      if (!userId && !username) return;

      setLoading(true);
      setError(null);

      try {
        let preferences: UserPreferencesResponse;

        // Use either userId or username, with userId taking precedence
        if (userId) {
          preferences = await UsersService.getUserPreferencesById(userId);
        } else if (username) {
          preferences = await UsersService.getUserPreferencesByUsername(username);
        } else {
          throw new Error('Either userId or username must be provided');
        }

        setArtistIds(preferences.artists || []);
        setAlbumIds(preferences.albums || []);
        setTrackIds(preferences.tracks || []);

        // Determine if the user has any preferences
        const hasAnyPreferences =
          (preferences.artists && preferences.artists.length > 0) ||
          (preferences.albums && preferences.albums.length > 0) ||
          (preferences.tracks && preferences.tracks.length > 0);

        setHasPreferences(hasAnyPreferences);

        // Fetch details for all preference types
        if (hasAnyPreferences) {
          if (preferences.artists && preferences.artists.length > 0) await fetchArtistDetails(preferences.artists);
          if (preferences.albums && preferences.albums.length > 0) await fetchAlbumDetails(preferences.albums);
          if (preferences.tracks && preferences.tracks.length > 0) await fetchTrackDetails(preferences.tracks);
        }

      } catch (err) {
        console.error('Error fetching preferences:', err);

        // Handle 404 differently - it means no preferences, not an error
        if (axios.isAxiosError(err) && err.response?.status === 404) {
          setHasPreferences(false);
          setArtistIds([]);
          setAlbumIds([]);
          setTrackIds([]);
        } else {
          setError(`Failed to load ${username ? username + "'s" : "user's"} preferences.`);
        }
      } finally {
        setLoading(false);
      }
    };

    fetchUserPreferences();
  }, [userId, username]);

  // Fetch artist details
  const fetchArtistDetails = async (ids: string[]) => {
    if (ids.length === 0) return;

    try {
      setArtistsLoading(true);

      // For artists, we need to fetch them one by one (no batch endpoint available)
      const artistDetails = await Promise.all(
        ids.map(async (id) => {
          try {
            return await CatalogService.getArtist(id);
          } catch (err) {
            console.error(`Error fetching artist with ID ${id}:`, err);
            return null;
          }
        })
      );

      // Filter out any failed requests
      const validArtists = artistDetails.filter(artist => artist !== null);
      setArtistItems(validArtists);
    } catch (err) {
      console.error('Error fetching artist details:', err);
    } finally {
      setArtistsLoading(false);
    }
  };

  // Fetch album details
  const fetchAlbumDetails = async (ids: string[]) => {
    if (ids.length === 0) return;

    try {
      setAlbumsLoading(true);

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

      setAlbumItems(fetchedAlbums);
    } catch (err) {
      console.error('Error fetching album details:', err);
    } finally {
      setAlbumsLoading(false);
    }
  };

  // Fetch track details
  const fetchTrackDetails = async (ids: string[]) => {
    if (ids.length === 0) return;

    try {
      setTracksLoading(true);

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

      setTrackItems(fetchedTracks);
    } catch (err) {
      console.error('Error fetching track details:', err);
    } finally {
      setTracksLoading(false);
    }
  };

  // Handle "View all" clicks
  const handleViewAll = (type: 'artists' | 'albums' | 'tracks') => {
    // In a real app, navigate to a dedicated page
    console.log(`View all ${type} clicked`);
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
          {username ? `${username}'s` : "User's"} Music Preferences
        </h3>
      </div>

      <div className="p-6">
        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6 flex items-center">
            <AlertCircle className="h-5 w-5 mr-2" />
            <span>{error}</span>
          </div>
        )}

        {!hasPreferences || (artistItems.length === 0 && albumItems.length === 0 && trackItems.length === 0) ? (
          <div className="text-center py-6 bg-gray-50 rounded-lg border border-gray-200">
            <Music className="h-16 w-16 mx-auto text-gray-400 mb-2" />
            <h3 className="text-lg font-medium text-gray-900 mb-2">No Public Preferences</h3>
            <p className="text-gray-500">
              {username ? `${username} hasn't` : "This user hasn't"} shared any public music preferences yet.
            </p>
          </div>
        ) : (
          <div className="space-y-8">
            {/* Artists Section */}
            {(artistsLoading || artistItems.length > 0) && (
              <div>
                <div className="flex justify-between items-center mb-4">
                  <h4 className="text-md font-medium text-gray-800 flex items-center">
                    <User className="h-5 w-5 mr-2 text-indigo-600" />
                    Favorite Artists
                  </h4>

                  {artistItems.length > 4 && (
                    <button
                      onClick={() => handleViewAll('artists')}
                      className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                    >
                      View all <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  )}
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
                ) : artistItems.length > 0 ? (
                  <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                    {artistItems.slice(0, 4).map(artist => (
                      <ArtistCard key={artist.spotifyId} artist={artist} />
                    ))}
                  </div>
                ) : null}
              </div>
            )}

            {/* Albums Section */}
            {(albumsLoading || albumItems.length > 0) && (
              <div>
                <div className="flex justify-between items-center mb-4">
                  <h4 className="text-md font-medium text-gray-800 flex items-center">
                    <Disc className="h-5 w-5 mr-2 text-purple-600" />
                    Favorite Albums
                  </h4>

                  {albumItems.length > 4 && (
                    <button
                      onClick={() => handleViewAll('albums')}
                      className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                    >
                      View all <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  )}
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
                ) : albumItems.length > 0 ? (
                  <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                    {albumItems.slice(0, 4).map(album => (
                      <AlbumCard key={album.spotifyId} album={album} />
                    ))}
                  </div>
                ) : null}
              </div>
            )}

            {/* Tracks Section */}
            {(tracksLoading || trackItems.length > 0) && (
              <div>
                <div className="flex justify-between items-center mb-4">
                  <h4 className="text-md font-medium text-gray-800 flex items-center">
                    <Music className="h-5 w-5 mr-2 text-pink-600" />
                    Favorite Tracks
                  </h4>

                  {trackItems.length > 4 && (
                    <button
                      onClick={() => handleViewAll('tracks')}
                      className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                    >
                      View all <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                  )}
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
                ) : trackItems.length > 0 ? (
                  <div className="bg-white rounded-lg shadow overflow-hidden">
                    {trackItems.slice(0, 4).map((track, index) => (
                      <TrackRow key={track.spotifyId} track={track} index={index} />
                    ))}
                  </div>
                ) : null}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default PublicProfilePreferencesTab;