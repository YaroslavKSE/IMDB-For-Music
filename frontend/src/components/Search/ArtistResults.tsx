import React from 'react';
import { ChevronRight } from 'lucide-react';
import ArtistCard from './ArtistCard';
import LoadMoreButton from './LoadMoreButton';
import { ArtistSummary } from '../../api/catalog';

interface ArtistResultsProps {
    artists: ArtistSummary[];
    totalCount: number;
    isLoading: boolean;
    isLoadingMore: boolean;
    compact?: boolean;
    onLoadMore: (e: React.MouseEvent) => void;
    onShowMore?: () => void;
}

const ArtistResults: React.FC<ArtistResultsProps> = ({
                                                         artists,
                                                         totalCount,
                                                         isLoading,
                                                         isLoadingMore,
                                                         compact = false,
                                                         onLoadMore,
                                                         onShowMore
                                                     }) => {
    if (isLoading && artists.length === 0) {
        return (
            <div className="animate-pulse grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                {[...Array(compact ? 5 : 10)].map((_, i) => (
                    <div key={i} className="bg-white rounded-lg shadow overflow-hidden">
                        <div className="aspect-square bg-gray-200 w-full rounded-full mx-auto p-2"></div>
                        <div className="p-3 text-center">
                            <div className="h-4 bg-gray-200 rounded w-3/4 mx-auto mb-2"></div>
                            <div className="h-3 bg-gray-200 rounded w-1/2 mx-auto"></div>
                        </div>
                    </div>
                ))}
            </div>
        );
    }

    if (!isLoading && artists.length === 0) {
        return null;
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-bold">Artists</h2>
                {compact && onShowMore && totalCount > artists.length && (
                    <button
                        type="button"
                        onClick={onShowMore}
                        className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                    >
                        View all <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                )}
            </div>

            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                {artists.map((artist) => (
                    <ArtistCard key={artist.spotifyId} artist={artist} />
                ))}
            </div>

            {!compact && artists.length < totalCount && (
                <LoadMoreButton
                    isLoading={isLoadingMore}
                    onClick={onLoadMore}
                    currentCount={artists.length}
                    totalCount={totalCount}
                />
            )}
        </div>
    );
};

export default ArtistResults;