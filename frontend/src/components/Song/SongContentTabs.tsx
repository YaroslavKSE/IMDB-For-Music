import { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { MessageSquare, History, ListMusic } from 'lucide-react';
import EmptyState from '../common/EmptyState';

interface SongContentTabsProps {
    activeTab: 'reviews' | 'lists' | 'my-history';
    setActiveTab: (tab: 'reviews' | 'lists' | 'my-history') => void;
    handleTrackInteraction: () => void;
}

const SongContentTabs = ({
                             activeTab,
                             setActiveTab,
                             handleTrackInteraction
                         }: SongContentTabsProps) => {
    const navigate = useNavigate();

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden mb-8">
            <div className="border-b border-gray-200">
                <nav className="flex -mb-px">
                    <TabButton
                        active={activeTab === 'reviews'}
                        onClick={() => setActiveTab('reviews')}
                        icon={<MessageSquare className="h-4 w-4 mr-2" />}
                        label="Reviews"
                    />
                    <TabButton
                        active={activeTab === 'lists'}
                        onClick={() => setActiveTab('lists')}
                        icon={<ListMusic className="h-4 w-4 mr-2" />}
                        label="In Lists"
                    />
                    <TabButton
                        active={activeTab === 'my-history'}
                        onClick={() => setActiveTab('my-history')}
                        icon={<History className="h-4 w-4 mr-2" />}
                        label="My History"
                    />
                </nav>
            </div>

            {/* Reviews Tab Content */}
            {activeTab === 'reviews' && (
                <div className="p-6">
                    <EmptyState
                        title="No reviews yet"
                        message="Be the first to share your thoughts about this track."
                        icon={<MessageSquare className="h-12 w-12 text-gray-400" />}
                        action={{
                            label: "Write a Review",
                            onClick: () => handleTrackInteraction()
                        }}
                    />
                </div>
            )}

            {/* Lists Tab Content */}
            {activeTab === 'lists' && (
                <div className="p-6">
                    <EmptyState
                        title="Not in any lists yet"
                        message="This track hasn't been added to any lists yet."
                        icon={<ListMusic className="h-12 w-12 text-gray-400" />}
                        action={{
                            label: "Create a List",
                            onClick: () => navigate('/lists/create')
                        }}
                    />
                </div>
            )}

            {/* History Tab Content */}
            {activeTab === 'my-history' && (
                <div className="p-6">
                    <EmptyState
                        title="No history"
                        message="You haven't interacted with this track yet."
                        icon={<History className="h-12 w-12 text-gray-400" />}
                        action={{
                            label: "Log interaction",
                            onClick: () => handleTrackInteraction()
                        }}
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

export default SongContentTabs;