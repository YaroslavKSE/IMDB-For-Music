import { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import { Disc } from 'lucide-react';
import CatalogService, { AlbumDetail } from '../api/catalog';
import EmptyState from '../components/common/EmptyState';
import { getPreviewUrl } from '../utils/preview-extractor';
import AlbumHeader from '../components/Album/AlbumHeader';
import AlbumContentTabs from '../components/Album/AlbumContentTabs';
import LoadingState from '../components/Album/LoadingState';

const Album = () => {
    const { id } = useParams<{ id: string }>();
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

    if (loading) {
        return <LoadingState />;
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
            <AlbumHeader
                album={album}
                handleAlbumInteraction={handleAlbumInteraction}
            />

            <AlbumContentTabs
                activeTab={activeTab}
                setActiveTab={setActiveTab}
                album={album}
                playingTrack={playingTrack}
                hoveredTrack={hoveredTrack}
                setHoveredTrack={setHoveredTrack}
                handlePreviewToggle={handlePreviewToggle}
                handleTrackInteraction={handleTrackInteraction}
                handleAlbumInteraction={handleAlbumInteraction}
            />
        </div>
    );
};

export default Album;