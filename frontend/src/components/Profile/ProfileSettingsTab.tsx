import { useState } from 'react';
import { Settings, LogOut } from 'lucide-react';
import useAuthStore from '../../store/authStore';

const ProfileSettingsTab = () => {
  const { logout } = useAuthStore();
  const [showLogoutConfirm, setShowLogoutConfirm] = useState(false);

  const handleLogout = async () => {
    await logout();
    window.location.href = '/';
  };

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-6 py-4 bg-primary-50 border-b border-primary-100">
        <h3 className="text-lg font-medium text-primary-800 flex items-center">
          <Settings className="h-5 w-5 mr-2" />
          Account Settings
        </h3>
      </div>

      <div className="p-6 space-y-6">
        <div>
          <h4 className="text-base font-medium text-gray-900 mb-4">Notification Preferences</h4>
          <div className="space-y-3">
            <div className="flex items-center">
              <input
                id="email-notifications"
                name="email-notifications"
                type="checkbox"
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <label htmlFor="email-notifications" className="ml-3 block text-sm text-gray-700">
                Email notifications
              </label>
            </div>
            <div className="flex items-center">
              <input
                id="web-notifications"
                name="web-notifications"
                type="checkbox"
                defaultChecked
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <label htmlFor="web-notifications" className="ml-3 block text-sm text-gray-700">
                Web notifications
              </label>
            </div>
          </div>
        </div>

        <div className="pt-6 border-t border-gray-200">
          <h4 className="text-base font-medium text-gray-900 mb-4">Privacy Settings</h4>
          <div className="space-y-3">
            <div className="flex items-center">
              <input
                id="public-profile"
                name="public-profile"
                type="checkbox"
                defaultChecked
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <label htmlFor="public-profile" className="ml-3 block text-sm text-gray-700">
                Public profile
              </label>
            </div>
            <div className="flex items-center">
              <input
                id="show-ratings"
                name="show-ratings"
                type="checkbox"
                defaultChecked
                className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
              />
              <label htmlFor="show-ratings" className="ml-3 block text-sm text-gray-700">
                Show my ratings publicly
              </label>
            </div>
          </div>
        </div>

        <div className="pt-6 border-t border-gray-200">
          {!showLogoutConfirm ? (
            <button
              type="button"
              onClick={() => setShowLogoutConfirm(true)}
              className="inline-flex items-center px-4 py-2 border border-red-300 shadow-sm text-sm font-medium rounded-md text-red-700 bg-white hover:bg-red-50 focus:outline-none"
            >
              <LogOut className="h-4 w-4 mr-2" />
              Logout from all devices
            </button>
          ) : (
            <div className="bg-red-50 border border-red-300 rounded-md p-4">
              <p className="text-sm text-red-600 mb-3">
                Are you sure you want to log out from all devices? This will invalidate all active
                sessions and require re-login.
              </p>
              <div className="flex space-x-3">
                <button
                  type="button"
                  onClick={handleLogout}
                  className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md text-white bg-red-600 hover:bg-red-700 focus:outline-none"
                >
                  <LogOut className="h-4 w-4 mr-2" />
                  Confirm Logout
                </button>
                <button
                  type="button"
                  onClick={() => setShowLogoutConfirm(false)}
                  className="inline-flex items-center px-4 py-2 border border-gray-300 shadow-sm text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
                >
                  Cancel
                </button>
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProfileSettingsTab;