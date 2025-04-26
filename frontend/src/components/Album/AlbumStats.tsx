import { useEffect, useState } from 'react';
import { Users, Heart, MessageSquare, Star } from 'lucide-react';
import InteractionService, { ItemStats } from '../../api/interaction';

interface AlbumStatsProps {
    albumId: string;
}

const AlbumStats = ({ albumId }: AlbumStatsProps) => {
    const [stats, setStats] = useState<ItemStats | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        const fetchStats = async () => {
            if (!albumId) return;

            try {
                setLoading(true);
                const response = await InteractionService.getItemStats(albumId);
                setStats(response);
            } catch (err) {
                console.error('Error fetching album stats:', err);
                setError('Could not load album statistics');
            } finally {
                setLoading(false);
            }
        };

        fetchStats();
    }, [albumId]);

    if (loading) {
        return (
            <div className="animate-pulse bg-gray-100 h-24 rounded-md w-full max-w-md mx-auto"></div>
        );
    }

    if (error || !stats) {
        return null;
    }

    const averageRating = stats.hasRatings ? (stats.averageRating / 2).toFixed(1) : '-';

    const maxBarHeight = 50; // Now 50px instead of 100px
    const maxDistributionValue = Math.max(...stats.ratingDistribution, 1);
    const normalizedDistribution = stats.ratingDistribution.map(
        count => (count / maxDistributionValue) * maxBarHeight
    );

    return (
        <div className="pt-4">
            <div className="flex flex-col md:flex-row items-center justify-between">
                {/* Main left section: Average + Distribution */}
                <div className="flex items-end space-x-4 flex-grow">
                    {/* Average Rating */}
                    <div className="flex flex-col items-center">
                        <div className="text-2xl font-bold text-gray-900">{averageRating}</div>
                        <div className="flex text-yellow-400">
                            {Array(5).fill(0).map((_, idx) => (
                                <Star key={idx} className="h-4 w-4 fill-yellow-400" />
                            ))}
                        </div>
                    </div>

                    {/* Rating Distribution */}
                    <div className="flex items-end space-x-1 h-[50px]">
                        {stats.hasRatings ? (
                            [...normalizedDistribution].reverse().map((height, idx) => (
                                <div key={idx} className="flex flex-col items-center">
                                    <div
                                        className="w-3 bg-primary-500 rounded-sm"
                                        style={{ height: `${height}px`, minHeight: '2px' }}
                                    ></div>
                                </div>
                            ))
                        ) : (
                            <div className="text-sm text-gray-500 text-center w-full">No ratings yet</div>
                        )}
                    </div>

                    {/* Single star icon at the end */}
                    <div className="flex items-center ml-4">
                        <Star className="h-5 w-5 text-yellow-400 fill-yellow-400" />
                    </div>
                </div>
            </div>

            {/* Stats Summary */}
            <div className="flex space-x-6 mt-3 text-xs text-gray-600">
                <div className="flex items-center">
                    <Users className="h-3 w-3 mr-1" />
                    <span>{stats.totalUsersInteracted} interacted</span>
                </div>
                <div className="flex items-center">
                    <Heart className="h-3 w-3 mr-1" />
                    <span>{stats.totalLikes} likes</span>
                </div>
                <div className="flex items-center">
                    <MessageSquare className="h-3 w-3 mr-1" />
                    <span>{stats.totalReviews} reviews</span>
                </div>
            </div>
        </div>
    );
};

export default AlbumStats;
