import { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Heart, MessageSquare, SlidersHorizontal } from 'lucide-react';
import InteractionService, { InteractionDetailDTO } from '../../api/interaction';
import useAuthStore from '../../store/authStore';
import NormalizedStarDisplay from '../CreateInteraction/NormalizedStarDisplay';

interface LatestInteractionComponentProps {
    itemId: string;
    itemType: 'Album' | 'Track';
    onCreateInteraction?: () => void;
}

const LatestInteractionComponent = ({ itemId, itemType, onCreateInteraction }: LatestInteractionComponentProps) => {
    const { user, isAuthenticated } = useAuthStore();
    const navigate = useNavigate();
    const [latestInteraction, setLatestInteraction] = useState<InteractionDetailDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchUserLatestInteraction = async () => {
            if (!isAuthenticated || !user || !itemId) {
                setLoading(false);
                return;
            }

            setLoading(true);
            setError(null);

            try {
                // Get the user's latest interaction with this item (limit: 1, offset: 0)
                const { items, totalCount } = await InteractionService.getUserItemHistory(
                    user.id,
                    itemId,
                    1,
                    0
                );

                if (items.length > 0 && totalCount > 0) {
                    setLatestInteraction(items[0]);
                } else {
                    setLatestInteraction(null);
                }
            } catch (err) {
                console.error('Error fetching user interaction:', err);
                setError('Failed to load your interaction history.');
            } finally {
                setLoading(false);
            }
        };

        fetchUserLatestInteraction();
    }, [user, itemId, itemType, isAuthenticated]);

    if (loading) {
        return (
            <div className="bg-white shadow rounded-lg p-4">
                <div className="animate-pulse flex space-x-4">
                    <div className="rounded-full bg-gray-200 h-12 w-12"></div>
                    <div className="flex-1 space-y-4 py-1">
                        <div className="h-4 bg-gray-200 rounded w-3/4"></div>
                        <div className="space-y-2">
                            <div className="h-4 bg-gray-200 rounded"></div>
                        </div>
                    </div>
                </div>
            </div>
        );
    }

    if (error) {
        return null;
    }

    // If no interaction yet, show prompt to create one
    if (!latestInteraction) {
        return (
            <div className="bg-white shadow rounded-lg p-4">
                <div className="text-center py-4">
                    <p className="text-gray-600 mb-3">You haven't interacted with this {itemType.toLowerCase()} yet.</p>
                    <button
                        onClick={isAuthenticated ?
                            (onCreateInteraction || (() => navigate(`/create-interaction/${itemType.toLowerCase()}/${itemId}`))) :
                            () => navigate('/login', { state: { from: window.location.pathname } })}
                        className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
                    >
                        {isAuthenticated ? `Rate this ${itemType.toLowerCase()}` : 'Log in to rate'}
                    </button>
                </div>
            </div>
        );
    }

    // Format the date nicely
    const interactionDate = new Date(latestInteraction.createdAt).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
    });

    return (
        <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-4">
                <div className="flex justify-between items-center mb-3">
                    <h3 className="text-sm font-medium text-gray-700">Your Latest Activity</h3>
                    <span className="text-xs text-gray-500">{interactionDate}</span>
                </div>

                <Link
                    to={`/interaction/${latestInteraction.aggregateId}`}
                    className="flex items-center space-x-4 hover:bg-gray-50 p-2 rounded-md transition-colors"
                >
                    {/* ItemHistory indicators */}
                    <div className="flex items-center space-x-3">
                        {/* Rating */}
                        {latestInteraction.rating && (
                            <div className="flex items-center">
                                <NormalizedStarDisplay
                                    currentGrade={latestInteraction.rating.normalizedGrade}
                                    minGrade={1}
                                    maxGrade={10}
                                    size="sm"
                                />

                                {latestInteraction.rating.isComplex && (
                                    <SlidersHorizontal
                                        className="ml-1 h-4 w-4 text-primary-500"
                                    />
                                )}
                            </div>
                        )}

                        {/* Review icon */}
                        {latestInteraction.review ? (
                            <MessageSquare
                                className="h-5 w-5 text-primary-600"
                            />
                        ) : (
                            <MessageSquare
                                className="h-5 w-5 text-gray-300"
                            />
                        )}

                        {/* Like icon */}
                        {latestInteraction.isLiked ? (
                            <Heart className="h-5 w-5 text-red-500 fill-red-500"/>
                        ) : (
                            <Heart className="h-5 w-5 text-gray-300"/>
                        )}
                    </div>
                </Link>
            </div>
        </div>
    );
};

export default LatestInteractionComponent;