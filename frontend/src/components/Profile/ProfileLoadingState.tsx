import { Loader } from 'lucide-react';

const ProfileLoadingState = () => {
  return (
    <div className="flex justify-center items-center h-64">
      <div className="flex flex-col items-center">
        <Loader className="h-10 w-10 text-primary-600 animate-spin mb-4" />
        <p className="text-gray-600">Loading your profile...</p>
      </div>
    </div>
  );
};

export default ProfileLoadingState;