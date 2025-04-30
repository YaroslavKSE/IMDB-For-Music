import { useState, useEffect, useRef } from 'react';
import {X, Heart, Star, Headphones, Check, Play, Pause, SlidersHorizontal} from 'lucide-react';
import { AlbumDetail, TrackDetail } from '../../api/catalog';
import InteractionService, { PostInteractionRequest } from '../../api/interaction';
import useAuthStore from '../../store/authStore';
import { useNavigate } from 'react-router-dom';

interface MusicInteractionModalProps {
    item: AlbumDetail | TrackDetail;
    itemType: 'Album' | 'Track';
    isOpen: boolean;
    onClose: () => void;
    onSuccess: (interactionId: string) => void;
}

const MusicInteractionModal = ({
                                   item,
                                   itemType,
                                   isOpen,
                                   onClose,
                                   onSuccess
                               }: MusicInteractionModalProps) => {
    const navigate = useNavigate();
    const { user } = useAuthStore();
    const [rating, setRating] = useState<number | null>(null);
    const [hoveredRating, setHoveredRating] = useState<number | null>(null);
    const [isLiked, setIsLiked] = useState(false);
    const [hasListened, setHasListened] = useState(false);
    const [reviewText, setReviewText] = useState('');
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [isPlaying, setIsPlaying] = useState(false);
    const audioRef = useRef<HTMLAudioElement | null>(null);

    // Handle type checking
    const isAlbum = (item: AlbumDetail | TrackDetail): item is AlbumDetail => {
        return itemType === 'Album' && 'artistName' in item;
    };

    // Get dynamic data based on item type
    const itemId = isAlbum(item) ? item.spotifyId : item.spotifyId;
    const itemName = item.name;
    const artistName = isAlbum(item)
        ? item.artistName
        : (item.artists && item.artists.length > 0 ? item.artists[0]?.name : 'Unknown Artist');

    // Get the correct image URL
    const getImageUrl = () => {
        if (isAlbum(item)) {
            return item.imageUrl || '/placeholder-album.jpg';
        } else {
            // For tracks, try multiple potential sources
            if (item.imageUrl) return item.imageUrl;
            if (item.album?.imageUrl) return item.album.imageUrl;
            if (item.images && item.images.length > 0) return item.images[0].url;
            return '/placeholder-album.jpg';
        }
    };

    const imageUrl = getImageUrl();

    // Reset state when modal opens
    useEffect(() => {
        if (isOpen) {
            setRating(null);
            setHoveredRating(null);
            setIsLiked(false);
            setHasListened(false);
            setReviewText('');
            setError(null);
            setIsPlaying(false);
            if (audioRef.current) {
                audioRef.current.pause();
                audioRef.current = null;
            }
        }
    }, [isOpen]);

    // Clean up audio on unmount or when modal closes
    useEffect(() => {
        return () => {
            if (audioRef.current) {
                audioRef.current.pause();
                audioRef.current = null;
            }
        };
    }, []);

    // Automatically set hasListened to true if user interacts in any other way
    useEffect(() => {
        if (rating !== null || isLiked || reviewText.trim() !== '') {
            setHasListened(true);
        }
    }, [rating, isLiked, reviewText]);

    // Also stop audio when modal closes
    useEffect(() => {
        if (!isOpen && audioRef.current) {
            audioRef.current.pause();
            setIsPlaying(false);
            audioRef.current = null;
        }
    }, [isOpen]);

    // Calculate the effective rating to display (hovered or selected)
    const displayRating = hoveredRating !== null ? hoveredRating : rating;

    // Handle rating selection with half-star precision
    const handleRatingClick = (value: number) => {
        setRating(value);
    };

    // Handle the listened button toggle
    const handleListenedToggle = () => {
        // Only allow toggling off if there are no other interactions
        if (hasListened && !isLiked && rating === null && reviewText.trim() === '') {
            setHasListened(false);
        } else {
            setHasListened(true);
        }
    };

    // Handle play/pause preview
    const handleTogglePreview = async () => {
        if (isPlaying && audioRef.current) {
            audioRef.current.pause();
            setIsPlaying(false);
            return;
        }

        try {
            // Only for tracks, or when dealing with a track inside an album
            let previewUrl;
            if (itemType === 'Track') {
                const track = item as TrackDetail;
                if(track.previewUrl){
                    previewUrl = track.previewUrl;
                }
            }

            if (!previewUrl) {
                console.error('No preview available');
                return;
            }

            if (audioRef.current) {
                audioRef.current.pause();
            }

            audioRef.current = new Audio(previewUrl);
            audioRef.current.addEventListener('ended', () => {
                setIsPlaying(false);
            });

            await audioRef.current.play();
            setIsPlaying(true);
        } catch (err) {
            console.error('Error playing preview:', err);
        }
    };

    // Create star components with improved clickable areas
    const renderStars = () => {
        const stars = [];
        const totalStars = 5;

        for (let i = 0; i < totalStars; i++) {
            const starValue = i + 1;
            const halfStarValue = i + 0.5;

            // Calculate if this star should be filled or half-filled
            const isStarFilled = displayRating !== null && displayRating >= starValue;
            const isHalfStarFilled = displayRating !== null && displayRating >= halfStarValue && displayRating < starValue;

            // Create the star container with extended click area
            stars.push(
                <div key={i} className="relative inline-block w-8 h-8">
                    {/* Full star */}
                    <Star
                        className={`h-8 w-8 ${isStarFilled ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}`}
                    />

                    {/* Half star overlay */}
                    {isHalfStarFilled && (
                        <div className="absolute inset-0 overflow-hidden w-1/2">
                            <Star className="h-8 w-8 text-yellow-400 fill-yellow-400" />
                        </div>
                    )}

                    {/* Invisible click targets for half and full stars */}
                    <div className="absolute inset-0 flex">
                        {/* Left half (for half-star) */}
                        <div
                            className="w-1/2 h-full cursor-pointer"
                            onClick={() => handleRatingClick(halfStarValue)}
                            onMouseEnter={() => setHoveredRating(halfStarValue)}
                            onMouseLeave={() => setHoveredRating(null)}
                        />

                        {/* Right half (for full star) */}
                        <div
                            className="w-1/2 h-full cursor-pointer"
                            onClick={() => handleRatingClick(starValue)}
                            onMouseEnter={() => setHoveredRating(starValue)}
                            onMouseLeave={() => setHoveredRating(null)}
                        />
                    </div>
                </div>
            );
        }

        // Create a container that wraps all stars and adds clickable areas between them
        return (
            <div className="flex items-center relative">
                {/* This overlay creates clickable areas across the entire star rating component */}
                <div className="absolute inset-0 flex space-x-0">
                    {Array.from({ length: totalStars * 2 }).map((_, index) => {
                        const value = (index + 1) / 2;
                        return (
                            <div
                                key={`click-${index}`}
                                className="h-full flex-1 cursor-pointer"
                                onClick={() => handleRatingClick(value)}
                                onMouseEnter={() => setHoveredRating(value)}
                                onMouseLeave={() => setHoveredRating(null)}
                            />
                        );
                    })}
                </div>
                {/* The actual stars with no spacing */}
                <div className="flex space-x-0">
                    {stars}
                </div>
            </div>
        );
    };

    // Handle form submission
    const handleSubmit = async () => {
        if (!user) {
            setError('You must be logged in to submit an interaction');
            return;
        }

        // Check if at least "Listened" is selected
        if (!hasListened) {
            setError(`Please select at least "Listened" to log your interaction`);
            return;
        }

        try {
            setIsSubmitting(true);
            setError(null);

            const interactionData: PostInteractionRequest = {
                userId: user.id,
                itemId: itemId,
                itemType: itemType,
                isLiked: isLiked,
                reviewText: reviewText.trim(),
            };

            // Only add rating data if a rating was selected
            if (rating !== null) {
                interactionData.useComplexGrading = false;
                interactionData.basicGrade = rating * 2; // Convert 5-star scale to 10-point scale
            }

            const result = await InteractionService.createInteraction(interactionData);

            if (result.interactionCreated) {
                onSuccess(result.interactionId);
            } else {
                setError(result.errorMessage || 'Failed to create interaction');
            }
        } catch (err) {
            console.error('Error submitting interaction:', err);
            setError('An error occurred while submitting your interaction');
        } finally {
            setIsSubmitting(false);
        }
    };

    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen p-4">
                {/* Backdrop */}
                <div
                    className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
                    onClick={onClose}
                ></div>

                {/* Modal */}
                <div className="bg-white rounded-lg shadow-xl w-full max-w-md mx-auto z-10 relative">
                    {/* Header */}
                    <div className="flex justify-between items-center p-4 border-b border-gray-200">
                        <h2 className="text-xl font-bold text-gray-900">Rate {itemType}</h2>
                        <button
                            onClick={onClose}
                            className="text-gray-400 hover:text-gray-500"
                        >
                            <X className="h-6 w-6" />
                        </button>
                    </div>

                    {/* Item info with image */}
                    <div className="p-4 flex flex-col items-center">
                        {/* Just the image, no click interactions */}
                        <img
                            src={imageUrl}
                            alt={itemName}
                            className="w-32 h-32 object-cover rounded-md shadow-md"
                        />

                        <div className="mt-3 text-center">
                            <h3 className="font-medium text-gray-900">{itemName}</h3>
                            <p className="text-sm text-gray-500">{artistName}</p>
                        </div>

                        {/* Preview button for track (outside of image) */}
                        {itemType === 'Track' && (
                            <button
                                onClick={handleTogglePreview}
                                className={`mt-3 flex items-center justify-center px-4 py-1.5 rounded-full text-sm font-medium ${
                                    isPlaying
                                        ? 'bg-primary-100 text-primary-700 border border-primary-200'
                                        : 'bg-gray-100 text-gray-800 border border-gray-200'
                                }`}
                            >
                                {isPlaying ? (
                                    <>
                                        <Pause className="h-4 w-4 mr-1.5" />
                                        Stop Preview
                                    </>
                                ) : (
                                    <>
                                        <Play className="h-4 w-4 mr-1.5 fill-gray-800" />
                                        Play Preview
                                    </>
                                )}
                            </button>
                        )}
                    </div>

                    {/* ItemHistory options */}
                    <div className="p-4 border-t border-gray-200">
                        {/* Rating */}
                        <div className="mb-4">
                            <div className="flex justify-center items-center">
                                {renderStars()}
                            </div>
                            {/* Fixed height container for the buttons to prevent layout jumps */}
                            <div className="mt-2 text-center h-6 flex items-center justify-center">
                                {rating !== null ? (
                                    <button
                                        onClick={() => setRating(null)}
                                        className="text-sm text-gray-500 hover:text-gray-700 flex items-center"
                                    >
                                        Clear rating
                                    </button>
                                ) : (
                                    <button
                                        onClick={() => navigate(`/create-interaction/${itemType.toLowerCase()}/${itemId}`)}
                                        className="text-sm text-gray-500 hover:text-primary-600 flex items-center"
                                    >
                                        <SlidersHorizontal className="h-4 w-4 mr-1" />
                                        <span>Use complex grading</span>
                                    </button>
                                )}
                            </div>
                        </div>

                        {/* Like and Listened buttons side by side */}
                        <div className="flex gap-3 mb-4">
                            <button
                                type="button"
                                onClick={() => setIsLiked(!isLiked)}
                                className={`flex items-center justify-center px-3 py-2 rounded-md flex-1 ${
                                    isLiked
                                        ? 'bg-red-50 text-red-500 border border-red-200'
                                        : 'bg-gray-50 text-gray-500 border border-gray-200'
                                }`}
                            >
                                <Heart
                                    className={`h-5 w-5 mr-2 ${isLiked ? 'fill-red-500' : ''}`}
                                />
                                {isLiked ? 'Liked' : 'Like'}
                            </button>

                            <button
                                type="button"
                                onClick={handleListenedToggle}
                                className={`flex items-center justify-center px-3 py-2 rounded-md flex-1 ${
                                    hasListened
                                        ? 'bg-blue-50 text-blue-600 border border-blue-200'
                                        : 'bg-gray-50 text-gray-500 border border-gray-200'
                                }`}
                                disabled={hasListened && (isLiked || rating !== null || reviewText.trim() !== '')}
                            >
                                {hasListened ? (
                                    <>
                                        <Check className="h-5 w-5 mr-2" />
                                        Listened
                                    </>
                                ) : (
                                    <>
                                        <Headphones className="h-5 w-5 mr-2" />
                                        Listened
                                    </>
                                )}
                            </button>
                        </div>

                        {/* Review */}
                        <div>
                            <label className="block text-sm font-medium text-gray-700 mb-2">
                                Your Review
                            </label>
                            <textarea
                                value={reviewText}
                                onChange={(e) => setReviewText(e.target.value)}
                                className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                                rows={4}
                                placeholder={`Write your thoughts about this ${itemType.toLowerCase()}...`}
                            />
                        </div>
                    </div>

                    {/* Footer */}
                    <div className="p-4 flex justify-end">
                        {error && (
                            <div className="mr-auto text-sm text-red-600">
                                {error}
                            </div>
                        )}
                        <button
                            type="button"
                            onClick={onClose}
                            className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 mr-3"
                        >
                            Cancel
                        </button>
                        <button
                            type="button"
                            onClick={handleSubmit}
                            disabled={isSubmitting || !hasListened}
                            className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed"
                        >
                            {isSubmitting ? (
                                <span className="flex items-center">
                  <svg
                      className="animate-spin -ml-1 mr-2 h-4 w-4 text-white"
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                  >
                    <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                    ></circle>
                    <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                    ></path>
                  </svg>
                  Submitting...
                </span>
                            ) : (
                                'Send'
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default MusicInteractionModal;