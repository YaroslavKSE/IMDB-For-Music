import { Link } from 'react-router-dom';
import {
    Heart,
    Star,
    Share,
    Disc,
    Calendar,
    Clock,
    Music,
    Play,
    Pause
} from 'lucide-react';
import { TrackDetail } from '../../api/catalog';
import { formatDuration, formatDate } from '../../utils/formatters';

interface SongHeaderProps {
    track: TrackDetail;
    isPlaying: boolean;
    handlePreviewToggle: () => Promise<void>;
    handleTrackInteraction: () => void;
}

const SongHeader = ({
                        track,
                        isPlaying,
                        handlePreviewToggle,
                        handleTrackInteraction
                    }: SongHeaderProps) => {
    return (
        <div className="flex flex-col md:flex-row gap-8 mb-8">
            {/* Track Artwork (from album) */}
            <div className="w-full md:w-64 flex-shrink-0">
                <div className="aspect-square w-full shadow-md rounded-lg overflow-hidden">
                    <img
                        src={track.imageUrl || track.album?.imageUrl || '/placeholder-album.jpg'}
                        alt={track.name}
                        className="w-full h-full object-cover"
                    />
                </div>

                {/* Primary Track Action Button */}
                <div className="mt-3">
                    <button
                        onClick={handleTrackInteraction}
                        className="w-full flex items-center justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
                    >
                        <Star className="h-4 w-4 mr-2" />
                        Rate Track
                    </button>
                </div>

                <div className="mt-3 flex justify-between space-x-2">
                    <button className="flex-1 flex items-center justify-center py-2 px-4 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none">
                        <Heart className="h-4 w-4 mr-2" />
                        Add to List
                    </button>

                    <button className="flex items-center justify-center p-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none">
                        <Share className="h-4 w-4" />
                    </button>
                </div>
            </div>

            {/* Track Info */}
            <div className="flex-grow">
                <div className="flex items-center text-gray-500 text-sm mb-2">
                    <span className="uppercase bg-gray-200 rounded px-2 py-0.5">
                        Track
                    </span>

                    {track.isExplicit && (
                        <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
                            Explicit
                        </span>
                    )}

                    <span className="ml-2 flex items-center">
                        <Clock className="h-3.5 w-3.5 mr-1" />
                        {formatDuration(track.durationMs)}
                    </span>
                </div>

                <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-2">{track.name}</h1>

                <div className="flex items-center mb-4">
                    {track.artists && track.artists.length > 0 ? (
                        <div className="flex flex-wrap items-center">
                            {track.artists.map((artist, index) => (
                                <span key={artist.spotifyId}>
                                    <Link
                                        to={`/artist/${artist.spotifyId}`}
                                        className="text-lg font-medium text-primary-600 hover:underline"
                                    >
                                        {artist.name}
                                    </Link>
                                    {index < track.artists.length - 1 && <span className="mx-1">,</span>}
                                </span>
                            ))}
                        </div>
                    ) : (
                        <span className="text-lg font-medium text-primary-600">{track.artistName}</span>
                    )}
                </div>

                <div className="mb-6">
                    <div className="flex items-center text-gray-600 mb-2">
                        <Disc className="h-4 w-4 mr-2" />
                        <span className="mr-1">From the album:</span>
                        <Link
                            to={`/album/${track.album?.spotifyId || track.albumId}`}
                            className="font-medium text-primary-600 hover:underline"
                        >
                            {track.album?.name || "Unknown Album"}
                        </Link>
                    </div>

                    {track.trackNumber && (
                        <div className="flex items-center text-gray-600">
                            <Music className="h-4 w-4 mr-2" />
                            <span>Track {track.trackNumber}</span>
                            {track.discNumber && track.discNumber > 1 && (
                                <span className="ml-1">on Disc {track.discNumber}</span>
                            )}
                        </div>
                    )}

                    {track.album?.releaseDate && (
                        <div className="flex items-center text-gray-600 mt-2">
                            <Calendar className="h-4 w-4 mr-2" />
                            <span>Released: {formatDate(track.album.releaseDate)}</span>
                        </div>
                    )}
                </div>

                {/* External links */}
                <div className="mt-4 flex flex-wrap gap-4 border-t border-gray-200 pt-2">
                    {track.externalUrls && track.externalUrls.length > 0 && (
                        <a
                            href={`https://open.spotify.com/track/${track.spotifyId}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="inline-flex items-center text-sm text-gray-600 hover:text-primary-600"
                        >
                            {/* Spotify logo SVG instead of ExternalLink icon */}
                            <svg
                                className="h-5 w-5 mr-1"
                                viewBox="0 0 24 24"
                                fill="none"
                            >
                                <circle cx="12" cy="12" r="12" fill="#1DB954" />
                                <path
                                    d="M17.9 10.9C14.7 9 9.35 8.8 6.3 9.75C5.8 9.9 5.3 9.6 5.15 9.15C5 8.65 5.3 8.15 5.75 8C9.3 6.95 15.15 7.15 18.85 9.35C19.3 9.6 19.45 10.2 19.2 10.65C18.95 11 18.35 11.15 17.9 10.9ZM17.8 13.9C17.55 14.25 17.1 14.35 16.75 14.1C14.05 12.45 9.95 11.9 6.8 12.85C6.4 12.95 5.95 12.75 5.85 12.35C5.75 11.95 5.95 11.5 6.35 11.4C10 10.35 14.5 10.95 17.6 12.85C17.9 13 18.05 13.5 17.8 13.9ZM16.6 16.8C16.4 17.1 16.05 17.2 15.75 17C13.4 15.55 10.45 15.3 6.95 16.1C6.6 16.2 6.3 15.95 6.2 15.65C6.1 15.3 6.35 15 6.65 14.9C10.45 14.1 13.75 14.35 16.35 16C16.7 16.15 16.75 16.5 16.6 16.8Z"
                                    fill="white"
                                />
                            </svg>
                            Listen on Spotify
                        </a>
                    )}

                    {/* Preview Play Button */}
                    <button
                        onClick={handlePreviewToggle}
                        className="inline-flex items-center text-sm text-gray-600 hover:text-primary-600"
                    >
                        {isPlaying ? (
                            <>
                                <Pause className="h-4 w-4 mr-1" />
                                Stop Preview
                            </>
                        ) : (
                            <>
                                <Play className="h-4 w-4 mr-1 fill-current" />
                                Play Preview
                            </>
                        )}
                    </button>
                </div>
            </div>
        </div>
    );
};

export default SongHeader;