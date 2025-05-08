import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { User, Heart, Star, MessageSquare, SlidersHorizontal, Trash2 } from 'lucide-react';
import { ItemHistoryEntry } from './ItemHistoryTypes';
import ComplexRatingModal from '../common/ComplexRatingModal';
import {DiaryEntry} from "../Diary/types.ts";

interface ItemHistoryEntryProps {
    entry: ItemHistoryEntry;
    onDeleteClick?: (e: React.MouseEvent, entry: DiaryEntry) => void;
    isPublic: boolean;
}

const ItemHistoryEntryComponent = ({ entry, onDeleteClick, isPublic }: ItemHistoryEntryProps) => {
    const navigate = useNavigate();
    const [isHovered, setIsHovered] = useState(false);
    const [isComplexRatingModalOpen, setIsComplexRatingModalOpen] = useState(false);

    const handleItemClick = () => {
        navigate(`/interaction/${entry.interaction.aggregateId}`);
    };

    const handleUserClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        if (!entry.userProfile) return;
        navigate(`/people/${entry.userProfile.id}`);
    };

    const handleDelete = (e: React.MouseEvent) => {
        e.stopPropagation(); // Prevent triggering the row click
        if (onDeleteClick) {
            onDeleteClick(e, entry);
        }
    };

    const handleComplexRatingClick = (e: React.MouseEvent) => {
        e.stopPropagation();
        if (entry.interaction.rating?.isComplex && entry.interaction.rating?.ratingId) {
            setIsComplexRatingModalOpen(true);
        }
    };

    return (
        <>
            <div
                className="flex items-center p-4 hover:bg-gray-50 cursor-pointer"
                onClick={handleItemClick}
                onMouseEnter={() => setIsHovered(true)}
                onMouseLeave={() => setIsHovered(false)}
            >
                {/* User avatar */}
                <div className="flex-shrink-0 h-12 w-12 rounded-full overflow-hidden mr-4">
                    {entry.userProfile?.avatarUrl ? (
                        <img
                            src={entry.userProfile.avatarUrl}
                            alt={`${entry.userProfile.name} ${entry.userProfile.surname}`}
                            className="h-full w-full object-cover"
                        />
                    ) : (
                        <div className="h-full w-full flex items-center justify-center bg-primary-100 text-primary-700 font-bold text-lg rounded-full">
                            {entry.userProfile ? (
                                `${entry.userProfile.name.charAt(0)}${entry.userProfile.surname.charAt(0)}`
                            ) : (
                                <User className="h-6 w-6 text-gray-400" />
                            )}
                        </div>
                    )}
                </div>

                {/* User details */}
                <div className="flex-grow min-w-0">
                    <div className="flex items-center">
                        <h3
                            className="text-base font-medium text-gray-900 truncate hover:text-primary-600 hover:underline cursor-pointer"
                            onClick={handleUserClick}
                        >
                            {entry.userProfile ?
                                `${entry.userProfile.name} ${entry.userProfile.surname}` :
                                'Unknown User'}
                        </h3>
                    </div>

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
                                            className={`h-5 w-5 ${isFilled || isHalf ? 'text-yellow-400' : 'text-gray-300'}`}
                                            fill={isFilled ? 'currentColor' : 'none'}
                                        />
                                        {isHalf && (
                                            <div className="absolute inset-0 overflow-hidden w-1/2">
                                                <Star className="h-5 w-5 text-yellow-400" fill='currentColor' />
                                            </div>
                                        )}
                                    </div>
                                );
                            })}
                        </div>
                        {/* Always render the SlidersHorizontal icon but make it invisible when not complex */}
                        {entry.interaction.rating?.isComplex ? (
                            <SlidersHorizontal
                                className="ml-1 h-4 w-4 text-primary-500 visible cursor-pointer hover:text-primary-700"
                                onClick={handleComplexRatingClick}
                            />
                        ) : (
                            <SlidersHorizontal
                                className="ml-1 h-4 w-4 invisible"
                            />
                        )}
                    </div>

                    {/* Review icon - show in gray if no review */}
                    {entry.interaction.review ? (
                        <MessageSquare
                            className="h-5 w-5 text-primary-600 cursor-pointer hover:text-primary-800"
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

                {/* View button (for individual interaction) */}
                {!isPublic && (
                    <button
                        onClick={handleDelete}
                        className={`ml-4 p-2 text-gray-500 hover:text-red-500 hover:bg-gray-100 rounded-full ${isHovered ? 'visible' : 'invisible'}`}
                        title="Delete interaction"
                    >
                        <Trash2 className="h-5 w-5"/>
                    </button>
                )}
            </div>

            {/* Complex Rating Modal */}
            {entry.interaction.rating?.isComplex && (
                <ComplexRatingModal
                    isOpen={isComplexRatingModalOpen}
                    onClose={() => setIsComplexRatingModalOpen(false)}
                    ratingId={entry.interaction.rating.ratingId}
                    itemName={entry.catalogItem?.name || 'Unknown Title'}
                    artistName={entry.catalogItem?.artistName || 'Unknown Artist'}
                    date={new Date(entry.interaction.createdAt).toLocaleString('en-US', {
                        year: 'numeric',
                        month: 'long',
                        day: 'numeric',
                        hour: '2-digit',
                        minute: '2-digit'
                    })}
                />
            )}
        </>
    );
};

export default ItemHistoryEntryComponent;