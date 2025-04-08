import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { Music, Disc, Heart, Star, MessageSquare, SlidersHorizontal } from 'lucide-react';
import { DiaryEntry } from './types';

interface DiaryEntryProps {
    entry: DiaryEntry;
    onReviewClick: (e: React.MouseEvent, entry: DiaryEntry) => void;
}

const DiaryEntryComponent = ({ entry, onReviewClick }: DiaryEntryProps) => {
    const navigate = useNavigate();
    const [isHovered, setIsHovered] = useState(false);

    const handleItemClick = () => {
        navigate(`/interaction/${entry.interaction.aggregateId}`);
    };

    const handleAlbumClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        if (!entry.catalogItem) return;

        if (entry.interaction.itemType === 'Album') {
            navigate(`/album/${entry.catalogItem.spotifyId}`);
        } else if (entry.interaction.itemType === 'Track') {
            navigate(`/track/${entry.catalogItem.spotifyId}`);
        }
    };

    return (
        <div
            className="flex items-center p-4 hover:bg-gray-50 cursor-pointer"
            onClick={handleItemClick}
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
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
            <div className="flex items-center space-x-4 ml-4">
                {/* Rating stars - always show */}
                <div className="flex items-center">
                    <div className="flex">
                        {[1, 2, 3, 4, 5].map((star) => {
                            // If there's no rating, show all empty stars
                            if (!entry.interaction.rating) {
                                return (
                                    <div key={star} className="relative">
                                        <Star className="h-5 w-5 text-gray-300" fill="none" />
                                    </div>
                                );
                            }

                            const ratingInStars = entry.interaction.rating.normalizedGrade / 2;
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
                    {entry.interaction.rating?.isComplex && (
                        <SlidersHorizontal className="ml-1 h-4 w-4 text-primary-500"/>
                    )}
                </div>

                {/* Review icon - show in gray if no review */}
                {entry.interaction.review ? (
                    <MessageSquare
                        className="h-5 w-5 text-primary-600 cursor-pointer hover:text-primary-800"
                        onClick={(e) => onReviewClick(e, entry)}
                    />
                ) : (
                    <MessageSquare
                        className="h-5 w-5 text-gray-300"
                    />
                )}

                {/* Like icon - show in gray if no like */}
                {entry.interaction.isLiked ? (
                    <Heart className="h-5 w-5 text-red-500 fill-red-500"/>
                ) : (
                    <Heart className="h-5 w-5 text-gray-300"/>
                )}
            </div>

            {/* Album navigation icon */}
            <button
                onClick={handleAlbumClick}
                className={`ml-4 p-2 text-gray-500 hover:text-primary-600 hover:bg-gray-100 rounded-full ${isHovered ? 'visible' : ''}`}
                title={`Go to ${entry.interaction.itemType === 'Album' ? 'album' : 'track\'s album'}`}
            >
                <Disc className="h-5 w-5" />
            </button>
        </div>
    );
};

export default DiaryEntryComponent;