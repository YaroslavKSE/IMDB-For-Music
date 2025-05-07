import React from 'react';
import {Scale, History, UserPlus, Users, Settings, UserCog, ListMusic} from 'lucide-react';

// Define the tab types that can be used in both Profile and UserProfile
export type ProfileTabType =
  | 'grading-methods'
  | 'history'
  | 'settings'
  | 'following'
  | 'followers'
  | 'preferences'
  | 'lists'

interface ProfileTabProps {
  active: boolean;
  onClick: () => void;
  icon: React.ReactNode;
  label: string;
}

interface ProfileTabsProps {
  activeTab: ProfileTabType;
  onTabChange: (tab: ProfileTabType) => void;
  isOwnProfile: boolean;
}

// Individual tab button component - extracted from ProfileTabButton.tsx
const ProfileTabButton: React.FC<ProfileTabProps> = ({
  active,
  onClick,
  icon,
  label
}) => {
  return (
    <button
      onClick={onClick}
      className={`mr-6 py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
        active
          ? 'border-primary-600 text-primary-600'
          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      }`}
    >
      <div className="mr-2">{icon}</div>
      <span>{label}</span>
    </button>
  );
};

const ProfileTabs: React.FC<ProfileTabsProps> = ({
  activeTab,
  onTabChange,
  isOwnProfile,
}) => {
  return (
    <div className="border-b border-gray-200">
      {/* Modified to have consistent width with the avatar section */}
      <nav className="flex overflow-x-auto px-6 md:pl-[136px]"> {/* Adjusted padding to match the avatar width + margin (24px + 6px margin) */}
          <ProfileTabButton
          active={activeTab === 'history'}
          onClick={() => onTabChange('history')}
          icon={<History className="h-4 w-4" />}
          label="Rating History"
        />

        <ProfileTabButton
            active={activeTab === 'lists'}
            onClick={() => onTabChange('lists')}
            icon={<ListMusic className="h-4 w-4" />}
            label="Lists"
        />

        <ProfileTabButton
            active={activeTab === 'grading-methods'}
            onClick={() => onTabChange('grading-methods')}
            icon={<Scale className="h-4 w-4" />}
            label="Grading Methods"
        />
        
        <ProfileTabButton
          active={activeTab === 'preferences'}
          onClick={() => onTabChange('preferences')}
          icon={<UserCog className="h-4 w-4" />}
          label="Preferences"
        />

        <ProfileTabButton
          active={activeTab === 'following'}
          onClick={() => onTabChange('following')}
          icon={<UserPlus className="h-4 w-4" />}
          label="Following"
        />

        <ProfileTabButton
          active={activeTab === 'followers'}
          onClick={() => onTabChange('followers')}
          icon={<Users className="h-4 w-4" />}
          label="Followers"
        />

        {isOwnProfile && (
          <ProfileTabButton
            active={activeTab === 'settings'}
            onClick={() => onTabChange('settings')}
            icon={<Settings className="h-4 w-4" />}
            label="Settings"
          />
        )}
      </nav>
    </div>
  );
};

export default ProfileTabs;