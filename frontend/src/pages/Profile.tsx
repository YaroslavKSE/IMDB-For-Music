import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { User, ListMusic, History, Settings } from 'lucide-react';
import useAuthStore from '../store/authStore';
import GradingMethodsTab from '../components/Profile/GradingMethodsTab';
import ProfileTabButton from '../components/Profile/ProfileTabButton';
import ProfileOverviewTab from '../components/Profile/ProfileOverviewTab';
import ProfileSettingsTab from '../components/Profile/ProfileSettingsTab';
import ProfileLoadingState from '../components/Profile/ProfileLoadingState';

// Tab types
type TabType = 'overview' | 'grading-methods' | 'history' | 'settings';

const Profile = () => {
  const navigate = useNavigate();
  const { user, fetchUserProfile, isLoading, isAuthenticated } = useAuthStore();
  const [activeTab, setActiveTab] = useState<TabType>('overview');

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

  // Show loading state if user data is not available yet
  if (isLoading || !user) {
    return <ProfileLoadingState />;
  }

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
      </div>
    </div>
  );
};

export default Profile;