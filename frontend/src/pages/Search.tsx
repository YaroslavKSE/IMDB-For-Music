import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Search, Disc, Music, User, Loader, ChevronRight } from 'lucide-react';
import CatalogService, { SearchResult, AlbumSummary, TrackSummary, ArtistSummary } from '../api/catalog';
import EmptyState from '../components/common/EmptyState';
import ArtistCard from "../components/Search/ArtistCard";
import TrackRow from "../components/Search/TrackRow";
import AlbumCard from "../components/Search/AlbumCard";

// Tabs definition for search filter
type SearchTab = 'all' | 'album' | 'track' | 'artist';

const SearchPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const query = searchParams.get('q') || '';
    const [searchQuery, setSearchQuery] = useState(query);
    const [activeTab, setActiveTab] = useState<SearchTab>('all');

    // Results state
    const [results, setResults] = useState<SearchResult | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Pagination for each content type
    const [albumsOffset, setAlbumsOffset] = useState(0);
    const [tracksOffset, setTracksOffset] = useState(0);
    const [artistsOffset, setArtistsOffset] = useState(0);

    // Expanded results for dedicated tabs
    const [albumsExpanded, setAlbumsExpanded] = useState<AlbumSummary[]>([]);
    const [tracksExpanded, setTracksExpanded] = useState<TrackSummary[]>([]);
    const [artistsExpanded, setArtistsExpanded] = useState<ArtistSummary[]>([]);

    // Loading states for each content type
    const [isLoadingMoreAlbums, setIsLoadingMoreAlbums] = useState(false);
    const [isLoadingMoreTracks, setIsLoadingMoreTracks] = useState(false);
    const [isLoadingMoreArtists, setIsLoadingMoreArtists] = useState(false);

    // Total counts
    const [albumsTotal, setAlbumsTotal] = useState(0);
    const [tracksTotal, setTracksTotal] = useState(0);
    const [artistsTotal, setArtistsTotal] = useState(0);

    // Constants
    const INITIAL_LIMIT = 5;  // Items to show on 'all' tab
    const EXPANDED_LIMIT = 20; // Items to show on dedicated tabs

    // Function to fetch search results - separated from component effect dependencies
    const fetchSearchResults = async (
        searchQuery: string,
        searchType: string,
        limit: number,
        offset: number
    ) => {
        if (!searchQuery.trim()) return null;

        try {
            return await CatalogService.search(searchQuery, searchType, limit, offset);
        } catch (err) {
            console.error('Error fetching search results:', err);
            setError('Failed to fetch search results. Please try again.');
            return null;
        }
    };

    // Initial search on query or tab change
    useEffect(() => {
        if (!query.trim()) return;

        const initialSearch = async () => {
            setLoading(true);
            setError(null);

            try {
                // For the 'all' tab, we need all types with small limit
                if (activeTab === 'all') {
                    const data = await fetchSearchResults(query, 'album,track,artist', INITIAL_LIMIT, 0);
                    if (data) {
                        setResults(data);

                        // Set total counts
                        setAlbumsTotal(data.totalResults || 0);
                        setTracksTotal(data.totalResults || 0);
                        setArtistsTotal(data.totalResults || 0);

                        // Reset offsets
                        setAlbumsOffset(0);
                        setTracksOffset(0);
                        setArtistsOffset(0);
                    }
                }
                // For specific tabs, fetch more items
                else {
                    const data = await fetchSearchResults(query, activeTab, EXPANDED_LIMIT, 0);

                    if (data) {
                        // Set results and update the appropriate expanded array
                        setResults(data);

                        // Safely extract and set albums
                        if (activeTab === 'album') {
                            const albums = data?.albums ?? [];
                            setAlbumsExpanded(albums);
                            setAlbumsTotal(data?.totalResults || 0);
                            setAlbumsOffset(0);
                        } else if (activeTab === 'track') {
                            const tracks = data?.tracks ?? [];
                            setTracksExpanded(tracks);
                            setTracksTotal(data?.totalResults || 0);
                            setTracksOffset(0);
                        } else if (activeTab === 'artist') {
                            const artists = data?.artists ?? [];
                            setArtistsExpanded(artists);
                            setArtistsTotal(data?.totalResults || 0);
                            setArtistsOffset(0);
                        }
                    }
                }
            } catch (err) {
                console.error('Error fetching search results:', err);
                setError('Failed to fetch search results. Please try again.');
            } finally {
                setLoading(false);
            }
        };

        initialSearch();
    }, [query, activeTab]);

    // Handler for changing the active tab
    const handleTabChange = (tab: SearchTab) => {
        setActiveTab(tab);

        // If we're switching to a specific tab and don't have data yet, fetch it
        if (tab !== 'all') {
            if ((tab === 'album' && albumsExpanded.length === 0) ||
                (tab === 'track' && tracksExpanded.length === 0) ||
                (tab === 'artist' && artistsExpanded.length === 0)) {
                // Data will be fetched by the useEffect that depends on activeTab
            }
        }
    };

    // Handler for search submission
    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (searchQuery.trim()) {
            setSearchParams({ q: searchQuery.trim() });
        }
    };

    // Function to safely handle array spreading
    const loadMoreAlbums = async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();

        if (isLoadingMoreAlbums) return;

        const newOffset = albumsOffset + EXPANDED_LIMIT;
        setIsLoadingMoreAlbums(true);

        try {
            const data = await fetchSearchResults(query, 'album', EXPANDED_LIMIT, newOffset);
            if (data) {
                const newAlbums = data.albums ?? [];
                setAlbumsExpanded(prev => [...prev, ...newAlbums]);
                setAlbumsOffset(newOffset);
            }
        } finally {
            setIsLoadingMoreAlbums(false);
        }
    };

    const loadMoreTracks = async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();

        if (isLoadingMoreTracks) return;

        const newOffset = tracksOffset + EXPANDED_LIMIT;
        setIsLoadingMoreTracks(true);

        try {
            const data = await fetchSearchResults(query, 'track', EXPANDED_LIMIT, newOffset);
            if (data) {
                const newTracks = data.tracks ?? [];
                setTracksExpanded(prev => [...prev, ...newTracks]);
                setTracksOffset(newOffset);
            }
        } finally {
            setIsLoadingMoreTracks(false);
        }
    };

    const loadMoreArtists = async (e: React.MouseEvent) => {
        e.preventDefault();
        e.stopPropagation();

        if (isLoadingMoreArtists) return;

        const newOffset = artistsOffset + EXPANDED_LIMIT;
        setIsLoadingMoreArtists(true);

        try {
            const data = await fetchSearchResults(query, 'artist', EXPANDED_LIMIT, newOffset);
            if (data) {
                const newArtists = data.artists ?? [];
                setArtistsExpanded(prev => [...prev, ...newArtists]);
                setArtistsOffset(newOffset);
            }
        } finally {
            setIsLoadingMoreArtists(false);
        }
    };

    // Extract counts from the results
    const albumsCount = activeTab === 'all' ? albumsTotal : (results?.totalResults || 0);
    const tracksCount = activeTab === 'all' ? tracksTotal : (results?.totalResults || 0);
    const artistsCount = activeTab === 'all' ? artistsTotal : (results?.totalResults || 0);

    // Handle "Show more" clicks
    const handleShowMoreAlbums = () => {
        setActiveTab('album');
        // The tab change will trigger the data fetch if needed
    };

    const handleShowMoreTracks = () => {
        setActiveTab('track');
        // The tab change will trigger the data fetch if needed
    };

    const handleShowMoreArtists = () => {
        setActiveTab('artist');
        // The tab change will trigger the data fetch if needed
    };

    return (
        <div className="max-w-6xl mx-auto py-8">
            <div className="mb-4">
                <h1 className="text-3xl font-bold mb-2">Search Results</h1>
                <p className="text-gray-600">
                    {query ? `Showing results for "${query}"` : 'Enter a search term to find music'}
                </p>
            </div>

            {/* Search Bar */}
            <div className="max-w-3xl mb-8">
                <form onSubmit={handleSearchSubmit} className="relative">
                    <div className="flex items-center">
                        <input
                            type="text"
                            placeholder="Search for artists, albums, or tracks..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            className="w-full py-3 px-5 pl-12 rounded-full text-base focus:outline-none border border-gray-300 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 shadow-sm"
                        />
                        <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                            <Search className="h-5 w-5 text-gray-400" />
                        </div>
                        <button
                            type="submit"
                            className="absolute right-3 bg-primary-600 text-white p-2 rounded-full hover:bg-primary-700 transition-colors"
                        >
                            <Search className="h-5 w-5" />
                        </button>
                    </div>
                </form>
            </div>

            {/* Search Tabs */}
            <div className="border-b border-gray-200 mb-6">
                <nav className="flex -mb-px">
                    <button
                        type="button"
                        onClick={() => handleTabChange('all')}
                        className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm ${
                            activeTab === 'all'
                                ? 'border-primary-600 text-primary-600'
                                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                        }`}
                    >
                        All Results
                    </button>
                    <button
                        type="button"
                        onClick={() => handleTabChange('album')}
                        className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
                            activeTab === 'album'
                                ? 'border-primary-600 text-primary-600'
                                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                        }`}
                    >
                        <Disc className="h-4 w-4 mr-1" />
                        Albums
                    </button>
                    <button
                        type="button"
                        onClick={() => handleTabChange('track')}
                        className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
                            activeTab === 'track'
                                ? 'border-primary-600 text-primary-600'
                                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                        }`}
                    >
                        <Music className="h-4 w-4 mr-1" />
                        Tracks
                    </button>
                    <button
                        type="button"
                        onClick={() => handleTabChange('artist')}
                        className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
                            activeTab === 'artist'
                                ? 'border-primary-600 text-primary-600'
                                : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                        }`}
                    >
                        <User className="h-4 w-4 mr-1" />
                        Artists
                    </button>
                </nav>
            </div>

            {/* Loading State */}
            {loading && !results && (
                <div className="flex justify-center items-center py-12">
                    <Loader className="h-8 w-8 text-primary-600 animate-spin" />
                    <span className="ml-2 text-gray-600">Loading results...</span>
                </div>
            )}

            {/* Error State */}
            {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-4">
                    {error}
                </div>
            )}

            {/* Empty State */}
            {!loading && !error && results && (
                (activeTab === 'all' &&
                    (!results.albums || results.albums.length === 0) &&
                    (!results.tracks || results.tracks.length === 0) &&
                    (!results.artists || results.artists.length === 0)) ||
                (activeTab === 'album' && (!results.albums || results.albums.length === 0)) ||
                (activeTab === 'track' && (!results.tracks || results.tracks.length === 0)) ||
                (activeTab === 'artist' && (!results.artists || results.artists.length === 0))
            ) && (
                <EmptyState
                    title="No results found"
                    message="Try adjusting your search or filter to find what you're looking for."
                    icon={<Search className="h-12 w-12 text-gray-400" />}
                />
            )}

            {/* Results Display */}
            {!loading && !error && results && (
                <div className="space-y-10">
                    {/* All Results Tab */}
                    {activeTab === 'all' && (
                        <>
                            {/* Albums Section */}
                            {results.albums && results.albums.length > 0 && (
                                <div>
                                    <div className="flex justify-between items-center mb-4">
                                        <h2 className="text-xl font-bold">Albums</h2>
                                        {albumsCount > INITIAL_LIMIT && (
                                            <button
                                                type="button"
                                                onClick={handleShowMoreAlbums}
                                                className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                                            >
                                                View all <ChevronRight className="h-4 w-4 ml-1" />
                                            </button>
                                        )}
                                    </div>
                                    <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                                        {results.albums.slice(0, INITIAL_LIMIT).map((album) => (
                                            <AlbumCard key={album.spotifyId} album={album} />
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* Tracks Section */}
                            {results.tracks && results.tracks.length > 0 && (
                                <div>
                                    <div className="flex justify-between items-center mb-4">
                                        <h2 className="text-xl font-bold">Tracks</h2>
                                        {tracksCount > INITIAL_LIMIT && (
                                            <button
                                                type="button"
                                                onClick={handleShowMoreTracks}
                                                className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                                            >
                                                View all <ChevronRight className="h-4 w-4 ml-1" />
                                            </button>
                                        )}
                                    </div>
                                    <div className="bg-white rounded-lg shadow overflow-hidden">
                                        {results.tracks.slice(0, INITIAL_LIMIT).map((track, index) => (
                                            <TrackRow key={track.spotifyId} track={track} index={index} />
                                        ))}
                                    </div>
                                </div>
                            )}

                            {/* Artists Section */}
                            {results.artists && results.artists.length > 0 && (
                                <div>
                                    <div className="flex justify-between items-center mb-4">
                                        <h2 className="text-xl font-bold">Artists</h2>
                                        {artistsCount > INITIAL_LIMIT && (
                                            <button
                                                type="button"
                                                onClick={handleShowMoreArtists}
                                                className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                                            >
                                                View all <ChevronRight className="h-4 w-4 ml-1" />
                                            </button>
                                        )}
                                    </div>
                                    <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                                        {results.artists.slice(0, INITIAL_LIMIT).map((artist) => (
                                            <ArtistCard key={artist.spotifyId} artist={artist} />
                                        ))}
                                    </div>
                                </div>
                            )}
                        </>
                    )}

                    {/* Albums Tab */}
                    {activeTab === 'album' && results.albums && (
                        <div>
                            <h2 className="text-xl font-bold mb-4">Albums</h2>
                            {albumsExpanded.length === 0 && results.albums ? (
                                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                                    {results.albums.map((album) => (
                                        <AlbumCard key={album.spotifyId} album={album} />
                                    ))}
                                </div>
                            ) : (
                                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-5 gap-4">
                                    {albumsExpanded.map((album) => (
                                        <AlbumCard key={album.spotifyId} album={album} />
                                    ))}
                                </div>
                            )}

                            {/* Load more button */}
                            {(results.albums.length < albumsTotal || albumsExpanded.length < albumsTotal) && (
                                <div className="mt-8 text-center">
                                    <button
                                        type="button"
                                        onClick={loadMoreAlbums}
                                        disabled={isLoadingMoreAlbums}
                                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {isLoadingMoreAlbums ? (
                                            <span className="flex items-center justify-center">
                                                <Loader className="h-4 w-4 animate-spin mr-2" />
                                                Loading...
                                            </span>
                                        ) : (
                                            `Load More (${albumsExpanded.length || results.albums.length} of ${albumsTotal})`
                                        )}
                                    </button>
                                </div>
                            )}
                        </div>
                    )}

                    {/* Tracks Tab */}
                    {activeTab === 'track' && results.tracks && (
                        <div>
                            <h2 className="text-xl font-bold mb-4">Tracks</h2>
                            <div className="bg-white rounded-lg shadow overflow-hidden">
                                {tracksExpanded.length === 0 && results.tracks ? (
                                    results.tracks.map((track, index) => (
                                        <TrackRow key={track.spotifyId} track={track} index={index} />
                                    ))
                                ) : (
                                    tracksExpanded.map((track, index) => (
                                        <TrackRow key={track.spotifyId} track={track} index={index} />
                                    ))
                                )}
                            </div>

                            {/* Load more button */}
                            {(results.tracks.length < tracksTotal || tracksExpanded.length < tracksTotal) && (
                                <div className="mt-8 text-center">
                                    <button
                                        type="button"
                                        onClick={loadMoreTracks}
                                        disabled={isLoadingMoreTracks}
                                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {isLoadingMoreTracks ? (
                                            <span className="flex items-center justify-center">
                                                <Loader className="h-4 w-4 animate-spin mr-2" />
                                                Loading...
                                            </span>
                                        ) : (
                                            `Load More (${tracksExpanded.length || results.tracks.length} of ${tracksTotal})`
                                        )}
                                    </button>
                                </div>
                            )}
                        </div>
                    )}

                    {/* Artists Tab */}
                    {activeTab === 'artist' && results.artists && (
                        <div>
                            <h2 className="text-xl font-bold mb-4">Artists</h2>
                            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                                {artistsExpanded.length === 0 && results.artists ? (
                                    results.artists.map((artist) => (
                                        <ArtistCard key={artist.spotifyId} artist={artist} />
                                    ))
                                ) : (
                                    artistsExpanded.map((artist) => (
                                        <ArtistCard key={artist.spotifyId} artist={artist} />
                                    ))
                                )}
                            </div>

                            {/* Load more button */}
                            {(results.artists.length < artistsTotal || artistsExpanded.length < artistsTotal) && (
                                <div className="mt-8 text-center">
                                    <button
                                        type="button"
                                        onClick={loadMoreArtists}
                                        disabled={isLoadingMoreArtists}
                                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {isLoadingMoreArtists ? (
                                            <span className="flex items-center justify-center">
                                                <Loader className="h-4 w-4 animate-spin mr-2" />
                                                Loading...
                                            </span>
                                        ) : (
                                            `Load More (${artistsExpanded.length || results.artists.length} of ${artistsTotal})`
                                        )}
                                    </button>
                                </div>
                            )}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default SearchPage;