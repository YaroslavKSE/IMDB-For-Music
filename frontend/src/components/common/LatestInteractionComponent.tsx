import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Heart, MessageSquare, SlidersHorizontal } from 'lucide-react';
import InteractionService, { InteractionDetailDTO } from '../../api/interaction';
import UsersService, { PublicUserProfile } from '../../api/users';
import NormalizedStarDisplay from '../CreateInteraction/NormalizedStarDisplay';

interface LatestInteractionComponentProps {
    itemId: string;
    itemType: 'Album' | 'Track';
}

const LatestInteractionComponent = ({ itemId, itemType }: LatestInteractionComponentProps) => {
    const [latestInteraction, setLatestInteraction] = useState<InteractionDetailDTO | null>(null);
    const [userProfile, setUserProfile] = useState<PublicUserProfile | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [currentDate] = useState(() => {
        const now = new Date();
        return now.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric'
        });
    });

    useEffect(() => {
        const fetchLatestInteraction = async () => {
            if (!itemId) return;

            setLoading(true);
            setError(null);

            try {
                // Get the latest interaction for this item (limit: 1, offset: 0)
                // Get the latest interaction for this item (limit: 1, offset: 0)
                // Using getItemReviews as it returns interactions from all users for the item
                const { items, totalCount } = await InteractionService.getItemReviews(
                    itemId,
                    1,
                    0
                );

                if (items.length > 0 && totalCount > 0) {
                    const interaction = items[0];
                    setLatestInteraction(interaction);

                    // Fetch user profile for the interaction
                    if (interaction.userId) {
                        const profile = await UsersService.getUserProfileById(interaction.userId);
                        setUserProfile(profile);
                    }
                }
            } catch (err) {
                console.error('Error fetching latest interaction:', err);
                setError('Failed to load latest interaction.');
            } finally {
                setLoading(false);
            }
        };

        fetchLatestInteraction();
    }, [itemId, itemType]);

    if (loading || !latestInteraction || !userProfile) {
        return null;
    }

    if (error) {
        return null;
    }

    return (
        <div className="bg-white shadow rounded-lg overflow-hidden h-full">
            <div className="flex items-center p-4 hover:bg-gray-50">
                {/* User profile image */}
                <div className="flex-shrink-0 h-16 w-16 rounded-full overflow-hidden mr-4">
                    {userProfile?.avatarUrl ? (
                        <img
                            src={userProfile.avatarUrl}
                            alt={userProfile.name}
                            className="h-full w-full object-cover"
                        />
                    ) : (
                        <div className="h-full w-full flex items-center justify-center bg-primary-100 text-primary-700 text-xl font-bold">
                            {userProfile.name.charAt(0)}{userProfile.surname.charAt(0)}
                        </div>
                    )}
                </div>

                {/* Interaction details */}
                <div className="flex-grow min-w-0">
                    <div className="flex items-center">
                        <Link
                            to={`/people/${userProfile.id}`}
                            className="text-base font-medium text-gray-900 hover:text-primary-600 truncate"
                        >
                            {userProfile.name} {userProfile.surname}
                        </Link>
                    </div>
                    <div className="mt-1 text-xs text-gray-500">
                        {currentDate}
                    </div>
                </div>

                {/* Interaction indicators */}
                <div className="flex items-center space-x-4 ml-4">
                    {/* Rating stars */}
                    <div className="flex items-center">
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
                                        className="ml-1 h-4 w-4 text-primary-500 cursor-pointer hover:text-primary-700"
                                    />
                                )}
                            </div>
                        )}
                    </div>

                    {/* Review icon */}
                    {latestInteraction.review ? (
                        <Link to={`/interaction/${latestInteraction.aggregateId}`}>
                            <MessageSquare
                                className="h-5 w-5 text-primary-600 cursor-pointer hover:text-primary-800"
                            />
                        </Link>
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
            </div>
        </div>
    );
};

export default LatestInteractionComponent;