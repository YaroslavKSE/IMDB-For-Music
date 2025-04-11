import { useState, useEffect } from 'react';
import { useSearchParams, useNavigate } from 'react-router-dom';
import { Search, Loader, UserPlus, UserCheck, Users } from 'lucide-react';
import UsersService, { UserSummary } from '../api/users';
import useAuthStore from '../store/authStore';

const People = () => {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const { isAuthenticated } = useAuthStore();
    const searchQuery = searchParams.get('q') || '';
    const [localSearchQuery, setLocalSearchQuery] = useState(searchQuery);
    const [page, setPage] = useState(1);
    const [users, setUsers] = useState<UserSummary[]>([]);
    const [userFollowingStatus, setUserFollowingStatus] = useState<Record<string, boolean>>({});
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [totalPages, setTotalPages] = useState(1);
    const [totalUsers, setTotalUsers] = useState(0);
    const [followingChanges, setFollowingChanges] = useState<Record<string, boolean>>({});

    // Fetch users on initial load and when search/page changes
    useEffect(() => {
        const fetchUsers = async () => {
            try {
                setLoading(true);
                setError(null);
                const response = await UsersService.getUsers(page, 20, searchQuery);
                setUsers(response.items);
                setTotalPages(response.totalPages);
                setTotalUsers(response.totalCount);

                // If authenticated, check following status for each user
                if (isAuthenticated && response.items.length > 0) {
                    const followStatusMap: Record<string, boolean> = {};

                    // For better performance, we can do these in parallel
                    const followStatusPromises = response.items.map(async (user) => {
                        try {
                            const isFollowing = await UsersService.checkFollowingStatus(user.id);
                            return { userId: user.id, isFollowing };
                        } catch (error) {
                            console.error(`Error checking follow status for user ${user.id}:`, error);
                            return { userId: user.id, isFollowing: false };
                        }
                    });

                    const results = await Promise.all(followStatusPromises);
                    results.forEach(({ userId, isFollowing }) => {
                        followStatusMap[userId] = isFollowing;
                    });

                    setUserFollowingStatus(followStatusMap);
                }
            } catch (err) {
                console.error('Error fetching users:', err);
                setError('Failed to load users. Please try again.');
            } finally {
                setLoading(false);
            }
        };

        fetchUsers();
    }, [page, searchQuery, isAuthenticated]);

    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (localSearchQuery !== searchQuery) {
            setPage(1); // Reset to first page on new search
            setSearchParams(localSearchQuery ? { q: localSearchQuery } : {});
        }
    };

    const handlePageChange = (newPage: number) => {
        if (newPage < 1 || newPage > totalPages) return;
        setPage(newPage);
        // Scroll to top when changing pages
        window.scrollTo({ top: 0, behavior: 'smooth' });
    };

    const handleFollowToggle = async (userId: string) => {
        if (!isAuthenticated) {
            navigate('/login', { state: { from: '/people' } });
            return;
        }

        try {
            const currentlyFollowing = followingChanges[userId] !== undefined
                ? followingChanges[userId]
                : userFollowingStatus[userId];

            // Optimistically update UI
            setFollowingChanges({
                ...followingChanges,
                [userId]: !currentlyFollowing
            });

            if (currentlyFollowing) {
                await UsersService.unfollowUser(userId);
            } else {
                await UsersService.followUser(userId);
            }
        } catch (error) {
            console.error('Error toggling follow status:', error);

            // Revert the optimistic update on error
            setFollowingChanges({
                ...followingChanges,
                [userId]: userFollowingStatus[userId]
            });
        }
    };

    // Determine if a user is being followed (considering both original status and pending changes)
    const isFollowing = (userId: string) => {
        return followingChanges[userId] !== undefined
            ? followingChanges[userId]
            : userFollowingStatus[userId] || false;
    };

    return (
        <div className="max-w-6xl mx-auto py-8">
            <div className="mb-6">
                <h1 className="text-3xl font-bold mb-2">People</h1>
                <p className="text-gray-600">Connect with other music enthusiasts</p>
            </div>

            {/* Search Bar */}
            <div className="mb-8">
                <form onSubmit={handleSearchSubmit} className="relative max-w-3xl">
                    <div className="flex items-center">
                        <input
                            type="text"
                            placeholder="Search by name or username..."
                            value={localSearchQuery}
                            onChange={(e) => setLocalSearchQuery(e.target.value)}
                            className="w-full py-3 px-5 pl-12 rounded-full text-base focus:outline-none border border-gray-300 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 shadow-sm"
                        />
                        <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                            <Search className="h-5 w-5 text-gray-400" />
                        </div>
                        <button
                            type="submit"
                            className="absolute right-3 bg-primary-600 text-white p-2 rounded-full hover:bg-primary-700 transition-colors"
                        >
                            <Search className="h-5 w-5" />
                        </button>
                    </div>
                </form>
            </div>

            {/* Loading State */}
            {loading && (
                <div className="flex justify-center items-center py-12">
                    <Loader className="h-8 w-8 text-primary-600 animate-spin mr-3" />
                    <span className="text-lg text-gray-600">Loading users...</span>
                </div>
            )}

            {/* Error State */}
            {error && !loading && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-4">
                    {error}
                </div>
            )}

            {/* Users List */}
            {!loading && !error && (
                <>
                    {/* Results count */}
                    <div className="mb-4 text-gray-600">
                        {totalUsers > 0 ? (
                            <>
                                Found <span className="font-medium">{totalUsers}</span>
                                {searchQuery ? ` users matching "${searchQuery}"` : ' users'}
                            </>
                        ) : (
                            'No users found'
                        )}
                    </div>

                    {/* Users grid */}
                    {users.length > 0 ? (
                        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
                            {users.map((user) => (
                                <div key={user.id} className="bg-white rounded-lg shadow overflow-hidden hover:shadow-md transition-shadow duration-200">
                                    <div className="p-4 flex flex-col items-center text-center">
                                        <div className="h-20 w-20 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-2xl font-bold mb-3 border-2 border-primary-200">
                                            {user.name.charAt(0).toUpperCase()}{user.surname.charAt(0).toUpperCase()}
                                        </div>
                                        <h3 className="font-medium text-gray-900 mb-1">{user.name} {user.surname}</h3>
                                        <p className="text-sm text-gray-600 mb-4">@{user.username}</p>

                                        <div className="flex space-x-2 mt-2">
                                            <button
                                                onClick={() => navigate(`/people/${user.id}`)}
                                                className="px-3 py-1.5 border border-gray-300 rounded text-sm font-medium bg-white hover:bg-gray-50"
                                            >
                                                View Profile
                                            </button>

                                            {isAuthenticated && (
                                                <button
                                                    onClick={() => handleFollowToggle(user.id)}
                                                    className={`px-3 py-1.5 border rounded text-sm font-medium flex items-center ${
                                                        isFollowing(user.id)
                                                        ? 'border-primary-300 bg-primary-50 text-primary-700 hover:bg-primary-100'
                                                        : 'border-blue-300 bg-blue-50 text-blue-700 hover:bg-blue-100'
                                                    }`}
                                                >
                                                    {isFollowing(user.id) ? (
                                                        <>
                                                            <UserCheck className="h-4 w-4 mr-1" />
                                                            Following
                                                        </>
                                                    ) : (
                                                        <>
                                                            <UserPlus className="h-4 w-4 mr-1" />
                                                            Follow
                                                        </>
                                                    )}
                                                </button>
                                            )}
                                        </div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : (
                        <div className="bg-white rounded-lg p-8 text-center shadow">
                            <Users className="h-16 w-16 text-gray-400 mx-auto mb-4" />
                            <h3 className="text-lg font-medium text-gray-900 mb-2">No users found</h3>
                            <p className="text-gray-600">
                                {searchQuery
                                    ? `No users matching "${searchQuery}"`
                                    : "There are no users to display"}
                            </p>
                        </div>
                    )}

                    {/* Pagination */}
                    {totalPages > 1 && (
                        <div className="mt-8 flex justify-center">
                            <nav className="inline-flex shadow rounded-md">
                                <button
                                    onClick={() => handlePageChange(page - 1)}
                                    disabled={page === 1}
                                    className="px-3 py-2 border border-r-0 border-gray-300 rounded-l-md bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    Previous
                                </button>
                                <div className="flex">
                                    {Array.from({ length: Math.min(5, totalPages) }, (_, i) => {
                                        // Logic to show relevant page numbers
                                        const pageNum = Math.min(Math.max(1, page - 2 + i), totalPages);
                                        return (
                                            <button
                                                key={pageNum}
                                                onClick={() => handlePageChange(pageNum)}
                                                className={`px-4 py-2 border border-r-0 border-gray-300 ${
                                                    page === pageNum 
                                                    ? 'bg-primary-600 text-white'
                                                    : 'bg-white text-gray-700 hover:bg-gray-50'
                                                }`}
                                            >
                                                {pageNum}
                                            </button>
                                        );
                                    })}
                                </div>
                                <button
                                    onClick={() => handlePageChange(page + 1)}
                                    disabled={page === totalPages}
                                    className="px-3 py-2 border border-gray-300 rounded-r-md bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                                >
                                    Next
                                </button>
                            </nav>
                        </div>
                    )}
                </>
            )}
        </div>
    );
};

export default People;
