import { ReactNode } from 'react';
import { useNavigate } from 'react-router-dom';
import { Music, MessageSquare, ListMusic, History } from 'lucide-react';
import EmptyState from '../common/EmptyState';
import { AlbumDetail, TrackSummary } from '../../api/catalog';
import AlbumTrackList from './AlbumTrackList';
import ItemHistoryComponent from '../common/ItemHistoryComponent';
import ItemReviewsComponent from '../common/ItemReviewsComponent';
import useAuthStore from '../../store/authStore';

interface AlbumContentTabsProps {
    activeTab: 'tracks' | 'reviews' | 'lists' | 'my-history';
    setActiveTab: (tab: 'tracks' | 'reviews' | 'lists' | 'my-history') => void;
    album: AlbumDetail;
    playingTrack: string | null;
    hoveredTrack: string | null;
    setHoveredTrack: (trackId: string | null) => void;
    handlePreviewToggle: (track: TrackSummary) => Promise<void>;
    handleTrackInteraction: (track: TrackSummary) => void;
    handleAlbumInteraction: () => void;
    tracksTotal: number;
    tracksOffset: number;
    loadingMoreTracks: boolean;
    onLoadMoreTracks: () => void;
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
                              handleAlbumInteraction,
                              tracksTotal,
                              tracksOffset,
                              loadingMoreTracks,
                              onLoadMoreTracks
                          }: AlbumContentTabsProps) => {
    const navigate = useNavigate();
    const { isAuthenticated } = useAuthStore();

    return (
        <div className="bg-white rounded-lg shadow-md overflow-hidden mb-8">
            <div className="border-b border-gray-200 overflow-x-auto">
                <nav className="flex -mb-px whitespace-nowrap">
                    <TabButton
                        active={activeTab === 'tracks'}
                        onClick={() => setActiveTab('tracks')}
                        icon={<Music className="h-4 w-4 mr-1 sm:mr-2" />}
                        label="Tracks"
                    />
                    <TabButton
                        active={activeTab === 'reviews'}
                        onClick={() => setActiveTab('reviews')}
                        icon={<MessageSquare className="h-4 w-4 mr-1 sm:mr-2" />}
                        label="Reviews"
                    />
                    <TabButton
                        active={activeTab === 'lists'}
                        onClick={() => setActiveTab('lists')}
                        icon={<ListMusic className="h-4 w-4 mr-1 sm:mr-2" />}
                        label="Lists"
                    />
                    <TabButton
                        active={activeTab === 'my-history'}
                        onClick={() => setActiveTab('my-history')}
                        icon={<History className="h-4 w-4 mr-1 sm:mr-2" />}
                        label="History"
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
                    tracksTotal={tracksTotal}
                    tracksOffset={tracksOffset}
                    loadingMoreTracks={loadingMoreTracks}
                    onLoadMore={onLoadMoreTracks}
                />
            )}

            {/* Reviews Tab Content */}
            {activeTab === 'reviews' && (
                <div className="p-4 sm:p-6">
                    <ItemReviewsComponent
                        itemId={album.spotifyId}
                        itemType="Album"
                        onWriteReview={handleAlbumInteraction}
                    />
                </div>
            )}

            {/* Lists Tab Content */}
            {activeTab === 'lists' && (
                <div className="p-4 sm:p-6">
                    <EmptyState
                        title="Not in any lists yet"
                        message="This album hasn't been added to any lists yet."
                        icon={<ListMusic className="h-10 w-10 sm:h-12 sm:w-12 text-gray-400" />}
                        action={{
                            label: "Create List",
                            onClick: () => navigate('/lists/create')
                        }}
                    />
                </div>
            )}

            {/* History Tab Content */}
            {activeTab === 'my-history' && (
                <div className="p-4 sm:p-6">
                    {isAuthenticated ? (
                        <ItemHistoryComponent itemId={album.spotifyId} itemType="Album" />
                    ) : (
                        <EmptyState
                            title="Please log in"
                            message="You need to be logged in to see your history with this album."
                            icon={<History className="h-10 w-10 sm:h-12 sm:w-12 text-gray-400" />}
                            action={{
                                label: "Log In",
                                onClick: () => navigate('/login', { state: { from: `/album/${album.spotifyId}` } })
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
            className={`py-3 px-3 sm:px-6 border-b-2 font-medium text-xs sm:text-sm flex items-center ${
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