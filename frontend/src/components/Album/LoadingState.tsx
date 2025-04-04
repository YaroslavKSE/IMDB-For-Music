import { Loader } from 'lucide-react';

const LoadingState = () => {
    return (
        <div className="flex justify-center items-center py-20">
            <Loader className="h-10 w-10 text-primary-600 animate-spin" />
            <span className="ml-3 text-lg text-gray-600">Loading album details...</span>
        </div>
    );
};

export default LoadingState;