import { RefreshCw, Calendar } from 'lucide-react';
import { useNavigate } from 'react-router-dom';

// Loading state component
export const DiaryLoadingState = () => {
    return (
        <div className="max-w-6xl mx-auto py-8">
            <div className="flex flex-col items-center justify-center py-12">
                <RefreshCw className="h-12 w-12 text-primary-600 animate-spin mb-4" />
                <h2 className="text-xl font-medium text-gray-700">Loading your diary entries...</h2>
            </div>
        </div>
    );
};

// Error state component
interface DiaryErrorStateProps {
    error: string;
    onRetry: () => void;
}

export const DiaryErrorState = ({ error, onRetry }: DiaryErrorStateProps) => {
    return (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6">
            {error}
            <button
                onClick={onRetry}
                className="ml-2 underline hover:text-red-900"
            >
                Try again
            </button>
        </div>
    );
};

// Empty state component
export const DiaryEmptyState = () => {
    const navigate = useNavigate();

    return (
        <div className="bg-white rounded-lg shadow p-8 text-center">
            <Calendar className="mx-auto h-16 w-16 text-gray-400 mb-4" />
            <h2 className="text-xl font-medium text-gray-900 mb-2">Your diary is empty</h2>
            <p className="text-gray-600 mb-6">
                Start building your music diary by rating, reviewing, and liking albums and tracks.
            </p>
            <button
                onClick={() => navigate('/search')}
                className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700"
            >
                Discover Music to Add
            </button>
        </div>
    );
};