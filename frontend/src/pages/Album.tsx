import { useState, useEffect, useRef } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Disc, Loader } from 'lucide-react';
import CatalogService, {AlbumDetail, TrackDetail, TrackSummary} from '../api/catalog';
import EmptyState from '../components/common/EmptyState';
import {getSeveralPreviewsUrl, getTrackPreviewUrl} from '../utils/preview-extractor';
import AlbumHeader from '../components/Album/AlbumHeader';
import AlbumContentTabs from '../components/Album/AlbumContentTabs';
import LoadingState from '../components/Album/LoadingState';
import MusicInteractionModal from '../components/CreateInteraction/MusicInteractionModal.tsx';
import useAuthStore from '../store/authStore';

const Album = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { isAuthenticated } = useAuthStore();
    const [album, setAlbum] = useState<AlbumDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [loadingMoreTracks, setLoadingMoreTracks] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'tracks' | 'reviews' | 'lists' | 'my-history'>('tracks');
    const [hoveredTrack, setHoveredTrack] = useState<string | null>(null);
    const [playingTrack, setPlayingTrack] = useState<string | null>(null);
    const [isInteractionModalOpen, setIsInteractionModalOpen] = useState(false);
    const [interactionSuccess, setInteractionSuccess] = useState(false);
    const [selectedTrack, setSelectedTrack] = useState<TrackDetail | null>(null);
    const [isAlbumInteraction, setIsAlbumInteraction] = useState(true);
    const [tracksOffset, setTracksOffset] = useState(0);
    const [tracksTotal, setTracksTotal] = useState(0);
    const audioRef = useRef<HTMLAudioElement | null>(null);
    const animationFrameRef = useRef<number | null>(null);

    useEffect(() => {
        const fetchAlbumDetails = async () => {
            if (!id) return;

            setLoading(true);
            setError(null);

            try {
                const albumData = await CatalogService.getAlbum(id);

                // Update the tracks offset and total for pagination tracking
                if (albumData.tracks) {
                    setTracksOffset(albumData.tracks.length);
                    setTracksTotal(albumData.totalTracks || albumData.tracks.length);
                }

                await loadAlbumPreviews(albumData);
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

    const loadAlbumPreviews = async (albumData: AlbumDetail | null) => {
        if(albumData == null) return;
        const previewsArray = await getSeveralPreviewsUrl(albumData.spotifyId);
        let i = 0;
        if(previewsArray){
            for(const track of albumData.tracks){
                if (i < previewsArray.length) {
                    track.previewUrl = previewsArray[i];
                    i++;
                }
            }
        }
    };

    const loadMoreTracks = async () => {
        if (!id || !album || loadingMoreTracks) return;

        setLoadingMoreTracks(true);

        try {
            // Fetch additional tracks using the catalog API
            const response = await CatalogService.getAlbumTracks(album.spotifyId, 50, tracksOffset);

            // Update the state with the new tracks
            const newTracks = response.tracks;
            const updatedAlbum = { ...album };

            // Append the new tracks to the existing ones
            updatedAlbum.tracks = [...updatedAlbum.tracks, ...newTracks];

            // Update the tracksOffset for next pagination
            setTracksOffset(tracksOffset + newTracks.length);
            loadAlbumPreviews(updatedAlbum);

            setAlbum(updatedAlbum);

        } catch (err) {
            console.error('Error loading more tracks:', err);
            setError('Failed to load additional tracks. Please try again.');
        } finally {
            setLoadingMoreTracks(false);
        }
    };

    const handlePreviewToggle = async (track: TrackSummary) => {
        if (playingTrack === track.spotifyId) {
            // Stop playing the current track
            if (audioRef.current) {
                audioRef.current.pause();
            }
            setPlayingTrack(null);
        } else {
            if(!track.previewUrl && track.trackNumber && track.trackNumber > 100){
                track.previewUrl = await getTrackPreviewUrl(track.spotifyId) as string;
                console.log("track preview extracted");
            }
            const previewUrl = track.previewUrl;
            if (!previewUrl) return;
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
            setPlayingTrack(track.spotifyId);
        }
    };

    const handleAlbumInteraction = () => {
        if (!isAuthenticated) {
            navigate('/login', { state: { from: `/album/${id}` } });
            return;
        }
        setIsAlbumInteraction(true);
        setSelectedTrack(null);
        setIsInteractionModalOpen(true);
    };

    const handleTrackInteraction = async (track: TrackSummary) => {
        if (!isAuthenticated) {
            navigate('/login', { state: { from: `/album/${id}` } });
            return;
        }
        // Stop any playing preview
        if (audioRef.current) {
            audioRef.current.pause();
            setPlayingTrack(null);
        }

        if (!album) return;
        try {
            // Need to fetch the full TrackDetail since TrackSummary isn't enough
            setIsInteractionModalOpen(false); // Close any open modal first

            // Fetch the complete track details
            const trackDetail = await CatalogService.getTrack(track.spotifyId);
            trackDetail.previewUrl = track.previewUrl;
            // Set the track and open the modal
            setSelectedTrack(trackDetail);
            setIsAlbumInteraction(false);
            setIsInteractionModalOpen(true);
        } catch (error) {
            console.error('Error fetching track details:', error);
            // Optionally show an error message to the user
        }
    };

    const handleInteractionSuccess = (interactionId: string) => {
        console.log('Interaction created with ID:', interactionId);
        setIsInteractionModalOpen(false);
        setInteractionSuccess(true);

        // After a successful interaction, make sure we're on the appropriate tab
        if (isAlbumInteraction) {
            setActiveTab('reviews');
        }

        // Show success message briefly
        setTimeout(() => {
            setInteractionSuccess(false);
        }, 3000);
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

    // Check if we need to show the load more button
    const showLoadMoreButton = tracksOffset < tracksTotal;

    return (
        <div className="max-w-6xl mx-auto pb-12">
            {interactionSuccess && (
                <div className="fixed top-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded z-50 shadow-md">
                    Your interaction has been posted successfully!
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

            {/* Load More Tracks Button */}
            {activeTab === 'tracks' && showLoadMoreButton && (
                <div className="flex justify-center mt-8">
                    <button
                        onClick={loadMoreTracks}
                        disabled={loadingMoreTracks}
                        className="px-6 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed flex items-center"
                    >
                        {loadingMoreTracks ? (
                            <>
                                <Loader className="h-4 w-4 animate-spin mr-2" />
                                Loading...
                            </>
                        ) : (
                            `Load More Tracks (${tracksOffset} of ${tracksTotal})`
                        )}
                    </button>
                </div>
            )}

            {/* Modal for Album interaction */}
            {album && isAlbumInteraction && (
                <MusicInteractionModal
                    item={album}
                    itemType="Album"
                    isOpen={isInteractionModalOpen}
                    onClose={() => setIsInteractionModalOpen(false)}
                    onSuccess={handleInteractionSuccess}
                />
            )}

            {/* Modal for Track interaction */}
            {selectedTrack && !isAlbumInteraction && (
                <MusicInteractionModal
                    item={selectedTrack}
                    itemType="Track"
                    isOpen={isInteractionModalOpen}
                    onClose={() => setIsInteractionModalOpen(false)}
                    onSuccess={handleInteractionSuccess}
                />
            )}
        </div>
    );
};

export default Album;