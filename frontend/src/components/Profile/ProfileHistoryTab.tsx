import { useState, useEffect, useRef, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import { Calendar, Filter, Search, Music, Disc, ArrowDown, Loader, RefreshCw } from 'lucide-react';
import InteractionService, { InteractionDetailDTO } from '../../api/interaction';
import useAuthStore from '../../store/authStore';
import CatalogService, { AlbumSummary, TrackSummary } from '../../api/catalog';
import { formatDate } from '../../utils/formatters';
import NormalizedStarDisplay from '../CreateInteraction/NormalizedStarDisplay';

// Define types for catalog items
type CatalogItem = AlbumSummary | TrackSummary;

const ProfileHistoryTab = () => {
  const navigate = useNavigate();
  const { user } = useAuthStore();
  const [interactions, setInteractions] = useState<InteractionDetailDTO[]>([]);
  const [loading, setLoading] = useState(true);
  const [loadingMore, setLoadingMore] = useState(false);
  const [hasMore, setHasMore] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [offset, setOffset] = useState(0);
  const [totalCount, setTotalCount] = useState(0);
  const [catalogItems, setCatalogItems] = useState<Map<string, CatalogItem>>(new Map());

  // For infinite scrolling
  const observerRef = useRef<IntersectionObserver | null>(null);
  const loadMoreTriggerRef = useRef<HTMLDivElement | null>(null);

  const limit = 20; // Number of items to fetch per page

  // Fetch interactions
  const fetchInteractions = useCallback(async (offsetValue = 0, append = false) => {
    if (!user?.id) return;

    try {
      if (offsetValue === 0) {
        setLoading(true);
      } else {
        setLoadingMore(true);
      }

      setError(null);

      // Get user interactions
      const response = await InteractionService.getUserInteractionsByUserId(
        user.id,
        limit,
        offsetValue
      );

      const newInteractions = response.items;
      setTotalCount(response.totalCount);

      // Update interactions list
      if (append) {
        setInteractions(prev => [...prev, ...newInteractions]);
      } else {
        setInteractions(newInteractions);
      }

      // Determine if there are more results to load
      setHasMore(offsetValue + newInteractions.length < response.totalCount);
      setOffset(offsetValue + newInteractions.length);

      // Fetch catalog item details for all the interactions
      if (newInteractions.length > 0) {
        await fetchCatalogItems(newInteractions);
      }
    } catch (err) {
      console.error('Error fetching interactions:', err);
      setError('Failed to load your rating history. Please try again.');
    } finally {
      setLoading(false);
      setLoadingMore(false);
    }
  }, [user?.id]);

  // Fetch catalog items for interactions
  const fetchCatalogItems = async (interactionsList: InteractionDetailDTO[]) => {
    // Extract unique item IDs and types
    const itemsToFetch = new Map<string, string>(); // Map of ID to type

    interactionsList.forEach(interaction => {
      if (!catalogItems.has(interaction.itemId)) {
        itemsToFetch.set(interaction.itemId, interaction.itemType.toLowerCase());
      }
    });

    if (itemsToFetch.size === 0) return;

    // Separate IDs by type
    const albumIds: string[] = [];
    const trackIds: string[] = [];

    itemsToFetch.forEach((type, id) => {
      if (type === 'album') albumIds.push(id);
      else if (type === 'track') trackIds.push(id);
    });

    try {
      const newCatalogItems = new Map(catalogItems);

      // Fetch albums in batches
      if (albumIds.length > 0) {
        // Process in batches of 20 (API limit)
        for (let i = 0; i < albumIds.length; i += 20) {
          const batch = albumIds.slice(i, i + 20);
          const albumsResponse = await CatalogService.getBatchAlbums(batch);

          if (albumsResponse.albums) {
            albumsResponse.albums.forEach(album => {
              newCatalogItems.set(album.spotifyId, album);
            });
          }
        }
      }

      // Fetch tracks in batches
      if (trackIds.length > 0) {
        // Process in batches of 20 (API limit)
        for (let i = 0; i < trackIds.length; i += 20) {
          const batch = trackIds.slice(i, i + 20);
          const tracksResponse = await CatalogService.getBatchTracks(batch);

          if (tracksResponse.tracks) {
            tracksResponse.tracks.forEach(track => {
              newCatalogItems.set(track.spotifyId, track);
            });
          }
        }
      }

      setCatalogItems(newCatalogItems);
    } catch (err) {
      console.error('Error fetching catalog items:', err);
    }
  };

  // Initial load
  useEffect(() => {
    if (user?.id) {
      fetchInteractions(0, false);
    }
  }, [user?.id, fetchInteractions]);

  // Set up intersection observer for infinite scrolling
  useEffect(() => {
    if (loading || !hasMore) return;

    // Disconnect previous observer
    if (observerRef.current) {
      observerRef.current.disconnect();
    }

    // Create new intersection observer
    observerRef.current = new IntersectionObserver(entries => {
      const [entry] = entries;
      if (entry.isIntersecting && !loadingMore) {
        loadMoreInteractions();
      }
    }, { threshold: 0.5 });

    // Observe the load more trigger element
    if (loadMoreTriggerRef.current) {
      observerRef.current.observe(loadMoreTriggerRef.current);
    }

    // Clean up observer on unmount
    return () => {
      if (observerRef.current) {
        observerRef.current.disconnect();
      }
    };
  }, [loading, loadingMore, hasMore]);

  // Load more interactions
  const loadMoreInteractions = () => {
    if (!loadingMore && hasMore) {
      fetchInteractions(offset, true);
    }
  };

  // Handle interaction click
  const handleInteractionClick = (interactionId: string) => {
    navigate(`/interaction/${interactionId}`);
  };

  if (loading && interactions.length === 0) {
    return (
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="p-6">
          <div className="flex justify-center items-center py-12">
            <RefreshCw className="h-8 w-8 text-primary-600 animate-spin mr-3" />
            <span className="text-gray-600">Loading your rating history...</span>
          </div>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="p-6">
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
            {error}
            <button
              onClick={() => fetchInteractions(0, false)}
              className="ml-2 underline hover:text-red-900"
            >
              Try again
            </button>
          </div>
        </div>
      </div>
    );
  }

  if (interactions.length === 0) {
    return (
      <div className="bg-white shadow rounded-lg overflow-hidden">
        <div className="p-6 text-center">
          <Music className="h-16 w-16 text-gray-400 mx-auto mb-3" />
          <h3 className="text-lg font-medium text-gray-900">No Rating History</h3>
          <p className="text-gray-500 mt-2 mb-6">
            You haven't rated any music yet. Start exploring and rating albums and tracks to build your history.
          </p>
          <button
            onClick={() => navigate('/')}
            className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
          >
            Discover Music
          </button>
        </div>
      </div>
    );
  }

  // Group interactions by date for better display
  const groupedInteractions: Record<string, InteractionDetailDTO[]> = {};

  interactions.forEach(interaction => {
    const date = new Date(interaction.createdAt).toLocaleDateString('en-US', {
      year: 'numeric',
      month: 'long',
      day: 'numeric'
    });

    if (!groupedInteractions[date]) {
      groupedInteractions[date] = [];
    }

    groupedInteractions[date].push(interaction);
  });

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-6 py-4 bg-primary-50 border-b border-primary-100">
        <h3 className="text-lg font-medium text-primary-800 flex items-center">
          <Calendar className="h-5 w-5 mr-2" />
          Your Rating History
        </h3>
      </div>

      <div className="p-6">
        {/* Filter controls */}
        <div className="mb-6 flex justify-between items-center">
          <div className="flex items-center">
            <Filter className="h-5 w-5 text-gray-400 mr-2" />
            <span className="text-gray-700">
              {totalCount} {totalCount === 1 ? 'entry' : 'entries'}
            </span>
          </div>

          <div className="relative">
            <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
              <Search className="h-4 w-4 text-gray-400" />
            </div>
            <input
              type="text"
              placeholder="Search your ratings..."
              className="block pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
            />
          </div>
        </div>

        {/* Timeline of interactions */}
        <div className="space-y-8">
          {Object.entries(groupedInteractions).map(([date, dayInteractions]) => (
            <div key={date} className="space-y-4">
              <div className="flex items-center">
                <div className="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center">
                  <Calendar className="h-5 w-5 text-primary-600" />
                </div>
                <h4 className="ml-3 text-lg font-medium text-gray-900">{date}</h4>
              </div>

              <div className="ml-5 pl-6 border-l-2 border-gray-200 space-y-4">
                {dayInteractions.map(interaction => {
                  const catalogItem = catalogItems.get(interaction.itemId);
                  return (
                    <div
                      key={interaction.aggregateId}
                      className="bg-white border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow cursor-pointer"
                      onClick={() => handleInteractionClick(interaction.aggregateId)}
                    >
                      <div className="flex">
                        {/* Item image */}
                        <div className="flex-shrink-0 mr-4">
                          <div className="w-16 h-16 bg-gray-200 rounded overflow-hidden">
                            {catalogItem?.imageUrl ? (
                              <img
                                src={catalogItem.imageUrl}
                                alt={catalogItem.name}
                                className="w-full h-full object-cover"
                              />
                            ) : (
                              interaction.itemType === 'Album' ? (
                                <Disc className="w-full h-full p-3 text-gray-400" />
                              ) : (
                                <Music className="w-full h-full p-3 text-gray-400" />
                              )
                            )}
                          </div>
                        </div>

                        {/* Item details */}
                        <div className="flex-grow">
                          <div className="flex flex-col sm:flex-row sm:justify-between sm:items-start">
                            <div>
                              <h4 className="font-medium text-gray-900 line-clamp-1">
                                {catalogItem?.name || 'Unknown Item'}
                              </h4>
                              <p className="text-sm text-gray-600 line-clamp-1">
                                {catalogItem?.artistName || 'Unknown Artist'}
                              </p>
                              <p className="text-xs text-gray-500 mt-1">
                                {formatDate(interaction.createdAt)}
                              </p>
                            </div>

                            <div className="flex items-center space-x-3 mt-2 sm:mt-0">
                              {/* Rating display */}
                              {interaction.rating && (
                                <div className="flex items-center">
                                  <NormalizedStarDisplay
                                    currentGrade={interaction.rating.normalizedGrade}
                                    minGrade={1}
                                    maxGrade={10}
                                    size="sm"
                                  />
                                </div>
                              )}

                              {/* Review indicator */}
                              {interaction.review && (
                                <div className="bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full">
                                  Review
                                </div>
                              )}

                              {/* Like indicator */}
                              {interaction.isLiked && (
                                <div className="bg-red-100 text-red-800 text-xs px-2 py-1 rounded-full">
                                  Loved
                                </div>
                              )}
                            </div>
                          </div>

                          {/* Preview of review text if available */}
                          {interaction.review && interaction.review.reviewText && (
                            <div className="mt-2 text-sm text-gray-700 line-clamp-2">
                              {interaction.review.reviewText}
                            </div>
                          )}
                        </div>
                      </div>
                    </div>
                  );
                })}
              </div>
            </div>
          ))}

          {/* Load more trigger for infinite scrolling */}
          {hasMore && (
            <div
              ref={loadMoreTriggerRef}
              className="flex justify-center items-center py-8"
            >
              {loadingMore ? (
                <div className="flex items-center">
                  <Loader className="h-5 w-5 text-primary-600 animate-spin mr-2" />
                  <span className="text-gray-600">Loading more...</span>
                </div>
              ) : (
                <div className="text-gray-500 text-sm flex items-center">
                  <ArrowDown className="h-4 w-4 mr-1" />
                  Scroll for more
                </div>
              )}
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProfileHistoryTab;