import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import useAuthStore from '../store/authStore';
import InteractionService from '../api/interaction';
import CatalogService from '../api/catalog';
import ReviewModal from "../components/Diary/ReviewModal.tsx";
import { DiaryEntry, GroupedEntries } from '../components/Diary/types';
import { DiaryLoadingState, DiaryErrorState, DiaryEmptyState } from '../components/Diary/DiaryStates';
import DiaryDateGroup from '../components/Diary/DiaryDateGroup';
import DiaryPagination from '../components/Diary/DiaryPagination';

const Diary = () => {
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuthStore();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [diaryEntries, setDiaryEntries] = useState<DiaryEntry[]>([]);
    const [groupedEntries, setGroupedEntries] = useState<GroupedEntries[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [reviewModalOpen, setReviewModalOpen] = useState(false);
    const [selectedReview, setSelectedReview] = useState<{
        review: { reviewId: string; reviewText: string };
        itemName: string;
        artistName: string;
        date: string;
    } | null>(null);
    const itemsPerPage = 20;

    // Load diary entries
    useEffect(() => {
        if (!isAuthenticated || !user) {
            navigate('/login', { state: { from: '/diary' } });
            return;
        }

        loadDiaryEntries();
    }, );

    // Group entries by date whenever diary entries change
    useEffect(() => {
        if (diaryEntries.length === 0) return;

        // Sort all entries by timestamp first (newest first)
        const sortedEntries = [...diaryEntries].sort((a, b) =>
            new Date(b.interaction.createdAt).getTime() - new Date(a.interaction.createdAt).getTime()
        );

        // Group entries by date
        const grouped: Record<string, DiaryEntry[]> = {};
        sortedEntries.forEach(entry => {
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

    const loadDiaryEntries = async () => {
        if (!user) return;

        setLoading(true);
        setError(null);

        try {
            // Fetch all interactions for the user
            const interactions = await InteractionService.getUserInteractionsByUserId(user.id);

            // Calculate total pages
            const total = Math.ceil(interactions.length / itemsPerPage);
            setTotalPages(total || 1);

            // Get paginated interactions
            const startIdx = (currentPage - 1) * itemsPerPage;
            const endIdx = startIdx + itemsPerPage;
            const paginatedInteractions = interactions.slice(startIdx, endIdx);

            // Extract album and track ids
            const albumIds: string[] = [];
            const trackIds: string[] = [];

            paginatedInteractions.forEach(interaction => {
                if (interaction.itemType === 'Album') {
                    albumIds.push(interaction.itemId);
                } else if (interaction.itemType === 'Track') {
                    trackIds.push(interaction.itemId);
                }
            });

            // Fetch album and track details in batches
            const [albumsResponse, tracksResponse] = await Promise.all([
                albumIds.length > 0 ? CatalogService.getBatchAlbums(albumIds) : { albums: [] },
                trackIds.length > 0 ? CatalogService.getBatchTracks(trackIds) : { tracks: [] }
            ]);

            // Create lookup maps for quick access
            const albumsMap = new Map();
            const tracksMap = new Map();

            albumsResponse.albums?.forEach(album => {
                albumsMap.set(album.spotifyId, album);
            });

            tracksResponse.tracks?.forEach(track => {
                tracksMap.set(track.spotifyId, track);
            });

            // Combine interactions with catalog items
            const entries = paginatedInteractions.map(interaction => {
                const entry: DiaryEntry = { interaction };

                if (interaction.itemType === 'Album') {
                    entry.catalogItem = albumsMap.get(interaction.itemId);
                } else if (interaction.itemType === 'Track') {
                    entry.catalogItem = tracksMap.get(interaction.itemId);
                }

                return entry;
            });

            setDiaryEntries(entries);
        } catch (err) {
            console.error('Error loading diary entries:', err);
            setError('Failed to load your diary entries. Please try again later.');
        } finally {
            setLoading(false);
        }
    };

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

    const handlePageChange = (page: number) => {
        if (page < 1 || page > totalPages) return;
        setCurrentPage(page);
    };

    // Show loading state
    if (loading && diaryEntries.length === 0) {
        return <DiaryLoadingState />;
    }

    return (
        <div className="max-w-6xl mx-auto py-8">
            <div className="mb-6">
                <h1 className="text-3xl font-bold text-gray-900 mb-2">Your Music Diary</h1>
                <p className="text-gray-600">A chronological record of your music experiences and thoughts</p>
            </div>

            {error && <DiaryErrorState error={error} onRetry={loadDiaryEntries} />}

            {groupedEntries.length === 0 && !loading && !error ? (
                <DiaryEmptyState />
            ) : (
                <>
                    {/* Diary entries by date */}
                    <div className="space-y-8">
                        {groupedEntries.map((group) => (
                            <DiaryDateGroup
                                key={group.date}
                                group={group}
                                onReviewClick={handleReviewClick}
                            />
                        ))}
                    </div>

                    {/* Pagination */}
                    <DiaryPagination
                        currentPage={currentPage}
                        totalPages={totalPages}
                        onPageChange={handlePageChange}
                    />
                </>
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

export default Diary;