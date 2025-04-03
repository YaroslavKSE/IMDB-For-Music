import {TrackSummary} from "../../api/catalog.ts";
import {formatDuration} from "../../utils/formatters.ts";
import {Disc2} from "lucide-react";
import { useState } from "react";

const TrackRow = ({ track, index }: { track: TrackSummary; index: number }) => {
    const [isHovered, setIsHovered] = useState(false);

    return (
        <div className={`flex items-center px-6 py-5 ${
            index % 2 === 0 ? 'bg-white' : 'bg-gray-50'
        }`}>
            <div className="flex-shrink-0 mr-6">
                <img
                    src={track.imageUrl || '/placeholder-album.jpg'}
                    alt={track.name}
                    className="w-16 h-16 object-cover shadow"
                />
            </div>
            <div className="min-w-0 flex-1">
                <div className="flex items-center justify-between">
                    <div className="min-w-0 flex-1">
                        <a
                            href={`/track/${track.spotifyId}`}
                            className="block"
                            onMouseEnter={() => setIsHovered(true)}
                            onMouseLeave={() => setIsHovered(false)}
                        >
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
                            <p className="text-sm text-gray-500 truncate mt-1">{track.artistName}</p>
                        </a>
                    </div>
                    <div className="ml-4 flex-shrink-0 flex items-center">
                        <span className="text-sm text-gray-500">{formatDuration(track.durationMs)}</span>
                        <a
                            href={`/album/${track.albumId}`}
                            className="ml-4 text-gray-500 hover:text-primary-600 focus:outline-none"
                            title="View Album"
                        >
                            <Disc2 className="h-6 w-6" />
                        </a>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default TrackRow;