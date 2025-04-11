import { useState, useEffect, useCallback } from 'react';
import { useNavigate } from 'react-router-dom';
import useAuthStore from '../store/authStore';
import InteractionService from '../api/interaction';
import CatalogService from '../api/catalog';
import ReviewModal from "../components/Diary/ReviewModal.tsx";
import { DiaryEntry, GroupedEntries } from '../components/Diary/types';
import { DiaryLoadingState, DiaryErrorState, DiaryEmptyState } from '../components/Diary/DiaryStates';
import DiaryDateGroup from '../components/Diary/DiaryDateGroup';
import DiaryPagination from '../components/Diary/DiaryPagination';
import { AlertTriangle } from 'lucide-react';

const Diary = () => {
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuthStore();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [diaryEntries, setDiaryEntries] = useState<DiaryEntry[]>([]);
    const [groupedEntries, setGroupedEntries] = useState<GroupedEntries[]>([]);
    const [currentPage, setCurrentPage] = useState(1);
    const [totalPages, setTotalPages] = useState(1);
    const [totalInteractions, setTotalInteractions] = useState(0);
    const [reviewModalOpen, setReviewModalOpen] = useState(false);
    const [selectedReview, setSelectedReview] = useState<{
        review: { reviewId: string; reviewText: string };
        itemName: string;
        artistName: string;
        date: string;
    } | null>(null);
    const [deleteModalOpen, setDeleteModalOpen] = useState(false);
    const [entryToDelete, setEntryToDelete] = useState<DiaryEntry | null>(null);
    const [deleteSuccess, setDeleteSuccess] = useState(false);
    const [noInteractions, setNoInteractions] = useState(false);
    const itemsPerPage = 20;

    // Wrap loadDiaryEntries with useCallback
    const loadDiaryEntries = useCallback(async () => {
        if (!user) return;

        setLoading(true);
        setError(null);
        setNoInteractions(false);

        try {
            // Fetch all interactions for the user to get the total count
            const interactions = await InteractionService.getUserInteractionsByUserId(user.id);

            // Calculate total pages and set total interactions
            const total = interactions.length;
            setTotalInteractions(total);
            const pages = Math.ceil(total / itemsPerPage);
            setTotalPages(pages || 1);

            if (total === 0) {
                setNoInteractions(true);
                setLoading(false);
                return;
            }

            // Sort interactions by date (newest first)
            const sortedInteractions = [...interactions].sort((a, b) =>
                new Date(b.createdAt).getTime() - new Date(a.createdAt).getTime()
            );

            // Get paginated interactions for current page
            const startIdx = (currentPage - 1) * itemsPerPage;
            const endIdx = startIdx + itemsPerPage;
            const paginatedInteractions = sortedInteractions.slice(startIdx, endIdx);

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
        } catch (err: unknown) {
            console.error('Error loading diary entries:', err);
            if (err && typeof err === 'object' && 'response' in err &&
                err.response && typeof err.response === 'object' && 'status' in err.response &&
                err.response.status === 404) {
                setNoInteractions(true);
            } else {
                setError('Failed to load your diary entries. Please try again later.');
            }
        } finally {
            setLoading(false);
        }
    }, [user, currentPage, itemsPerPage]);

    // Load diary entries
    useEffect(() => {
        if (!isAuthenticated || !user) {
            navigate('/login', { state: { from: '/diary' } });
            return;
        }

        loadDiaryEntries();
    }, [isAuthenticated, user, navigate, currentPage, loadDiaryEntries]); // Added currentPage as dependency

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

    // Show success message briefly
    useEffect(() => {
        if (deleteSuccess) {
            const timer = setTimeout(() => {
                setDeleteSuccess(false);
            }, 3000);
            return () => clearTimeout(timer);
        }
    }, [deleteSuccess]);

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

    const handleDeleteClick = (e: React.MouseEvent, entry: DiaryEntry) => {
        e.stopPropagation(); // Prevent triggering the row click
        setEntryToDelete(entry);
        setDeleteModalOpen(true);
    };

    const confirmDelete = async () => {
        if (!entryToDelete) return;

        try {
            await InteractionService.deleteInteraction(entryToDelete.interaction.aggregateId);

            // Remove the deleted entry from the diary entries
            const updatedEntries = diaryEntries.filter(
                entry => entry.interaction.aggregateId !== entryToDelete.interaction.aggregateId
            );
            setDiaryEntries(updatedEntries);

            // Update total interactions count
            setTotalInteractions(prev => prev - 1);

            // Update total pages
            const newTotalPages = Math.ceil((totalInteractions - 1) / itemsPerPage);
            setTotalPages(newTotalPages || 1);

            // Adjust current page if necessary
            if (currentPage > newTotalPages && newTotalPages > 0) {
                setCurrentPage(newTotalPages);
            }

            // Show success message
            setDeleteSuccess(true);
        } catch (err: unknown) {
            console.error('Error deleting entry:', err);
            setError('Failed to delete the entry. Please try again.');
        } finally {
            setDeleteModalOpen(false);
            setEntryToDelete(null);
        }
    };

    const handlePageChange = (page: number) => {
        if (page < 1 || page > totalPages) return;
        setCurrentPage(page);
        // When page changes, we'll scroll to top for better UX
        window.scrollTo(0, 0);
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

            {/* Success notification */}
            {deleteSuccess && (
                <div className="fixed top-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded z-50 shadow-md">
                    Entry has been deleted successfully!
                </div>
            )}

            {(groupedEntries.length === 0 && !loading && !error) || noInteractions ? (
                <DiaryEmptyState />
            ) : (
                <>
                    {totalInteractions > 0 && (
                        <div className="mb-4 text-sm text-gray-600">
                            Showing {Math.min((currentPage - 1) * itemsPerPage + 1, totalInteractions)} to {Math.min(currentPage * itemsPerPage, totalInteractions)} of {totalInteractions} total entries
                        </div>
                    )}

                    {/* Diary entries by date */}
                    <div className="space-y-8">
                        {groupedEntries.map((group) => (
                            <DiaryDateGroup
                                key={group.date}
                                group={group}
                                onReviewClick={handleReviewClick}
                                onDeleteClick={handleDeleteClick}
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

            {/* Delete Confirmation Modal */}
            {deleteModalOpen && entryToDelete && (
                <div className="fixed inset-0 z-50 overflow-y-auto">
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
                                        onClick={() => setDeleteModalOpen(false)}
                                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50"
                                    >
                                        Cancel
                                    </button>
                                    <button
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

export default Diary;