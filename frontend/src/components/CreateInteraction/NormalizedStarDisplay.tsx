import { Star } from 'lucide-react';

interface NormalizedStarDisplayProps {
    currentGrade: number;
    minGrade: number;
    maxGrade: number;
    size?: 'sm' | 'md' | 'lg';
}

/**
 * Calculates the normalized star rating (0.5-5 scale) based on current grade and range
 * Uses the same algorithm as the backend
 */
const calculateNormalizedStars = (current: number, min: number, max: number): number => {
    // Ensure we don't divide by zero
    if (max === min) return 2.5; // Default to middle value

    // Calculate normalized percentage (0-1 range)
    const range = max - min;
    const normalizedPercentage = (current - min) / range;

    // Convert to 1-10 scale (same algorithm as backend)
    const normalizedValue = 1 + normalizedPercentage * 9;

    // Convert to 0.5-5 star scale
    const starRating = normalizedValue / 2;

    // Ensure minimum of 0.5 stars
    return Math.max(0.51, starRating);
};

const NormalizedStarDisplay: React.FC<NormalizedStarDisplayProps> = ({
                                                                         currentGrade,
                                                                         minGrade,
                                                                         maxGrade,
                                                                         size = 'sm'
                                                                     }) => {
    // Calculate star rating (0.5-5 scale)
    const starRating = calculateNormalizedStars(currentGrade, minGrade, maxGrade);

    // Determine star sizes based on the size prop
    const starSizes = {
        sm: 'h-3 w-3',
        md: 'h-4 w-4',
        lg: 'h-5 w-5'
    };

    const starSize = starSizes[size];

    // Render 5 stars
    return (
        <div className="flex">
            {[1, 2, 3, 4, 5].map((position) => {
                // Full star
                if (starRating >= position) {
                    return (
                        <Star
                            key={position}
                            className={`${starSize} text-yellow-400`}
                            fill="#FBBF24"
                        />
                    );
                }
                // Half star (if rating is between this position - 0.5 and this position)
                else if (starRating > position - 0.5) {
                    return (
                        <div key={position} className="relative">
                            <Star className={`${starSize} text-gray-300`} />
                            <div className="absolute inset-0 overflow-hidden w-1/2">
                                <Star className={`${starSize} text-yellow-400`} fill="#FBBF24" />
                            </div>
                        </div>
                    );
                }
                // Empty star
                else {
                    return (
                        <Star
                            key={position}
                            className={`${starSize} text-gray-300`}
                            fill="none"
                        />
                    );
                }
            })}
        </div>
    );
};

export default NormalizedStarDisplay;