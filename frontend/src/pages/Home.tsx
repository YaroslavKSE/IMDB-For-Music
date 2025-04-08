import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { Calendar, Disc, RefreshCcw, ArrowRight } from 'lucide-react';
import CatalogService, { AlbumSummary } from '../api/catalog';
import { formatDate } from '../utils/formatters';

const Home = () => {
  const [newReleases, setNewReleases] = useState<AlbumSummary[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchNewReleases = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await CatalogService.getNewReleases(10, 0);
        setNewReleases(data.albums);
      } catch (err) {
        console.error('Error fetching new releases:', err);
        setError('Failed to load new releases. Please try again later.');
      } finally {
        setLoading(false);
      }
    };

    fetchNewReleases();
  }, []);

  return (
      <div className="max-w-6xl mx-auto py-8">
        <div className="mb-6">
          <h1 className="text-3xl font-bold text-gray-900 mb-2">Welcome to BeatRate</h1>
          <p className="text-gray-600">Discover, rate, and share your music experiences</p>
        </div>

        <div className="bg-white shadow rounded-lg overflow-hidden mb-8">
          <div className="px-6 py-4 bg-gradient-to-r from-primary-600 to-primary-700 flex justify-between items-center">
            <h2 className="text-xl font-bold text-white flex items-center">
              <Disc className="mr-2 h-5 w-5" />
              New Releases
            </h2>
            <Link
                to="/search?type=album"
                className="text-white hover:text-primary-100 flex items-center text-sm font-medium"
            >
              View all <ArrowRight className="ml-1 h-4 w-4" />
            </Link>
          </div>

          {loading ? (
              <div className="p-8 flex justify-center">
                <RefreshCcw className="h-8 w-8 text-primary-500 animate-spin" />
              </div>
          ) : error ? (
              <div className="p-8 text-center">
                <div className="text-red-500 mb-4">{error}</div>
                <button
                    onClick={() => window.location.reload()}
                    className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700"
                >
                  <RefreshCcw className="mr-2 h-4 w-4" /> Try Again
                </button>
              </div>
          ) : (
              <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-5 gap-4 p-6">
                {newReleases.map((album) => (
                    <Link
                        key={album.spotifyId}
                        to={`/album/${album.spotifyId}`}
                        className="bg-white rounded-lg overflow-hidden shadow-sm hover:shadow-md transition-shadow duration-200"
                    >
                      <div className="aspect-square w-full overflow-hidden">
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
                          <Calendar className="h-3 w-3 mr-1" />
                          <span>{formatDate(album.releaseDate || '')}</span>
                          <span className="mx-1">•</span>
                          <span className="capitalize">{album.albumType}</span>
                        </div>
                      </div>
                    </Link>
                ))}
              </div>
          )}
        </div>

        {/* Feature highlights section */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
          <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-2">Rate Your Way</h3>
              <p className="text-gray-600 mb-4">
                Create custom grading methods to evaluate music your way.
              </p>
              <Link
                  to="/grading-methods/create"
                  className="text-primary-600 hover:text-primary-800 font-medium flex items-center"
              >
                Create a grading method <ArrowRight className="ml-1 h-4 w-4" />
              </Link>
            </div>
          </div>

          <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-2">Share Reviews</h3>
              <p className="text-gray-600 mb-4">
                Document your music journey and share detailed reviews.
              </p>
              <Link
                  to="/search"
                  className="text-primary-600 hover:text-primary-800 font-medium flex items-center"
              >
                Find music to review <ArrowRight className="ml-1 h-4 w-4" />
              </Link>
            </div>
          </div>

          <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6">
              <h3 className="text-lg font-bold text-gray-900 mb-2">Build Collections</h3>
              <p className="text-gray-600 mb-4">
                Create lists of your favorite tracks and albums.
              </p>
              <Link
                  to="/lists"
                  className="text-primary-600 hover:text-primary-800 font-medium flex items-center"
              >
                Explore lists <ArrowRight className="ml-1 h-4 w-4" />
              </Link>
            </div>
          </div>
        </div>
      </div>
  );
};

export default Home;