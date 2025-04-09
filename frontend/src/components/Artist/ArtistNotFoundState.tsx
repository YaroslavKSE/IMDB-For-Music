import { User } from 'lucide-react';
import EmptyState from '../common/EmptyState';

const ArtistNotFoundState = () => {
    return (
        <div className="max-w-4xl mx-auto py-8">
            <EmptyState
                title="Artist Not Found"
                message="We couldn't find the artist you're looking for."
                icon={<User className="h-12 w-12 text-gray-400" />}
            />
        </div>
    );
};

export default ArtistNotFoundState;