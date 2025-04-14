import React from 'react';
import { Loader } from 'lucide-react';
import { LoadMoreButtonProps } from './types';

const LoadMoreButton: React.FC<LoadMoreButtonProps> = ({
                                                           isLoading,
                                                           onClick,
                                                           currentCount,
                                                           totalCount
                                                       }) => {
    return (
        <div className="mt-8 text-center">
            <button
                type="button"
                onClick={onClick}
                disabled={isLoading}
                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
            >
                {isLoading ? (
                    <span className="flex items-center justify-center">
            <Loader className="h-4 w-4 animate-spin mr-2" />
            Loading...
          </span>
                ) : (
                    `Load More (${currentCount} of ${totalCount})`
                )}
            </button>
        </div>
    );
};

export default LoadMoreButton;