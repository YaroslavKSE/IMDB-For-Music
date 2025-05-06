import React, { useState, useEffect } from 'react';
import { ListOverview, ListItem } from '../../api/lists';
import { Music, Disc, Trash2, AlertTriangle, Medal } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import CatalogService from '../../api/catalog';

interface ListRowItemProps {
    list: ListOverview;
    onDelete: (listId: string) => void;
}

const ListRowItem: React.FC<ListRowItemProps> = ({ list, onDelete }) => {
    const navigate = useNavigate();
    const [showDeleteConfirm, setShowDeleteConfirm] = useState(false);
    const [itemImages, setItemImages] = useState<Record<string, string>>({});
    const [, setIsLoadingImages] = useState(false);

    // Fetch images for the preview items
    useEffect(() => {
        const fetchItemImages = async () => {
            if (list.previewItems.length === 0) return;

            setIsLoadingImages(true);

            try {
                // Extract the Spotify IDs from the preview items
                const itemIds = list.previewItems.map(item => item.spotifyId);

                // Fetch item details using the Spotify IDs
                const previewResponse = await CatalogService.getItemPreviewInfo(
                    itemIds,
                    [list.listType.toLowerCase()]
                );

                // Create a map of Spotify IDs to image URLs
                const newItemImages: Record<string, string> = {};

                previewResponse.results?.forEach(group => {
                    group.items?.forEach(item => {
                        if (item.imageUrl) {
                            newItemImages[item.spotifyId] = item.imageUrl;
                        }
                    });
                });

                setItemImages(newItemImages);
            } catch (error) {
                console.error('Error fetching item images:', error);
            } finally {
                setIsLoadingImages(false);
            }
        };

        fetchItemImages();
    }, [list.previewItems, list.listType]);

    // Handle row click to navigate to list detail
    const handleRowClick = () => {
        navigate(`/lists/${list.listId}`);
    };

    // Handle delete button click with confirmation
    const handleDeleteClick = (e: React.MouseEvent) => {
        e.stopPropagation(); // Prevent row click
        setShowDeleteConfirm(true);
    };

    const confirmDelete = (e: React.MouseEvent) => {
        e.stopPropagation(); // Prevent row click
        onDelete(list.listId);
        setShowDeleteConfirm(false);
    };

    const cancelDelete = (e: React.MouseEvent) => {
        e.stopPropagation(); // Prevent row click
        setShowDeleteConfirm(false);
    };

    // Render an item with image if available, or a placeholder
    const renderItem = (item: ListItem, index: number) => {
        const hasImage = itemImages[item.spotifyId];

        return (
            <div
                key={`${item.spotifyId}-${index}`}
                className="w-24 h-24 relative rounded overflow-hidden"
                style={{
                    marginLeft: index > 0 ? '-30px' : '0',
                    zIndex: 5 - index,
                    boxShadow: index > 0 ? '-2px 0 4px rgba(0, 0, 0, 0.15)' : 'none',
                }}
            >
                {hasImage ? (
                    <img
                        src={itemImages[item.spotifyId]}
                        alt=""
                        className="h-full w-full object-cover"
                    />
                ) : (
                    <div className="bg-gray-200 h-full w-full flex items-center justify-center">
                        {list.listType === 'Album' ? (
                            <Disc className="h-10 w-10 text-gray-400" />
                        ) : (
                            <Music className="h-10 w-10 text-gray-400" />
                        )}
                    </div>
                )}
            </div>
        );
    };

    // Custom style for the trophy icon with only the outline colored
    const trophyStyle = {
        color: '#7a24ec', // This sets the stroke color
        fill: 'none'      // This ensures the fill is transparent
    };

    return (
        <div
            className="bg-white rounded-lg shadow hover:shadow-md transition-shadow duration-200 mb-6 overflow-hidden cursor-pointer"
            onClick={handleRowClick}
        >
            <div className="flex items-center p-5">
                {/* Preview items visualization */}
                <div className="flex mr-6 flex-shrink-0">
                    {list.previewItems && list.previewItems.length > 0 ? (
                        <div className="flex">
                            {list.previewItems.slice(0, 5).map((item, index) => (
                                renderItem(item, index)
                            ))}
                        </div>
                    ) : (
                        <div className="w-24 h-24 bg-gray-100 rounded flex items-center justify-center">
                            {list.listType === 'Album' ? (
                                <Disc className="h-12 w-12 text-gray-400" />
                            ) : (
                                <Music className="h-12 w-12 text-gray-400" />
                            )}
                        </div>
                    )}
                </div>

                {/* List details */}
                <div className="flex-grow">
                    <h3 className="text-xl font-medium text-gray-900 mb-1">{list.listName}</h3>
                    <div className="flex items-center text-base text-gray-500 mb-1">
                        <span className="mr-3">
                            {list.totalItems} {list.totalItems === 1 ?
                            (list.listType === 'Album' ? 'album' : 'track') :
                            (list.listType === 'Album' ? 'albums' : 'tracks')}
                        </span>
                        {list.isRanked && (
                            <div className="flex items-center">
                                <Medal
                                    className="h-5 w-5 mr-1 inline"
                                    style={trophyStyle}
                                    strokeWidth={2}
                                />
                                <span style={{ color: '#7a24ec' }}>Ranked</span>
                            </div>
                        )}
                    </div>
                    {list.listDescription && (
                        <p className="text-sm text-gray-500 mt-2 line-clamp-2">{list.listDescription}</p>
                    )}
                </div>

                {/* Delete button */}
                {showDeleteConfirm ? (
                    <div className="flex items-center ml-auto mr-2">
                        <div className="text-red-600 mr-3 text-sm flex items-center">
                            <AlertTriangle className="h-5 w-5 mr-1" />
                            <span>Confirm</span>
                        </div>
                        <button
                            onClick={confirmDelete}
                            className="p-3 text-white bg-red-500 hover:bg-red-600 rounded-md mr-2"
                            title="Confirm delete"
                        >
                            <Trash2 className="h-5 w-5" />
                        </button>
                        <button
                            onClick={cancelDelete}
                            className="p-3 text-gray-600 bg-gray-100 hover:bg-gray-200 rounded-md"
                            title="Cancel"
                        >
                            <span className="font-bold text-xl">×</span>
                        </button>
                    </div>
                ) : (
                    <button
                        onClick={handleDeleteClick}
                        className="ml-auto p-3 text-gray-400 hover:text-red-500 hover:bg-gray-100 rounded-full transition-colors"
                        title="Delete list"
                    >
                        <Trash2 className="h-6 w-6" />
                    </button>
                )}
            </div>
        </div>
    );
};

export default ListRowItem;