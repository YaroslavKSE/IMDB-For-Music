import {ArtistSummary} from "../../api/catalog.ts";

const ArtistCard = ({ artist }: { artist: ArtistSummary }) => {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-md transition-shadow duration-200">
            <a href={`/artist/${artist.spotifyId}`} className="block">
                <div className="aspect-square w-full overflow-hidden bg-gray-200 rounded-full mx-auto p-2">
                    <img
                        src={artist.imageUrl || '/placeholder-artist.jpg'}
                        alt={artist.name}
                        className="w-full h-full object-cover rounded-full"
                    />
                </div>
                <div className="p-3 text-center">
                    <h3 className="font-medium text-gray-900 truncate">{artist.name}</h3>
                    <p className="text-sm text-gray-600">Artist</p>
                </div>
            </a>
        </div>
    );
};

export default ArtistCard;