import { Heart, Headphones, Check } from 'lucide-react';

interface InteractionButtonsProps {
    isLiked: boolean;
    setIsLiked: (val: boolean) => void;
    hasListened: boolean;
    handleListenedToggle: () => void;
    rating: number | null;
    reviewText: string;
    useComplexGrading: boolean;
}

const InteractionButtons = ({
                                isLiked,
                                setIsLiked,
                                hasListened,
                                handleListenedToggle,
                                rating,
                                reviewText,
                                useComplexGrading
                            }: InteractionButtonsProps) => (
    <div className="flex gap-4 mb-6">
        <button
            type="button"
            onClick={() => setIsLiked(!isLiked)}
            className={`flex items-center justify-center px-4 py-3 rounded-md flex-1 ${
                isLiked
                    ? 'bg-red-50 text-red-500 border border-red-200'
                    : 'bg-gray-50 text-gray-500 border border-gray-200'
            }`}
        >
            <Heart className={`h-5 w-5 mr-2 ${isLiked ? 'fill-red-500' : ''}`} />
            {isLiked ? 'Liked' : 'Like'}
        </button>

        <button
            type="button"
            onClick={handleListenedToggle}
            className={`flex items-center justify-center px-4 py-3 rounded-md flex-1 ${
                hasListened
                    ? 'bg-blue-50 text-blue-600 border border-blue-200'
                    : 'bg-gray-50 text-gray-500 border border-gray-200'
            }`}
            disabled={hasListened && (isLiked || rating !== null || reviewText.trim() !== '' || useComplexGrading)}
        >
            {hasListened ? (
                <>
                    <Check className="h-5 w-5 mr-2" />
                    Listened
                </>
            ) : (
                <>
                    <Headphones className="h-5 w-5 mr-2" />
                    Listened
                </>
            )}
        </button>
    </div>
);

export default InteractionButtons;
