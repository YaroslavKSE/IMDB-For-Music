import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Disc } from 'lucide-react';
import CatalogService, { AlbumDetail } from '../api/catalog';
import EmptyState from '../components/common/EmptyState';
import { getPreviewUrl } from '../utils/preview-extractor';
import AlbumHeader from '../components/Album/AlbumHeader';
import AlbumContentTabs from '../components/Album/AlbumContentTabs';
import LoadingState from '../components/Album/LoadingState';
import MusicInteractionModal from '../components/common/MusicInteractionModal';
import useAuthStore from '../store/authStore';

const Album = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { isAuthenticated } = useAuthStore();
    const [album, setAlbum] = useState<AlbumDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'tracks' | 'reviews' | 'lists' | 'my-history'>('tracks');
    const [hoveredTrack, setHoveredTrack] = useState<string | null>(null);
    const [playingTrack, setPlayingTrack] = useState<string | null>(null);
    const [isInteractionModalOpen, setIsInteractionModalOpen] = useState(false);
    const [interactionSuccess, setInteractionSuccess] = useState(false);
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
        if (!isAuthenticated) {
            navigate('/login', { state: { from: `/album/${id}` } });
            return;
        }
        setIsInteractionModalOpen(true);
    };

    const handleInteractionSuccess = (interactionId: string) => {
        console.log('Interaction created with ID:', interactionId);
        setIsInteractionModalOpen(false);
        setInteractionSuccess(true);

        // After a successful interaction, switch to the reviews tab
        setActiveTab('reviews');

        // Show success message briefly
        setTimeout(() => {
            setInteractionSuccess(false);
        }, 3000);
    };

    const handleTrackInteraction = (trackId: string, trackName: string) => {
        // Implementation for track interaction will be handled separately
        console.log('Log interaction for track:', trackId, trackName);
        // We'll implement this with a modal later
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
            {interactionSuccess && (
                <div className="fixed top-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded z-50 shadow-md">
                    Your review has been posted successfully!
                </div>
            )}

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

            {album && (
                <MusicInteractionModal
                    item={album}
                    itemType="Album"
                    isOpen={isInteractionModalOpen}
                    onClose={() => setIsInteractionModalOpen(false)}
                    onSuccess={handleInteractionSuccess}
                />
            )}
        </div>
    );
};

export default Album;