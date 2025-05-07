import { useState, useEffect } from 'react';
import {Loader, RefreshCw, History, AlertTriangle} from 'lucide-react';
import InteractionService from '../../api/interaction';
import CatalogService from '../../api/catalog';
import UsersService from '../../api/users';
import { ItemHistoryEntry, GroupedHistoryEntries } from '../ItemHistory/ItemHistoryTypes';
import HistoryDateGroup from '../ItemHistory/HistoryDateGroup';
import useAuthStore from '../../store/authStore';
import EmptyState from "./EmptyState.tsx";

interface ItemHistoryComponentProps {
    itemId: string;
    itemType: 'Album' | 'Track';
    onLogInteraction?: () => void;
    refreshTrigger?: number; // New prop to trigger refresh
}

const ItemHistoryComponent = ({
                                  itemId,
                                  itemType,
                                  onLogInteraction,
                                  refreshTrigger = 0 // Default value to avoid undefined
                              }: ItemHistoryComponentProps) => {
    const { user, isAuthenticated } = useAuthStore();
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [historyEntries, setHistoryEntries] = useState<ItemHistoryEntry[]>([]);
    const [groupedEntries, setGroupedEntries] = useState<GroupedHistoryEntries[]>([]);
    const [offset, setOffset] = useState(0);
    const [, setTotalInteractions] = useState(0);
    const [hasMore, setHasMore] = useState(true);
    const itemsPerPage = 10;

    // New states for delete functionality
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [entryToDelete, setEntryToDelete] = useState<ItemHistoryEntry | null>(null);
    const [deleteSuccess, setDeleteSuccess] = useState(false);

    // Load item interactions history
    useEffect(() => {
        const loadItemHistory = async () => {
            if (!isAuthenticated || !user || !itemId) return;

            setLoading(true);
            setError(null);

            try {
                // Fetch user's interactions with this item
                const { items: interactions, totalCount } =
                    await InteractionService.getUserItemHistory(user.id, itemId, itemsPerPage, 0);

                setTotalInteractions(totalCount);
                setHasMore(totalCount > itemsPerPage);

                if (totalCount === 0) {
                    setLoading(false);
                    return;
                }

                // No need to fetch user profiles since we're showing the current user's history
                // But we'll still create a map with the current user's profile
                const userProfiles = new Map();
                if (user) {
                    try {
                        const profile = await UsersService.getUserProfileById(user.id);
                        userProfiles.set(user.id, profile);
                    } catch (error) {
                        console.error(`Failed to fetch profile for user ${user.id}:`, error);
                    }
                }

                // Fetch catalog item preview
                const itemIds: string[] = interactions.map(interaction => interaction.itemId);
                const previewResponse = await CatalogService.getItemPreviewInfo(itemIds, [itemType.toLowerCase()]);
                const itemsMap = new Map();

                previewResponse.results?.forEach(resultGroup => {
                    resultGroup.items?.forEach(item => {
                        const catalogItem = {
                            spotifyId: item.spotifyId,
                            name: item.name,
                            imageUrl: item.imageUrl,
                            artistName: item.artistName
                        };
                        itemsMap.set(item.spotifyId, catalogItem);
                    });
                });

                // Combine interactions with user profiles and catalog items
                const entries = interactions.map(interaction => {
                    const entry: ItemHistoryEntry = {
                        interaction,
                        userProfile: userProfiles.get(interaction.userId),
                        catalogItem: itemsMap.get(interaction.itemId)
                    };
                    return entry;
                });

                setHistoryEntries(entries);
                setOffset(interactions.length);
            } catch (err) {
                console.error('Error loading item history:', err);
            } finally {
                setLoading(false);
            }
        };

        loadItemHistory();
    }, [user, itemId, itemType, isAuthenticated, refreshTrigger]); // Added refreshTrigger dependency

    // Load more history entries
    const loadMoreHistory = async () => {
        if (!isAuthenticated || !user || !itemId || loadingMore || !hasMore) return;

        const currentOffset = offset;
        setLoadingMore(true);

        try {
            // Fetch additional interactions
            const { items: interactions, totalCount } =
                await InteractionService.getUserItemHistory(user.id, itemId, itemsPerPage, currentOffset);

            if (interactions.length === 0) {
                setHasMore(false);
                setLoadingMore(false);
                return;
            }

            // Fetch catalog item preview
            const itemIds: string[] = interactions.map(interaction => interaction.itemId);
            const previewResponse = await CatalogService.getItemPreviewInfo(itemIds, [itemType.toLowerCase()]);
            const itemsMap = new Map();

            previewResponse.results?.forEach(resultGroup => {
                resultGroup.items?.forEach(item => {
                    const catalogItem = {
                        spotifyId: item.spotifyId,
                        name: item.name,
                        imageUrl: item.imageUrl,
                        artistName: item.artistName
                    };
                    itemsMap.set(item.spotifyId, catalogItem);
                });
            });

            // Use the same user profile as before since it's all for the current user
            const newEntries = interactions.map(interaction => {
                const entry: ItemHistoryEntry = {
                    interaction,
                    userProfile: user ? {
                        id: user.id,
                        username: user.username || '',
                        name: user.name,
                        surname: user.surname,
                        avatarUrl: user.avatarUrl,
                        followerCount: 0,
                        followingCount: 0,
                        createdAt: ''
                    } : undefined,
                    catalogItem: itemsMap.get(interaction.itemId)
                };
                return entry;
            });

            // Update hasMore status based on whether we've reached the total count
            const newOffset = currentOffset + interactions.length;
            const newHasMore = newOffset < totalCount;
            setHasMore(newHasMore);

            // Append new entries to existing entries
            setHistoryEntries(prevEntries => [...prevEntries, ...newEntries]);
            setOffset(newOffset);
        } catch (err) {
            console.error('Error loading more history entries:', err);
        } finally {
            setLoadingMore(false);
        }
    };

    // Group entries by date whenever history entries change
    useEffect(() => {
        if (historyEntries.length === 0) return;

        // Group entries by date
        const grouped: Record<string, ItemHistoryEntry[]> = {};
        historyEntries.forEach(entry => {
            const date = new Date(entry.interaction.createdAt).toLocaleDateString('en-US', {
                year: 'numeric',
                month: 'long',
                day: 'numeric'
            });

            if (!grouped[date]) {
                grouped[date] = [];
            }
            grouped[date].push(entry);
        });

        // Convert to array sorted by date (newest first)
        const result: GroupedHistoryEntries[] = Object.keys(grouped)
            .map(date => ({ date, entries: grouped[date] }))
            .sort((a, b) => new Date(b.entries[0].interaction.createdAt).getTime() -
                new Date(a.entries[0].interaction.createdAt).getTime());

        setGroupedEntries(result);
    }, [historyEntries]);

    // Show success message briefly
    useEffect(() => {
        if (deleteSuccess) {
            const timer = setTimeout(() => {
                setDeleteSuccess(false);
            }, 3000);
            return () => clearTimeout(timer);
        }
    }, [deleteSuccess]);

    // Handle delete click - prepare for deletion
    const handleDeleteClick = (e: React.MouseEvent, entry: ItemHistoryEntry) => {
        e.stopPropagation(); // Prevent triggering the row click
        setEntryToDelete(entry);
        setDeleteModalOpen(true);
    };

    // Confirm deletion of entry
    const confirmDelete = async () => {
        if (!entryToDelete) return;

        try {
            await InteractionService.deleteInteraction(entryToDelete.interaction.aggregateId);

            // Remove the deleted entry from the history entries
            const updatedEntries = historyEntries.filter(
                entry => entry.interaction.aggregateId !== entryToDelete.interaction.aggregateId
            );
            setHistoryEntries(updatedEntries);

            // Update total interactions count
            setTotalInteractions(prev => prev - 1);

            // Show success message
            setDeleteSuccess(true);
        } catch (err) {
            console.error('Error deleting entry:', err);
            setError('Failed to delete the entry. Please try again.');
        } finally {
            setDeleteModalOpen(false);
            setEntryToDelete(null);
        }
    };

    if (loading && historyEntries.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-8">
                <RefreshCw className="h-8 w-8 text-primary-600 animate-spin mb-4" />
                <div className="text-gray-600">Loading history...</div>
            </div>
        );
    }

    if (error) {
        return (
            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6">
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

    if (historyEntries.length === 0) {
        return (
            <EmptyState
                title="No history"
                message={`You haven't interacted with this ${itemType.toLowerCase()}.`}
                icon={<History className="h-12 w-12 text-gray-400" />}
                action={onLogInteraction ? {
                    label: "Write a Review",
                    onClick: onLogInteraction
                } : undefined}
            />
        );
    }

    return (
        <div className="space-y-6">
            {/* Success notification */}
            {deleteSuccess && (
                <div className="fixed top-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded z-50 shadow-md">
                    Entry has been deleted successfully!
                </div>
            )}

            {/* History entries by date */}
            <div className="space-y-6">
                {groupedEntries.map((group) => (
                    <HistoryDateGroup
                        key={group.date}
                        group={group}
                        onDeleteClick={handleDeleteClick}
                        isPublic={false}
                    />
                ))}
            </div>

            {/* Loading more indicator */}
            {loadingMore && (
                <div className="flex items-center justify-center py-4">
                    <Loader className="h-5 w-5 text-primary-600 animate-spin mr-2" />
                    <span className="text-gray-600">Loading more entries...</span>
                </div>
            )}

            {/* Load more button - only show if hasMore and not currently loading */}
            {hasMore && !loadingMore && (
                <div className="flex justify-center pt-2">
                    <button
                        onClick={loadMoreHistory}
                        className="px-4 py-2 bg-white border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
                    >
                        Load More
                    </button>
                </div>
            )}

            {/* Delete Confirmation Modal */}
            {deleteModalOpen && entryToDelete && (
                <div className="fixed inset-0 z-50">
                    <div className="flex items-center justify-center min-h-screen p-4">
                        {/* Backdrop */}
                        <div
                            className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
                            onClick={() => setDeleteModalOpen(false)}
                        ></div>

                        {/* Modal */}
                        <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full z-10">
                            <div className="p-6">
                                <div className="flex items-center mb-4">
                                    <AlertTriangle className="h-8 w-8 text-red-500 mr-4" />
                                    <h3 className="text-lg font-bold text-gray-900">Delete Entry</h3>
                                </div>

                                <p className="mb-4">
                                    Are you sure you want to delete this entry for "{entryToDelete.catalogItem?.name || 'Unknown Title'}"?
                                    This action cannot be undone.
                                </p>

                                <div className="flex justify-end space-x-3 mt-6">
                                    <button
                                        type="button"
                                        onClick={() => setDeleteModalOpen(false)}
                                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                                    >
                                        Cancel
                                    </button>
                                    <button
                                        type="button"
                                        onClick={confirmDelete}
                                        className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-red-600 hover:bg-red-700"
                                    >
                                        Delete
                                    </button>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            )}
        </div>
    );
};

export default ItemHistoryComponent;