import { useEffect, useState, useCallback } from 'react';
import { Disc, Music, User, ChevronRight} from 'lucide-react';
import { ArtistDetail, TrackSummary, AlbumSummary } from '../../api/catalog';
import CatalogService from '../../api/catalog';
import EmptyState from '../common/EmptyState';
import AlbumCard from "./AlbumCard.tsx";
import TrackRow from "./TrackRow.tsx";
import TabButton from "./TabButton.tsx";
import {getTrackPreviewUrl} from "../../utils/preview-extractor.ts";

interface ArtistContentTabsProps {
    activeTab: 'overview' | 'albums' | 'top-tracks';
    setActiveTab: (tab: 'overview' | 'albums' | 'top-tracks') => void;
    artist: ArtistDetail;
}

const ArtistContentTabs = ({
                               activeTab,
                               setActiveTab,
                               artist
                           }: ArtistContentTabsProps) => {
    const [albumType, setAlbumType] = useState<'album' | 'single'>('album');
    const [topTracks, setTopTracks] = useState<TrackSummary[]>([]);
    const [albums, setAlbums] = useState<AlbumSummary[]>([]);
    const [albumsOffset, setAlbumsOffset] = useState(0);
    const [albumsTotal, setAlbumsTotal] = useState(0);
    const [isLoadingTracks, setIsLoadingTracks] = useState(false);
    const [isLoadingAlbums, setIsLoadingAlbums] = useState(false);
    const [tracksError, setTracksError] = useState<string | null>(null);
    const [albumsError, setAlbumsError] = useState<string | null>(null);
    const [playingTrack, setPlayingTrack] = useState<string | null>(null);
    const [audio, setAudio] = useState<HTMLAudioElement | null>(null);

    // Wrap the fetch functions in useCallback to prevent unnecessary re-creation
    const fetchTopTracks = useCallback(async () => {
        setIsLoadingTracks(true);
        setTracksError(null);
        try {
            const data = await CatalogService.getArtistTopTracks(artist.spotifyId);
            setTopTracks(data.tracks);
        } catch (error) {
            console.error('Error fetching top tracks:', error);
            setTracksError('Failed to load top tracks');
        } finally {
            setIsLoadingTracks(false);
        }
    }, [artist.spotifyId]);

    const fetchAlbums = useCallback(async (offset: number) => {
        setIsLoadingAlbums(true);
        setAlbumsError(null);
        try {
            const includeGroups = albumType === 'album' ? 'album' : 'single';
            const data = await CatalogService.getArtistAlbums(artist.spotifyId, 20, offset, includeGroups);
            if (offset === 0) {
                setAlbums(data.albums);
            } else {
                setAlbums(prev => [...prev, ...data.albums]);
            }
            setAlbumsTotal(data.totalResults);
            setAlbumsOffset(offset);
        } catch (error) {
            console.error('Error fetching albums:', error);
            setAlbumsError('Failed to load albums');
        } finally {
            setIsLoadingAlbums(false);
        }
    }, [artist.spotifyId, albumType]);

    // Fetch top tracks and initial albums on component mount
    useEffect(() => {
        if (artist?.spotifyId) {
            fetchTopTracks();
            fetchAlbums(0);
        }
    }, [artist?.spotifyId, fetchTopTracks, fetchAlbums]);

    const handleAlbumTypeChange = (type: 'album' | 'single') => {
        if (type !== albumType) {
            setAlbumType(type);
            setAlbums([]);
            setAlbumsOffset(0);
            // This will trigger a re-fetch due to albumType being in the dependency array of fetchAlbums
        }
    };

    // Clean up audio player when component unmounts
    useEffect(() => {
        return () => {
            if (audio) {
                audio.pause();
                audio.src = '';
            }
        };
    }, [audio]);

    const loadMoreAlbums = () => {
        const newOffset = albumsOffset + 20;
        fetchAlbums(newOffset);
    };

    const handlePreviewPlay = async (trackId: string) => {
        try {
            // Stop current audio if playing
            if (audio) {
                audio.pause();
                if (playingTrack === trackId) {
                    setPlayingTrack(null);
                    return;
                }
            }

            // Get preview URL
            const previewUrl = await getTrackPreviewUrl(trackId);
            if (!previewUrl) {
                console.error('No preview URL available');
                return;
            }

            // Create and play audio
            const newAudio = new Audio(previewUrl);
            newAudio.addEventListener('ended', () => setPlayingTrack(null));
            await newAudio.play();

            setAudio(newAudio);
            setPlayingTrack(trackId);
        } catch (error) {
            console.error('Error playing preview:', error);
        }
    };

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
                    <div className="flex flex-col space-y-8">
                        {/* Top Tracks Section */}
                        <div>
                            <div className="flex justify-between items-center mb-4">
                                <h3 className="text-lg font-bold text-gray-900 flex items-center">
                                    <Music className="h-5 w-5 mr-2 text-primary-600" />
                                    Popular Tracks
                                </h3>
                                <button
                                    onClick={() => setActiveTab('top-tracks')}
                                    className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                                >
                                    Show more <ChevronRight className="h-4 w-4 ml-1" />
                                </button>
                            </div>

                            {isLoadingTracks ? (
                                <div className="animate-pulse space-y-4">
                                    {[...Array(5)].map((_, i) => (
                                        <div key={i} className="flex items-center p-2">
                                            <div className="w-6 h-6 bg-gray-200 rounded-full mr-3"></div>
                                            <div className="flex-grow">
                                                <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                                                <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                                            </div>
                                        </div>
                                    ))}
                                </div>
                            ) : tracksError ? (
                                <div className="text-red-500 text-sm p-4">{tracksError}</div>
                            ) : topTracks.length === 0 ? (
                                <div className="text-gray-500 text-sm p-4">No tracks found</div>
                            ) : (
                                <div className="space-y-1">
                                    {topTracks.slice(0, 5).map((track, index) => (
                                        <TrackRow
                                            key={track.spotifyId}
                                            track={track}
                                            index={index}
                                            isPlaying={playingTrack === track.spotifyId}
                                            onPlayClick={() => handlePreviewPlay(track.spotifyId)}
                                        />
                                    ))}
                                </div>
                            )}
                        </div>

                        {/* Albums Section */}
                        <div>
                            <div className="flex justify-between items-center mb-4">
                                <h3 className="text-lg font-bold text-gray-900 flex items-center">
                                    <Disc className="h-5 w-5 mr-2 text-primary-600" />
                                    Albums
                                </h3>
                                <button
                                    onClick={() => setActiveTab('albums')}
                                    className="text-primary-600 hover:text-primary-800 flex items-center text-sm font-medium"
                                >
                                    Show more <ChevronRight className="h-4 w-4 ml-1" />
                                </button>
                            </div>

                            {isLoadingAlbums ? (
                                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
                                    {[...Array(5)].map((_, i) => (
                                        <div key={i} className="animate-pulse">
                                            <div className="bg-gray-200 aspect-square rounded-md mb-2"></div>
                                            <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                                            <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                                        </div>
                                    ))}
                                </div>
                            ) : albumsError ? (
                                <div className="text-red-500 text-sm p-4">{albumsError}</div>
                            ) : albums.length === 0 ? (
                                <div className="text-gray-500 text-sm p-4">No albums found</div>
                            ) : (
                                <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-5 gap-4">
                                    {albums.slice(0, 5).map(album => (
                                        <AlbumCard key={album.spotifyId} album={album} />
                                    ))}
                                </div>
                            )}
                        </div>
                    </div>
                </div>
            )}

            {/* Albums Tab Content */}
            {activeTab === 'albums' && (
                <div className="p-6">
                    <div className="flex justify-between items-center mb-6">
                        <h3 className="text-xl font-bold text-gray-900 flex items-center">
                            <Disc className="h-5 w-5 mr-2 text-primary-600" />
                            {albumType === 'album' ? 'Albums' : 'Singles & EPs'}
                        </h3>

                        <div className="flex space-x-2">
                            <button
                                onClick={() => handleAlbumTypeChange('album')}
                                className={`px-4 py-2 rounded-md font-medium text-sm ${
                                    albumType === 'album'
                                        ? 'bg-primary-600 text-white'
                                        : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                                }`}
                            >
                                Albums
                            </button>
                            <button
                                onClick={() => handleAlbumTypeChange('single')}
                                className={`px-4 py-2 rounded-md font-medium text-sm ${
                                    albumType === 'single'
                                        ? 'bg-primary-600 text-white'
                                        : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                                }`}
                            >
                                Singles & EPs
                            </button>
                        </div>
                    </div>

                    {isLoadingAlbums && albums.length === 0 ? (
                        <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
                            {[...Array(8)].map((_, i) => (
                                <div key={i} className="animate-pulse">
                                    <div className="bg-gray-200 aspect-square rounded-md mb-2"></div>
                                    <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                                    <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                                </div>
                            ))}
                        </div>
                    ) : albumsError ? (
                        <div className="text-red-500 text-sm p-4">{albumsError}</div>
                    ) : albums.length === 0 ? (
                        <EmptyState
                            title={`No ${albumType === 'album' ? 'albums' : 'singles'} found`}
                            message={`This artist doesn't have any ${albumType === 'album' ? 'albums' : 'singles or EPs'} yet.`}
                            icon={<Disc className="h-12 w-12 text-gray-400" />}
                        />
                    ) : (
                        <>
                            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4">
                                {albums.map(album => (
                                    <AlbumCard key={album.spotifyId} album={album} />
                                ))}
                            </div>

                            {/* Load more button */}
                            {albums.length < albumsTotal && (
                                <div className="mt-8 text-center">
                                    <button
                                        onClick={loadMoreAlbums}
                                        disabled={isLoadingAlbums}
                                        className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none disabled:opacity-50 disabled:cursor-not-allowed"
                                    >
                                        {isLoadingAlbums ? 'Loading...' : `Load More (${albums.length} of ${albumsTotal})`}
                                    </button>
                                </div>
                            )}
                        </>
                    )}
                </div>
            )}

            {/* Top Tracks Tab Content */}
            {activeTab === 'top-tracks' && (
                <div className="p-6">
                    <h3 className="text-xl font-bold text-gray-900 mb-6 flex items-center">
                        <Music className="h-5 w-5 mr-2 text-primary-600" />
                        Top Tracks
                    </h3>

                    {isLoadingTracks && topTracks.length === 0 ? (
                        <div className="space-y-4">
                            {[...Array(10)].map((_, i) => (
                                <div key={i} className="animate-pulse flex items-center p-2">
                                    <div className="w-6 h-6 bg-gray-200 rounded-full mr-3"></div>
                                    <div className="flex-grow">
                                        <div className="h-4 bg-gray-200 rounded w-3/4 mb-2"></div>
                                        <div className="h-3 bg-gray-200 rounded w-1/2"></div>
                                    </div>
                                </div>
                            ))}
                        </div>
                    ) : tracksError ? (
                        <div className="text-red-500 text-sm p-4">{tracksError}</div>
                    ) : topTracks.length === 0 ? (
                        <EmptyState
                            title="No top tracks found"
                            message="This artist doesn't have any top tracks yet."
                            icon={<Music className="h-12 w-12 text-gray-400" />}
                        />
                    ) : (
                        <div className="bg-white rounded-lg overflow-hidden">
                            {topTracks.map((track, index) => (
                                <TrackRow
                                    key={track.spotifyId}
                                    track={track}
                                    index={index}
                                    isPlaying={playingTrack === track.spotifyId}
                                    onPlayClick={() => handlePreviewPlay(track.spotifyId)}
                                />
                            ))}
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default ArtistContentTabs;