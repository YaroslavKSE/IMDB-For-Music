import { ReactNode } from 'react';
import { useNavigate, useParams } from 'react-router-dom';
import { MessageSquare, History, ListMusic } from 'lucide-react';
import EmptyState from '../common/EmptyState';
import ItemHistoryComponent from '../common/ItemHistoryComponent';
import ItemReviewsComponent from '../common/ItemReviewsComponent';
import useAuthStore from '../../store/authStore';

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
    const { id } = useParams<{ id: string }>();
    const { isAuthenticated } = useAuthStore();

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
                        label="History"
                    />
                </nav>
            </div>

            {/* Reviews Tab Content */}
            {activeTab === 'reviews' && (
                <div className="p-6">
                    {id ? (
                        <ItemReviewsComponent
                            itemId={id}
                            itemType="Track"
                            onWriteReview={handleTrackInteraction}
                        />
                    ) : (
                        <EmptyState
                            title="Track not found"
                            message="Unable to load track information."
                            icon={<MessageSquare className="h-12 w-12 text-gray-400" />}
                        />
                    )}
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
                    {isAuthenticated && id ? (
                        <ItemHistoryComponent itemId={id} itemType="Track" onLogInteraction={handleTrackInteraction} />
                    ) : (
                        <EmptyState
                            title={isAuthenticated ? "No history" : "Please log in"}
                            message={isAuthenticated
                                ? "You haven't interacted with this track yet."
                                : "You need to be logged in to see your history with this track."}
                            icon={<History className="h-12 w-12 text-gray-400" />}
                            action={{
                                label: isAuthenticated ? "Log interaction" : "Log In",
                                onClick: isAuthenticated
                                    ? handleTrackInteraction
                                    : () => navigate('/login', { state: { from: `/track/${id}` } })
                            }}
                        />
                    )}
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