import React, { useState, useEffect } from 'react';
import { ListOverview } from '../../api/lists';
import { Music, Disc, Medal } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import { Link } from 'react-router-dom';
import CatalogService from '../../api/catalog';

interface ListsTabRowProps {
    list: ListOverview;
    userAvatar?: string;
    userName: string;
    userSurname?: string;
    userId: string;
}

const ListsTabRow: React.FC<ListsTabRowProps> = ({ list, userAvatar, userName, userSurname, userId }) => {
    const navigate = useNavigate();
    const [itemImages, setItemImages] = useState<Record<string, string>>({});

    // Fetch images for the list items when component mounts
    useEffect(() => {
        const fetchItemImages = async () => {
            if (list.previewItems.length === 0) return;

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
            }
        };

        fetchItemImages();
    }, [list.previewItems, list.listType]);

    // Handle row click to navigate to list detail
    const handleRowClick = () => {
        navigate(`/lists/${list.listId}`);
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
                                <div
                                    key={`${item.spotifyId}-${index}`}
                                    className="w-24 h-24 relative rounded overflow-hidden"
                                    style={{
                                        marginLeft: index > 0 ? '-30px' : '0',
                                        zIndex: 5 - index,
                                        boxShadow: index > 0 ? '-2px 0 4px rgba(0, 0, 0, 0.15)' : 'none',
                                    }}
                                >
                                    {itemImages[item.spotifyId] ? (
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
                    {/* User info */}
                    <div className="flex items-center mb-2 group">
                        <Link to={`/people/${userId}`} className="flex items-center">
                            {userAvatar ? (
                                <img
                                    src={userAvatar}
                                    alt={`${userName} ${userSurname}`}
                                    className="h-8 w-8 rounded-full object-cover mr-2"
                                />
                            ) : (
                                <div className="h-8 w-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-sm font-bold mr-2">
                                    {userName.charAt(0).toUpperCase()}{userSurname?.charAt(0).toUpperCase()}
                                </div>
                            )}
                            <span className="text-sm font-medium text-gray-700 group-hover:text-primary-600">
                                {userName} {userSurname}
                            </span>
                        </Link>
                    </div>

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
            </div>
        </div>
    );
};

export default ListsTabRow;