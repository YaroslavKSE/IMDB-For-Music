import { ReactNode } from 'react';
import { Disc, Music } from 'lucide-react';
import EmptyState from '../common/EmptyState';

interface ArtistContentTabsProps {
    activeTab: 'overview' | 'albums' | 'top-tracks' | 'reviews';
    setActiveTab: (tab: 'overview' | 'albums' | 'top-tracks' | 'reviews') => void;
}

const ArtistContentTabs = ({
                               activeTab,
                               setActiveTab
                           }: ArtistContentTabsProps) => {

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden mb-8">
            <div className="border-b border-gray-200">
                <nav className="flex -mb-px">
                    <TabButton
                        active={activeTab === 'overview'}
                        onClick={() => setActiveTab('overview')}
                        icon={<User className="h-4 w-4 mr-2" />}
                        label="Overview"
                    />
                    <TabButton
                        active={activeTab === 'albums'}
                        onClick={() => setActiveTab('albums')}
                        icon={<Disc className="h-4 w-4 mr-2" />}
                        label="Albums"
                    />
                    <TabButton
                        active={activeTab === 'top-tracks'}
                        onClick={() => setActiveTab('top-tracks')}
                        icon={<Music className="h-4 w-4 mr-2" />}
                        label="Top Tracks"
                    />
                </nav>
            </div>

            {/* Overview Tab Content */}
            {activeTab === 'overview' && (
                <div className="p-6">
                    <EmptyState
                        title="Artist Overview"
                        message="More detailed artist information will be displayed here in the future."
                        icon={<User className="h-12 w-12 text-gray-400" />}
                    />
                </div>
            )}

            {/* Albums Tab Content */}
            {activeTab === 'albums' && (
                <div className="p-6">
                    <EmptyState
                        title="No albums loaded yet"
                        message="Albums from this artist will appear here."
                        icon={<Disc className="h-12 w-12 text-gray-400" />}
                    />
                </div>
            )}

            {/* Top Tracks Tab Content */}
            {activeTab === 'top-tracks' && (
                <div className="p-6">
                    <EmptyState
                        title="No tracks loaded yet"
                        message="Top tracks from this artist will appear here."
                        icon={<Music className="h-12 w-12 text-gray-400" />}
                    />
                </div>
            )}
        </div>
    );
};

// Helper component for tabs
interface TabButtonProps {
    active: boolean;
    onClick: () => void;
    icon: ReactNode;
    label: string;
}

const TabButton = ({ active, onClick, icon, label }: TabButtonProps) => {
    return (
        <button
            onClick={onClick}
            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center ${
                active
                    ? 'border-primary-600 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
        >
            {icon}
            {label}
        </button>
    );
};

// Import the missing User icon
import { User } from 'lucide-react';

export default ArtistContentTabs;