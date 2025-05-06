import { useState, useEffect } from 'react';
import { ListMusic, RefreshCw, Loader } from 'lucide-react';
import { ListOverview } from '../../api/lists.ts';
import ListsService from '../../api/lists.ts';
import UsersService, { PublicUserProfile } from '../../api/users.ts';
import ListsTabRow from './ListTabRow.tsx';
import EmptyState from './EmptyState.tsx';

interface InListsTabProps {
    spotifyId: string;
}

const InListsTab = ({ spotifyId }: InListsTabProps) => {
    const [lists, setLists] = useState<ListOverview[]>([]);
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [hasMore, setHasMore] = useState(false);
    const [offset, setOffset] = useState(0);
    const [totalLists, setTotalLists] = useState(0);
    const [error, setError] = useState<string | null>(null);
    const [userProfiles, setUserProfiles] = useState<Map<string, PublicUserProfile>>(new Map());

    const ITEMS_PER_PAGE = 10;

    // Fetch lists containing this item
    useEffect(() => {
        const fetchLists = async () => {
            if (!spotifyId) return;

            setLoading(true);
            setError(null);

            try {
                const response = await ListsService.getListsBySpotifyId(spotifyId, ITEMS_PER_PAGE, 0);

                setLists(response.lists);
                setTotalLists(response.totalCount);
                setOffset(response.lists.length);
                setHasMore(response.totalCount > response.lists.length);

                // Fetch user profiles for list creators
                if (response.lists.length > 0) {
                    const userIds = [...new Set(response.lists.map(list => list.userId))];

                    try {
                        const profiles = await UsersService.getUserProfilesBatch(userIds);
                        const profilesMap = new Map<string, PublicUserProfile>();

                        profiles.forEach(profile => {
                            profilesMap.set(profile.id, profile);
                        });

                        setUserProfiles(profilesMap);
                    } catch (error) {
                        console.error('Error fetching user profiles:', error);
                    }
                }
            } catch (err) {
                console.error('Error fetching lists:', err);
                setError('Failed to load lists containing this album.');
            } finally {
                setLoading(false);
            }
        };

        fetchLists();
    }, [spotifyId]);

    // Load more lists
    const loadMoreLists = async () => {
        if (loadingMore || !hasMore) return;

        setLoadingMore(true);

        try {
            const response = await ListsService.getListsBySpotifyId(
                spotifyId,
                ITEMS_PER_PAGE,
                offset
            );

            const newLists = response.lists;
            setLists(prev => [...prev, ...newLists]);
            setOffset(prev => prev + newLists.length);
            setHasMore(offset + newLists.length < response.totalCount);

            // Fetch profiles for new list creators
            if (newLists.length > 0) {
                const newUserIds = [...new Set(newLists.map(list => list.userId))];
                const idsToFetch = newUserIds.filter(id => !userProfiles.has(id));

                if (idsToFetch.length > 0) {
                    try {
                        const profiles = await UsersService.getUserProfilesBatch(idsToFetch);

                        setUserProfiles(prev => {
                            const updated = new Map(prev);
                            profiles.forEach(profile => {
                                updated.set(profile.id, profile);
                            });
                            return updated;
                        });
                    } catch (error) {
                        console.error('Error fetching user profiles:', error);
                    }
                }
            }
        } catch (err) {
            console.error('Error loading more lists:', err);
            setError('Failed to load more lists.');
        } finally {
            setLoadingMore(false);
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center py-10">
                <RefreshCw className="h-8 w-8 text-primary-600 animate-spin mr-3" />
                <span className="text-lg text-gray-600">Loading lists...</span>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
                {error}
                <button
                    onClick={() => window.location.reload()}
                    className="ml-2 underline hover:text-red-900"
                >
                    Try again
                </button>
            </div>
        );
    }

    if (lists.length === 0) {
        return (
            <EmptyState
                title="Not in any lists yet"
                message="This album hasn't been added to any lists yet."
                icon={<ListMusic className="h-12 w-12 text-gray-400" />}
            />
        );
    }

    return (
        <div className="space-y-2">
            <div className="mb-4 text-gray-600">
                <span>Found in <span className="font-medium">{totalLists}</span> {totalLists === 1 ? 'list' : 'lists'}</span>
            </div>

            {/* Lists */}
            <div className="space-y-2">
                {lists.map((list) => {
                    const profile = userProfiles.get(list.userId);
                    return (
                        <ListsTabRow
                            key={list.listId}
                            list={list}
                            userAvatar={profile?.avatarUrl}
                            userName={profile?.name || 'Unknown'}
                            userSurname={profile?.surname}
                            userId={list.userId}
                        />
                    );
                })}
            </div>

            {/* Load more */}
            {hasMore && (
                <div className="mt-6 text-center">
                    <button
                        onClick={loadMoreLists}
                        disabled={loadingMore}
                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
                    >
                        {loadingMore ? (
                            <div className="flex items-center">
                                <Loader className="h-5 w-5 text-primary-600 animate-spin mr-2" />
                                <span>Loading more...</span>
                            </div>
                        ) : (
                            <span>Load More ({lists.length} of {totalLists})</span>
                        )}
                    </button>
                </div>
            )}
        </div>
    );
};

export default InListsTab;