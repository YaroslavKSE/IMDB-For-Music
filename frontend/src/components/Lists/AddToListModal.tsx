import { useState, useEffect, useRef, useCallback } from 'react';
import { X, Loader, Check, Search, Disc, Music, Medal } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import useAuthStore from '../../store/authStore';
import ListsService, { ListOverview } from '../../api/lists';

interface AddToListModalProps {
    isOpen: boolean;
    onClose: () => void;
    spotifyId: string;
    itemType: 'Album' | 'Track';
    onSuccess?: () => void;
}

const AddToListModal = ({ isOpen, onClose, spotifyId, itemType, onSuccess }: AddToListModalProps) => {
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuthStore();
    const [lists, setLists] = useState<ListOverview[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [selectedListId, setSelectedListId] = useState<string | null>(null);
    const [searchQuery, setSearchQuery] = useState('');
    const [filteredLists, setFilteredLists] = useState<ListOverview[]>([]);
    const [submitting, setSubmitting] = useState(false);
    const [totalLists, setTotalLists] = useState(0);
    const [offset, setOffset] = useState(0);
    const [hasMore, setHasMore] = useState(false);
    const [loadingMore, setLoadingMore] = useState(false);
    const modalRef = useRef<HTMLDivElement>(null);

    // Items per page
    const PAGE_SIZE = 20;

    // Load user's lists of the matching type when the modal opens

    const fetchLists = useCallback(async (offset: number) => {
        if (!user) return;

        try {
            if (offset === 0) {
                setLoading(true);
            } else {
                setLoadingMore(true);
            }

            setError(null);

            const response = await ListsService.getUserLists(
                user.id,
                PAGE_SIZE,
                offset,
                itemType
            );

            if (offset === 0) {
                setLists(response.lists);
            } else {
                setLists(prev => [...prev, ...response.lists]);
            }

            setFilteredLists(response.lists);
            setTotalLists(response.totalCount);
            setOffset(offset + response.lists.length);
            setHasMore((offset + response.lists.length) < response.totalCount);
        } catch (err) {
            console.error('Error fetching lists:', err);
            setError('Failed to load your lists. Please try again.');
        } finally {
            if (offset === 0) {
                setLoading(false);
            } else {
                setLoadingMore(false);
            }
        }
    }, [user, itemType]); // include dependencies used inside fetchLists

    useEffect(() => {
        if (isOpen && isAuthenticated && user) {
            fetchLists(0);
        }
    }, [isOpen, isAuthenticated, user, itemType, fetchLists]);

    // Filter lists based on search query
    useEffect(() => {
        if (searchQuery.trim() === '') {
            setFilteredLists(lists);
            return;
        }

        const filtered = lists.filter(list =>
            list.listName.toLowerCase().includes(searchQuery.toLowerCase()) ||
            (list.listDescription && list.listDescription.toLowerCase().includes(searchQuery.toLowerCase()))
        );
        setFilteredLists(filtered);
    }, [searchQuery, lists]);


    // Load more lists
    const handleLoadMore = async () => {
        if (loadingMore || !hasMore) return;
        fetchLists(offset);
    };

    // Handle adding item to selected list
    const handleAddToList = async () => {
        if (!selectedListId || !user || submitting) return;

        setSubmitting(true);
        setError(null);

        try {
            // Call the API to add the item to the list
            const response = await ListsService.insertListItem(selectedListId, {
                spotifyId: spotifyId
                // Position not specified, so it will be added to the end
            });

            if (response.success) {
                // Show success message briefly
                setError(null);
                const successMessage = document.createElement('div');
                successMessage.className = 'fixed top-4 right-4 bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded z-50 shadow-md';
                successMessage.textContent = 'Successfully added to list!';
                document.body.appendChild(successMessage);

                // Remove the message after a short delay
                setTimeout(() => {
                    document.body.removeChild(successMessage);
                }, 3000);

                if (onSuccess) {
                    onSuccess();
                }
                onClose();
            } else {
                setError(response.errorMessage || 'Failed to add item to list.');
            }
        } catch (err) {
            console.error('Error adding item to list:', err);
            setError('An error occurred while adding to list. Please try again.');
        } finally {
            setSubmitting(false);
        }
    };

    // Handle click outside modal to close
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (modalRef.current && !modalRef.current.contains(event.target as Node)) {
                onClose();
            }
        };

        if (isOpen) {
            document.addEventListener('mousedown', handleClickOutside);
        }

        return () => {
            document.removeEventListener('mousedown', handleClickOutside);
        };
    }, [isOpen, onClose]);

    if (!isOpen) return null;

    // Redirect to login if not authenticated
    if (!isAuthenticated) {
        navigate('/login', { state: { from: window.location.pathname } });
        return null;
    }

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto bg-black bg-opacity-50 flex items-center justify-center">
            <div ref={modalRef} className="relative bg-white rounded-lg shadow-xl max-w-md w-full z-10">
                {/* Header */}
                <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
                    <h3 className="text-lg font-medium text-gray-900 flex items-center">
                        {itemType === 'Album' ? (
                            <Disc className="h-5 w-5 mr-2 text-primary-600" />
                        ) : (
                            <Music className="h-5 w-5 mr-2 text-primary-600" />
                        )}
                        Add to {itemType} List
                    </h3>
                    <button
                        onClick={onClose}
                        className="text-gray-400 hover:text-gray-500 focus:outline-none"
                    >
                        <X className="h-5 w-5" />
                    </button>
                </div>

                {/* Search input */}
                <div className="px-6 py-4 border-b border-gray-200">
                    <div className="relative">
                        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Search className="h-5 w-5 text-gray-400" />
                        </div>
                        <input
                            type="text"
                            placeholder="Search your lists..."
                            value={searchQuery}
                            onChange={(e) => setSearchQuery(e.target.value)}
                            className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
                        />
                    </div>
                </div>

                {/* Error message */}
                {error && (
                    <div className="px-6 pt-4">
                        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
                            {error}
                        </div>
                    </div>
                )}

                {/* Lists */}
                <div className="overflow-y-auto max-h-60 p-6">
                    {loading ? (
                        <div className="flex justify-center items-center py-10">
                            <Loader className="h-8 w-8 text-primary-600 animate-spin mr-3" />
                            <span className="text-gray-600">Loading your lists...</span>
                        </div>
                    ) : filteredLists.length === 0 ? (
                        <div className="text-center py-8 text-gray-500">
                            {searchQuery ? (
                                <p>No lists match your search.</p>
                            ) : (
                                <>
                                    <p className="mb-4">You don't have any {itemType.toLowerCase()} lists yet.</p>
                                    <button
                                        onClick={() => navigate('/lists')}
                                        className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
                                    >
                                        Create a List
                                    </button>
                                </>
                            )}
                        </div>
                    ) : (
                        <>
                            <div className="space-y-3">
                                {filteredLists.map((list) => (
                                    <div
                                        key={list.listId}
                                        className={`p-3 border rounded-md cursor-pointer transition-colors ${
                                            selectedListId === list.listId
                                                ? 'border-primary-500 bg-primary-50'
                                                : 'border-gray-200 hover:bg-gray-50'
                                        }`}
                                        onClick={() => setSelectedListId(list.listId)}
                                    >
                                        <div className="flex justify-between items-start">
                                            <div className="flex-grow pr-4">
                                                <h4 className="font-medium text-gray-900">{list.listName}</h4>
                                                <div className="flex items-center text-xs text-gray-600 mt-1">
                                                    <span>{list.totalItems} item{list.totalItems !== 1 && 's'}</span>
                                                    {list.isRanked && (
                                                        <div className="flex items-center ml-2">
                                                            <Medal className="h-3 w-3 mr-1 inline text-primary-600" strokeWidth={2} style={{ fill: 'none' }} />
                                                            <span className="text-primary-600">Ranked</span>
                                                        </div>
                                                    )}
                                                </div>
                                                {list.listDescription && (
                                                    <p className="text-xs text-gray-500 mt-1 line-clamp-2">{list.listDescription}</p>
                                                )}
                                            </div>
                                            {selectedListId === list.listId && (
                                                <div className="flex items-center justify-center h-5 w-5 bg-primary-100 rounded-full">
                                                    <Check className="h-3 w-3 text-primary-600" />
                                                </div>
                                            )}
                                        </div>
                                    </div>
                                ))}
                            </div>

                            {/* Load more button */}
                            {hasMore && (
                                <div className="mt-4 text-center">
                                    <button
                                        onClick={handleLoadMore}
                                        disabled={loadingMore}
                                        className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                                    >
                                        {loadingMore ? (
                                            <div className="flex items-center justify-center">
                                                <Loader className="h-4 w-4 text-gray-600 animate-spin mr-2" />
                                                <span>Loading...</span>
                                            </div>
                                        ) : (
                                            <span>Load More ({lists.length} of {totalLists})</span>
                                        )}
                                    </button>
                                </div>
                            )}
                        </>
                    )}
                </div>

                {/* Footer */}
                <div className="px-6 py-4 border-t border-gray-200 flex justify-between items-center">
                    <div className="flex space-x-2 ml-auto">
                        <button
                            onClick={onClose}
                            className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
                        >
                            Cancel
                        </button>
                        <button
                            onClick={handleAddToList}
                            disabled={!selectedListId || submitting}
                            className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed flex items-center"
                        >
                            {submitting ? (
                                <>
                                    <Loader className="h-4 w-4 mr-2 animate-spin" />
                                    Adding...
                                </>
                            ) : (
                                'Add to List'
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AddToListModal;