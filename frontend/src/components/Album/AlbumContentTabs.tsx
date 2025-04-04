import { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { Music, MessageSquare, ListMusic, History } from 'lucide-react';
import EmptyState from '../common/EmptyState';
import { AlbumDetail } from '../../api/catalog';
import AlbumTrackList from './AlbumTrackList';

interface AlbumContentTabsProps {
    activeTab: 'tracks' | 'reviews' | 'lists' | 'my-history';
    setActiveTab: (tab: 'tracks' | 'reviews' | 'lists' | 'my-history') => void;
    album: AlbumDetail;
    playingTrack: string | null;
    hoveredTrack: string | null;
    setHoveredTrack: (trackId: string | null) => void;
    handlePreviewToggle: (trackId: string) => Promise<void>;
    handleTrackInteraction: (trackId: string, trackName: string) => void;
    handleAlbumInteraction: () => void;
}

const AlbumContentTabs = ({
                              activeTab,
                              setActiveTab,
                              album,
                              playingTrack,
                              hoveredTrack,
                              setHoveredTrack,
                              handlePreviewToggle,
                              handleTrackInteraction,
                              handleAlbumInteraction
                          }: AlbumContentTabsProps) => {
    const navigate = useNavigate();

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden mb-8">
            <div className="border-b border-gray-200">
                <nav className="flex -mb-px">
                    <TabButton
                        active={activeTab === 'tracks'}
                        onClick={() => setActiveTab('tracks')}
                        icon={<Music className="h-4 w-4 mr-2" />}
                        label="Tracklist"
                    />
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

            {/* Tracks Tab Content */}
            {activeTab === 'tracks' && (
                <AlbumTrackList
                    tracks={album.tracks}
                    playingTrack={playingTrack}
                    hoveredTrack={hoveredTrack}
                    setHoveredTrack={setHoveredTrack}
                    handlePreviewToggle={handlePreviewToggle}
                    handleTrackInteraction={handleTrackInteraction}
                />
            )}

            {/* Reviews Tab Content */}
            {activeTab === 'reviews' && (
                <div className="p-6">
                    <EmptyState
                        title="No reviews yet"
                        message="Be the first to share your thoughts about this album."
                        icon={<MessageSquare className="h-12 w-12 text-gray-400" />}
                        action={{
                            label: "Write a Review",
                            onClick: () => handleAlbumInteraction()
                        }}
                    />
                </div>
            )}

            {/* Lists Tab Content */}
            {activeTab === 'lists' && (
                <div className="p-6">
                    <EmptyState
                        title="Not in any lists yet"
                        message="This album hasn't been added to any lists yet."
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
                        message="You haven't interacted with this album."
                        icon={<History className="h-12 w-12 text-gray-400" />}
                        action={{
                            label: "Log interaction",
                            onClick: () => handleAlbumInteraction()
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

export default AlbumContentTabs;