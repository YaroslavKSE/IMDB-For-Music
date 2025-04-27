import { useState, useEffect } from 'react';
import { Loader, RefreshCw } from 'lucide-react';
import InteractionService from '../../api/interaction';
import CatalogService from '../../api/catalog';
import { DiaryEntry, GroupedEntries } from '../Diary/types';
import DiaryDateGroup from '../Diary/DiaryDateGroup';
import ReviewModal from "../Diary/ReviewModal";
import useAuthStore from '../../store/authStore';

interface ItemHistoryComponentProps {
    itemId: string;
    itemType: 'Album' | 'Track';
}

const ItemHistoryComponent = ({ itemId, itemType }: ItemHistoryComponentProps) => {
    const { user, isAuthenticated } = useAuthStore();
    const [loading, setLoading] = useState(true);
    const [loadingMore, setLoadingMore] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [diaryEntries, setDiaryEntries] = useState<DiaryEntry[]>([]);
    const [groupedEntries, setGroupedEntries] = useState<GroupedEntries[]>([]);
    const [offset, setOffset] = useState(0);
    const [, setTotalInteractions] = useState(0);
    const [reviewModalOpen, setReviewModalOpen] = useState(false);
    const [selectedReview, setSelectedReview] = useState<{
        review: { reviewId: string; reviewText: string };
        itemName: string;
        artistName: string;
        date: string;
    } | null>(null);
    const [hasMore, setHasMore] = useState(true);
    const itemsPerPage = 10;

    // Load the user's history for this item
    useEffect(() => {
        const loadItemHistory = async () => {
            if (!isAuthenticated || !user || !itemId) return;

            setLoading(true);
            setError(null);

            try {
                // Fetch interactions for this user and item
                const { items: interactions, totalCount } =
                    await InteractionService.getUserItemHistory(user.id, itemId, itemsPerPage, 0);

                setTotalInteractions(totalCount);
                setHasMore(totalCount > itemsPerPage);

                if (totalCount === 0) {
                    setLoading(false);
                    return;
                }

                // Use the same approach as the Diary page to construct entries
                // Extract all item ids for preview info
                const itemIds: string[] = interactions.map(interaction => interaction.itemId);

                // Fetch preview information for all items in a single request
                const previewResponse = await CatalogService.getItemPreviewInfo(itemIds, [itemType.toLowerCase()]);

                // Create lookup maps for quick access
                const itemsMap = new Map();

                // Process results from the preview response
                previewResponse.results?.forEach(resultGroup => {
                    resultGroup.items?.forEach(item => {
                        // Create a simplified catalog item with the preview information
                        const catalogItem = {
                            spotifyId: item.spotifyId,
                            name: item.name,
                            imageUrl: item.imageUrl,
                            artistName: item.artistName
                        };

                        itemsMap.set(item.spotifyId, catalogItem);
                    });
                });

                // Combine interactions with catalog items
                const entries = interactions.map(interaction => {
                    const entry: DiaryEntry = { interaction };
                    entry.catalogItem = itemsMap.get(interaction.itemId);
                    return entry;
                });

                setDiaryEntries(entries);
                setOffset(interactions.length);
            } catch (err) {
                console.error('Error loading item history:', err);
                setError('Failed to load your history for this item. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        loadItemHistory();
    }, [user, itemId, itemType, isAuthenticated]);

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

            // Extract all item ids for preview info
            const itemIds: string[] = interactions.map(interaction => interaction.itemId);

            // Fetch preview information for all items in a single request
            const previewResponse = await CatalogService.getItemPreviewInfo(itemIds, [itemType.toLowerCase()]);

            // Create lookup maps for quick access
            const itemsMap = new Map();

            // Process results from the preview response
            previewResponse.results?.forEach(resultGroup => {
                resultGroup.items?.forEach(item => {
                    // Create a simplified catalog item with the preview information
                    const catalogItem = {
                        spotifyId: item.spotifyId,
                        name: item.name,
                        imageUrl: item.imageUrl,
                        artistName: item.artistName
                    };

                    itemsMap.set(item.spotifyId, catalogItem);
                });
            });

            // Combine interactions with catalog items
            const newEntries = interactions.map(interaction => {
                const entry: DiaryEntry = { interaction };
                entry.catalogItem = itemsMap.get(interaction.itemId);
                return entry;
            });

            // Update hasMore status based on whether we've reached the total count
            const newOffset = currentOffset + interactions.length;
            const newHasMore = newOffset < totalCount;
            setHasMore(newHasMore);

            // Append new entries to existing entries
            setDiaryEntries(prevEntries => [...prevEntries, ...newEntries]);
            setOffset(newOffset);
        } catch (err) {
            console.error('Error loading more history entries:', err);
            setError('Failed to load more entries. Please try again later.');
        } finally {
            setLoadingMore(false);
        }
    };

    // Group entries by date whenever diary entries change
    useEffect(() => {
        if (diaryEntries.length === 0) return;

        // Group entries by date
        const grouped: Record<string, DiaryEntry[]> = {};
        diaryEntries.forEach(entry => {
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
        const result: GroupedEntries[] = Object.keys(grouped)
            .map(date => ({ date, entries: grouped[date] }))
            .sort((a, b) => new Date(b.entries[0].interaction.createdAt).getTime() -
                new Date(a.entries[0].interaction.createdAt).getTime());

        setGroupedEntries(result);
    }, [diaryEntries]);

    const handleReviewClick = (e: React.MouseEvent, entry: DiaryEntry) => {
        e.stopPropagation(); // Prevent triggering the row click

        if (!entry.interaction.review) return;

        const formattedDate = new Date(entry.interaction.createdAt).toLocaleString('en-US', {
            year: 'numeric',
            month: 'long',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });

        setSelectedReview({
            review: entry.interaction.review,
            itemName: entry.catalogItem?.name || 'Unknown Title',
            artistName: entry.catalogItem?.artistName || 'Unknown Artist',
            date: formattedDate
        });

        setReviewModalOpen(true);
    };

    // We omit delete functionality for item history view
    const handleDeleteClick = (e: React.MouseEvent, entry: DiaryEntry) => {
        e.stopPropagation(); // Prevent triggering the row click
        // For history tab, we'll navigate to the interaction detail page instead of offering delete
        window.location.href = `/interaction/${entry.interaction.aggregateId}`;
    };

    if (loading && diaryEntries.length === 0) {
        return (
            <div className="flex flex-col items-center justify-center py-8">
                <RefreshCw className="h-8 w-8 text-primary-600 animate-spin mb-4" />
                <div className="text-gray-600">Loading your history...</div>
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

    if (diaryEntries.length === 0) {
        return (
            <div className="text-center py-8 text-gray-500">
                You haven't interacted with this {itemType.toLowerCase()} yet.
            </div>
        );
    }

    return (
        <div className="space-y-6">
            {/* Diary entries by date */}
            <div className="space-y-6">
                {groupedEntries.map((group) => (
                    <DiaryDateGroup
                        key={group.date}
                        group={group}
                        onReviewClick={handleReviewClick}
                        onDeleteClick={handleDeleteClick}
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
            {/* Review Modal */}
            {selectedReview && (
                <ReviewModal
                    isOpen={reviewModalOpen}
                    onClose={() => setReviewModalOpen(false)}
                    review={selectedReview.review}
                    itemName={selectedReview.itemName}
                    artistName={selectedReview.artistName}
                    date={selectedReview.date}
                />
            )}
        </div>
    );
};

export default ItemHistoryComponent;