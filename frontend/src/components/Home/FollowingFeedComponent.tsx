import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { RefreshCcw, ArrowRight, Heart, SlidersHorizontal, Calendar, MessageSquare, UserRoundPlus } from 'lucide-react';
import InteractionService, { InteractionDetailDTO } from '../../api/interaction';
import CatalogService from '../../api/catalog';
import UsersService from '../../api/users';
import useAuthStore from '../../store/authStore';
import { formatDate } from '../../utils/formatters';
import NormalizedStarDisplay from '../CreateInteraction/NormalizedStarDisplay';

interface FollowingFeedItemData {
    interaction: InteractionDetailDTO;
    catalogItem?: {
        spotifyId: string;
        name: string;
        imageUrl: string;
        artistName: string;
    };
    userProfile?: {
        id: string;
        name: string;
        surname: string;
        username: string;
        avatarUrl?: string;
    };
}

const FollowingFeedComponent = () => {
    const { user, isAuthenticated } = useAuthStore();
    const [interactions, setInteractions] = useState<FollowingFeedItemData[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [, setTotalInteractions] = useState(0);

    useEffect(() => {
        const fetchFollowingFeed = async () => {
            if (!isAuthenticated || !user) {
                setLoading(false);
                return;
            }

            try {
                setLoading(true);
                const { items, totalCount } = await InteractionService.getUserFollowingFeed(user.id, 5, 0);
                setTotalInteractions(totalCount);

                if (items.length === 0) {
                    setInteractions([]);
                    setLoading(false);
                    return;
                }

                const itemIds = items.map(interaction => interaction.itemId);
                const previewResponse = await CatalogService.getItemPreviewInfo(itemIds, ['album', 'track']);

                const itemsMap = new Map<string, FollowingFeedItemData['catalogItem']>();
                previewResponse.results?.forEach(group => {
                    group.items?.forEach(item => {
                        itemsMap.set(item.spotifyId, {
                            spotifyId: item.spotifyId,
                            name: item.name,
                            imageUrl: item.imageUrl,
                            artistName: item.artistName,
                        });
                    });
                });

                const userIds = [...new Set(items.map(i => i.userId))];
                const userProfiles = await UsersService.getUserProfilesBatch(userIds);
                const userProfilesMap = new Map<string, FollowingFeedItemData['userProfile']>();
                userProfiles.forEach(profile => {
                    userProfilesMap.set(profile.id, {
                        id: profile.id,
                        name: profile.name,
                        surname: profile.surname,
                        username: profile.username,
                        avatarUrl: profile.avatarUrl,
                    });
                });

                const feedItems = items.map(interaction => ({
                    interaction,
                    catalogItem: itemsMap.get(interaction.itemId),
                    userProfile: userProfilesMap.get(interaction.userId),
                }));

                setInteractions(feedItems);
            } catch (err) {
                console.error('Error fetching following feed:', err);
                setError('Failed to load following feed.');
            } finally {
                setLoading(false);
            }
        };

        fetchFollowingFeed();
    }, [isAuthenticated, user]);

    if (!isAuthenticated) {
        return null;
    }

    return (
        <div className="bg-white shadow rounded-lg overflow-visible mb-8">
            <div className="px-3 py-3 sm:px-6 sm:py-4 bg-gradient-to-r from-primary-600 to-primary-700 flex justify-between items-center">
                <h2 className="text-lg sm:text-xl font-bold text-white flex items-center">
                    <UserRoundPlus className="mr-2 h-4 sm:h-5 w-4 sm:w-5" />
                    Following Feed
                </h2>
                <Link
                    to="/following-feed"
                    className="text-white hover:text-primary-100 flex items-center text-xs sm:text-sm font-medium"
                    aria-label="View all interactions from people you follow"
                >
                    View all <ArrowRight className="ml-1 h-3 sm:h-4 w-3 sm:w-4" />
                </Link>
            </div>

            {loading ? (
                <div className="p-6 sm:p-8 flex justify-center">
                    <RefreshCcw className="h-8 w-8 text-primary-500 animate-spin" />
                </div>
            ) : interactions.length === 0 || error ? (
                <div className="p-6 sm:p-8 text-center">
                    <p className="text-gray-500">People you follow haven't interacted with any music.</p>
                    <Link
                        to="/people"
                        className="mt-4 inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700"
                    >
                        Discover People
                    </Link>
                </div>
            ) : (
                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-3 sm:gap-4 p-4 sm:p-6" role="list">
                    {interactions.map((item) => (
                        <Link
                            key={item.interaction.aggregateId}
                            to={`/interaction/${item.interaction.aggregateId}`}
                            className="relative group bg-white rounded-lg overflow-visible shadow-sm hover:shadow-md transition-shadow duration-200"
                            role="listitem"
                        >
                            {/* Tooltip */}
                            <div
                                className="absolute bottom-full left-1/2 transform -translate-x-1/2 mb-1 opacity-0 group-hover:opacity-100 pointer-events-none bg-white text-black text-xs rounded border border-gray-200 shadow z-10 whitespace-nowrap p-1"
                            >
                                {item.userProfile ? (
                                    <>
                                        {item.userProfile.name} listened to {item.interaction.itemType.toLowerCase()}{' '}
                                        <strong>{item.catalogItem?.name}</strong>
                                    </>
                                ) : (
                                    <>
                                        Someone listened to {item.interaction.itemType.toLowerCase()}{' '}
                                        <strong>{item.catalogItem?.name}</strong>
                                    </>
                                )}

                            </div>

                            <div className="aspect-square w-full overflow-hidden rounded-t-lg">
                                <img
                                    src={item.catalogItem?.imageUrl || '/placeholder-album.jpg'}
                                    alt={`${item.catalogItem?.name || 'Unknown'} by ${item.catalogItem?.artistName || 'Unknown Artist'}`}
                                    className="w-full h-full object-cover"
                                />
                            </div>
                            <div className="p-1.5">

                                {/* User info */}
                                <div className="flex items-center mb-1">
                                    {item.userProfile?.avatarUrl ? (
                                        <img
                                            src={item.userProfile.avatarUrl}
                                            alt={`${item.userProfile.name} ${item.userProfile.surname}`}
                                            className="w-7 h-7 rounded-full mr-2 object-cover"
                                        />
                                    ) : (
                                        <div
                                            className="w-7 h-7 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-xs font-bold mr-2">
                                            {item.userProfile?.name.charAt(0) || '?'}{item.userProfile?.surname.charAt(0) || ''}
                                        </div>
                                    )}
                                    <div className="truncate text-sm leading-tight">
                                        {item.userProfile ? `${item.userProfile.name} ${item.userProfile.surname}` : 'Unknown User'}
                                    </div>
                                </div>

                                {/* Interaction details */}
                                <div className="flex items-center text-xs">
                                    {item.interaction.rating && (
                                        <div className="flex items-center">
                                            <NormalizedStarDisplay
                                                currentGrade={item.interaction.rating.normalizedGrade}
                                                minGrade={1}
                                                maxGrade={10}
                                            />

                                            {item.interaction.rating.isComplex && (
                                                <SlidersHorizontal className="h-4 w-4 ml-1 text-primary-500"/>
                                            )}
                                        </div>
                                    )}

                                    {item.interaction.isLiked &&
                                        <Heart className="h-4 w-4 ml-1 text-red-500 fill-red-500"/>}
                                    {item.interaction.review &&
                                        <MessageSquare className="h-4 w-4 ml-1 text-primary-600"/>}
                                </div>

                                <div className="flex items-center mt-1 text-xs text-gray-500">
                                    <Calendar className="h-3 w-3 mr-1"/>
                                    <span>{formatDate(item.interaction.createdAt)}</span>
                                </div>
                            </div>
                        </Link>
                    ))}
                </div>
            )}
        </div>
    );
};

export default FollowingFeedComponent;
