import { User } from 'lucide-react';
import EmptyState from '../common/EmptyState';

interface ErrorStateProps {
    error: string;
}

const ArtistErrorState = ({ error }: ErrorStateProps) => {
    return (
        <div className="max-w-4xl mx-auto py-8">
            <EmptyState
                title="Failed to load artist"
                message={error}
                icon={<User className="h-12 w-12 text-gray-400" />}
                action={{
                    label: "Try Again",
                    onClick: () => window.location.reload()
                }}
            />
        </div>
    );
};

export default ArtistErrorState;