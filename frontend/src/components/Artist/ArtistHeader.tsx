import { Heart, Share, Music, Users } from 'lucide-react';
import { ArtistDetail } from '../../api/catalog';
import { formatNumberWithCommas } from '../../utils/formatters';

interface ArtistHeaderProps {
    artist: ArtistDetail;
    handleArtistInteraction?: () => void;
}

const ArtistHeader = ({ artist }: ArtistHeaderProps) => {
    const formatFollowers = (count: number): string => {
        return formatNumberWithCommas(count);
    };

    // Function to generate a CSS gradient based on artist popularity
    const getPopularityGradient = (popularity: number) => {
        if (popularity >= 90) {
            return 'from-purple-500 to-purple-700';
        } else if (popularity >= 80) {
            return 'from-red-500 to-red-700';
        } else if (popularity >= 70) {
            return 'from-orange-500 to-orange-700';
        } else if (popularity >= 60) {
            return 'from-yellow-500 to-yellow-700';
        } else if (popularity >= 50) {
            return 'from-green-500 to-green-700';
        } else if (popularity >= 40) {
            return 'from-teal-500 to-teal-700';
        } else {
            return 'from-blue-500 to-blue-700';
        }
    };

    return (
        <div className="flex flex-col md:flex-row gap-8 mb-8">
            {/* Artist Image */}
            <div className="w-full md:w-64 flex-shrink-0">
                <div className="aspect-square w-full shadow-md rounded-full overflow-hidden border-4 border-white">
                    <img
                        src={artist.imageUrl || '/placeholder-artist.jpg'}
                        alt={artist.name}
                        className="w-full h-full object-cover"
                    />
                </div>

                {/* Action Buttons */}
                <div className="mt-4 flex justify-between space-x-2">
                    <button className="flex-1 flex items-center justify-center py-2 px-4 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none">
                        <Heart className="h-4 w-4 mr-2" />
                        Follow
                    </button>

                    <button className="flex items-center justify-center p-2 border border-gray-300 text-sm font-medium rounded-md text-gray-700 bg-white hover:bg-gray-50 focus:outline-none">
                        <Share className="h-4 w-4" />
                    </button>
                </div>
            </div>

            {/* Artist Info */}
            <div className="flex-grow">
                <div className="flex items-center text-gray-500 text-sm mb-2">
                    <span className="uppercase bg-gray-200 rounded px-2 py-0.5">
                        Artist
                    </span>
                </div>

                <h1 className="text-3xl md:text-4xl font-bold text-gray-900 mb-4">{artist.name}</h1>

                <div className="grid grid-cols-1 md:grid-cols-2 gap-4 mb-6">
                    {/* Followers */}
                    <div className="bg-gradient-to-br from-primary-50 to-primary-100 p-4 rounded-lg shadow-sm border border-primary-200">
                        <div className="flex items-center">
                            <Users className="h-6 w-6 text-primary-600 mr-3" />
                            <div>
                                <div className="text-sm font-medium text-primary-700">Spotify Followers</div>
                                <div className="text-2xl font-bold text-primary-800">
                                    {formatFollowers(artist.followersCount)}
                                </div>
                            </div>
                        </div>
                    </div>

                    {/* Popularity */}
                    <div className={`bg-gradient-to-br ${getPopularityGradient(artist.popularity || 0)} p-4 rounded-lg shadow-sm`}>
                        <div className="flex items-center">
                            <Music className="h-6 w-6 text-white mr-3" />
                            <div>
                                <div className="text-sm font-medium text-white opacity-90">Popularity</div>
                                <div className="text-2xl font-bold text-white">
                                    {artist.popularity || 0}/100
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                {/* External links */}
                {artist.externalUrls && artist.externalUrls.length > 0 && (
                    <div className="mt-4">
                        <a
                            href={`https://open.spotify.com/artist/${artist.spotifyId}`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="inline-flex items-center text-sm text-gray-600 hover:text-primary-600"
                        >
                            {/* Spotify logo SVG */}
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
                    </div>
                )}
            </div>
        </div>
    );
};

export default ArtistHeader;