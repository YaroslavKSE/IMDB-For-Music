import { useEffect } from 'react';
import useAuthStore from '../store/authStore';

const Profile = () => {
  const { user, fetchUserProfile, isLoading } = useAuthStore();

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
    <div className="max-w-4xl mx-auto">
      <div className="bg-white shadow rounded-lg overflow-hidden">
        {/* Profile Header */}
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

        {/* Profile Content */}
        <div className="p-6">
          <div className="mb-8">
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

          {/* Empty state card */}
          <div className="bg-gray-50 p-8 rounded-md text-center mt-8">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              className="h-16 w-16 mx-auto text-gray-400 mb-4"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"
              />
            </svg>
            <h3 className="text-xl font-semibold mb-2">Welcome to Music Evaluation Platform!</h3>
            <p className="text-gray-600 mb-6 max-w-md mx-auto">
              Discover new music, share your opinions through ratings and reviews, and connect with other music enthusiasts.
            </p>
            <button className="bg-primary-600 text-white px-6 py-3 rounded-md font-medium hover:bg-primary-700 transition-colors">
              Start Exploring
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Profile;