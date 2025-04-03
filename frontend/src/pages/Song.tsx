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
    Clock,
    MessageSquare,
    History,
    ListMusic
} from 'lucide-react';
import CatalogService, { TrackDetail } from '../api/catalog';
import { formatDuration, formatDate } from '../utils/formatters';
import EmptyState from '../components/common/EmptyState';

const Song = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const [track, setTrack] = useState<TrackDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'reviews' | 'lists' | 'my-history'>('reviews');

    useEffect(() => {
        const fetchTrackDetails = async () => {
            if (!id) return;

            setLoading(true);
            setError(null);

            try {
                const trackData = await CatalogService.getTrack(id);
                setTrack(trackData);
            } catch (err) {
                console.error('Error fetching track details:', err);
                setError('Failed to load track information. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchTrackDetails();
    }, [id]);

    const handleTrackInteraction = () => {
        // This will be implemented later when we add the interaction functionality
        console.log('Log interaction for track:', track?.spotifyId);
        alert('Track interaction logged!');
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center py-20">
                <Loader className="h-10 w-10 text-primary-600 animate-spin" />
                <span className="ml-3 text-lg text-gray-600">Loading track details...</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="max-w-4xl mx-auto py-8">
                <EmptyState
                    title="Failed to load track"
                    message={error}
                    icon={<Music className="h-12 w-12 text-gray-400" />}
                    action={{
                        label: "Try Again",
                        onClick: () => window.location.reload()
                    }}
                />
            </div>
        );
    }

    if (!track) {
        return (
            <div className="max-w-4xl mx-auto py-8">
                <EmptyState
                    title="Track Not Found"
                    message="We couldn't find the track you're looking for."
                    icon={<Music className="h-12 w-12 text-gray-400" />}
                />
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto pb-12">
            {/* Track Header */}
            <div className="flex flex-col md:flex-row gap-8 mb-8">
                {/* Track Artwork (from album) */}
                <div className="w-full md:w-64 flex-shrink-0">
                    <div className="aspect-square w-full shadow-md rounded-lg overflow-hidden">
                        <img
                            src={track.imageUrl || track.album?.imageUrl || '/placeholder-album.jpg'}
                            alt={track.name}
                            className="w-full h-full object-cover"
                        />
                    </div>

                    {/* Primary Track Action Button */}
                    <div className="mt-4">
                        <button
                            onClick={handleTrackInteraction}
                            className="w-full flex items-center justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
                        >
                            <Star className="h-4 w-4 mr-2" />
                            Rate Track
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

                {/* Track Info */}
                <div className="flex-grow">
                    <div className="flex items-center text-gray-500 text-sm mb-2">
                        <span className="uppercase bg-gray-200 rounded px-2 py-0.5">
                            Track
                        </span>

                        {track.isExplicit && (
                            <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                                Explicit
                            </span>
                        )}

                        <span className="ml-2 flex items-center">
                            <Clock className="h-3.5 w-3.5 mr-1" />
                            {formatDuration(track.durationMs)}
                        </span>
                    </div>

                    <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-2">{track.name}</h1>

                    <div className="flex items-center mb-4">
                        {track.artists && track.artists.length > 0 ? (
                            <div className="flex flex-wrap items-center">
                                {track.artists.map((artist, index) => (
                                    <span key={artist.spotifyId}>
                                        <Link
                                            to={`/artist/${artist.spotifyId}`}
                                            className="text-lg font-medium text-primary-600 hover:underline"
                                        >
                                            {artist.name}
                                        </Link>
                                        {index < track.artists.length - 1 && <span className="mx-1">,</span>}
                                    </span>
                                ))}
                            </div>
                        ) : (
                            <span className="text-lg font-medium text-primary-600">{track.artistName}</span>
                        )}
                    </div>

                    <div className="mb-6">
                        <div className="flex items-center text-gray-600 mb-2">
                            <Disc className="h-4 w-4 mr-2" />
                            <span className="mr-1">From the album:</span>
                            <Link
                                to={`/album/${track.album?.spotifyId || track.albumId}`}
                                className="font-medium text-primary-600 hover:underline"
                            >
                                {track.album?.name || "Unknown Album"}
                            </Link>
                        </div>

                        {track.trackNumber && (
                            <div className="flex items-center text-gray-600">
                                <Music className="h-4 w-4 mr-2" />
                                <span>Track {track.trackNumber}</span>
                                {track.discNumber && track.discNumber > 1 && (
                                    <span className="ml-1">on Disc {track.discNumber}</span>
                                )}
                            </div>
                        )}

                        {track.album?.releaseDate && (
                            <div className="flex items-center text-gray-600 mt-2">
                                <Calendar className="h-4 w-4 mr-2" />
                                <span>Released: {formatDate(track.album.releaseDate)}</span>
                            </div>
                        )}
                    </div>

                    {/* External links */}
                    <div className="mt-4 flex flex-wrap gap-4 border-t border-gray-200 pt-2">
                        {track.externalUrls && track.externalUrls.length > 0 && (
                            <a
                                href={`https://open.spotify.com/track/${track.spotifyId}`}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="inline-flex items-center text-sm text-gray-600 hover:text-primary-600"
                            >
                                <ExternalLink className="h-4 w-4 mr-1" />
                                Listen on Spotify
                            </a>
                        )}

                        {track.previewUrl && (
                            <a
                                href={track.previewUrl}
                                target="_blank"
                                rel="noopener noreferrer"
                                className="inline-flex items-center text-sm text-gray-600 hover:text-primary-600"
                            >
                                <Music className="h-4 w-4 mr-1" />
                                Play Preview
                            </a>
                        )}
                    </div>
                </div>
            </div>

            {/* Content Tabs */}
            <div className="bg-white rounded-lg shadow-md overflow-hidden mb-8">
                <div className="border-b border-gray-200">
                    <nav className="flex -mb-px">
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

                {/* Reviews Tab Content */}
                {activeTab === 'reviews' && (
                    <div className="p-6">
                        <EmptyState
                            title="No reviews yet"
                            message="Be the first to share your thoughts about this track."
                            icon={<MessageSquare className="h-12 w-12 text-gray-400" />}
                            action={{
                                label: "Write a Review",
                                onClick: () => handleTrackInteraction()
                            }}
                        />
                    </div>
                )}

                {/* Lists Tab Content */}
                {activeTab === 'lists' && (
                    <div className="p-6">
                        <EmptyState
                            title="Not in any lists yet"
                            message="This track hasn't been added to any lists yet."
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
                            message="You haven't interacted with this track yet."
                            icon={<History className="h-12 w-12 text-gray-400" />}
                            action={{
                                label: "Log interaction",
                                onClick: () => handleTrackInteraction()
                            }}
                        />
                    </div>
                )}
            </div>
        </div>
    );
};

export default Song;