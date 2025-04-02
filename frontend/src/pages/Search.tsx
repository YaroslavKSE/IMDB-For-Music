import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Music, Disc, User, Loader } from 'lucide-react';
import CatalogService, {
    SearchResult,
    AlbumSummary,
    TrackSummary,
    ArtistSummary
} from '../api/catalog';
import EmptyState from '../components/common/EmptyState';
import { formatDuration } from '../utils/formatters';

// Tabs definition for search filter
type SearchTab = 'all' | 'album' | 'track' | 'artist';

const Search = () => {
    const [searchParams] = useSearchParams();
    const query = searchParams.get('q') || '';
    const [activeTab, setActiveTab] = useState<SearchTab>('all');
    const [results, setResults] = useState<SearchResult | null>(null);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        // Don't search if query is empty
        if (!query.trim()) return;

        const fetchResults = async () => {
            setLoading(true);
            setError(null);
            try {
                // Determine which types to search based on active tab
                let typeParam = 'album,track,artist';
                if (activeTab !== 'all') {
                    typeParam = activeTab;
                }

                const data = await CatalogService.search(query, typeParam);
                setResults(data);
            } catch (err) {
                console.error('Error fetching search results:', err);
                setError('Failed to fetch search results. Please try again.');
            } finally {
                setLoading(false);
            }
        };

        fetchResults();
    }, [query, activeTab]);

    // Handler for changing the active tab
    const handleTabChange = (tab: SearchTab) => {
        setActiveTab(tab);
    };

    return (
        <div className="max-w-6xl mx-auto">
            <div className="mb-8">
                <h1 className="text-3xl font-bold mb-2">Search Results</h1>
                <p className="text-gray-600">
                    {query ? `Showing results for "${query}"` : 'Enter a search term to find music'}
                </p>
            </div>

            {/* Search Tabs */}
            <div className="border-b border-gray-200 mb-6">
                <nav className="flex -mb-px">
                    <button
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
            {loading && (
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
                    results.albums?.length === 0 &&
                    results.tracks?.length === 0 &&
                    results.artists?.length === 0) ||
                (activeTab === 'album' && results.albums?.length === 0) ||
                (activeTab === 'track' && results.tracks?.length === 0) ||
                (activeTab === 'artist' && results.artists?.length === 0)
            ) && (
                <EmptyState
                    title="No results found"
                    message="Try adjusting your search or filter to find what you're looking for."
                    icon={<Disc className="h-12 w-12 text-gray-400" />}
                />
            )}

            {/* Results Display */}
            {!loading && !error && results && (
                <div className="space-y-8">
                    {/* Albums Section */}
                    {(activeTab === 'all' || activeTab === 'album') && results.albums && results.albums.length > 0 && (
                        <div>
                            {activeTab === 'all' && <h2 className="text-xl font-bold mb-4">Albums</h2>}
                            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                                {results.albums.map((album) => (
                                    <AlbumCard key={album.spotifyId} album={album} />
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Tracks Section */}
                    {(activeTab === 'all' || activeTab === 'track') && results.tracks && results.tracks.length > 0 && (
                        <div>
                            {activeTab === 'all' && <h2 className="text-xl font-bold mb-4">Tracks</h2>}
                            <div className="bg-white rounded-lg shadow overflow-hidden">
                                {results.tracks.map((track, index) => (
                                    <TrackRow key={track.spotifyId} track={track} index={index} />
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Artists Section */}
                    {(activeTab === 'all' || activeTab === 'artist') && results.artists && results.artists.length > 0 && (
                        <div>
                            {activeTab === 'all' && <h2 className="text-xl font-bold mb-4">Artists</h2>}
                            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                                {results.artists.map((artist) => (
                                    <ArtistCard key={artist.spotifyId} artist={artist} />
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

// Album Card Component
const AlbumCard = ({ album }: { album: AlbumSummary }) => {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-md transition-shadow duration-200">
            <a href={`/album/${album.spotifyId}`} className="block">
                <div className="aspect-square w-full overflow-hidden bg-gray-200">
                    <img
                        src={album.imageUrl || '/placeholder-album.jpg'}
                        alt={album.name}
                        className="w-full h-full object-cover"
                    />
                </div>
                <div className="p-3">
                    <h3 className="font-medium text-gray-900 truncate">{album.name}</h3>
                    <p className="text-sm text-gray-600 truncate">{album.artistName}</p>
                    <div className="flex items-center mt-1 text-xs text-gray-500">
                        <span>{album.releaseDate?.split('-')[0] || 'Unknown year'}</span>
                        <span className="mx-1">•</span>
                        <span>{album.albumType === 'album' ? 'Album' : album.albumType}</span>
                    </div>
                </div>
            </a>
        </div>
    );
};

// Track Row Component
const TrackRow = ({ track, index }: { track: TrackSummary; index: number }) => {
    return (
        <div className={`flex items-center px-4 py-3 ${
            index % 2 === 0 ? 'bg-white' : 'bg-gray-50'
        }`}>
            <div className="flex-shrink-0 mr-4">
                <img
                    src={track.imageUrl || '/placeholder-album.jpg'}
                    alt={track.name}
                    className="w-12 h-12 object-cover shadow"
                />
            </div>
            <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between">
                    <div className="min-w-0 flex-1">
                        <a
                            href={`/track/${track.spotifyId}`}
                            className="block hover:text-primary-600"
                        >
                            <h4 className="text-sm font-medium text-gray-900 truncate flex items-center">
                                {track.name}
                                {track.isExplicit && (
                                    <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                                        E
                                    </span>
                                )}
                            </h4>
                            <p className="text-xs text-gray-500 truncate">{track.artistName}</p>
                        </a>
                    </div>
                    <div className="ml-4 flex-shrink-0 flex items-center">
                        <span className="text-xs text-gray-500">{formatDuration(track.durationMs)}</span>
                        <a
                            href={`/album/${track.albumId}`}
                            className="ml-3 text-gray-500 hover:text-primary-600 focus:outline-none"
                            title="View Album"
                        >
                            <Disc className="h-5 w-5" />
                        </a>
                    </div>
                </div>
            </div>
        </div>
    );
};

// Artist Card Component
const ArtistCard = ({ artist }: { artist: ArtistSummary }) => {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-md transition-shadow duration-200">
            <a href={`/artist/${artist.spotifyId}`} className="block">
                <div className="aspect-square w-full overflow-hidden bg-gray-200 rounded-full mx-auto p-2">
                    <img
                        src={artist.imageUrl || '/placeholder-artist.jpg'}
                        alt={artist.name}
                        className="w-full h-full object-cover rounded-full"
                    />
                </div>
                <div className="p-3 text-center">
                    <h3 className="font-medium text-gray-900 truncate">{artist.name}</h3>
                    <p className="text-sm text-gray-600">Artist</p>
                </div>
            </a>
        </div>
    );
};

export default Search;