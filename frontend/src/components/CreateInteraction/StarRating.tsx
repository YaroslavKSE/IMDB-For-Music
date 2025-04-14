import { Star } from 'lucide-react';

const StarRating = ({
                        displayRating,
                        setRating,
                        setHoveredRating
                    }: {
    displayRating: number | null;
    setRating: (value: number) => void;
    setHoveredRating: (value: number | null) => void;
}) => {
    const stars = [];
    const totalStars = 5;

    for (let i = 0; i < totalStars; i++) {
        const starValue = i + 1;
        const halfStarValue = i + 0.5;
        const isStarFilled = displayRating !== null && displayRating >= starValue;
        const isHalfStarFilled = displayRating !== null && displayRating >= halfStarValue && displayRating < starValue;

        stars.push(
            <div key={i} className="relative inline-block w-8 h-8">
                <Star className={`h-8 w-8 ${isStarFilled ? 'text-yellow-400 fill-yellow-400' : 'text-gray-300'}`} />
                {isHalfStarFilled && (
                    <div className="absolute inset-0 overflow-hidden w-1/2">
                        <Star className="h-8 w-8 text-yellow-400 fill-yellow-400" />
                    </div>
                )}
                <div className="absolute inset-0 flex">
                    <div
                        className="w-1/2 h-full cursor-pointer"
                        onClick={() => setRating(halfStarValue)}
                        onMouseEnter={() => setHoveredRating(halfStarValue)}
                        onMouseLeave={() => setHoveredRating(null)}
                    />
                    <div
                        className="w-1/2 h-full cursor-pointer"
                        onClick={() => setRating(starValue)}
                        onMouseEnter={() => setHoveredRating(starValue)}
                        onMouseLeave={() => setHoveredRating(null)}
                    />
                </div>
            </div>
        );
    }

    return (
        <div className="flex items-center relative">
            <div className="absolute inset-0 flex space-x-0">
                {Array.from({ length: totalStars * 2 }).map((_, index) => {
                    const value = (index + 1) / 2;
                    return (
                        <div
                            key={`click-${index}`}
                            className="h-full flex-1 cursor-pointer"
                            onClick={() => setRating(value)}
                            onMouseEnter={() => setHoveredRating(value)}
                            onMouseLeave={() => setHoveredRating(null)}
                        />
                    );
                })}
            </div>
            <div className="flex space-x-0">
                {stars}
            </div>
        </div>
    );
};

export default StarRating;