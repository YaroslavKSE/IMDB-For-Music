import { useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';

const NotFound = () => {
  const navigate = useNavigate();

  // Auto-redirect after 10 seconds
  useEffect(() => {
    const redirectTimer = setTimeout(() => {
      navigate('/');
    }, 10000);

    return () => clearTimeout(redirectTimer);
  }, [navigate]);

  return (
    <div className="flex flex-col items-center justify-center py-12 px-4 sm:px-6 lg:px-8 text-center">
      <div className="max-w-md w-full space-y-8">
        {/* 404 Graphic */}
        <div className="relative">
          <div className="text-primary-600 text-9xl font-extrabold tracking-widest">404</div>
          <div className="absolute inset-0 flex items-center justify-center opacity-10">
            <svg
              xmlns="http://www.w3.org/2000/svg"
              className="h-64 w-64 text-primary-800"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={1}
                d="M9 19V6l12-3v13M9 19c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zm12-3c0 1.105-1.343 2-3 2s-3-.895-3-2 1.343-2 3-2 3 .895 3 2zM9 10l12-3"
              />
            </svg>
          </div>
        </div>

        {/* Error Message */}
        <div>
          <h1 className="text-3xl font-bold text-gray-900 mb-4">Page Not Found</h1>
          <p className="text-gray-600 mb-8">
            The page you're looking for doesn't exist or has been moved.
            Don't worry, there's plenty of music to discover elsewhere!
          </p>

          {/* Action Buttons */}
          <div className="flex flex-col sm:flex-row gap-4 justify-center">
            <Link
              to="/"
              className="bg-primary-600 text-white px-6 py-3 rounded-md font-medium hover:bg-primary-700 transition-colors flex-1 text-center"
            >
              Go to Homepage
            </Link>
            <button
              onClick={() => navigate(-1)}
              className="bg-white border border-gray-300 text-gray-700 px-6 py-3 rounded-md font-medium hover:bg-gray-50 transition-colors flex-1"
            >
              Go Back
            </button>
          </div>

          {/* Auto-redirect Message */}
          <p className="text-gray-500 mt-8 text-sm">
            You'll be automatically redirected to the homepage in 10 seconds.
          </p>
        </div>
      </div>
    </div>
  );
};

export default NotFound;