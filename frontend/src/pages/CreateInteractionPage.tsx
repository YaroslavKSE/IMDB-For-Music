import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Heart, Star, Headphones, ChevronLeft, Save, Check, Play, Pause } from 'lucide-react';
import useAuthStore from '../store/authStore';
import CatalogService from '../api/catalog';
import InteractionService, {
    GradingMethodSummary,
    GradingMethodDetail,
    GradeComponent,
    BlockComponent,
    PostInteractionRequest
} from '../api/interaction';
import { getPreviewUrl } from '../utils/preview-extractor';
import LoadingState from '../components/Album/LoadingState';
import ErrorState from '../components/Song/ErrorState';
import NotFoundState from '../components/Song/NotFoundState';
import GradeSlider from "../components/CreateInteraction/GradeSlider.tsx";
import BlockGrader from "../components/CreateInteraction/BlockGrader.tsx";

// Main interaction page component
const CreateInteractionPage = () => {
    const { itemType, itemId } = useParams<{ itemType: string; itemId: string }>();
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuthStore();

    // States for the interaction
    //eslint-disable-next-line
    const [item, setItem] = useState<any>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [submitError, setSubmitError] = useState<string | null>(null);
    const [submitSuccess, setSubmitSuccess] = useState(false);

    // Basic interaction states
    const [rating, setRating] = useState<number | null>(null);
    const [hoveredRating, setHoveredRating] = useState<number | null>(null);
    const [isLiked, setIsLiked] = useState(false);
    const [hasListened, setHasListened] = useState(false);
    const [reviewText, setReviewText] = useState('');

    // Complex grading states
    const [useComplexGrading, setUseComplexGrading] = useState(false);
    const [gradingMethods, setGradingMethods] = useState<GradingMethodSummary[]>([]);
    const [selectedMethodId, setSelectedMethodId] = useState<string | null>(null);
    const [selectedMethod, setSelectedMethod] = useState<GradingMethodDetail | null>(null);
    const [gradeValues, setGradeValues] = useState<Record<string, number>>({});

    // Preview playback states (for tracks)
    const [isPlaying, setIsPlaying] = useState(false);
    const [audio, setAudio] = useState<HTMLAudioElement | null>(null);

    // Determine if we're dealing with an album or track
    const formattedItemType = itemType === 'album' ? 'Album' : 'Track';

    // Load item details and user's grading methods
    useEffect(() => {
        if (!itemId) return;

        const fetchData = async () => {
            setLoading(true);
            setError(null);

            try {
                // Fetch item details based on type
                let itemData;
                if (itemType === 'album') {
                    itemData = await CatalogService.getAlbum(itemId);
                } else if (itemType === 'track') {
                    itemData = await CatalogService.getTrack(itemId);
                } else {
                    setError('Invalid item type specified');
                    setLoading(false);
                    return;
                }

                setItem(itemData);

                // If user is authenticated, fetch their grading methods
                if (isAuthenticated && user?.id) {
                    const methods = await InteractionService.getUserGradingMethods(user.id);
                    setGradingMethods(methods);
                }
            } catch (err) {
                console.error('Error fetching data:', err);
                setError('Failed to load item information. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchData();

        // Clean up audio on unmount
        return () => {
            if (audio) {
                audio.pause();
                audio.src = '';
            }
        };
    }, [itemId, itemType, isAuthenticated, user, audio]);

    // Load selected grading method details when changed
    useEffect(() => {
        if (!selectedMethodId) {
            setSelectedMethod(null);
            setGradeValues({});
            return;
        }

        const fetchGradingMethod = async () => {
            try {
                const methodDetail = await InteractionService.getGradingMethodById(selectedMethodId);
                setSelectedMethod(methodDetail);

                // Initialize grade values to minimum grades
                const initialValues: Record<string, number> = {};

                // Helper function to recursively initialize values
                const initializeGradeValues = (
                    components: (GradeComponent | BlockComponent)[],
                    path = ''
                ) => {
                    components.forEach(component => {
                        if (component.componentType === 'grade') {
                            const fullPath = path ? `${path}.${component.name}` : component.name;
                            initialValues[fullPath] = component.minGrade;
                        } else if (component.componentType === 'block') {
                            const newPath = path ? `${path}.${component.name}` : component.name;
                            initializeGradeValues(component.subComponents, newPath);
                        }
                    });
                };

                initializeGradeValues(methodDetail.components);
                setGradeValues(initialValues);
            } catch (err) {
                console.error('Error fetching grading method:', err);
                setSubmitError('Failed to load grading method details');
                setSelectedMethod(null);
            }
        };

        fetchGradingMethod();
    }, [selectedMethodId]);

    // Set hasListened when other interactions are engaged
    useEffect(() => {
        if (rating !== null || isLiked || reviewText.trim() !== '') {
            setHasListened(true);
        }
    }, [rating, isLiked, reviewText]);

    // Handle audio cleanup when playing state changes
    useEffect(() => {
        if (!isPlaying && audio) {
            audio.pause();
        }
    }, [isPlaying, audio]);

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

    // Handle play/pause preview for tracks
    const handleTogglePreview = async () => {
        if (isPlaying && audio) {
            audio.pause();
            setIsPlaying(false);
            return;
        }

        try {
            if (formattedItemType !== 'Track') return;

            const previewUrl = await getPreviewUrl(itemId as string);
            if (!previewUrl) {
                console.error('No preview available');
                return;
            }

            if (audio) {
                audio.pause();
            }

            const newAudio = new Audio(previewUrl);
            newAudio.addEventListener('ended', () => {
                setIsPlaying(false);
            });

            await newAudio.play();
            setAudio(newAudio);
            setIsPlaying(true);
        } catch (err) {
            console.error('Error playing preview:', err);
        }
    };

    // Handle grade value changes for complex grading
    const handleGradeChange = (name: string, value: number) => {
        setGradeValues(prev => ({
            ...prev,
            [name]: value
        }));
    };

    // Format grade inputs for API submission
    const formatGradeInputs = () => {
        return Object.entries(gradeValues).map(([name, value]) => ({
            componentName: name,
            value
        }));
    };

    // Submit the interaction
    const handleSubmit = async () => {
        if (!user) {
            setSubmitError('You must be logged in to submit an interaction');
            return;
        }

        // Check if at least "Listened" is selected
        if (!hasListened) {
            setSubmitError(`Please select at least "Listened" to log your interaction`);
            return;
        }

        // Check if all required fields for complex grading are filled
        if (useComplexGrading && !selectedMethodId) {
            setSubmitError('Please select a grading method');
            return;
        }

        try {
            setIsSubmitting(true);
            setSubmitError(null);

            const interactionData: PostInteractionRequest = {
                userId: user.id,
                itemId: itemId as string,
                itemType: formattedItemType,
                isLiked: isLiked,
                reviewText: reviewText.trim(),
            };

            // Add grading data based on selected method
            if (useComplexGrading && selectedMethodId) {
                interactionData.useComplexGrading = true;
                interactionData.gradingMethodId = selectedMethodId;
                interactionData.gradeInputs = formatGradeInputs();
            } else if (rating !== null) {
                interactionData.useComplexGrading = false;
                interactionData.basicGrade = rating * 2; // Convert 5-star scale to 10-point scale
            }

            const result = await InteractionService.createInteraction(interactionData);

            if (result.interactionCreated) {
                setSubmitSuccess(true);
                // Redirect after success
                setTimeout(() => {
                    navigate(`/${itemType}/${itemId}`);
                }, 1500);
            } else {
                setSubmitError(result.errorMessage || 'Failed to create interaction');
            }
        } catch (err) {
            console.error('Error submitting interaction:', err);
            setSubmitError('An error occurred while submitting your interaction');
        } finally {
            setIsSubmitting(false);
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

    // Render loading state
    if (loading) {
        return <LoadingState />;
    }

    // Render error state
    if (error) {
        return <ErrorState error={error} />;
    }

    // Render not found state
    if (!item) {
        return <NotFoundState />;
    }

    // Get artist name based on item type
    const artistName = formattedItemType === 'Album'
        ? item.artistName
        : (item.artists && item.artists.length > 0 ? item.artists[0]?.name : 'Unknown Artist');

    // Get image URL based on item type
    const imageUrl = item.imageUrl ||
        (formattedItemType === 'Track' && item.album?.imageUrl) ||
        '/placeholder-album.jpg';

    return (
        <div className="max-w-6xl mx-auto pb-12">
            {/* Header */}
            <div className="bg-white shadow rounded-lg mb-6">
                <div className="px-6 py-4 flex justify-between items-center">
                    <button
                        onClick={() => navigate(-1)}
                        className="flex items-center text-gray-600 hover:text-gray-900"
                    >
                        <ChevronLeft className="h-5 w-5 mr-1" />
                        Back
                    </button>
                    <h1 className="text-2xl font-bold text-center text-gray-900">
                        Rate {formattedItemType}
                    </h1>
                    <div className="w-6"></div> {/* Empty div for flex balance */}
                </div>
            </div>

            {/* Success message */}
            {submitSuccess && (
                <div className="fixed top-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded z-50 shadow-md">
                    Your interaction has been submitted successfully!
                </div>
            )}

            {/* Main content */}
            <div className="bg-white shadow rounded-lg mb-6">
                <div className="p-6">
                    {/* Item info with image */}
                    <div className="flex flex-col md:flex-row gap-8 mb-8">
                        {/* Item image */}
                        <div className="w-full md:w-64 flex-shrink-0">
                            <div className="aspect-square w-full shadow-md rounded-lg overflow-hidden">
                                <img
                                    src={imageUrl}
                                    alt={item.name}
                                    className="w-full h-full object-cover"
                                />
                            </div>

                            {/* Preview button for tracks */}
                            {formattedItemType === 'Track' && (
                                <button
                                    onClick={handleTogglePreview}
                                    className={`w-full mt-3 flex items-center justify-center py-2 px-4 border text-sm font-medium rounded-md ${
                                        isPlaying
                                            ? 'bg-primary-100 text-primary-700 border-primary-200'
                                            : 'bg-gray-100 text-gray-800 border-gray-200'
                                    }`}
                                >
                                    {isPlaying ? (
                                        <>
                                            <Pause className="h-4 w-4 mr-2" />
                                            Stop Preview
                                        </>
                                    ) : (
                                        <>
                                            <Play className="h-4 w-4 mr-2 fill-current" />
                                            Play Preview
                                        </>
                                    )}
                                </button>
                            )}
                        </div>

                        {/* Item details */}
                        <div className="flex-grow">
                            <div className="flex items-center text-gray-500 text-sm mb-2">
                <span className="uppercase bg-gray-200 rounded px-2 py-0.5">
                  {formattedItemType}
                </span>
                                {formattedItemType === 'Track' && item.isExplicit && (
                                    <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                    Explicit
                  </span>
                                )}
                            </div>

                            <h2 className="text-3xl font-bold text-gray-900 mb-2">{item.name}</h2>
                            <p className="text-lg text-gray-600 mb-4">{artistName}</p>

                            {formattedItemType === 'Album' && (
                                <div className="text-sm text-gray-500">
                                    {item.releaseDate && (
                                        <p>Released: {new Date(item.releaseDate).toLocaleDateString()}</p>
                                    )}
                                    {item.totalTracks && (
                                        <p>Tracks: {item.totalTracks}</p>
                                    )}
                                </div>
                            )}

                            {formattedItemType === 'Track' && item.album && (
                                <div className="text-sm text-gray-500">
                                    <p>From album: {item.album.name}</p>
                                    {item.durationMs && (
                                        <p>Duration: {Math.floor(item.durationMs / 60000)}:{((item.durationMs % 60000) / 1000).toFixed(0).padStart(2, '0')}</p>
                                    )}
                                </div>
                            )}
                        </div>
                    </div>

                    {/* Error message */}
                    {submitError && (
                        <div className="mb-6 bg-red-50 border border-red-200 text-red-700 p-4 rounded-md">
                            {submitError}
                        </div>
                    )}

                    {/* Grading type selector */}
                    <div className="mb-6 p-4 bg-gray-50 rounded-lg border border-gray-200">
                        <div className="flex items-center justify-between mb-4">
                            <h3 className="text-lg font-medium text-gray-900">Grading Method</h3>
                            <div className="flex gap-2">
                                <button
                                    onClick={() => setUseComplexGrading(false)}
                                    className={`px-4 py-2 rounded-md text-sm font-medium ${
                                        !useComplexGrading
                                            ? 'bg-primary-600 text-white'
                                            : 'bg-white text-gray-700 border border-gray-300'
                                    }`}
                                >
                                    Simple
                                </button>
                                <button
                                    onClick={() => setUseComplexGrading(true)}
                                    className={`px-4 py-2 rounded-md text-sm font-medium ${
                                        useComplexGrading
                                            ? 'bg-primary-600 text-white'
                                            : 'bg-white text-gray-700 border border-gray-300'
                                    }`}
                                >
                                    Complex
                                </button>
                            </div>
                        </div>

                        {/* Simple grading (star rating) */}
                        {!useComplexGrading && (
                            <div className="bg-white p-4 rounded-md border border-gray-200">
                                <h4 className="text-base font-medium text-gray-700 mb-2">Rate with Stars</h4>
                                <div className="flex justify-center items-center">
                                    {renderStars()}
                                </div>
                                {rating !== null && (
                                    <div className="mt-2 text-center">
                                        <button
                                            onClick={() => setRating(null)}
                                            className="text-sm text-gray-500 hover:text-gray-700"
                                        >
                                            Clear rating
                                        </button>
                                    </div>
                                )}
                            </div>
                        )}

                        {/* Complex grading method selector */}
                        {useComplexGrading && (
                            <div className="bg-white p-4 rounded-md border border-gray-200">
                                <div className="mb-4">
                                    <label className="block text-sm font-medium text-gray-700 mb-1">
                                        Select Grading Method
                                    </label>
                                    <select
                                        value={selectedMethodId || ''}
                                        onChange={(e) => setSelectedMethodId(e.target.value || null)}
                                        className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                                    >
                                        <option value="">Select a grading method...</option>
                                        {gradingMethods.map((method) => (
                                            <option key={method.id} value={method.id}>
                                                {method.name}
                                            </option>
                                        ))}
                                    </select>
                                </div>

                                {/* Show selected method details and graders */}
                                {selectedMethod && (
                                    <div className="mt-4">
                                        <h4 className="text-base font-medium text-gray-700 mb-2">
                                            {selectedMethod.name}
                                        </h4>
                                        <div className="text-sm text-gray-600 mb-4">
                                            Grading range: {selectedMethod.minPossibleGrade} - {selectedMethod.maxPossibleGrade}
                                        </div>

                                        {/* Render grading components */}
                                        <div className="space-y-4">
                                            {selectedMethod.components.map((component, index) => (
                                                <div key={index}>
                                                    {component.componentType === 'grade' ? (
                                                        <GradeSlider
                                                            component={component}
                                                            value={gradeValues[component.name] ?? null}
                                                            onChange={handleGradeChange}
                                                        />
                                                    ) : (
                                                        <BlockGrader
                                                            component={component as BlockComponent}
                                                            values={gradeValues}
                                                            onChange={handleGradeChange}
                                                        />
                                                    )}
                                                </div>
                                            ))}
                                        </div>
                                    </div>
                                )}
                            </div>
                        )}
                    </div>

                    {/* Like and Listened buttons */}
                    <div className="flex gap-4 mb-6">
                        <button
                            type="button"
                            onClick={() => setIsLiked(!isLiked)}
                            className={`flex items-center justify-center px-4 py-3 rounded-md flex-1 ${
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
                            className={`flex items-center justify-center px-4 py-3 rounded-md flex-1 ${
                                hasListened
                                    ? 'bg-blue-50 text-blue-600 border border-blue-200'
                                    : 'bg-gray-50 text-gray-500 border border-gray-200'
                            }`}
                            disabled={hasListened && (isLiked || rating !== null || reviewText.trim() !== '' || useComplexGrading)}
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

                    {/* Review textarea */}
                    <div className="mb-6">
                        <label className="block text-base font-medium text-gray-700 mb-2">
                            Your Review
                        </label>
                        <textarea
                            value={reviewText}
                            onChange={(e) => setReviewText(e.target.value)}
                            className="w-full px-4 py-3 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                            rows={6}
                            placeholder={`Write your thoughts about this ${formattedItemType.toLowerCase()}...`}
                        />
                    </div>

                    {/* Submit button */}
                    <div className="flex justify-end">
                        <button
                            type="button"
                            onClick={handleSubmit}
                            disabled={isSubmitting || !hasListened}
                            className="px-6 py-3 flex items-center justify-center border border-transparent text-base font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed"
                        >
                            {isSubmitting ? (
                                <>
                                    <svg className="animate-spin -ml-1 mr-3 h-5 w-5 text-white" xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24">
                                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4"></circle>
                                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"></path>
                                    </svg>
                                    Submitting...
                                </>
                            ) : (
                                <>
                                    <Save className="h-5 w-5 mr-2" />
                                    Submit Interaction
                                </>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CreateInteractionPage;