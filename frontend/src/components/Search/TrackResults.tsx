import React from 'react';
import { ChevronRight } from 'lucide-react';
import TrackRow from './TrackRow';
import LoadMoreButton from './LoadMoreButton';
import { TrackSummary } from '../../api/catalog';

interface TrackResultsProps {
    tracks: TrackSummary[];
    totalCount: number;
    isLoading: boolean;
    isLoadingMore: boolean;
    compact?: boolean;
    onLoadMore: (e: React.MouseEvent) => void;
    onShowMore?: () => void;
}

const TrackResults: React.FC<TrackResultsProps> = ({
                                                       tracks,
                                                       totalCount,
                                                       isLoading,
                                                       isLoadingMore,
                                                       compact = false,
                                                       onLoadMore,
                                                       onShowMore
                                                   }) => {
    if (isLoading && tracks.length === 0) {
        return (
            <div className="animate-pulse space-y-4 bg-white rounded-lg shadow overflow-hidden">
                {[...Array(compact ? 5 : 10)].map((_, i) => (
                    <div key={i} className="flex items-center px-6 py-4">
                        <div className="w-12 h-12 bg-gray-200 mr-4"></div>
                        <div className="flex-1">
                            <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                            <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                        </div>
                    </div>
                ))}
            </div>
        );
    }

    if (!isLoading && tracks.length === 0) {
        return null;
    }

    return (
        <div>
            <div className="flex justify-between items-center mb-4">
                <h2 className="text-xl font-bold">Tracks</h2>
                {compact && onShowMore && totalCount > tracks.length && (
                    <button
                        type="button"
                        onClick={onShowMore}
                        className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                    >
                        View all <ChevronRight className="h-4 w-4 ml-1" />
                    </button>
                )}
            </div>

            <div className="bg-white rounded-lg shadow overflow-hidden">
                {tracks.map((track, index) => (
                    <TrackRow key={track.spotifyId} track={track} index={index} />
                ))}
            </div>

            {!compact && tracks.length < totalCount && (
                <LoadMoreButton
                    isLoading={isLoadingMore}
                    onClick={onLoadMore}
                    currentCount={tracks.length}
                    totalCount={totalCount}
                />
            )}
        </div>
    );
};

export default TrackResults;