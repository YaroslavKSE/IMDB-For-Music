import { useState, useEffect } from 'react';
import { X, Loader, Music, User, Disc } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import CatalogService from '../../api/catalog';

type PreferenceType = 'artists' | 'albums' | 'tracks';

interface PreferenceItemProps {
  id: string;
  type: PreferenceType;
  onRemove: () => void;
  isLoading: boolean;
}

const PreferenceItem: React.FC<PreferenceItemProps> = ({ id, type, onRemove, isLoading }) => {
  const [itemInfo, setItemInfo] = useState<any | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const navigate = useNavigate();

  useEffect(() => {
    const fetchItemDetails = async () => {
      try {
        setLoading(true);
        setError(null);

        let details;

        // Call the appropriate API based on item type
        // Note: The CatalogService expects the spotify ID, not a singular type name
        switch (type) {
          case 'artists':
            details = await CatalogService.getArtist(id);
            break;
          case 'albums':
            details = await CatalogService.getAlbum(id);
            break;
          case 'tracks':
            details = await CatalogService.getTrack(id);
            break;
        }

        setItemInfo(details);
      } catch (err) {
        console.error(`Error fetching ${type} details:`, err);
        // Use singular form in error message
        const singularType = type === 'artists' ? 'artist' :
                            type === 'albums' ? 'album' : 'track';
        setError(`Failed to load ${singularType} details`);
      } finally {
        setLoading(false);
      }
    };

    fetchItemDetails();
  }, [id, type]);

  // Get the appropriate icon based on item type
  const getItemIcon = () => {
    switch (type) {
      case 'artists':
        return <User className="h-4 w-4 text-indigo-600" />;
      case 'albums':
        return <Disc className="h-4 w-4 text-purple-600" />;
      case 'tracks':
        return <Music className="h-4 w-4 text-pink-600" />;
    }
  };

  // Handle navigation to item page
  const handleItemClick = () => {
    if (!itemInfo) return;

    // Navigate to the appropriate page based on item type
    switch (type) {
      case 'artists':
        navigate(`/artist/${id}`);
        break;
      case 'albums':
        navigate(`/album/${id}`);
        break;
      case 'tracks':
        navigate(`/track/${id}`);
        break;
    }
  };

  if (loading) {
    return (
      <div className="bg-white border border-gray-200 rounded-md p-3 flex items-center animate-pulse">
        <div className="w-10 h-10 bg-gray-200 rounded-md mr-3"></div>
        <div className="flex-grow">
          <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
          <div className="h-3 bg-gray-200 rounded w-1/2"></div>
        </div>
      </div>
    );
  }

  if (error || !itemInfo) {
    return (
      <div className="bg-white border border-gray-200 rounded-md p-3 flex items-center">
        <div className="mr-3 text-gray-400">{getItemIcon()}</div>
        <div className="flex-grow min-w-0">
          <p className="font-medium text-gray-800 truncate">Unknown {type === 'artists' ? 'artist' : type === 'albums' ? 'album' : 'track'}</p>
          <p className="text-xs text-gray-500 truncate">ID: {id}</p>
        </div>
        <button
          onClick={onRemove}
          disabled={isLoading}
          className="ml-2 text-gray-400 hover:text-red-500 focus:outline-none"
          title="Remove from favorites"
        >
          {isLoading ? (
            <Loader className="h-4 w-4 animate-spin" />
          ) : (
            <X className="h-4 w-4" />
          )}
        </button>
      </div>
    );
  }

  return (
    <div className="bg-white border border-gray-200 rounded-md p-3 flex items-center hover:shadow-sm group hover:border-primary-300 transition-colors">
      <div
        className="flex-grow flex items-center min-w-0 cursor-pointer"
        onClick={handleItemClick}
        title={`Go to ${type === 'artists' ? 'artist' : type === 'albums' ? 'album' : 'track'} page`}
      >
        <div className="w-10 h-10 bg-gray-100 rounded-md mr-3 overflow-hidden">
          {itemInfo.imageUrl && (
            <img
              src={itemInfo.imageUrl}
              alt={itemInfo.name}
              className="w-full h-full object-cover"
            />
          )}
        </div>
        <div className="min-w-0">
          <p className="font-medium text-gray-800 truncate hover:text-primary-600">{itemInfo.name}</p>
          {type === 'albums' || type === 'tracks' ? (
            <p className="text-xs text-gray-500 truncate">{itemInfo.artistName}</p>
          ) : type === 'artists' && itemInfo.genres && itemInfo.genres.length > 0 ? (
            <p className="text-xs text-gray-500 truncate capitalize">{itemInfo.genres.slice(0, 2).join(', ')}</p>
          ) : (
            <p className="text-xs text-gray-500 truncate">{type === 'artists' ? 'artist' : type === 'albums' ? 'album' : 'track'}</p>
          )}
        </div>
      </div>
      <button
        onClick={(e) => {
          e.stopPropagation(); // Prevent triggering the parent onClick
          onRemove();
        }}
        disabled={isLoading}
        className="ml-2 text-gray-400 hover:text-red-500 focus:outline-none opacity-0 group-hover:opacity-100 transition-opacity"
        title="Remove from favorites"
      >
        {isLoading ? (
          <Loader className="h-4 w-4 animate-spin" />
        ) : (
          <X className="h-4 w-4" />
        )}
      </button>
    </div>
  );
};

export default PreferenceItem;