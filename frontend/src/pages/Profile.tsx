import { useState, useEffect } from 'react';
import useAuthStore from '../store/authStore';
import GradingMethodsTab from '../components/Profile/GradingMethodsTab';

const Profile = () => {
  const { user, fetchUserProfile, isLoading } = useAuthStore();
  const [activeTab, setActiveTab] = useState<'overview' | 'grading-methods' | 'history' | 'settings'>('overview');

  // Ensure user data is loaded
  useEffect(() => {
    if (!user) {
      fetchUserProfile();
    }
  }, [user, fetchUserProfile]);

  // Show loading state if user data is not available yet
  if (isLoading || !user) {
    return (
        <div className="flex justify-center items-center h-64">
          <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary-600"></div>
        </div>
    );
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
              </div>
            </div>
          </div>

          {/* Profile Tabs */}
          <div className="border-b border-gray-200">
            <nav className="flex -mb-px">
              <button
                  onClick={() => setActiveTab('overview')}
                  className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm ${
                      activeTab === 'overview'
                          ? 'border-primary-600 text-primary-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Overview
              </button>
              <button
                  onClick={() => setActiveTab('grading-methods')}
                  className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm ${
                      activeTab === 'grading-methods'
                          ? 'border-primary-600 text-primary-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Grading Methods
              </button>
              <button
                  onClick={() => setActiveTab('history')}
                  className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm ${
                      activeTab === 'history'
                          ? 'border-primary-600 text-primary-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Rating History
              </button>
              <button
                  onClick={() => setActiveTab('settings')}
                  className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm ${
                      activeTab === 'settings'
                          ? 'border-primary-600 text-primary-600'
                          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
                  }`}
              >
                Settings
              </button>
            </nav>
          </div>
        </div>

        {/* Tab Content */}
        <div className="mb-8">
          {activeTab === 'overview' && (
              <div className="bg-white shadow rounded-lg overflow-hidden">
                <div className="p-6">
                  <h2 className="text-xl font-bold mb-4">Account Information</h2>
                  <div className="bg-gray-50 p-4 rounded-md">
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div>
                        <p className="text-gray-500 text-sm">First Name</p>
                        <p>{user.name}</p>
                      </div>
                      <div>
                        <p className="text-gray-500 text-sm">Last Name</p>
                        <p>{user.surname}</p>
                      </div>
                      <div>
                        <p className="text-gray-500 text-sm">Email</p>
                        <p>{user.email}</p>
                      </div>
                      {user.createdAt && (
                          <div>
                            <p className="text-gray-500 text-sm">Member Since</p>
                            <p>{new Date(user.createdAt).toLocaleDateString()}</p>
                          </div>
                      )}
                    </div>
                  </div>
                </div>
              </div>
          )}

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

          {activeTab === 'settings' && (
              <div className="bg-white shadow rounded-lg overflow-hidden">
                <div className="p-6 text-center">
                  <h2 className="text-xl font-bold mb-4">Account Settings</h2>
                  <p className="text-gray-500">
                    Account settings will be displayed here.
                  </p>
                </div>
              </div>
          )}
        </div>
      </div>
  );
};

export default Profile;