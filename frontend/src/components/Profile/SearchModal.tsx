import React, { useState, useEffect, useRef, useCallback } from 'react';
import { X, Search, Loader, Check, Music, User, Disc } from 'lucide-react';
import CatalogService, { SearchResult, AlbumSummary, TrackSummary, ArtistSummary } from '../../api/catalog';
import UserPreferencesService from '../../api/preferences';

type PreferenceType = 'artists' | 'albums' | 'tracks';

interface PreferenceSearchModalProps {
  isOpen: boolean;
  onClose: () => void;
  type: PreferenceType;
  onPreferenceAdded: () => void;
}

const SearchModal: React.FC<PreferenceSearchModalProps> = ({
  isOpen,
  onClose,
  type,
  onPreferenceAdded
}) => {
  const MAX_ITEMS_PER_CATEGORY = 5;

  const [query, setQuery] = useState('');
  const [searchResults, setSearchResults] = useState<SearchResult | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedItems, setSelectedItems] = useState<string[]>([]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [addingIds, setAddingIds] = useState<Record<string, boolean>>({});

  // NEW: Add state to track current preferences count
  const [currentCount, setCurrentCount] = useState(0);

  const searchInputRef = useRef<HTMLInputElement>(null);
  const debounceTimerRef = useRef<NodeJS.Timeout | null>(null);

  // NEW: Get current count of preferences for the selected type
  useEffect(() => {
    const fetchCurrentPreferences = async () => {
      try {
        const preferences = await UserPreferencesService.getUserPreferences();
        const count = type === 'artists'
          ? preferences.artists.length
          : type === 'albums'
            ? preferences.albums.length
            : preferences.tracks.length;

        setCurrentCount(count);
      } catch (error) {
        console.error('Error fetching preference count:', error);
        setCurrentCount(0);
      }
    };

    if (isOpen) {
      fetchCurrentPreferences();
    }
  }, [isOpen, type]);

  // Focus search input when modal opens
  useEffect(() => {
    if (isOpen && searchInputRef.current) {
      setTimeout(() => {
        searchInputRef.current?.focus();
      }, 100);
    }
  }, [isOpen]);

  // Get icon based on preference type
  const getTypeIcon = () => {
    switch (type) {
      case 'artists':
        return <User className="h-5 w-5 mr-2 text-indigo-600" />;
      case 'albums':
        return <Disc className="h-5 w-5 mr-2 text-purple-600" />;
      case 'tracks':
        return <Music className="h-5 w-5 mr-2 text-pink-600" />;
    }
  };

  // Get title based on preference type
  const getTypeTitle = () => {
    switch (type) {
      case 'artists':
        return 'Artists';
      case 'albums':
        return 'Albums';
      case 'tracks':
        return 'Tracks';
    }
  };

  // Calculate remaining slots
  const remainingSlots = Math.max(0, MAX_ITEMS_PER_CATEGORY - currentCount);

  // Search function with debounce
  const performSearch = useCallback(async (searchQuery: string) => {
    if (!searchQuery.trim()) {
      setSearchResults(null);
      return;
    }

    try {
      setLoading(true);
      setError(null);

      // Get the type string for the search API
      const searchType = type === 'artists' ? 'artist' :
                       type === 'albums' ? 'album' : 'track';

      const results = await CatalogService.search(searchQuery, searchType);
      setSearchResults(results);
    } catch (err) {
      console.error('Search error:', err);
      setError('Failed to search. Please try again.');
    } finally {
      setLoading(false);
    }
  }, [type]);

  // Debounced search handler
  const handleSearch = (value: string) => {
    setQuery(value);

    // Clear existing timer
    if (debounceTimerRef.current) {
      clearTimeout(debounceTimerRef.current);
    }

    // Set new timer
    debounceTimerRef.current = setTimeout(() => {
      performSearch(value);
    }, 500); // 500ms debounce delay
  };

  // Clean up timer on unmount
  useEffect(() => {
    return () => {
      if (debounceTimerRef.current) {
        clearTimeout(debounceTimerRef.current);
      }
    };
  }, []);

  // Toggle selection of an item
  const toggleSelection = (id: string) => {
    if (selectedItems.includes(id)) {
      setSelectedItems(prev => prev.filter(itemId => itemId !== id));
    } else {
      // Check if we can add more items
      if (selectedItems.length < remainingSlots) {
        setSelectedItems(prev => [...prev, id]);
      } else {
        // Show a warning
        alert(`You can only add up to ${MAX_ITEMS_PER_CATEGORY} ${type} to your preferences.`);
      }
    }
  };

  // Add a single item directly
  const addSingleItem = async (id: string) => {
    if (addingIds[id]) return; // Prevent double submission

    // Check if we can add one more item
    if (currentCount >= MAX_ITEMS_PER_CATEGORY) {
      alert(`You've reached the maximum of ${MAX_ITEMS_PER_CATEGORY} ${type}.`);
      return;
    }

    try {
      setAddingIds(prev => ({ ...prev, [id]: true }));
      // Use singular form: 'artist', 'album', 'track'
      const itemType = type === 'artists' ? 'artist' :
                      type === 'albums' ? 'album' : 'track';
      await UserPreferencesService.addPreference(itemType, id);
      onPreferenceAdded();
    } catch (err) {
      console.error('Error adding preference:', err);
      setError('Failed to add preference. Please try again.');
    } finally {
      setAddingIds(prev => ({ ...prev, [id]: false }));
    }
  };

  // Submit selected items
  const handleSubmit = async () => {
    if (selectedItems.length === 0) return;

    // Check if we've exceeded the limit
    if (currentCount + selectedItems.length > MAX_ITEMS_PER_CATEGORY) {
      alert(`You can only have ${MAX_ITEMS_PER_CATEGORY} ${type} in your preferences.`);
      return;
    }

    try {
      setIsSubmitting(true);
      setError(null);

      // Convert plural type names to singular for API call
      // Create arrays with the selected IDs in the correct format
      const artists = type === 'artists' ? selectedItems : [];
      const albums = type === 'albums' ? selectedItems : [];
      const tracks = type === 'tracks' ? selectedItems : [];

      // Call the bulk add method with the selected IDs
      await UserPreferencesService.bulkAddPreferences(artists, albums, tracks);

      onPreferenceAdded();
    } catch (err) {
      console.error('Error adding preferences:', err);
      setError('Failed to add preferences. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  // Render result items based on type
  const renderResults = () => {
    if (!searchResults) return null;

    switch (type) {
      case 'artists':
        return searchResults.artists?.map(artist => (
          <ArtistResultItem
            key={artist.spotifyId}
            artist={artist}
            selected={selectedItems.includes(artist.spotifyId)}
            onSelect={() => toggleSelection(artist.spotifyId)}
            onAdd={() => addSingleItem(artist.spotifyId)}
            isAdding={addingIds[artist.spotifyId] || false}
          />
        ));
      case 'albums':
        return searchResults.albums?.map(album => (
          <AlbumResultItem
            key={album.spotifyId}
            album={album}
            selected={selectedItems.includes(album.spotifyId)}
            onSelect={() => toggleSelection(album.spotifyId)}
            onAdd={() => addSingleItem(album.spotifyId)}
            isAdding={addingIds[album.spotifyId] || false}
          />
        ));
      case 'tracks':
        return searchResults.tracks?.map(track => (
          <TrackResultItem
            key={track.spotifyId}
            track={track}
            selected={selectedItems.includes(track.spotifyId)}
            onSelect={() => toggleSelection(track.spotifyId)}
            onAdd={() => addSingleItem(track.spotifyId)}
            isAdding={addingIds[track.spotifyId] || false}
          />
        ));
      default:
        return null;
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white rounded-lg shadow-xl w-full max-w-lg max-h-[90vh] flex flex-col">
        {/* Header */}
        <div className="px-6 py-4 border-b border-gray-200 flex justify-between items-center">
          <h3 className="text-lg font-medium text-gray-900 flex items-center">
            {getTypeIcon()}
            Add Favorite {getTypeTitle()}
            <span className="ml-2 text-xs text-gray-500">
              ({MAX_ITEMS_PER_CATEGORY - currentCount} remaining)
            </span>
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
              placeholder={`Search for ${type}`}
              value={query}
              onChange={(e) => handleSearch(e.target.value)}
              className="block w-full pl-10 pr-3 py-2 border border-gray-300 rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm"
            />
          </div>

          {/* Display warning if at maximum limit */}
          {remainingSlots <= 0 && (
            <div className="mt-2 text-sm text-amber-600 bg-amber-50 p-2 rounded border border-amber-200">
              You've reached the maximum of {MAX_ITEMS_PER_CATEGORY} {type}. Please remove some to add more.
            </div>
          )}
        </div>

        {/* Results */}
        <div className="flex-1 overflow-y-auto p-4">
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
          ) : searchResults ? (
            <div className="space-y-3">
              {renderResults()}
              {(searchResults.artists?.length === 0 ||
                searchResults.albums?.length === 0 ||
                searchResults.tracks?.length === 0) && (
                <div className="text-center py-8">
                  <p className="text-gray-500">No results found. Try a different search term.</p>
                </div>
              )}
            </div>
          ) : (
            <div className="text-center py-12">
              <Search className="h-12 w-12 text-gray-300 mx-auto mb-4" />
              <p className="text-gray-500">Search for {type} to add to your favorites</p>
            </div>
          )}
        </div>

        {/* Footer */}
        <div className="px-6 py-4 border-t border-gray-200 flex justify-between items-center">
          <div className="text-sm text-gray-500">
            {selectedItems.length > 0 ? (
              <span>
                {selectedItems.length} item{selectedItems.length !== 1 ? 's' : ''} selected
                {remainingSlots > 0 && (
                  <span className="text-xs text-gray-400 ml-1">
                    (max {MAX_ITEMS_PER_CATEGORY})
                  </span>
                )}
              </span>
            ) : (
              <span>Select items or add them individually</span>
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
              onClick={handleSubmit}
              disabled={selectedItems.length === 0 || isSubmitting || remainingSlots <= 0}
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed flex items-center"
            >
              {isSubmitting ? (
                <>
                  <Loader className="h-4 w-4 mr-2 animate-spin" />
                  Adding...
                </>
              ) : (
                <>
                  <Check className="h-4 w-4 mr-2" />
                  Add Selected
                </>
              )}
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

// Artist result item component
interface ArtistResultItemProps {
  artist: ArtistSummary;
  selected: boolean;
  onSelect: () => void;
  onAdd: () => void;
  isAdding: boolean;
}

const ArtistResultItem: React.FC<ArtistResultItemProps> = ({
  artist,
  selected,
  onSelect,
  onAdd,
  isAdding
}) => {
  return (
    <div className={`p-3 border rounded-md flex items-center ${selected ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:bg-gray-50'}`}>
      <div className="flex-shrink-0 mr-3">
        <input
          type="checkbox"
          checked={selected}
          onChange={onSelect}
          className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
        />
      </div>
      <div className="h-10 w-10 rounded-full overflow-hidden bg-gray-100 mr-3">
        {artist.imageUrl && (
          <img
            src={artist.imageUrl}
            alt={artist.name}
            className="h-full w-full object-cover"
          />
        )}
      </div>
      <div className="flex-grow min-w-0">
        <p className="font-medium text-gray-900 truncate">{artist.name}</p>
        {artist.genres && artist.genres.length > 0 && (
          <p className="text-xs text-gray-500 truncate capitalize">
            {artist.genres.slice(0, 3).join(', ')}
          </p>
        )}
      </div>
      <button
        onClick={onAdd}
        disabled={isAdding}
        className="ml-2 px-2 py-1 text-xs text-primary-600 hover:text-primary-800 focus:outline-none"
      >
        {isAdding ? (
          <Loader className="h-4 w-4 animate-spin" />
        ) : (
          'Add'
        )}
      </button>
    </div>
  );
};

// Album result item component
interface AlbumResultItemProps {
  album: AlbumSummary;
  selected: boolean;
  onSelect: () => void;
  onAdd: () => void;
  isAdding: boolean;
}

const AlbumResultItem: React.FC<AlbumResultItemProps> = ({
  album,
  selected,
  onSelect,
  onAdd,
  isAdding
}) => {
  return (
    <div className={`p-3 border rounded-md flex items-center ${selected ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:bg-gray-50'}`}>
      <div className="flex-shrink-0 mr-3">
        <input
          type="checkbox"
          checked={selected}
          onChange={onSelect}
          className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
        />
      </div>
      <div className="h-10 w-10 rounded-md overflow-hidden bg-gray-100 mr-3">
        {album.imageUrl && (
          <img
            src={album.imageUrl}
            alt={album.name}
            className="h-full w-full object-cover"
          />
        )}
      </div>
      <div className="flex-grow min-w-0">
        <p className="font-medium text-gray-900 truncate">{album.name}</p>
        <p className="text-xs text-gray-500 truncate">{album.artistName}</p>
      </div>
      <button
        onClick={onAdd}
        disabled={isAdding}
        className="ml-2 px-2 py-1 text-xs text-primary-600 hover:text-primary-800 focus:outline-none"
      >
        {isAdding ? (
          <Loader className="h-4 w-4 animate-spin" />
        ) : (
          'Add'
        )}
      </button>
    </div>
  );
};

// Track result item component
interface TrackResultItemProps {
  track: TrackSummary;
  selected: boolean;
  onSelect: () => void;
  onAdd: () => void;
  isAdding: boolean;
}

const TrackResultItem: React.FC<TrackResultItemProps> = ({
  track,
  selected,
  onSelect,
  onAdd,
  isAdding
}) => {
  return (
    <div className={`p-3 border rounded-md flex items-center ${selected ? 'border-primary-500 bg-primary-50' : 'border-gray-200 hover:bg-gray-50'}`}>
      <div className="flex-shrink-0 mr-3">
        <input
          type="checkbox"
          checked={selected}
          onChange={onSelect}
          className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
        />
      </div>
      <div className="h-10 w-10 rounded-md overflow-hidden bg-gray-100 mr-3">
        {track.imageUrl && (
          <img
            src={track.imageUrl}
            alt={track.name}
            className="h-full w-full object-cover"
          />
        )}
      </div>
      <div className="flex-grow min-w-0">
        <p className="font-medium text-gray-900 truncate">{track.name}</p>
        <p className="text-xs text-gray-500 truncate">{track.artistName}</p>
      </div>
      <button
        onClick={onAdd}
        disabled={isAdding}
        className="ml-2 px-2 py-1 text-xs text-primary-600 hover:text-primary-800 focus:outline-none"
      >
        {isAdding ? (
          <Loader className="h-4 w-4 animate-spin" />
        ) : (
          'Add'
        )}
      </button>
    </div>
  );
};

export default SearchModal;