import { useState, useEffect, useRef } from 'react';
import { useParams } from 'react-router-dom';
import CatalogService, { TrackDetail } from '../api/catalog';
import { getPreviewUrl } from '../utils/preview-extractor';
import SongHeader from '../components/Song/SongHeader';
import SongContentTabs from '../components/Song/SongContentTabs';
import LoadingState from '../components/Song/LoadingState';
import ErrorState from '../components/Song/ErrorState';
import NotFoundState from '../components/Song/NotFoundState';

const Song = () => {
    const { id } = useParams<{ id: string }>();
    const [track, setTrack] = useState<TrackDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'reviews' | 'lists' | 'my-history'>('reviews');
    const [isPlaying, setIsPlaying] = useState(false);
    const audioRef = useRef<HTMLAudioElement | null>(null);

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

    // Clean up audio when component unmounts
    useEffect(() => {
        return () => {
            if (audioRef.current) {
                audioRef.current.pause();
            }
        };
    }, []);

    const handlePreviewToggle = async () => {
        if (!track) return;

        if (isPlaying) {
            // Pause the current track
            if (audioRef.current) {
                audioRef.current.pause();
                setIsPlaying(false);
            }
        } else {
            // Start playing the track
            try {
                const previewUrl = await getPreviewUrl(track.spotifyId);
                if (!previewUrl) {
                    console.error('No preview URL available for this track');
                    return;
                }

                // If there's already an audio element, pause it
                if (audioRef.current) {
                    audioRef.current.pause();
                }

                // Create a new audio element
                audioRef.current = new Audio(previewUrl);

                // Set up ended event to clear the playing state
                audioRef.current.addEventListener('ended', () => {
                    setIsPlaying(false);
                });

                await audioRef.current.play();
                setIsPlaying(true);
            } catch (error) {
                console.error('Error playing preview:', error);
            }
        }
    };

    const handleTrackInteraction = () => {
        // This will be implemented later when we add the interaction functionality
        console.log('Log interaction for track:', track?.spotifyId);
        alert('Track interaction logged!');
    };

    if (loading) {
        return <LoadingState />;
    }

    if (error) {
        return <ErrorState error={error} />;
    }

    if (!track) {
        return <NotFoundState />;
    }

    return (
        <div className="max-w-6xl mx-auto pb-12">
            <SongHeader
                track={track}
                isPlaying={isPlaying}
                handlePreviewToggle={handlePreviewToggle}
                handleTrackInteraction={handleTrackInteraction}
            />

            <SongContentTabs
                activeTab={activeTab}
                setActiveTab={setActiveTab}
                handleTrackInteraction={handleTrackInteraction}
            />
        </div>
    );
};

export default Song;