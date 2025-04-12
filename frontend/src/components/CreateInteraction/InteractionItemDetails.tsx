import { Play, Pause } from 'lucide-react';

const InteractionItemDetails = ({
                                    item,
                                    formattedItemType,
                                    isPlaying,
                                    handleTogglePreview
                                }: {
    //eslint-disable-next-line
    item: any,
    formattedItemType: string,
    isPlaying: boolean,
    handleTogglePreview: () => void
}) => {
    const artistName = formattedItemType === 'Album'
        ? item.artistName
        : (item.artists && item.artists.length > 0 ? item.artists[0]?.name : 'Unknown Artist');

    const imageUrl = item.imageUrl ||
        (formattedItemType === 'Track' && item.album?.imageUrl) ||
        '/placeholder-album.jpg';

    return (
        <div className="flex flex-col md:flex-row gap-8 mb-8">
            <div className="w-full md:w-64 flex-shrink-0">
                <div className="aspect-square w-full shadow-md rounded-lg overflow-hidden">
                    <img
                        src={imageUrl}
                        alt={item.name}
                        className="w-full h-full object-cover"
                    />
                </div>

                {formattedItemType === 'Track' && (
                    <button
                        onClick={handleTogglePreview}
                        className={`w-full mt-3 flex items-center justify-center py-2 px-4 border text-sm font-medium rounded-md ${
                            isPlaying
                                ? 'bg-primary-100 text-primary-700 border-primary-200'
                                : 'bg-gray-100 text-gray-800 border-gray-200'
                        }`}
                    >
                        {isPlaying ? (
                            <>
                                <Pause className="h-4 w-4 mr-2" />
                                Stop Preview
                            </>
                        ) : (
                            <>
                                <Play className="h-4 w-4 mr-2 fill-current" />
                                Play Preview
                            </>
                        )}
                    </button>
                )}
            </div>

            <div className="flex-grow">
                <div className="flex items-center text-gray-500 text-sm mb-2">
          <span className="uppercase bg-gray-200 rounded px-2 py-0.5">
            {formattedItemType}
          </span>
                    {formattedItemType === 'Track' && item.isExplicit && (
                        <span className="ml-2 px-1.5 py-0.5 text-xs bg-gray-200 text-gray-700 rounded">
              Explicit
            </span>
                    )}
                </div>

                <h2 className="text-3xl font-bold text-gray-900 mb-2">{item.name}</h2>
                <p className="text-lg text-gray-600 mb-4">{artistName}</p>

                {formattedItemType === 'Album' && (
                    <div className="text-sm text-gray-500">
                        {item.releaseDate && <p>Released: {new Date(item.releaseDate).toLocaleDateString()}</p>}
                        {item.totalTracks && <p>Tracks: {item.totalTracks}</p>}
                    </div>
                )}

                {formattedItemType === 'Track' && item.album && (
                    <div className="text-sm text-gray-500">
                        <p>From album: {item.album.name}</p>
                        {item.durationMs && (
                            <p>Duration: {Math.floor(item.durationMs / 60000)}:{((item.durationMs % 60000) / 1000).toFixed(0).padStart(2, '0')}</p>
                        )}
                    </div>
                )}
            </div>
        </div>
    );
};

export default InteractionItemDetails;
