import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Star, Heart, MessageSquare, SlidersHorizontal, ExternalLink } from 'lucide-react';
import InteractionService, { InteractionDetailDTO } from '../../api/interaction';
import { formatDate } from '../../utils/formatters';
import NormalizedStarDisplay from '../CreateInteraction/NormalizedStarDisplay';
import useAuthStore from '../../store/authStore';

interface LatestUserInteractionProps {
    itemId: string;
    itemType: 'Album' | 'Track';
}

const LatestUserInteraction = ({ itemId }: LatestUserInteractionProps) => {
    const [latestInteraction, setLatestInteraction] = useState<InteractionDetailDTO | null>(null);
    const [loading, setLoading] = useState(true);
    const [, setError] = useState<string | null>(null);
    const { user, isAuthenticated } = useAuthStore();
    const navigate = useNavigate();

    useEffect(() => {
        const fetchLatestInteraction = async () => {
            if (!isAuthenticated || !user || !itemId) return;

            setLoading(true);
            setError(null);

            try {
                // Get only the most recent interaction (limit: 1)
                const { items,  } = await InteractionService.getUserItemHistory(
                    user.id,
                    itemId,
                    1,
                    0
                );

                if (items.length > 0) {
                    setLatestInteraction(items[0]);
                }
            } catch (err) {
                console.error('Error fetching latest interaction:', err);
                setError('Failed to load your latest interaction.');
            } finally {
                setLoading(false);
            }
        };

        fetchLatestInteraction();
    }, [itemId, user, isAuthenticated]);

    const handleViewInteractionDetail = () => {
        if (latestInteraction) {
            navigate(`/interaction/${latestInteraction.aggregateId}`);
        }
    };

    if (!isAuthenticated || loading || !latestInteraction) {
        return null;
    }

    return (
        <div className="mt-4 bg-gray-50 border border-gray-200 rounded-md p-3">
            <div className="flex items-center justify-between mb-1">
                <h4 className="text-sm font-medium text-gray-700">Your latest interaction</h4>
                <button
                    onClick={handleViewInteractionDetail}
                    className="text-xs text-primary-600 flex items-center hover:text-primary-800"
                >
                    <ExternalLink className="h-3 w-3 mr-1" />
                    View details
                </button>
            </div>

            <div className="space-y-2">
                {/* Interaction date */}
                <div className="text-xs text-gray-500">
                    {formatDate(latestInteraction.createdAt)}
                </div>

                {/* Rating */}
                {latestInteraction.rating && (
                    <div className="flex items-center">
                        <Star className="h-4 w-4 text-gray-600 mr-1.5" />
                        <div className="flex items-center">
                            <NormalizedStarDisplay
                                currentGrade={latestInteraction.rating.normalizedGrade}
                                minGrade={1}
                                maxGrade={10}
                                size="sm"
                            />

                            {latestInteraction.rating.isComplex && (
                                <SlidersHorizontal className="h-3 w-3 ml-1 text-primary-600" />
                            )}
                        </div>
                    </div>
                )}

                {/* Like status */}
                {latestInteraction.isLiked && (
                    <div className="flex items-center">
                        <Heart className="h-4 w-4 text-red-500 fill-red-500 mr-1.5" />
                        <span className="text-sm text-gray-700">Liked</span>
                    </div>
                )}

                {/* Review snippet */}
                {latestInteraction.review && (
                    <div className="flex items-start">
                        <MessageSquare className="h-4 w-4 text-gray-600 mr-1.5 mt-0.5" />
                        <div className="flex-1">
                            <div className="text-sm text-gray-700 truncate">
                                {latestInteraction.review.reviewText.length > 80
                                    ? `${latestInteraction.review.reviewText.substring(0, 80)}...`
                                    : latestInteraction.review.reviewText}
                            </div>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

export default LatestUserInteraction;