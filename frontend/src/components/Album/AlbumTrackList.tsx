import { Link } from 'react-router-dom';
import { Play, Pause, Star } from 'lucide-react';
import { formatDuration } from '../../utils/formatters';
import { TrackSummary } from '../../api/catalog';
import AudioVisualizer from './AudioVisualizer';

interface AlbumTrackListProps {
    tracks: TrackSummary[];
    playingTrack: string | null;
    hoveredTrack: string | null;
    handlePreviewToggle: (trackId: string) => Promise<void>;
    handleTrackInteraction: (trackId: string, trackName: string) => void;
    setHoveredTrack: (trackId: string | null) => void;
}

const AlbumTrackList = ({
                            tracks,
                            playingTrack,
                            hoveredTrack,
                            handlePreviewToggle,
                            handleTrackInteraction,
                            setHoveredTrack
                        }: AlbumTrackListProps) => {
    return (
        <div className="divide-y divide-gray-200">
            {tracks.map((track, index) => (
                <div
                    key={track.spotifyId}
                    className={`flex items-center px-6 py-3 hover:bg-gray-50 ${index % 2 === 0 ? 'bg-white' : 'bg-gray-50'}`}
                    onMouseEnter={() => setHoveredTrack(track.spotifyId)}
                    onMouseLeave={() => setHoveredTrack(null)}
                >
                    <div
                        className="w-8 text-center relative"
                    >
                        {/* Playing animation when track is playing but not hovered */}
                        {playingTrack === track.spotifyId && hoveredTrack !== track.spotifyId ? (
                            <AudioVisualizer />
                        ) : hoveredTrack === track.spotifyId ? (
                            <button
                                onClick={() => handlePreviewToggle(track.spotifyId)}
                                className="absolute inset-0 flex items-center justify-center text-primary-600 hover:text-primary-800"
                                title={playingTrack === track.spotifyId ? "Stop preview" : "Play preview"}
                            >
                                {playingTrack === track.spotifyId ? (
                                    <Pause className="h-5 w-5 fill-current" />
                                ) : (
                                    <Play className="h-5 w-5 fill-current" />
                                )}
                            </button>
                        ) : (
                            <span className="text-gray-500 font-medium">
                {track.trackNumber || index + 1}
              </span>
                        )}
                    </div>

                    <div className="flex-grow min-w-0 ml-4">
                        <div className="flex items-center">
                            <Link
                                to={`/track/${track.spotifyId}`}
                                className="text-gray-900 font-medium hover:text-primary-600 truncate flex items-center"
                            >
                                {track.name}
                            </Link>

                            {track.isExplicit && (
                                <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                  E
                </span>
                            )}
                        </div>

                        <div className="text-sm text-gray-500 truncate">
                            {track.artistName}
                        </div>
                    </div>

                    <div className="ml-auto flex items-center">
            <span className="text-sm text-gray-500 mr-4">
              {formatDuration(track.durationMs)}
            </span>

                        <button
                            onClick={() => handleTrackInteraction(track.spotifyId, track.name)}
                            className="text-gray-400 hover:text-primary-600 focus:outline-none"
                        >
                            <Star className="h-5 w-5" />
                        </button>
                    </div>
                </div>
            ))}
        </div>
    );
};

export default AlbumTrackList;