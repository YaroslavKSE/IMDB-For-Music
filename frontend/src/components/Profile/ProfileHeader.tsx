import { useState } from 'react';
import { UserProfile } from '../../api/auth';
import { PublicUserProfile } from '../../api/users';
import { Edit, Calendar } from 'lucide-react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { formatDate } from '../../utils/formatters';
import AvatarUploadModal from './AvatarUploadModal';
import FollowButton from './FollowButton';

interface ProfileHeaderProps {
  profile: UserProfile | PublicUserProfile;
  isOwnProfile: boolean;
  isFollowing?: boolean;
  onFollowToggle?: () => void;
  onAvatarUpdate?: () => void;
  isAuthenticated: boolean;
  followLoading?: boolean;
}

const ProfileHeader = ({
  profile,
  isOwnProfile,
  isFollowing = false,
  onFollowToggle,
  onAvatarUpdate,
  isAuthenticated,
  followLoading = false
}: ProfileHeaderProps) => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const [isAvatarUploadModalOpen, setIsAvatarUploadModalOpen] = useState(false);

  // Define variables based on profile type with proper fallbacks
  const followerCount = 'followerCount' in profile ? profile.followerCount || 0 : 0;
  const followingCount = 'followingCount' in profile ? profile.followingCount || 0 : 0;

  // Better date formatting with proper fallback
  const formatMemberSince = (dateString?: string) => {
    if (!dateString) return 'Unknown';

    try {
      return formatDate(dateString);
    } catch (error) {
      console.error('Error formatting date:', error);
      return 'Unknown';
    }
  };

  const memberSince = formatMemberSince(profile.createdAt);

  const handleAvatarUploadSuccess = () => {
    setIsAvatarUploadModalOpen(false);
    if (onAvatarUpdate) {
      onAvatarUpdate();
    }
  };

  // Tab navigation functions
  const navigateToTab = (tab: string) => {
    // Create a new URLSearchParams instance to preserve other params
    const newParams = new URLSearchParams(searchParams);
    // Set the tab parameter
    newParams.set('tab', tab);

    // Navigate to the same path but with the updated tab parameter
    if (isOwnProfile) {
      navigate(`/profile?${newParams.toString()}`);
    } else {
      navigate(`/people/${profile.id}?${newParams.toString()}`);
    }
  };

  return (
    <div className="bg-gradient-to-r from-primary-700 to-primary-900 px-6 py-8 text-white">
      <div className="flex flex-col md:flex-row md:items-center">
        <div className="flex-shrink-0 mb-4 md:mb-0 md:mr-6">
          {/* Avatar with Edit Button (if own profile) */}
          <div className="relative group">
            {profile.avatarUrl ? (
              <img
                src={profile.avatarUrl}
                alt={`${profile.name} ${profile.surname} avatar`}
                className="h-24 w-24 rounded-full object-cover border-4 border-white"
              />
            ) : (
              <div className="h-24 w-24 rounded-full bg-primary-600 flex items-center justify-center text-3xl font-bold border-4 border-white">
                {profile.name.charAt(0).toUpperCase()}{profile.surname.charAt(0).toUpperCase()}
              </div>
            )}
            {isOwnProfile && (
              <button
                onClick={() => setIsAvatarUploadModalOpen(true)}
                className="absolute bottom-1 right-1 bg-white rounded-full p-1.5 shadow-md opacity-0 group-hover:opacity-100 transition-opacity duration-200"
                aria-label="Edit profile picture"
              >
                <Edit className="h-3.5 w-3.5 text-primary-700" />
              </button>
            )}
          </div>
        </div>
        <div className="flex-grow">
          <h1 className="text-2xl md:text-3xl font-bold">{profile.name} {profile.surname}</h1>
          {'email' in profile && profile.email && (
            <p className="text-primary-100 mt-1">{profile.email}</p>
          )}
          {profile.username && (
            <p className="text-primary-200 mt-1">@{profile.username}</p>
          )}
          <div className="mt-2 flex flex-wrap gap-3">
            <div className="bg-white bg-opacity-20 px-3 py-1 rounded-full text-sm flex items-center">
              <Calendar className="h-4 w-4 mr-1" />
              Member since {memberSince}
            </div>

            <button
              onClick={() => navigateToTab('followers')}
              className="bg-white bg-opacity-20 px-3 py-1 rounded-full text-sm"
            >
              <span className="font-medium">{followerCount}</span> Followers
            </button>

            <button
              onClick={() => navigateToTab('following')}
              className="bg-white bg-opacity-20 px-3 py-1 rounded-full text-sm"
            >
              <span className="font-medium">{followingCount}</span> Following
            </button>

            {!isOwnProfile && isAuthenticated && (
              <FollowButton
                isFollowing={isFollowing}
                isLoading={followLoading}
                onClick={onFollowToggle}
                variant="header"
              />
            )}
          </div>
        </div>
      </div>

      {/* Avatar Upload Modal */}
      {isAvatarUploadModalOpen && (
        <AvatarUploadModal
          isOpen={isAvatarUploadModalOpen}
          onClose={() => setIsAvatarUploadModalOpen(false)}
          onSuccess={handleAvatarUploadSuccess}
          currentAvatarUrl={profile.avatarUrl}
        />
      )}
    </div>
  );
};

export default ProfileHeader;