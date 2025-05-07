import { ArtistSummary } from "../../api/catalog.ts";

const ArtistCard = ({ artist }: { artist: ArtistSummary }) => {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden hover:shadow-md transition-shadow duration-200">
            <a href={`/artist/${artist.spotifyId}`} className="block">
                <div className="w-full shadow-md rounded-full overflow-hidden border-4 border-white aspect-square">
                    <img
                        src={artist.imageUrl || '/placeholder-artist.jpg'}
                        alt={artist.name}
                        className="w-full h-full object-cover"
                    />
                </div>
                <div className="p-3 text-center">
                    <h3 className="font-medium text-gray-900 truncate">{artist.name}</h3>
                </div>
            </a>
        </div>
    );
};

export default ArtistCard;