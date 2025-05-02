import { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import useAuthStore from '../store/authStore';
import UsersService from '../api/users';
import GradingMethodsTab from '../components/Profile/GradingMethodsTab';
import ProfileOverviewTab from '../components/Profile/ProfileOverviewTab';
import ProfileSettingsTab from '../components/Profile/ProfileSettingsTab';
import ProfilePreferencesTab from '../components/Profile/ProfilePreferencesTab';
import ProfileHistoryTab from '../components/Profile/ProfileHistoryTab';
import ProfileLoadingState from '../components/Profile/ProfileLoadingState';
import ProfileHeader from '../components/Profile/ProfileHeader';
import ProfileTabs, { ProfileTabType } from '../components/Profile/ProfileTabs';
import SocialTabContent from '../components/Profile/SocialTabContent';

const Profile = () => {
  const navigate = useNavigate();
  const [searchParams, setSearchParams] = useSearchParams();
  const { user, fetchUserProfile, isLoading, isAuthenticated } = useAuthStore();

  // Get active tab from URL params or default to overview
  const tabParam = searchParams.get('tab');
  const [activeTab, setActiveTab] = useState<ProfileTabType>(
    tabParam as ProfileTabType || 'overview'
  );

  const [followersData, setFollowersData] = useState([]);
  const [followingData, setFollowingData] = useState([]);
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

  // Update URL when tab changes
  const handleTabChange = (tab: ProfileTabType) => {
    setActiveTab(tab);
    setSearchParams({ tab });
  };

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
    return <ProfileLoadingState message="Loading your profile..." />;
  }

  return (
    <div className="max-w-6xl mx-auto">
      {/* Profile Header */}
      <div className="bg-white shadow rounded-lg overflow-hidden mb-6">
        <ProfileHeader
          profile={user}
          isOwnProfile={true}
          isAuthenticated={isAuthenticated}
          onAvatarUpdate={fetchUserProfile}
        />

        {/* Profile Tabs */}
        <ProfileTabs
          activeTab={activeTab}
          onTabChange={handleTabChange}
          isOwnProfile={true}
        />
      </div>

      {/* Tab Content */}
      <div className="mb-8">
        {activeTab === 'overview' && <ProfileOverviewTab user={user} />}
        {activeTab === 'grading-methods' && <GradingMethodsTab />}
        {activeTab === 'preferences' && <ProfilePreferencesTab />}
        {activeTab === 'history' && <ProfileHistoryTab />}
        {activeTab === 'settings' && <ProfileSettingsTab />}

        {/* Following Tab */}
        {activeTab === 'following' && (
          <SocialTabContent
            type="following"
            users={followingData}
            loading={socialLoading}
            error={socialError}
            isOwnProfile={true}
            username={user.username}
            isAuthenticated={isAuthenticated}
            navigateToDiscoverPeople={() => navigate('/people')}
          />
        )}

        {/* Followers Tab */}
        {activeTab === 'followers' && (
          <SocialTabContent
            type="followers"
            users={followersData}
            loading={socialLoading}
            error={socialError}
            isOwnProfile={true}
            username={user.username}
            isAuthenticated={isAuthenticated}
          />
        )}
      </div>
    </div>
  );
};

export default Profile;