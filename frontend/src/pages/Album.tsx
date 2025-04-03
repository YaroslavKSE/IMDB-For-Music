import { useState, useEffect } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import {
    Music,
    Heart,
    Star,
    Share,
    ExternalLink,
    Disc,
    Calendar,
    Loader,
    Tag,
    List,
    MessageSquare,
    History,
    ListMusic,
    Play
} from 'lucide-react';
import CatalogService, { AlbumDetail } from '../api/catalog';
import { formatDuration, formatDate } from '../utils/formatters';
import EmptyState from '../components/common/EmptyState';

const Album = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [album, setAlbum] = useState<AlbumDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'tracks' | 'reviews' | 'lists' | 'my-history'>('tracks');
    const [hoveredTrack, setHoveredTrack] = useState<string | null>(null);

    useEffect(() => {
        const fetchAlbumDetails = async () => {
            if (!id) return;

            setLoading(true);
            setError(null);

            try {
                const albumData = await CatalogService.getAlbum(id);
                setAlbum(albumData);
            } catch (err) {
                console.error('Error fetching album details:', err);
                setError('Failed to load album information. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchAlbumDetails();
    }, [id]);

    const handleAlbumInteraction = () => {
        // This will be implemented later when we add the interaction functionality
        console.log('Log interaction for album:', album?.spotifyId);
        alert('Album interaction logged!');
    };

    const handleTrackInteraction = (trackId: string, trackName: string) => {
        // This will be implemented later when we add the interaction functionality
        console.log('Log interaction for track:', trackId, trackName);
        alert(`Track interaction logged for: ${trackName}`);
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center py-20">
                <Loader className="h-10 w-10 text-primary-600 animate-spin" />
                <span className="ml-3 text-lg text-gray-600">Loading album details...</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="max-w-4xl mx-auto py-8">
                <EmptyState
                    title="Failed to load album"
                    message={error}
                    icon={<Disc className="h-12 w-12 text-gray-400" />}
                    action={{
                        label: "Try Again",
                        onClick: () => window.location.reload()
                    }}
                />
            </div>
        );
    }

    if (!album) {
        return (
            <div className="max-w-4xl mx-auto py-8">
                <EmptyState
                    title="Album Not Found"
                    message="We couldn't find the album you're looking for."
                    icon={<Disc className="h-12 w-12 text-gray-400" />}
                />
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto pb-12">
            {/* Album Header */}
            <div className="flex flex-col md:flex-row gap-8 mb-8">
                {/* Album Artwork */}
                <div className="w-full md:w-64 flex-shrink-0">
                    <div className="aspect-square w-full shadow-md rounded-lg overflow-hidden">
                        <img
                            src={album.imageUrl || '/placeholder-album.jpg'}
                            alt={album.name}
                            className="w-full h-full object-cover"
                        />
                    </div>

                    {/* Primary Album Action Button */}
                    <div className="mt-4">
                        <button
                            onClick={handleAlbumInteraction}
                            className="w-full flex items-center justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
                        >
                            <Star className="h-4 w-4 mr-2" />
                            Rate Album
                        </button>
                    </div>

                    <div className="mt-3 flex justify-between space-x-2">
                        <button className="flex-1 flex items-center justify-center py-2 px-4 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none">
                            <Heart className="h-4 w-4 mr-2" />
                            Add to List
                        </button>

                        <button className="flex items-center justify-center p-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none">
                            <Share className="h-4 w-4" />
                        </button>
                    </div>
                </div>

                {/* Album Info */}
                <div className="flex-grow">
                    <div className="flex items-center text-gray-500 text-sm mb-2">
                        <span className="uppercase bg-gray-200 rounded px-2 py-0.5">
                            {album.albumType === 'album' ? 'Album' : album.albumType}
                        </span>

                        {album.releaseDate && (
                            <span className="ml-2 flex items-center">
                                <Calendar className="h-3.5 w-3.5 mr-1" />
                                {formatDate(album.releaseDate)}
                            </span>
                        )}
                    </div>

                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-2">{album.name}</h1>

                    <div className="flex items-center mb-4">
                        <Link to={`/artist/${album.artists[0]?.spotifyId || '#'}`} className="text-lg font-medium text-primary-600 hover:underline">
                            {album.artistName}
                        </Link>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-y-2 gap-x-8 text-sm mb-6">
                        {album.totalTracks && (
                            <div className="flex items-center text-gray-600">
                                <Music className="h-4 w-4 mr-2" />
                                <span>{album.totalTracks} tracks</span>
                            </div>
                        )}

                        {album.label && (
                            <div className="flex items-center text-gray-600">
                                <Tag className="h-4 w-4 mr-2" />
                                <span>Label: {album.label}</span>
                            </div>
                        )}

                        {album.genres && album.genres.length > 0 && (
                            <div className="flex items-center text-gray-600 col-span-2">
                                <List className="h-4 w-4 mr-2" />
                                <span>Genres: {album.genres.join(', ')}</span>
                            </div>
                        )}
                    </div>

                    {album.copyright && (
                        <div className="text-xs text-gray-500 mt-2 border-t border-gray-200 pt-2">
                            {album.copyright}
                        </div>
                    )}

                    {album.externalUrls && album.externalUrls.length > 0 && (
                        <div className="mt-4">
                            <a
                                href={`https://open.spotify.com/album/${album.spotifyId}`}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="inline-flex items-center text-sm text-gray-600 hover:text-primary-600"
                            >
                                <ExternalLink className="h-4 w-4 mr-1" />
                                Listen on Spotify
                            </a>
                        </div>
                    )}
                </div>
            </div>

            {/* Content Tabs */}
            <div className="bg-white rounded-lg shadow-md overflow-hidden mb-8">
                <div className="border-b border-gray-200">
                    <nav className="flex -mb-px">
                        <button
                            onClick={() => setActiveTab('tracks')}
                            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center ${
                                activeTab === 'tracks'
                                    ? 'border-primary-600 text-primary-600'
                                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                            }`}
                        >
                            <Music className="h-4 w-4 mr-2" />
                            Tracklist
                        </button>
                        <button
                            onClick={() => setActiveTab('reviews')}
                            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center ${
                                activeTab === 'reviews'
                                    ? 'border-primary-600 text-primary-600'
                                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                            }`}
                        >
                            <MessageSquare className="h-4 w-4 mr-2" />
                            Reviews
                        </button>
                        <button
                            onClick={() => setActiveTab('lists')}
                            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center ${
                                activeTab === 'lists'
                                    ? 'border-primary-600 text-primary-600'
                                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                            }`}
                        >
                            <ListMusic className="h-4 w-4 mr-2" />
                            In Lists
                        </button>
                        <button
                            onClick={() => setActiveTab(`my-history`)}
                            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center ${
                                activeTab === 'my-history'
                                    ? 'border-primary-600 text-primary-600'
                                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                            }`}
                        >
                            <History className="h-4 w-4 mr-2" />
                            My History
                        </button>
                    </nav>
                </div>

                {/* Tracks Tab Content */}
                {activeTab === 'tracks' && (
                    <div className="divide-y divide-gray-200">
                        {album.tracks.map((track, index) => (
                            <div
                                key={track.spotifyId}
                                className={`flex items-center px-6 py-3 hover:bg-gray-50 ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}
                            >
                                <div
                                    className="w-8 text-center relative"
                                    onMouseEnter={() => setHoveredTrack(track.spotifyId)}
                                    onMouseLeave={() => setHoveredTrack(null)}
                                >
                                    {hoveredTrack === track.spotifyId ? (
                                        <a
                                            href={`https://open.spotify.com/track/${track.spotifyId}`}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className="absolute inset-0 flex items-center justify-center text-primary-600 hover:text-primary-800"
                                            title="Play on Spotify"
                                        >
                                            <Play className="h-5 w-5 fill-current" />
                                        </a>
                                    ) : (
                                        <span className="text-gray-500 font-medium">
                                            {track.trackNumber || index + 1}
                                        </span>
                                    )}
                                </div>

                                <div className="flex-grow min-w-0 ml-4">
                                    <div className="flex items-center">
                                        <Link
                                            to={`/track/${track.spotifyId}`}
                                            className="text-gray-900 font-medium hover:text-primary-600 truncate flex items-center"
                                        >
                                            {track.name}
                                        </Link>

                                        {track.isExplicit && (
                                            <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                                                E
                                            </span>
                                        )}
                                    </div>

                                    <div className="text-sm text-gray-500 truncate">
                                        {track.artistName}
                                    </div>
                                </div>

                                <div className="ml-auto flex items-center">
                                    <span className="text-sm text-gray-500 mr-4">
                                        {formatDuration(track.durationMs)}
                                    </span>

                                    <button
                                        onClick={() => handleTrackInteraction(track.spotifyId, track.name)}
                                        className="text-gray-400 hover:text-primary-600 focus:outline-none"
                                    >
                                        <Star className="h-5 w-5" />
                                    </button>
                                </div>
                            </div>
                        ))}
                    </div>
                )}

                {/* Reviews Tab Content */}
                {activeTab === 'reviews' && (
                    <div className="p-6">
                        <EmptyState
                            title="No reviews yet"
                            message="Be the first to share your thoughts about this album."
                            icon={<MessageSquare className="h-12 w-12 text-gray-400" />}
                            action={{
                                label: "Write a Review",
                                onClick: () => handleAlbumInteraction()
                            }}
                        />
                    </div>
                )}

                {/* Lists Tab Content */}
                {activeTab === 'lists' && (
                    <div className="p-6">
                        <EmptyState
                            title="Not in any lists yet"
                            message="This album hasn't been added to any lists yet."
                            icon={<ListMusic className="h-12 w-12 text-gray-400" />}
                            action={{
                                label: "Create a List",
                                onClick: () => navigate('/lists/create')
                            }}
                        />
                    </div>
                )}

                {/* History Tab Content */}
                {activeTab === 'my-history' && (
                    <div className="p-6">
                        <EmptyState
                            title="No history"
                            message="You haven't interacted with this album."
                            icon={<History className="h-12 w-12 text-gray-400" />}
                            action={{
                                label: "Log interaction",
                                onClick: () => handleAlbumInteraction()
                            }}
                        />
                    </div>
                )}
            </div>
        </div>
    );
};

export default Album;