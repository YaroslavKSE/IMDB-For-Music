import { useState, useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';
import { Music, Disc, User, Loader, Search } from 'lucide-react';
import CatalogService, {SearchResult} from '../api/catalog';
import EmptyState from '../components/common/EmptyState';
import ArtistCard from "../components/Search/ArtistCard.tsx";
import TrackRow from "../components/Search/TrackRow.tsx";
import AlbumCard from "../components/Search/AlbumCard.tsx";

// Tabs definition for search filter
type SearchTab = 'all' | 'album' | 'track' | 'artist';

const SearchPage = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const query = searchParams.get('q') || '';
    const [searchQuery, setSearchQuery] = useState(query);
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

    // Handler for search submission
    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (searchQuery.trim()) {
            setSearchParams({ q: searchQuery.trim() });
        }
    };

    return (
        <div className="max-w-6xl mx-auto">
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

export default SearchPage;