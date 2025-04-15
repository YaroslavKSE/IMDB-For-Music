import { useState } from 'react';
import { UserProfile } from '../../api/auth';
import ProfileInfoCard from './ProfileInfoCard';
import ProfileEditForm from './ProfileEditForm';

interface ProfileOverviewTabProps {
  user: UserProfile;
}

const ProfileOverviewTab = ({ user }: ProfileOverviewTabProps) => {
  const [isEditing, setIsEditing] = useState(false);

  const handleStartEditing = () => {
    setIsEditing(true);
  };

  const handleCancelEditing = () => {
    setIsEditing(false);
  };

  const handleEditSuccess = () => {
    setIsEditing(false);
  };

  return (
    <div className="space-y-6">
      {isEditing ? (
        <ProfileEditForm onCancel={handleCancelEditing} onSuccess={handleEditSuccess} />
      ) : (
        <ProfileInfoCard
          user={user}
          onEdit={handleStartEditing}
        />
      )}
    </div>
  );
};

export default ProfileOverviewTab;