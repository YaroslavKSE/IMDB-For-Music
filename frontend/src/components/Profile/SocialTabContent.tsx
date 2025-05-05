import React from 'react';
import { UserSubscriptionResponse } from '../../api/users';
import UserCard from './UserCard';
import ProfileLoadingState from './ProfileLoadingState';
import { Users, UserPlus } from 'lucide-react';
import FollowButton from './FollowButton';

interface SocialTabContentProps {
  type: 'followers' | 'following';
  users: UserSubscriptionResponse[];
  loading: boolean;
  error: string | null;
  isOwnProfile: boolean;
  username?: string;
  onFollow?: () => void;
  isFollowing?: boolean;
  followLoading?: boolean;
  isAuthenticated?: boolean;
  navigateToDiscoverPeople?: () => void;
}

const SocialTabContent: React.FC<SocialTabContentProps> = ({
  type,
  users,
  loading,
  error,
  isOwnProfile,
  username = 'User',
  onFollow,
  isFollowing = false,
  followLoading = false,
  isAuthenticated = false,
  navigateToDiscoverPeople
}) => {

  // Dynamic content based on type
  const getTitle = () => {
    if (type === 'followers') {
      return isOwnProfile ? "People Following You" : `${username}'s Followers`;
    } else {
      return isOwnProfile ? "People You Follow" : `${username}'s Following`;
    }
  };

  const getEmptyStateIcon = () => {
    return type === 'followers' ? (
      <Users className="h-12 w-12 text-gray-400 mx-auto mb-3" />
    ) : (
      <UserPlus className="h-12 w-12 text-gray-400 mx-auto mb-3" />
    );
  };

  const getEmptyStateMessage = () => {
    if (type === 'followers') {
      return isOwnProfile
        ? "You don't have any followers yet."
        : `${username} doesn't have any followers yet.`;
    } else {
      return isOwnProfile
        ? "You are not following anyone yet."
        : `${username} isn't following anyone yet.`;
    }
  };

  const getEmptyStateAction = () => {
    if (type === 'followers' && !isOwnProfile && isAuthenticated && !isFollowing) {
      return (
        <div className="mt-4">
          <FollowButton
            isFollowing={isFollowing}
            isLoading={followLoading}
            onClick={onFollow}
            variant="default"
          />
        </div>
      );
    } else if (type === 'following' && isOwnProfile && navigateToDiscoverPeople) {
      return (
        <button
          onClick={navigateToDiscoverPeople}
          className="mt-4 inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
        >
          Discover People
        </button>
      );
    }
    return null;
  };

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="p-6">
        <h2 className="text-xl font-bold mb-4 flex items-center">
          {type === 'followers' ? (
            <Users className="h-5 w-5 mr-2 text-gray-500" />
          ) : (
            <UserPlus className="h-5 w-5 mr-2 text-gray-500" />
          )}
          {getTitle()}
        </h2>

        {loading ? (
          <ProfileLoadingState
            message={`Loading ${type}...`}
            height="py-12"
          />
        ) : error ? (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
            {error}
          </div>
        ) : users.length === 0 ? (
          <div className="border border-dashed border-gray-300 rounded-lg p-6 text-center">
            {getEmptyStateIcon()}
            <p className="text-gray-500 mb-4">{getEmptyStateMessage()}</p>
            {getEmptyStateAction()}
          </div>
        ) : (
          <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            {users.map(user => (
              <UserCard key={user.userId} user={user} />
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default SocialTabContent;