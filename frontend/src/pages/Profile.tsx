import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { User, ListMusic, History, Settings, Users, UserPlus, Loader } from 'lucide-react';
import useAuthStore from '../store/authStore';
import UsersService, { UserSubscriptionResponse } from '../api/users';
import GradingMethodsTab from '../components/Profile/GradingMethodsTab';
import ProfileTabButton from '../components/Profile/ProfileTabButton';
import ProfileOverviewTab from '../components/Profile/ProfileOverviewTab';
import ProfileSettingsTab from '../components/Profile/ProfileSettingsTab';
import ProfileLoadingState from '../components/Profile/ProfileLoadingState';
import { formatDate } from '../utils/formatters';

// Tab types - adding new tabs for following and followers
type TabType = 'overview' | 'grading-methods' | 'history' | 'settings' | 'following' | 'followers';

const Profile = () => {
  const navigate = useNavigate();
  const { user, fetchUserProfile, isLoading, isAuthenticated } = useAuthStore();
  const [activeTab, setActiveTab] = useState<TabType>('overview');
  const [followersData, setFollowersData] = useState<UserSubscriptionResponse[]>([]);
  const [followingData, setFollowingData] = useState<UserSubscriptionResponse[]>([]);
  const [socialLoading, setSocialLoading] = useState(false);
  const [socialError, setSocialError] = useState<string | null>(null);

  // Ensure user data is loaded and user is authenticated
  useEffect(() => {
    if (!isAuthenticated) {
      navigate('/login', { state: { from: '/profile' } });
      return;
    }

    if (!user) {
      fetchUserProfile();
    }
  }, [user, fetchUserProfile, navigate, isAuthenticated]);

  // Load following/followers data when those tabs are selected
  useEffect(() => {
    const loadSocialData = async () => {
      if (!isAuthenticated || !user) return;

      if (activeTab === 'following' || activeTab === 'followers') {
        setSocialLoading(true);
        setSocialError(null);

        try {
          if (activeTab === 'following') {
            const response = await UsersService.getUserFollowing();
            setFollowingData(response.items);
          } else {
            const response = await UsersService.getUserFollowers();
            setFollowersData(response.items);
          }
        } catch (err) {
          console.error(`Error loading ${activeTab} data:`, err);
          setSocialError(`Failed to load ${activeTab} data. Please try again.`);
        } finally {
          setSocialLoading(false);
        }
      }
    };

    loadSocialData();
  }, [activeTab, isAuthenticated, user]);

  // Show loading state if user data is not available yet
  if (isLoading || !user) {
    return <ProfileLoadingState />;
  }

  // Render a single user card (used in followers and following tabs)
  const renderUserCard = (userData: UserSubscriptionResponse) => (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow duration-200">
      <div className="p-4 flex flex-col items-center text-center">
        <div
          className="h-16 w-16 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-xl font-bold mb-3"
          onClick={() => navigate(`/people/${userData.userId}`)}
        >
          {userData.name.charAt(0).toUpperCase()}{userData.surname.charAt(0).toUpperCase()}
        </div>
        <h3 className="font-medium text-gray-900 mb-1">{userData.name} {userData.surname}</h3>
        <p className="text-sm text-gray-600 mb-2">@{userData.username}</p>
        <p className="text-xs text-gray-500">Following since {formatDate(userData.subscribedAt)}</p>

        <button
          onClick={() => navigate(`/people/${userData.userId}`)}
          className="mt-3 px-3 py-1.5 border border-gray-300 rounded text-sm font-medium bg-white hover:bg-gray-50"
        >
          View Profile
        </button>
      </div>
    </div>
  );

  return (
    <div className="max-w-6xl mx-auto">
      {/* Profile Header */}
      <div className="bg-white shadow rounded-lg overflow-hidden mb-6">
        <div className="bg-gradient-to-r from-primary-700 to-primary-900 px-6 py-8 text-white">
          <div className="flex flex-col md:flex-row md:items-center">
            <div className="flex-shrink-0 mb-4 md:mb-0 md:mr-6">
              <div className="h-24 w-24 rounded-full bg-primary-600 flex items-center justify-center text-3xl font-bold border-4 border-white">
                {user.name.charAt(0).toUpperCase()}{user.surname.charAt(0).toUpperCase()}
              </div>
            </div>
            <div>
              <h1 className="text-2xl md:text-3xl font-bold">{user.name} {user.surname}</h1>
              <p className="text-primary-100 mt-1">{user.email}</p>
              {user.username && (
                <p className="text-primary-200 mt-1">@{user.username}</p>
              )}
            </div>
          </div>
        </div>

        {/* Profile Tabs */}
        <div className="border-b border-gray-200">
          <nav className="flex -mb-px overflow-x-auto">
            <ProfileTabButton
              active={activeTab === 'overview'}
              onClick={() => setActiveTab('overview')}
              icon={<User className="h-4 w-4" />}
              label="Overview"
            />
            <ProfileTabButton
              active={activeTab === 'grading-methods'}
              onClick={() => setActiveTab('grading-methods')}
              icon={<ListMusic className="h-4 w-4" />}
              label="Grading Methods"
            />
            <ProfileTabButton
              active={activeTab === 'following'}
              onClick={() => setActiveTab('following')}
              icon={<UserPlus className="h-4 w-4" />}
              label="Following"
            />
            <ProfileTabButton
              active={activeTab === 'followers'}
              onClick={() => setActiveTab('followers')}
              icon={<Users className="h-4 w-4" />}
              label="Followers"
            />
            <ProfileTabButton
              active={activeTab === 'history'}
              onClick={() => setActiveTab('history')}
              icon={<History className="h-4 w-4" />}
              label="Rating History"
            />
            <ProfileTabButton
              active={activeTab === 'settings'}
              onClick={() => setActiveTab('settings')}
              icon={<Settings className="h-4 w-4" />}
              label="Settings"
            />
          </nav>
        </div>
      </div>

      {/* Tab Content */}
      <div className="mb-8">
        {activeTab === 'overview' && <ProfileOverviewTab user={user} />}
        {activeTab === 'grading-methods' && <GradingMethodsTab />}
        {activeTab === 'history' && (
          <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6 text-center">
              <h2 className="text-xl font-bold mb-4">Rating History</h2>
              <p className="text-gray-500">
                Your rating history will be displayed here.
              </p>
            </div>
          </div>
        )}
        {activeTab === 'settings' && <ProfileSettingsTab />}

        {/* Following Tab */}
        {activeTab === 'following' && (
          <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6">
              <h2 className="text-xl font-bold mb-4 flex items-center">
                <UserPlus className="h-5 w-5 mr-2 text-gray-500" />
                People You Follow
              </h2>

              {socialLoading ? (
                <div className="flex justify-center items-center py-12">
                  <Loader className="h-8 w-8 text-primary-600 animate-spin mr-3" />
                  <span className="text-gray-600">Loading...</span>
                </div>
              ) : socialError ? (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
                  {socialError}
                </div>
              ) : followingData.length === 0 ? (
                <div className="border border-dashed border-gray-300 rounded-lg p-6 text-center">
                  <UserPlus className="h-12 w-12 text-gray-400 mx-auto mb-3" />
                  <p className="text-gray-500 mb-4">You are not following anyone yet.</p>
                  <button
                    onClick={() => navigate('/people')}
                    className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
                  >
                    Discover People
                  </button>
                </div>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                  {followingData.map(user => renderUserCard(user))}
                </div>
              )}
            </div>
          </div>
        )}

        {/* Followers Tab */}
        {activeTab === 'followers' && (
          <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6">
              <h2 className="text-xl font-bold mb-4 flex items-center">
                <Users className="h-5 w-5 mr-2 text-gray-500" />
                People Following You
              </h2>

              {socialLoading ? (
                <div className="flex justify-center items-center py-12">
                  <Loader className="h-8 w-8 text-primary-600 animate-spin mr-3" />
                  <span className="text-gray-600">Loading...</span>
                </div>
              ) : socialError ? (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
                  {socialError}
                </div>
              ) : followersData.length === 0 ? (
                <div className="border border-dashed border-gray-300 rounded-lg p-6 text-center">
                  <Users className="h-12 w-12 text-gray-400 mx-auto mb-3" />
                  <p className="text-gray-500 mb-4">You don't have any followers yet.</p>
                  <p className="text-sm text-gray-500">
                    As you interact more with the community, people will start following you.
                  </p>
                </div>
              ) : (
                <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
                  {followersData.map(user => renderUserCard(user))}
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </div>
  );
};

export default Profile;