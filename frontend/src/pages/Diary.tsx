import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Calendar, Music, Disc, Star, Heart, MessageSquare, SlidersHorizontal, ChevronLeft, ChevronRight, RefreshCw, X } from 'lucide-react';
import useAuthStore from '../store/authStore';
import InteractionService, { UserInteractionDetail } from '../api/interaction';
import CatalogService, { AlbumSummary, TrackSummary } from '../api/catalog';

// Type to represent a diary entry with catalog item details
interface DiaryEntry {
    interaction: UserInteractionDetail;
    catalogItem?: AlbumSummary | TrackSummary;
}

// Group diary entries by date
interface GroupedEntries {
    date: string;
    entries: DiaryEntry[];
}

// Review modal props
interface ReviewModalProps {
    isOpen: boolean;
    onClose: () => void;
    review: {
        reviewId: string;
        reviewText: string;
    };
    itemName: string;
    artistName: string;
    date: string;
}

// Review Modal Component
const ReviewModal = ({ isOpen, onClose, review, itemName, artistName, date }: ReviewModalProps) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen p-4">
                {/* Backdrop */}
                <div
                    className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
                    onClick={onClose}
                ></div>

                {/* Modal */}
                <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full z-10">
                    {/* Header */}
                    <div className="flex justify-between items-center p-4 border-b border-gray-200">
                        <h2 className="text-lg font-bold text-gray-900">Review</h2>
                        <button
                            onClick={onClose}
                            className="text-gray-400 hover:text-gray-500 focus:outline-none"
                        >
                            <X className="h-5 w-5" />
                        </button>
                    </div>

                    {/* Content */}
                    <div className="p-4">
                        <div className="mb-3">
                            <h3 className="font-medium text-gray-900">{itemName}</h3>
                            <p className="text-sm text-gray-600">{artistName}</p>
                            <p className="text-xs text-gray-500 mt-1">{date}</p>
                        </div>

                        <div className="bg-gray-50 p-4 rounded-md border border-gray-200">
                            <p className="text-gray-800 whitespace-pre-wrap">{review.reviewText}</p>
                        </div>
                    </div>

                    {/* Footer */}
                    <div className="p-4 border-t border-gray-200 flex justify-end">
                        <button
                            onClick={onClose}
                            className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

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
    }, [isAuthenticated, user, navigate, currentPage]);

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
                CatalogService.getBatchAlbums(albumIds),
                CatalogService.getBatchTracks(trackIds)
            ]);

            // Create lookup maps for quick access
            const albumsMap = new Map<string, AlbumSummary>();
            const tracksMap = new Map<string, TrackSummary>();

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

    const handleItemClick = (entry: DiaryEntry) => {
        // Navigate to the interaction detail page
        navigate(`/interaction/${entry.interaction.aggregateId}`);
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

    const handleAlbumClick = (e: React.MouseEvent, entry: DiaryEntry) => {
        e.stopPropagation(); // Prevent triggering the row click

        if (!entry.catalogItem) return;

        if (entry.interaction.itemType === 'Album') {
            navigate(`/album/${entry.catalogItem.spotifyId}`);
        } else if (entry.interaction.itemType === 'Track') {
            const trackItem = entry.catalogItem as TrackSummary;
            if (trackItem.albumId) {
                navigate(`/album/${trackItem.albumId}`);
            }
        }
    };

    // Show loading state
    if (loading && diaryEntries.length === 0) {
        return (
            <div className="max-w-6xl mx-auto py-8">
                <div className="flex flex-col items-center justify-center py-12">
                    <RefreshCw className="h-12 w-12 text-primary-600 animate-spin mb-4" />
                    <h2 className="text-xl font-medium text-gray-700">Loading your diary entries...</h2>
                </div>
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto py-8">
            <div className="mb-6">
                <h1 className="text-3xl font-bold text-gray-900 mb-2">Your Music Diary</h1>
                <p className="text-gray-600">A chronological record of your music experiences and thoughts</p>
            </div>

            {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6">
                    {error}
                    <button
                        onClick={loadDiaryEntries}
                        className="ml-2 underline hover:text-red-900"
                    >
                        Try again
                    </button>
                </div>
            )}

            {groupedEntries.length === 0 && !loading && !error ? (
                <div className="bg-white rounded-lg shadow p-8 text-center">
                    <Calendar className="mx-auto h-16 w-16 text-gray-400 mb-4" />
                    <h2 className="text-xl font-medium text-gray-900 mb-2">Your diary is empty</h2>
                    <p className="text-gray-600 mb-6">
                        Start building your music diary by rating, reviewing, and liking albums and tracks.
                    </p>
                    <button
                        onClick={() => navigate('/search')}
                        className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700"
                    >
                        Discover Music to Add
                    </button>
                </div>
            ) : (
                <>
                    {/* Diary entries by date */}
                    <div className="space-y-8">
                        {groupedEntries.map((group) => (
                            <div key={group.date} className="bg-white rounded-lg shadow overflow-hidden">
                                <div className="bg-primary-50 px-6 py-3 border-b border-primary-100">
                                    <div className="flex items-center">
                                        <Calendar className="h-5 w-5 text-primary-600 mr-2" />
                                        <h2 className="text-lg font-medium text-primary-800">{group.date}</h2>
                                    </div>
                                </div>

                                <div className="divide-y divide-gray-200">
                                    {group.entries.map((entry) => (
                                        <div
                                            key={entry.interaction.aggregateId}
                                            className="flex items-center p-4 hover:bg-gray-50 cursor-pointer"
                                            onClick={() => handleItemClick(entry)}
                                        >
                                            {/* Item image */}
                                            <div className="flex-shrink-0 h-16 w-16 bg-gray-200 rounded-md overflow-hidden mr-4">
                                                {entry.catalogItem?.imageUrl ? (
                                                    <img
                                                        src={entry.catalogItem.imageUrl}
                                                        alt={entry.catalogItem.name}
                                                        className="h-full w-full object-cover"
                                                    />
                                                ) : (
                                                    <div className="h-full w-full flex items-center justify-center bg-gray-200">
                                                        {entry.interaction.itemType === 'Album' ? (
                                                            <Disc className="h-8 w-8 text-gray-400" />
                                                        ) : (
                                                            <Music className="h-8 w-8 text-gray-400" />
                                                        )}
                                                    </div>
                                                )}
                                            </div>

                                            {/* Item details */}
                                            <div className="flex-grow min-w-0">
                                                <div className="flex items-center">
                                                    <h3 className="text-base font-medium text-gray-900 truncate">
                                                        {entry.catalogItem?.name || 'Unknown Title'}
                                                    </h3>
                                                    <span className="ml-2 px-2 py-0.5 text-xs font-medium rounded-full bg-gray-100 text-gray-800">
                            {entry.interaction.itemType}
                          </span>
                                                </div>
                                                <p className="text-sm text-gray-500 truncate">
                                                    {entry.catalogItem?.artistName || 'Unknown Artist'}
                                                </p>
                                                <div className="mt-1 text-xs text-gray-500">
                                                    {new Date(entry.interaction.createdAt).toLocaleTimeString('en-US', {
                                                        hour: '2-digit',
                                                        minute: '2-digit',
                                                        hour12: true
                                                    })}
                                                </div>
                                            </div>

                                            {/* Interaction indicators */}
                                            <div className="flex items-center space-x-3 ml-4">
                                                {entry.interaction.rating && (
                                                    <div className="flex items-center">
                                                        <div className="flex">
                                                            {/* Render star rating (out of 5 stars) */}
                                                            {[1, 2, 3, 4, 5].map((star) => {
                                                                const ratingInStars = entry.interaction.rating!.normalizedGrade / 2;
                                                                const isFilled = star <= Math.floor(ratingInStars);
                                                                const isHalf = !isFilled && star === Math.ceil(ratingInStars) && !Number.isInteger(ratingInStars);

                                                                return (
                                                                    <div key={star} className="relative">
                                                                        <Star
                                                                            className={`h-5 w-5 ${isFilled || isHalf ? 'text-yellow-500' : 'text-gray-300'}`}
                                                                            fill={isFilled ? '#EAB308' : 'none'}
                                                                        />
                                                                        {isHalf && (
                                                                            <div className="absolute inset-0 overflow-hidden w-1/2">
                                                                                <Star className="h-5 w-5 text-yellow-500" fill="#EAB308" />
                                                                            </div>
                                                                        )}
                                                                    </div>
                                                                );
                                                            })}
                                                        </div>
                                                        {entry.interaction.rating.isComplex && (
                                                            <SlidersHorizontal className="ml-1 h-4 w-4 text-primary-500"/>
                                                        )}
                                                    </div>
                                                )}

                                                {entry.interaction.review && (
                                                    <MessageSquare
                                                        className="h-5 w-5 text-primary-600 cursor-pointer hover:text-primary-800"
                                                        onClick={(e) => handleReviewClick(e, entry)}
                                                    />
                                                )}

                                                {entry.interaction.isLiked && (
                                                    <Heart className="h-5 w-5 text-red-500 fill-red-500" />
                                                )}
                                            </div>

                                            {/* Album navigation icon */}
                                            <button
                                                onClick={(e) => handleAlbumClick(e, entry)}
                                                className="ml-4 p-2 text-gray-500 hover:text-primary-600 hover:bg-gray-100 rounded-full"
                                                title={`Go to ${entry.interaction.itemType === 'Album' ? 'album' : 'track\'s album'}`}
                                            >
                                                <Disc className="h-5 w-5" />
                                            </button>
                                        </div>
                                    ))}
                                </div>
                            </div>
                        ))}
                    </div>

                    {/* Pagination */}
                    {totalPages > 1 && (
                        <div className="flex justify-center mt-8">
                            <nav className="inline-flex rounded-md shadow">
                                <button
                                    onClick={() => handlePageChange(currentPage - 1)}
                                    disabled={currentPage === 1}
                                    className="inline-flex items-center px-3 py-2 rounded-l-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    <ChevronLeft className="h-5 w-5" />
                                </button>
                                <div className="px-4 py-2 border-t border-b border-gray-300 bg-white text-gray-700">
                                    Page {currentPage} of {totalPages}
                                </div>
                                <button
                                    onClick={() => handlePageChange(currentPage + 1)}
                                    disabled={currentPage === totalPages}
                                    className="inline-flex items-center px-3 py-2 rounded-r-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    <ChevronRight className="h-5 w-5" />
                                </button>
                            </nav>
                        </div>
                    )}
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