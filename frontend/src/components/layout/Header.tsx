import { useState, useEffect } from 'react';
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { Search, Menu, X, User, Home, Disc, ListMusic, TrendingUp, LogIn, Star, Headphones } from 'lucide-react';
import useAuthStore from '../../store/authStore';

const Header = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const [isSearchActive, setIsSearchActive] = useState(false);
  const [isMenuOpen, setIsMenuOpen] = useState(false);
  const [searchQuery, setSearchQuery] = useState('');

  const { isAuthenticated, user, logout } = useAuthStore();

  // Close mobile menu when navigating to a new page
  useEffect(() => {
    setIsMenuOpen(false);
    setIsSearchActive(false);
  }, [location.pathname]);

  const handleSearchSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (searchQuery.trim()) {
      navigate(`/search?q=${encodeURIComponent(searchQuery.trim())}`);
      setIsSearchActive(false);
      setSearchQuery(''); // Clear search input after submitting
    }
  };

  return (
      <header className="bg-white shadow-md">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            <div className="flex items-center">
              {/* Logo */}
              <div className="flex-shrink-0 flex items-center">
                <Link to="/" className="text-2xl font-bold text-primary-600 flex items-center">
                  <Headphones className="h-8 w-8 mr-2" />
                  MusicEval
                </Link>
              </div>

              {/* Desktop Navigation */}
              <nav className="hidden md:ml-6 md:flex md:space-x-8">
                <Link
                    to="/"
                    className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                        location.pathname === '/'
                            ? 'text-primary-600 border-b-2 border-primary-600'
                            : 'text-zinc-500 hover:text-zinc-900 border-b-2 border-transparent hover:border-zinc-300'
                    }`}
                >
                  <Home className="h-4 w-4 mr-1" />
                  Home
                </Link>
                <Link
                    to="/search"
                    className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                        location.pathname === '/search'
                            ? 'text-primary-600 border-b-2 border-primary-600'
                            : 'text-zinc-500 hover:text-zinc-900 border-b-2 border-transparent hover:border-zinc-300'
                    }`}
                >
                  <Disc className="h-4 w-4 mr-1" />
                  Explore
                </Link>
                <Link
                    to="/charts"
                    className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                        location.pathname === '/charts'
                            ? 'text-primary-600 border-b-2 border-primary-600'
                            : 'text-zinc-500 hover:text-zinc-900 border-b-2 border-transparent hover:border-zinc-300'
                    }`}
                >
                  <TrendingUp className="h-4 w-4 mr-1" />
                  Charts
                </Link>
                <Link
                    to="/lists"
                    className={`inline-flex items-center px-1 pt-1 text-sm font-medium ${
                        location.pathname === '/lists'
                            ? 'text-primary-600 border-b-2 border-primary-600'
                            : 'text-zinc-500 hover:text-zinc-900 border-b-2 border-transparent hover:border-zinc-300'
                    }`}
                >
                  <ListMusic className="h-4 w-4 mr-1" />
                  Lists
                </Link>
              </nav>
            </div>

            <div className="flex items-center">
              {/* Search toggle for mobile */}
              <button
                  onClick={() => setIsSearchActive(!isSearchActive)}
                  className="md:hidden p-2 text-zinc-400 hover:text-zinc-500"
              >
                <Search className="h-6 w-6" />
              </button>

              {/* Desktop search */}
              <div className="hidden md:block">
                <form onSubmit={handleSearchSubmit} className="relative">
                  <div className="flex items-center">
                    <input
                        type="text"
                        placeholder="Search music, artists, albums..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="w-64 py-2 px-4 pl-10 rounded-full text-sm focus:outline-none border border-zinc-300 focus:border-primary-500"
                    />
                    <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                      <Search className="h-4 w-4 text-zinc-400" />
                    </div>
                  </div>
                </form>
              </div>

              {/* Profile/Login button */}
              <div className="ml-4 flex items-center md:ml-6">
                {isAuthenticated ? (
                    <div className="relative">
                      <div className="flex items-center text-sm font-medium text-zinc-700 hover:text-primary-600">
                    <span className="mr-2 flex items-center">
                      <Star className="h-4 w-4 mr-1" />
                      <span>Profile</span>
                    </span>
                        <Link to="/profile" className="w-8 h-8 rounded-full bg-primary-100 flex items-center justify-center text-primary-500">
                          {user?.name ? (
                              <span>{user.name.charAt(0).toUpperCase()}</span>
                          ) : (
                              <User className="h-4 w-4" />
                          )}
                        </Link>
                      </div>
                    </div>
                ) : (
                    <Link
                        to="/login"
                        className="flex items-center px-4 py-2 text-sm font-medium text-white bg-primary-600 rounded-md hover:bg-primary-700"
                    >
                      <LogIn className="h-4 w-4 mr-1" />
                      Sign In
                    </Link>
                )}
              </div>

              {/* Mobile menu button */}
              <div className="flex items-center md:hidden ml-2">
                <button
                    onClick={() => setIsMenuOpen(!isMenuOpen)}
                    className="inline-flex items-center justify-center p-2 rounded-md text-zinc-400 hover:text-zinc-500 hover:bg-zinc-100 focus:outline-none"
                >
                  {isMenuOpen ? (
                      <X className="h-6 w-6" />
                  ) : (
                      <Menu className="h-6 w-6" />
                  )}
                </button>
              </div>
            </div>
          </div>
        </div>

        {/* Mobile search when active */}
        {isSearchActive && (
            <div className="md:hidden px-4 pb-4">
              <form onSubmit={handleSearchSubmit} className="relative">
                <div className="flex items-center">
                  <input
                      type="text"
                      placeholder="Search music, artists, albums..."
                      value={searchQuery}
                      onChange={(e) => setSearchQuery(e.target.value)}
                      className="w-full py-2 px-4 pl-10 rounded-full text-sm focus:outline-none border border-zinc-300 focus:border-primary-500"
                      autoFocus
                  />
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Search className="h-4 w-4 text-zinc-400" />
                  </div>
                  <button
                      type="button"
                      className="absolute inset-y-0 right-0 pr-3 flex items-center"
                      onClick={() => setIsSearchActive(false)}
                  >
                    <X className="h-4 w-4 text-zinc-400" />
                  </button>
                </div>
              </form>
            </div>
        )}

        {/* Mobile menu dropdown */}
        {isMenuOpen && (
            <div className="md:hidden">
              <div className="pt-2 pb-3 space-y-1">
                <Link
                    to="/"
                    className={`block pl-3 pr-4 py-2 border-l-4 ${
                        location.pathname === '/'
                            ? 'border-primary-500 text-primary-700 bg-primary-50'
                            : 'border-transparent text-zinc-600 hover:text-zinc-800 hover:bg-zinc-50 hover:border-zinc-300'
                    } text-base font-medium`}
                >
                  <Home className="inline h-5 w-5 mr-1" />
                  Home
                </Link>
                <Link
                    to="/search"
                    className={`block pl-3 pr-4 py-2 border-l-4 ${
                        location.pathname === '/search'
                            ? 'border-primary-500 text-primary-700 bg-primary-50'
                            : 'border-transparent text-zinc-600 hover:text-zinc-800 hover:bg-zinc-50 hover:border-zinc-300'
                    } text-base font-medium`}
                >
                  <Disc className="inline h-5 w-5 mr-1" />
                  Explore
                </Link>
                <Link
                    to="/charts"
                    className={`block pl-3 pr-4 py-2 border-l-4 ${
                        location.pathname === '/charts'
                            ? 'border-primary-500 text-primary-700 bg-primary-50'
                            : 'border-transparent text-zinc-600 hover:text-zinc-800 hover:bg-zinc-50 hover:border-zinc-300'
                    } text-base font-medium`}
                >
                  <TrendingUp className="inline h-5 w-5 mr-1" />
                  Charts
                </Link>
                <Link
                    to="/lists"
                    className={`block pl-3 pr-4 py-2 border-l-4 ${
                        location.pathname === '/lists'
                            ? 'border-primary-500 text-primary-700 bg-primary-50'
                            : 'border-transparent text-zinc-600 hover:text-zinc-800 hover:bg-zinc-50 hover:border-zinc-300'
                    } text-base font-medium`}
                >
                  <ListMusic className="inline h-5 w-5 mr-1" />
                  Lists
                </Link>
              </div>

              {isAuthenticated && (
                  <div className="pt-4 pb-3 border-t border-zinc-200">
                    <div className="flex items-center px-4">
                      <div className="flex-shrink-0">
                        <div className="h-10 w-10 rounded-full bg-primary-100 flex items-center justify-center text-primary-500">
                          {user?.name ? (
                              <span className="text-lg font-medium">{user.name.charAt(0).toUpperCase()}</span>
                          ) : (
                              <User className="h-6 w-6" />
                          )}
                        </div>
                      </div>
                      <div className="ml-3">
                        <div className="text-base font-medium text-zinc-800">{user?.name} {user?.surname}</div>
                        <div className="text-sm font-medium text-zinc-500">{user?.email}</div>
                      </div>
                    </div>
                    <div className="mt-3 space-y-1">
                      <Link to="/profile" className="block px-4 py-2 text-base font-medium text-zinc-500 hover:text-zinc-800 hover:bg-zinc-100">
                        Your Profile
                      </Link>
                      <Link to="/settings" className="block px-4 py-2 text-base font-medium text-zinc-500 hover:text-zinc-800 hover:bg-zinc-100">
                        Settings
                      </Link>
                      <button
                          onClick={logout}
                          className="block w-full text-left px-4 py-2 text-base font-medium text-zinc-500 hover:text-zinc-800 hover:bg-zinc-100"
                      >
                        Sign out
                      </button>
                    </div>
                  </div>
              )}
            </div>
        )}
      </header>
  );
};

export default Header;