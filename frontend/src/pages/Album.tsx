import { useState, useEffect, useRef } from 'react';
import { useParams, Link, useNavigate } from 'react-router-dom';
import {
    Music,
    Heart,
    Star,
    Share,
    Disc,
    Calendar,
    Loader,
    Tag,
    List,
    MessageSquare,
    History,
    ListMusic,
    Play,
    Pause
} from 'lucide-react';
import CatalogService, { AlbumDetail } from '../api/catalog';
import { formatDuration, formatDate } from '../utils/formatters';
import EmptyState from '../components/common/EmptyState';
import { getPreviewUrl } from '../utils/preview-extractor';

const Album = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [album, setAlbum] = useState<AlbumDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'tracks' | 'reviews' | 'lists' | 'my-history'>('tracks');
    const [hoveredTrack, setHoveredTrack] = useState<string | null>(null);
    const [playingTrack, setPlayingTrack] = useState<string | null>(null);
    const audioRef = useRef<HTMLAudioElement | null>(null);
    const animationFrameRef = useRef<number | null>(null);

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

    useEffect(() => {
        const animationFrameId = animationFrameRef.current; // Store the value in a variable

        return () => {
            if (audioRef.current) {
                audioRef.current.pause();
            }
            if (animationFrameId) {
                cancelAnimationFrame(animationFrameId); // Use the stored variable
            }
        };
    }, []);


    const handlePreviewToggle = async (trackId: string) => {
        const previewUrl = await getPreviewUrl(trackId);
        if (!previewUrl) return;

        if (playingTrack === trackId) {
            // Stop playing the current track
            if (audioRef.current) {
                audioRef.current.pause();
            }
            setPlayingTrack(null);
        } else {
            // Stop any currently playing track
            if (audioRef.current) {
                audioRef.current.pause();
            }

            // Start playing the new track
            audioRef.current = new Audio(previewUrl);

            // Set up ended event to clear the playing state
            audioRef.current.addEventListener('ended', () => {
                setPlayingTrack(null);
            });

            await audioRef.current.play();
            setPlayingTrack(trackId);
        }
    };

    const handleAlbumInteraction = () => {
        console.log('Log interaction for album:', album?.spotifyId);
        alert('Album interaction logged!');
    };

    const handleTrackInteraction = (trackId: string, trackName: string) => {
        console.log('Log interaction for track:', trackId, trackName);
        alert(`Track interaction logged for: ${trackName}`);
    };

    // Audio Visualizer Component
    const AudioVisualizer = () => {
        const [bars, setBars] = useState([
            { height: 6, animationDuration: '1.2s', animationDelay: '0s' },
            { height: 8, animationDuration: '1.5s', animationDelay: '0.2s' },
            { height: 5, animationDuration: '1.3s', animationDelay: '0.1s' },
        ]);

        useEffect(() => {
            // Use a faster interval for more dynamic updates
            const interval = setInterval(() => {
                setBars(() => [
                    {
                        height: 4 + Math.random() * 8,
                        animationDuration: `${1.2 + Math.random() * 0.5}s`,
                        animationDelay: `${Math.random() * 0.3}s`,
                    },
                    {
                        height: 4 + Math.random() * 8,
                        animationDuration: `${1.2 + Math.random() * 0.5}s`,
                        animationDelay: `${Math.random() * 0.3}s`,
                    },
                    {
                        height: 4 + Math.random() * 8,
                        animationDuration: `${1.2 + Math.random() * 0.5}s`,
                        animationDelay: `${Math.random() * 0.3}s`,
                    }
                ]);
            }, 1000); // Update every 1 second for a more dynamic effect

            return () => {
                clearInterval(interval);
            };
        }, []);

        return (
            <div className="flex justify-center items-end space-x-1 h-6">
                {bars.map((bar, index) => (
                    <div
                        key={index}
                        style={{
                            height: `${bar.height}px`,
                            width: '4px', // Wider bars
                            backgroundColor: '#4F46E5', // Indigo color for the bars
                            animation: `equalizer ${bar.animationDuration} ease-in-out infinite alternate`,
                            animationDelay: bar.animationDelay,
                            borderRadius: '1px',
                        }}
                    />
                ))}
                <style>{`
                    @keyframes equalizer {
                        0% {
                            height: 3px;
                        }
                        100% {
                            height: 16px;
                        }
                    }
                `}</style>
            </div>
        );
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
                                {/* Spotify logo SVG instead of ExternalLink icon */}
                                <svg
                                    className="h-5 w-5 mr-1"
                                    viewBox="0 0 24 24"
                                    fill="none"
                                >
                                    <circle cx="12" cy="12" r="12" fill="#1DB954" />
                                    <path
                                        d="M17.9 10.9C14.7 9 9.35 8.8 6.3 9.75C5.8 9.9 5.3 9.6 5.15 9.15C5 8.65 5.3 8.15 5.75 8C9.3 6.95 15.15 7.15 18.85 9.35C19.3 9.6 19.45 10.2 19.2 10.65C18.95 11 18.35 11.15 17.9 10.9ZM17.8 13.9C17.55 14.25 17.1 14.35 16.75 14.1C14.05 12.45 9.95 11.9 6.8 12.85C6.4 12.95 5.95 12.75 5.85 12.35C5.75 11.95 5.95 11.5 6.35 11.4C10 10.35 14.5 10.95 17.6 12.85C17.9 13 18.05 13.5 17.8 13.9ZM16.6 16.8C16.4 17.1 16.05 17.2 15.75 17C13.4 15.55 10.45 15.3 6.95 16.1C6.6 16.2 6.3 15.95 6.2 15.65C6.1 15.3 6.35 15 6.65 14.9C10.45 14.1 13.75 14.35 16.35 16C16.7 16.15 16.75 16.5 16.6 16.8Z"
                                        fill="white"
                                    />
                                </svg>
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
                                onMouseEnter={() => setHoveredTrack(track.spotifyId)}
                                onMouseLeave={() => setHoveredTrack(null)}
                            >
                                <div
                                    className="w-8 text-center relative"
                                >
                                    {/* Playing animation when track is playing but not hovered */}
                                    {playingTrack === track.spotifyId && hoveredTrack !== track.spotifyId ? (
                                        <AudioVisualizer />
                                    ) : hoveredTrack === track.spotifyId ? (
                                        <button
                                            onClick={() => handlePreviewToggle(track.spotifyId)}
                                            className="absolute inset-0 flex items-center justify-center text-primary-600 hover:text-primary-800"
                                            title={playingTrack === track.spotifyId ? "Stop preview" : "Play preview"}
                                        >
                                            {playingTrack === track.spotifyId ? (
                                                <Pause className="h-5 w-5 fill-current" />
                                            ) : (
                                                <Play className="h-5 w-5 fill-current" />
                                            )}
                                        </button>
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