import { Music } from 'lucide-react';
import EmptyState from '../common/EmptyState';

const NotFoundState = () => {
    return (
        <div className="max-w-4xl mx-auto py-8">
            <EmptyState
                title="Track Not Found"
                message="We couldn't find the track you're looking for."
                icon={<Music className="h-12 w-12 text-gray-400" />}
            />
        </div>
    );
};

export default NotFoundState;