import { Edit2, Mail, User, Calendar } from 'lucide-react';
import { UserProfile } from '../../api/auth';
import { formatDate } from '../../utils/formatters';

interface ProfileInfoCardProps {
  user: UserProfile;
  onEdit: () => void;
}

const ProfileInfoCard = ({ user, onEdit }: ProfileInfoCardProps) => {
  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="p-6">
        <div className="flex justify-between items-start">
          <h2 className="text-xl font-bold text-gray-900 mb-4">Account Information</h2>
          <button
            onClick={onEdit}
            className="px-3 py-1.5 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none flex items-center"
          >
            <Edit2 className="h-4 w-4 mr-1.5" />
            Edit Profile
          </button>
        </div>

        <div className="bg-gray-50 p-6 rounded-md">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            <div>
              <div className="flex items-center mb-3">
                <div className="bg-primary-100 p-2 rounded-md mr-3">
                  <User className="h-5 w-5 text-primary-600" />
                </div>
                <div>
                  <p className="text-gray-500 text-sm">First Name</p>
                  <p className="font-medium text-gray-900">{user.name}</p>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-center mb-3">
                <div className="bg-primary-100 p-2 rounded-md mr-3">
                  <User className="h-5 w-5 text-primary-600" />
                </div>
                <div>
                  <p className="text-gray-500 text-sm">Last Name</p>
                  <p className="font-medium text-gray-900">{user.surname}</p>
                </div>
              </div>
            </div>

            <div>
              <div className="flex items-center mb-3">
                <div className="bg-primary-100 p-2 rounded-md mr-3">
                  <Mail className="h-5 w-5 text-primary-600" />
                </div>
                <div>
                  <p className="text-gray-500 text-sm">Email</p>
                  <p className="font-medium text-gray-900">{user.email}</p>
                </div>
              </div>
            </div>

            {user.username && (
              <div>
                <div className="flex items-center mb-3">
                  <div className="bg-primary-100 p-2 rounded-md mr-3">
                    <User className="h-5 w-5 text-primary-600" />
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Username</p>
                    <p className="font-medium text-gray-900">@{user.username}</p>
                  </div>
                </div>
              </div>
            )}

            {user.createdAt && (
              <div>
                <div className="flex items-center mb-3">
                  <div className="bg-primary-100 p-2 rounded-md mr-3">
                    <Calendar className="h-5 w-5 text-primary-600" />
                  </div>
                  <div>
                    <p className="text-gray-500 text-sm">Member Since</p>
                    <p className="font-medium text-gray-900">{formatDate(user.createdAt)}</p>
                  </div>
                </div>
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ProfileInfoCard;