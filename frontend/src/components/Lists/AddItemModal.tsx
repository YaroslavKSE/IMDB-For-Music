import { useState, useEffect, useRef, useCallback } from 'react';
import { X, Search, Loader, Check, Music, Disc, Plus } from 'lucide-react';
import CatalogService, { AlbumSummary, TrackSummary } from '../../api/catalog';

interface AddItemModalProps {
    isOpen: boolean;
    onClose: () => void;
    onAddItems: (items: { spotifyId: string }[]) => void;
    listType: string;
    existingItemIds: string[];
}

const AddItemModal = ({ isOpen, onClose, onAddItems, listType, existingItemIds }: AddItemModalProps) => {
    const [query, setQuery] = useState('');
    const [searchResults, setSearchResults] = useState<AlbumSummary[] | TrackSummary[]>([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [selectedItems, setSelectedItems] = useState<string[]>([]);
    const [submitting, setSubmitting] = useState(false);
    const [totalResults, setTotalResults] = useState(0);
    const [hasMore, setHasMore] = useState(false);
    const [offset, setOffset] = useState(0);
    const [loadingMore, setLoadingMore] = useState(false);

    // Define the maximum number of items that can be selected at once
    const MAX_ITEMS = 20;

    const searchInputRef = useRef<HTMLInputElement>(null);
    const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);

    // Reset state when modal opens
    useEffect(() => {
        if (isOpen) {
            setQuery('');
            setSearchResults([]);
            setSelectedItems([]);
            setError(null);
            setOffset(0);
            setHasMore(false);
            setTotalResults(0);

            // Focus search input
            setTimeout(() => {
                searchInputRef.current?.focus();
            }, 100);
        }
    }, [isOpen]);

    // Get icon and type label based on list type
    const getTypeIcon = () => {
        return listType === 'Album'
            ? <Disc className="h-5 w-5 mr-2 text-purple-600" />
            : <Music className="h-5 w-5 mr-2 text-pink-600" />;
    };

    const getTypeLabel = () => {
        return listType === 'Album' ? 'Albums' : 'Tracks';
    };

    // Perform search with debounce
    const performSearch = useCallback(async (searchQuery: string, offset: number = 0) => {
        if (!searchQuery.trim()) {
            setSearchResults([]);
            setHasMore(false);
            setTotalResults(0);
            return;
        }

        try {
            if (offset === 0) {
                setLoading(true);
            } else {
                setLoadingMore(true);
            }
            setError(null);

            // Determine the search type based on list type
            const searchType = listType.toLowerCase();

            const results = await CatalogService.search(searchQuery, searchType, 20, offset);

            // Set the appropriate results based on list type
            const items = listType === 'Album' ? results.albums || [] : results.tracks || [];

            if (listType === 'Album') {
                const items = results.albums || [];
                if (offset === 0) {
                    setSearchResults(items as AlbumSummary[]);
                } else {
                    setSearchResults(prev => [...(prev as AlbumSummary[]), ...items]);
                }
            } else {
                const items = results.tracks || [];
                if (offset === 0) {
                    setSearchResults(items as TrackSummary[]);
                } else {
                    setSearchResults(prev => [...(prev as TrackSummary[]), ...items]);
                }
            }

            setTotalResults(results.totalResults || 0);
            setHasMore((offset + items.length) < (results.totalResults || 0));
            setOffset(offset + items.length);
        } catch (err) {
            console.error('Search error:', err);
            setError('Failed to search. Please try again.');
        } finally {
            if (offset === 0) {
                setLoading(false);
            } else {
                setLoadingMore(false);
            }
        }
    }, [listType]);

    // Debounced search handler
    const handleSearch = (value: string) => {
        setQuery(value);

        // Clear existing timer
        if (debounceTimerRef.current) {
            clearTimeout(debounceTimerRef.current);
        }

        // Set new timer
        debounceTimerRef.current = setTimeout(() => {
            performSearch(value, 0);
        }, 500); // 500ms debounce delay
    };

    // Load more results
    const handleLoadMore = () => {
        if (loadingMore || !hasMore) return;
        performSearch(query, offset);
    };

    // Toggle item selection
    const handleToggleSelect = (spotifyId: string) => {
        setSelectedItems(prev => {
            if (prev.includes(spotifyId)) {
                return prev.filter(id => id !== spotifyId);
            } else {
                // Check if adding this item would exceed the limit
                if (prev.length >= MAX_ITEMS) {
                    // Don't add the item if it would exceed the limit
                    return prev;
                }
                return [...prev, spotifyId];
            }
        });
    };

    // Add selected items
    const handleAddItems = async () => {
        if (selectedItems.length === 0) return;

        setSubmitting(true);

        try {
            const itemsToAdd = selectedItems.map(id => ({ spotifyId: id }));
            if(listType == "Album") {
                await CatalogService.getBatchAlbums(selectedItems);
            }
            else {
                await CatalogService.getBatchTracks(selectedItems)
            }
            onAddItems(itemsToAdd);
        } catch (err) {
            console.error('Error adding items:', err);
            setError('Failed to add items');
        } finally {
            setSubmitting(false);
            onClose();
        }
    };

    // Check if item is already in the list
    const isItemInList = (spotifyId: string) => {
        return existingItemIds.includes(spotifyId);
    };

    // Check if selection limit has been reached
    const isSelectionLimitReached = selectedItems.length >= MAX_ITEMS;

    // Clean up on unmount
    useEffect(() => {
        return () => {
            if (debounceTimerRef.current) {
                clearTimeout(debounceTimerRef.current);
            }
        };
    }, []);

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
                <div className="relative bg-white rounded-lg shadow-xl max-w-lg w-full z-10">
                    {/* Header */}
                    <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
                        <h3 className="text-lg font-medium text-gray-900 flex items-center">
                            {getTypeIcon()}
                            Add {getTypeLabel()} to List
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
                                ref={searchInputRef}
                                type="text"
                                placeholder={`Search for ${listType === 'Album' ? 'albums' : 'tracks'}...`}
                                value={query}
                                onChange={(e) => handleSearch(e.target.value)}
                                className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
                            />
                        </div>
                    </div>

                    {/* Results */}
                    <div className="flex-1 overflow-y-auto p-4 max-h-80">
                        {error && (
                            <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-4">
                                {error}
                            </div>
                        )}

                        {loading ? (
                            <div className="flex justify-center items-center py-12">
                                <Loader className="h-8 w-8 text-primary-600 animate-spin mr-3" />
                                <span className="text-gray-600">Searching...</span>
                            </div>
                        ) : searchResults.length > 0 ? (
                            <div className="space-y-3">
                                {searchResults.map((item) => {
                                    const isSelected = selectedItems.includes(item.spotifyId);
                                    const isInList = isItemInList(item.spotifyId);
                                    const isDisabled = isInList || (isSelectionLimitReached && !isSelected);

                                    return (
                                        <div
                                            key={item.spotifyId}
                                            className={`p-3 border rounded-md flex items-center ${
                                                isSelected
                                                    ? 'border-primary-500 bg-primary-50'
                                                    : isInList
                                                        ? 'border-gray-300 bg-gray-100'
                                                        : isDisabled
                                                            ? 'border-gray-200 bg-gray-50 opacity-70'
                                                            : 'border-gray-200 hover:bg-gray-50'
                                            }`}
                                        >
                                            <div className="flex-shrink-0 mr-3">
                                                {!isInList && (
                                                    <input
                                                        type="checkbox"
                                                        checked={isSelected}
                                                        onChange={() => handleToggleSelect(item.spotifyId)}
                                                        className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                                                        disabled={isDisabled && !isSelected}
                                                    />
                                                )}
                                                {isInList && (
                                                    <div className="h-4 w-4 flex items-center justify-center bg-gray-400 rounded">
                                                        <Check className="h-3 w-3 text-white" />
                                                    </div>
                                                )}
                                            </div>

                                            <div className="h-10 w-10 rounded overflow-hidden bg-gray-100 mr-3 flex-shrink-0">
                                                {item.imageUrl && (
                                                    <img
                                                        src={item.imageUrl}
                                                        alt={item.name}
                                                        className="h-full w-full object-cover"
                                                    />
                                                )}
                                                {!item.imageUrl && (
                                                    <div className="h-full w-full flex items-center justify-center">
                                                        {listType === 'Album' ? (
                                                            <Disc className="h-5 w-5 text-gray-400" />
                                                        ) : (
                                                            <Music className="h-5 w-5 text-gray-400" />
                                                        )}
                                                    </div>
                                                )}
                                            </div>

                                            <div className="flex-grow min-w-0">
                                                <p className="font-medium text-gray-900 truncate">{item.name}</p>
                                                <p className="text-xs text-gray-500 truncate">{item.artistName}</p>
                                            </div>

                                            {isInList ? (
                                                <span className="ml-2 text-xs text-gray-500"></span>
                                            ) : (
                                                <button
                                                    onClick={() => handleToggleSelect(item.spotifyId)}
                                                    disabled={isDisabled && !isSelected}
                                                    className={`ml-2 p-1 rounded-full ${
                                                        isSelected
                                                            ? 'bg-primary-100 text-primary-600'
                                                            : isDisabled
                                                                ? 'text-gray-300 cursor-not-allowed'
                                                                : 'text-gray-400 hover:text-primary-600 hover:bg-primary-50'
                                                    }`}
                                                >
                                                    {isSelected ? (
                                                        <Check className="h-5 w-5" />
                                                    ) : (
                                                        <Plus className="h-5 w-5" />
                                                    )}
                                                </button>
                                            )}
                                        </div>
                                    );
                                })}

                                {/* Load more button */}
                                {hasMore && (
                                    <div className="mt-4 text-center">
                                        <button
                                            onClick={handleLoadMore}
                                            disabled={loadingMore}
                                            className="px-4 py-2 border border-gray-300 rounded-md text-gray-700 hover:bg-gray-50 disabled:opacity-50"
                                        >
                                            {loadingMore ? (
                                                <>
                                                    <Loader className="h-4 w-4 inline-block mr-2 animate-spin" />
                                                    Loading...
                                                </>
                                            ) : (
                                                `Load More (${searchResults.length} of ${totalResults})`
                                            )}
                                        </button>
                                    </div>
                                )}
                            </div>
                        ) : query && !loading ? (
                            <div className="text-center py-8">
                                <p className="text-gray-500">No results found. Try a different search term.</p>
                            </div>
                        ) : (
                            <div className="text-center py-12">
                                <Search className="h-12 w-12 text-gray-300 mx-auto mb-4" />
                                <p className="text-gray-500">Search for {listType === 'Album' ? 'albums' : 'tracks'} to add to your list</p>
                            </div>
                        )}
                    </div>

                    {/* Footer */}
                    <div className="px-6 py-4 border-t border-gray-200 flex justify-between items-center">
                        <div className="text-sm text-gray-500">
                            {selectedItems.length > 0 ? (
                                <span>{selectedItems.length} of {MAX_ITEMS} item{selectedItems.length !== 1 ? 's' : ''} selected</span>
                            ) : (
                                <span>Select up to {MAX_ITEMS} items</span>
                            )}
                        </div>
                        <div className="flex space-x-2">
                            <button
                                onClick={onClose}
                                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
                            >
                                Cancel
                            </button>
                            <button
                                onClick={handleAddItems}
                                disabled={selectedItems.length === 0 || submitting}
                                className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed flex items-center"
                            >
                                {submitting ? (
                                    <>
                                        <Loader className="h-4 w-4 mr-2 animate-spin" />
                                        Adding...
                                    </>
                                ) : (
                                    <>
                                        <Plus className="h-4 w-4 mr-2" />
                                        Add Selected
                                    </>
                                )}
                            </button>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default AddItemModal;