import { Loader } from 'lucide-react';

interface ProfileLoadingStateProps {
  message?: string;
  icon?: React.ReactNode;
  height?: string;
}

const ProfileLoadingState: React.FC<ProfileLoadingStateProps> = ({
  message = 'Loading...',
  icon = <Loader className="h-10 w-10 text-primary-600 animate-spin mb-4" />,
  height = 'h-64'
}) => {
  return (
    <div className={`flex justify-center items-center ${height}`}>
      <div className="flex flex-col items-center">
        {icon}
        <p className="text-gray-600">{message}</p>
      </div>
    </div>
  );
};

export default ProfileLoadingState;