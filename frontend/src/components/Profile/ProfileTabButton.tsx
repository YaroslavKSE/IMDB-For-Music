import React, { ReactNode } from 'react';

// type TabType = 'overview' | 'grading-methods' | 'history' | 'settings';

interface ProfileTabButtonProps {
  active: boolean;
  onClick: () => void;
  icon: ReactNode;
  label: string;
}

const ProfileTabButton: React.FC<ProfileTabButtonProps> = ({
  active,
  onClick,
  icon,
  label
}) => {
  return (
    <button
      onClick={onClick}
      className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
        active
          ? 'border-primary-600 text-primary-600'
          : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
      }`}
    >
      {icon}
      <span className="ml-2">{label}</span>
    </button>
  );
};

export default ProfileTabButton;