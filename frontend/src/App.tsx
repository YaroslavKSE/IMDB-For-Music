import React, { useEffect, useState } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import useAuthStore from './store/authStore';
import { handleAuthCallback } from './utils/auth0-config';
import './App.css';

// Layout Components
import MainLayout from './components/layout/MainLayout';
import LoadingIndicator from './components/common/LoadingIndicator';

// Page Components
import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';
import Profile from './pages/Profile';
import Search from './pages/Search';
import Album from './pages/Album';
import Song from './pages/Song';
import Artist from './pages/Artist';
import NotFound from "./pages/NotFound";
import CreateGradingMethod from './pages/CreateGradingMethod';
import ViewGradingMethod from './pages/ViewGradingMethod';
import Diary from './pages/Diary';
import PeoplePage from './pages/People';
import UserProfilePage from './pages/UserProfile';
import CreateInteractionPage from './pages/CreateInteractionPage';
import InteractionDetailPage from './pages/InteractionDetailPage';
import FollowingFeed from './pages/FollowingFeed';

// Auth callback handler component
const AuthCallback = () => {
  const { socialLogin } = useAuthStore();
  const location = useLocation();

  useEffect(() => {
    const processAuth = async () => {
      try {
        const { accessToken, provider } = await handleAuthCallback();
        // Use the store's socialLogin function with the tokens
        await socialLogin(accessToken, provider);

        // Redirect to home page or the original destination after successful login
        window.location.href = location.state?.from || '/';
      } catch (error) {
        console.error('Auth callback error:', error);
        window.location.href = '/login';
      }
    };

    processAuth();
  }, [socialLogin, location.state]);

  // Show a subtle loading indicator while processing the callback
  return (
    <div className="min-h-screen flex flex-col justify-center items-center bg-gray-50">
      <LoadingIndicator size="large" text="Completing sign-in" />
    </div>
  );
};

// Protected route wrapper component
interface ProtectedRouteProps {
  children: React.ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
  const { isAuthenticated, isLoading } = useAuthStore();
  const location = useLocation();

  // If still loading, render the current route but with a subtle loading indicator
  if (isLoading) {
    return (
      <div className="opacity-50 pointer-events-none">
        {children}
        <div className="fixed top-0 left-0 right-0 z-50 bg-primary-600 h-1">
          <div className="h-full bg-primary-300 animate-pulse"></div>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" state={{ from: location.pathname }} replace />;
  }

  return <>{children}</>;
};

function App() {
  const { initializeAuth} = useAuthStore();
  const [isInitializing, setIsInitializing] = useState(true);

  useEffect(() => {
    const init = async () => {
      await initializeAuth();
      setIsInitializing(false);
    };

    init();
  }, [initializeAuth]);

  // Show a minimal loading indicator only during initial app load
  if (isInitializing) {
    return (
      <div className="min-h-screen bg-gray-50">
        <div className="fixed top-0 left-0 right-0 z-50 bg-primary-100 h-1">
          <div className="h-full bg-primary-600 w-24 animate-pulse"></div>
        </div>
      </div>
    );
  }

  return (
    <Router>
      <Routes>
        {/* Auth0 callback route - outside MainLayout */}
        <Route path="/callback" element={<AuthCallback />} />

        {/* Main layout with routes */}
        <Route path="/" element={<MainLayout />}>
          <Route index element={<Home />} />
          <Route path="login" element={<Login />} />
          <Route path="register" element={<Register />} />
          <Route path="search" element={<Search />} />
          <Route path="album/:id" element={<Album />} />
          <Route path="track/:id" element={<Song />} />
          <Route path="artist/:id" element={<Artist />} />

          {/* ItemHistory detail page - publicly viewable */}
          <Route path="interaction/:id" element={<InteractionDetailPage />} />

          {/* People Routes */}
          <Route path="people" element={<PeoplePage />} />
          <Route path="people/:id" element={<UserProfilePage />} />

          {/* Protected routes */}
          <Route
            path="profile"
            element={
              <ProtectedRoute>
                <Profile />
              </ProtectedRoute>
            }
          />
          {/* Diary route */}
          <Route
            path="diary"
            element={
              <ProtectedRoute>
                <Diary />
              </ProtectedRoute>
            }
          />
          {/* Grading method routes */}
          <Route
            path="grading-methods/create"
            element={
              <ProtectedRoute>
                <CreateGradingMethod />
              </ProtectedRoute>
            }
          />
          <Route
            path="grading-methods/:id"
            element={<ViewGradingMethod />}
          />

          <Route
            path="create-interaction/:itemType/:itemId"
            element={
              <ProtectedRoute>
                <CreateInteractionPage />
              </ProtectedRoute>
            }
          />

          <Route
              path="following-feed"
              element={
                <ProtectedRoute>
                  <FollowingFeed />
                </ProtectedRoute>
              }
          />

          {/* Catch-all route for 404 */}
          <Route path="*" element={<NotFound />} />
        </Route>
      </Routes>
    </Router>
  );
}

export default App;