import {AlbumSummary} from "../../api/catalog.ts";

const AlbumCard = ({ album }: { album: AlbumSummary }) => {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-md transition-shadow duration-200">
            <a href={`/album/${album.spotifyId}`} className="block">
                <div className="aspect-square w-full overflow-hidden bg-gray-200">
                    <img
                        src={album.imageUrl || '/placeholder-album.jpg'}
                        alt={album.name}
                        className="w-full h-full object-cover"
                    />
                </div>
                <div className="p-3">
                    <h3 className="font-medium text-gray-900 truncate">{album.name}</h3>
                    <p className="text-sm text-gray-600 truncate">{album.artistName}</p>
                    <div className="flex items-center mt-1 text-xs text-gray-500">
                        <span>{album.releaseDate?.split('-')[0] || 'Unknown year'}</span>
                        <span className="mx-1">•</span>
                        <span>{album.albumType === 'album' ? 'Album' : album.albumType}</span>
                    </div>
                </div>
            </a>
        </div>
    );
};

export default AlbumCard;