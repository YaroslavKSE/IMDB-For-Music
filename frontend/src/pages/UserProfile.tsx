import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { User, Calendar, UserPlus, UserCheck, Loader, AlertTriangle, ListMusic, MessageSquare, FileText } from 'lucide-react';
import { UserProfile as AuthUserProfile } from '../api/auth';
import UsersService from '../api/users';
import InteractionService, { GradingMethodSummary } from '../api/interaction';
import useAuthStore from '../store/authStore';
import { formatDate } from '../utils/formatters';

// Tabs for the user profile
type ProfileTab = 'interactions' | 'grading-methods' | 'following' | 'followers';

const UserProfile = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { isAuthenticated, user: currentUser } = useAuthStore();
    const [userProfile, setUserProfile] = useState<AuthUserProfile | null>(null);
    const [isFollowing, setIsFollowing] = useState(false);
    const [gradingMethods, setGradingMethods] = useState<GradingMethodSummary[]>([]);
    const [activeTab, setActiveTab] = useState<ProfileTab>('interactions');
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [followLoading, setFollowLoading] = useState(false);

    // Check if viewing own profile
    const isOwnProfile = currentUser?.id === id;

    // Fetch user profile data
    useEffect(() => {
        const fetchUserProfile = async () => {
            if (!id) return;

            try {
                setLoading(true);
                setError(null);

                // In a real implementation, we would have an API endpoint to get a user by ID
                // For now, we'll use a placeholder implementation

                // This would be replaced with: const userData = await UsersService.getUserById(id);
                const mockUserData: AuthUserProfile = {
                    id: id,
                    email: 'user@example.com', // This would normally be private/filtered for non-self profiles
                    name: 'John',
                    surname: 'Doe',
                    username: 'johndoe',
                    createdAt: '2024-01-15T12:00:00Z',
                    updatedAt: '2024-03-20T14:30:00Z'
                };

                setUserProfile(mockUserData);

                // Check if the current user is following this user
                if (isAuthenticated && !isOwnProfile) {
                    const followStatus = await UsersService.checkFollowingStatus(id);
                    setIsFollowing(followStatus);
                }

                // Fetch public grading methods
                try {
                    const methods = await InteractionService.getUserGradingMethods(id);
                    // Filter to only public methods if not viewing own profile
                    const filteredMethods = isOwnProfile
                        ? methods
                        : methods.filter(method => method.isPublic);

                    setGradingMethods(filteredMethods);
                } catch (err) {
                    console.error('Error fetching grading methods:', err);
                    // Non-critical error, don't set the main error state
                }

            } catch (err) {
                console.error('Error fetching user profile:', err);
                setError('Failed to load user profile. The user may not exist or has been removed.');
            } finally {
                setLoading(false);
            }
        };

        fetchUserProfile();
    }, [id, isAuthenticated, isOwnProfile]);

    const handleFollow = async () => {
        if (!isAuthenticated) {
            navigate('/login', { state: { from: `/people/${id}` } });
            return;
        }

        if (!id) return;

        try {
            setFollowLoading(true);

            if (isFollowing) {
                await UsersService.unfollowUser(id);
            } else {
                await UsersService.followUser(id);
            }

            setIsFollowing(!isFollowing);
        } catch (err) {
            console.error('Error toggling follow status:', err);
        } finally {
            setFollowLoading(false);
        }
    };

    if (loading) {
        return (
            <div className="flex justify-center items-center h-64">
                <Loader className="h-10 w-10 text-primary-600 animate-spin mr-4" />
                <p className="text-lg text-gray-600">Loading user profile...</p>
            </div>
        );
    }

    if (error || !userProfile) {
        return (
            <div className="max-w-4xl mx-auto py-8 px-4">
                <div className="bg-red-50 border border-red-200 text-red-700 p-6 rounded-lg">
                    <div className="flex items-center mb-4">
                        <AlertTriangle className="h-6 w-6 mr-2" />
                        <h2 className="text-xl font-semibold">User Not Found</h2>
                    </div>
                    <p>{error || "This user doesn't exist or has been removed."}</p>
                    <button
                        onClick={() => navigate('/people')}
                        className="mt-4 px-4 py-2 bg-white border border-red-300 rounded-md text-red-600 hover:bg-red-50"
                    >
                        Return to People Page
                    </button>
                </div>
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto py-8 px-4">
            {/* Profile Header */}
            <div className="bg-white shadow rounded-lg overflow-hidden mb-6">
                <div className="bg-gradient-to-r from-primary-700 to-primary-900 px-6 py-8 text-white">
                    <div className="flex flex-col md:flex-row md:items-center">
                        <div className="flex-shrink-0 mb-4 md:mb-0 md:mr-6">
                            <div className="h-24 w-24 rounded-full bg-primary-600 flex items-center justify-center text-3xl font-bold border-4 border-white">
                                {userProfile.name.charAt(0).toUpperCase()}{userProfile.surname.charAt(0).toUpperCase()}
                            </div>
                        </div>
                        <div className="flex-grow">
                            <h1 className="text-2xl md:text-3xl font-bold">{userProfile.name} {userProfile.surname}</h1>
                            {userProfile.username && (
                                <p className="text-primary-200 mt-1">@{userProfile.username}</p>
                            )}
                            <div className="mt-2 flex flex-wrap gap-3">
                                <div className="bg-white bg-opacity-20 px-3 py-1 rounded-full text-sm flex items-center">
                                    <Calendar className="h-4 w-4 mr-1" />
                                    Member since {formatDate(userProfile.createdAt)}
                                </div>

                                {!isOwnProfile && isAuthenticated && (
                                    <button
                                        onClick={handleFollow}
                                        disabled={followLoading}
                                        className={`px-4 py-1 rounded-full text-sm font-medium flex items-center transition-colors ${
                                            isFollowing 
                                                ? 'bg-white text-primary-700 hover:bg-primary-100' 
                                                : 'bg-blue-600 text-white hover:bg-blue-700'
                                        }`}
                                    >
                                        {followLoading ? (
                                            <Loader className="h-4 w-4 mr-1 animate-spin" />
                                        ) : isFollowing ? (
                                            <UserCheck className="h-4 w-4 mr-1" />
                                        ) : (
                                            <UserPlus className="h-4 w-4 mr-1" />
                                        )}
                                        {isFollowing ? 'Following' : 'Follow'}
                                    </button>
                                )}
                            </div>
                        </div>
                    </div>
                </div>

                {/* Profile Tabs */}
                <div className="border-b border-gray-200">
                    <nav className="flex overflow-x-auto">
                        <TabButton
                            active={activeTab === 'interactions'}
                            onClick={() => setActiveTab('interactions')}
                            icon={<MessageSquare className="h-4 w-4" />}
                            label="Recent Activity"
                        />
                        <TabButton
                            active={activeTab === 'grading-methods'}
                            onClick={() => setActiveTab('grading-methods')}
                            icon={<FileText className="h-4 w-4" />}
                            label="Grading Methods"
                        />
                        <TabButton
                            active={activeTab === 'following'}
                            onClick={() => setActiveTab('following')}
                            icon={<UserPlus className="h-4 w-4" />}
                            label="Following"
                        />
                        <TabButton
                            active={activeTab === 'followers'}
                            onClick={() => setActiveTab('followers')}
                            icon={<User className="h-4 w-4" />}
                            label="Followers"
                        />
                    </nav>
                </div>
            </div>

            {/* Tab Content */}
            <div className="bg-white shadow rounded-lg overflow-hidden">
                {/* Interactions Tab */}
                {activeTab === 'interactions' && (
                    <div className="p-6">
                        <div className="flex items-center mb-4">
                            <MessageSquare className="h-5 w-5 text-gray-500 mr-2" />
                            <h2 className="text-xl font-bold text-gray-900">Recent Activity</h2>
                        </div>
                        <div className="text-center p-8 text-gray-500">
                            <p>Recent activities will appear here.</p>
                            <p className="text-sm mt-2">This feature is coming soon.</p>
                        </div>
                    </div>
                )}

                {/* Grading Methods Tab */}
                {activeTab === 'grading-methods' && (
                    <div className="p-6">
                        <div className="flex items-center mb-4">
                            <FileText className="h-5 w-5 text-gray-500 mr-2" />
                            <h2 className="text-xl font-bold text-gray-900">Grading Methods</h2>
                        </div>

                        {gradingMethods.length > 0 ? (
                            <div className="space-y-4">
                                {gradingMethods.map(method => (
                                    <div
                                        key={method.id}
                                        className="border border-gray-200 rounded-lg p-4 hover:shadow-md transition-shadow"
                                    >
                                        <h3 className="font-medium text-lg">{method.name}</h3>
                                        <div className="mt-2 flex items-center justify-between">
                                            <div className="text-sm text-gray-500">
                                                Created: {formatDate(method.createdAt)}
                                            </div>
                                            <div className="flex">
                                                <span
                                                    className={`px-2 py-0.5 text-xs rounded-full ${
                                                        method.isPublic 
                                                        ? 'bg-green-100 text-green-800' 
                                                        : 'bg-gray-100 text-gray-800'
                                                    }`}
                                                >
                                                    {method.isPublic ? 'Public' : 'Private'}
                                                </span>
                                                <button
                                                    onClick={() => navigate(`/grading-methods/${method.id}`)}
                                                    className="ml-3 text-sm text-primary-600 hover:text-primary-800"
                                                >
                                                    View Details
                                                </button>
                                            </div>
                                        </div>
                                    </div>
                                ))}
                            </div>
                        ) : (
                            <div className="text-center p-8 border border-dashed border-gray-300 rounded-lg">
                                <ListMusic className="h-10 w-10 text-gray-400 mx-auto mb-2" />
                                <p className="text-gray-600">
                                    {isOwnProfile
                                        ? "You haven't created any grading methods yet."
                                        : "This user hasn't shared any grading methods."}
                                </p>

                                {isOwnProfile && (
                                    <button
                                        onClick={() => navigate('/grading-methods/create')}
                                        className="mt-3 px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700"
                                    >
                                        Create Grading Method
                                    </button>
                                )}
                            </div>
                        )}
                    </div>
                )}

                {/* Following Tab */}
                {activeTab === 'following' && (
                    <div className="p-6">
                        <div className="flex items-center mb-4">
                            <UserPlus className="h-5 w-5 text-gray-500 mr-2" />
                            <h2 className="text-xl font-bold text-gray-900">Following</h2>
                        </div>
                        <div className="text-center p-8 text-gray-500">
                            <p>People that this user follows will appear here.</p>
                            <p className="text-sm mt-2">This feature is coming soon.</p>
                        </div>
                    </div>
                )}

                {/* Followers Tab */}
                {activeTab === 'followers' && (
                    <div className="p-6">
                        <div className="flex items-center mb-4">
                            <User className="h-5 w-5 text-gray-500 mr-2" />
                            <h2 className="text-xl font-bold text-gray-900">Followers</h2>
                        </div>
                        <div className="text-center p-8 text-gray-500">
                            <p>People who follow this user will appear here.</p>
                            <p className="text-sm mt-2">This feature is coming soon.</p>
                        </div>
                    </div>
                )}
            </div>
        </div>
    );
};

// Helper component for tabs
interface TabButtonProps {
    active: boolean;
    onClick: () => void;
    icon: React.ReactNode;
    label: string;
}

const TabButton = ({ active, onClick, icon, label }: TabButtonProps) => {
    return (
        <button
            onClick={onClick}
            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center whitespace-nowrap ${
                active
                    ? 'border-primary-600 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
        >
            <div className="mr-2">{icon}</div>
            {label}
        </button>
    );
};

export default UserProfile;