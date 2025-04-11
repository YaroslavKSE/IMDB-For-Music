import {TrackSummary} from "../../api/catalog.ts";
import {useState} from "react";
import {Disc2, Pause, Play} from "lucide-react";
import {Link} from "react-router-dom";
import {formatDuration} from "../../utils/formatters.ts";

interface TrackRowProps {
    track: TrackSummary;
    index: number;
    isPlaying: boolean;
    onPlayClick: () => void;
}

const TrackRow = ({ track, index, isPlaying, onPlayClick }: TrackRowProps) => {
    const [isHovered, setIsHovered] = useState(false);

    return (
        <div
            className={`flex items-center px-6 py-5 ${
                index % 2 === 0 ? 'bg-white' : 'bg-gray-50'
            } hover:bg-gray-100`}
            onMouseEnter={() => setIsHovered(true)}
            onMouseLeave={() => setIsHovered(false)}
        >
            {/* Track number or play button */}
            <div className="w-8 flex-shrink-0 flex items-center justify-center mr-2">
                {isHovered || isPlaying ? (
                    <button
                        onClick={onPlayClick}
                        className="w-8 h-8 flex items-center justify-center text-primary-600"
                    >
                        {isPlaying ? (
                            <Pause className="h-5 w-5" />
                        ) : (
                            <Play className="h-5 w-5 fill-current" />
                        )}
                    </button>
                ) : (
                    <span className="text-gray-500 font-medium">{index + 1}</span>
                )}
            </div>

            {/* Track image */}
            <div className="w-16 h-16 flex-shrink-0 mr-4">
                <img
                    src={track.imageUrl || '/placeholder-album.jpg'}
                    alt={track.name}
                    className="w-full h-full object-cover shadow"
                />
            </div>

            {/* Track info */}
            <div className="flex-grow min-w-0">
                <Link to={`/track/${track.spotifyId}`} className="block">
                    <h4 className={`text-base font-medium truncate flex items-center ${
                        isHovered ? 'text-primary-600' : 'text-gray-900'
                    } transition-colors duration-200`}>
                        {track.name}
                        {track.isExplicit && (
                            <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                                E
                            </span>
                        )}
                    </h4>
                </Link>
                <p className="text-sm text-gray-500 truncate mt-1">{track.artistName}</p>
            </div>

            {/* Track duration */}
            <div className="ml-4 flex-shrink-0 flex items-center">
                <span className="text-sm text-gray-500 mr-4">{formatDuration(track.durationMs)}</span>

                {/* Album button - redirects to album page instead of track page */}
                <Link
                    to={`/album/${track.albumId}`}
                    className="text-gray-400 hover:text-primary-600 focus:outline-none"
                    title="View Album"
                >
                    <Disc2 className="h-6 w-6" />
                </Link>
            </div>
        </div>
    );
};

export default TrackRow;