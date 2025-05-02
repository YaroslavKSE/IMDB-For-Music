import React from 'react';
import { useNavigate } from 'react-router-dom';
import { formatDate } from '../../utils/formatters';
import { UserSubscriptionResponse } from '../../api/users';

interface UserCardProps {
  user: UserSubscriptionResponse;
  showDate?: boolean;
}

const UserCard: React.FC<UserCardProps> = ({ user, showDate = true }) => {
  const navigate = useNavigate();

  const navigateToProfile = () => {
    navigate(`/people/${user.userId}`);
  };

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-200 overflow-hidden hover:shadow-md transition-shadow duration-200">
      <div className="p-4 flex flex-col items-center text-center">
        {/* Avatar or Initials */}
        {user.avatarUrl ? (
          <img
            src={user.avatarUrl}
            alt={`${user.name} ${user.surname}`}
            className="h-16 w-16 rounded-full object-cover mb-3 cursor-pointer"
            onClick={navigateToProfile}
          />
        ) : (
          <div
            className="h-16 w-16 rounded-full bg-primary-100 flex items-center justify-center text-primary-700 text-xl font-bold mb-3 cursor-pointer"
            onClick={navigateToProfile}
          >
            {user.name.charAt(0).toUpperCase()}{user.surname.charAt(0).toUpperCase()}
          </div>
        )}

        {/* User info */}
        <h3 className="font-medium text-gray-900 mb-1">{user.name} {user.surname}</h3>
        <p className="text-sm text-gray-600 mb-2">@{user.username}</p>

        {/* Subscription date */}
        {showDate && user.subscribedAt && (
          <p className="text-xs text-gray-500">Following since {formatDate(user.subscribedAt)}</p>
        )}

        {/* View profile button */}
        <button
          onClick={navigateToProfile}
          className="mt-3 px-3 py-1.5 border border-gray-300 rounded text-sm font-medium bg-white hover:bg-gray-50"
        >
          View Profile
        </button>
      </div>
    </div>
  );
};

export default UserCard;