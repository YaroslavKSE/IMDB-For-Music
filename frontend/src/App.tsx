import React, { useEffect } from 'react';
import { BrowserRouter as Router, Routes, Route, Navigate, useNavigate } from 'react-router-dom';
import useAuthStore from './store/authStore';
import { handleAuthCallback } from './utils/auth0-config';
import './App.css';

// Layout Components
import MainLayout from './components/layout/MainLayout';

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
import InteractionDetailPage from './pages/InteractionDetailPage'; // Import the new page

// Auth callback handler component
const AuthCallback = () => {
    const navigate = useNavigate();
    const { socialLogin } = useAuthStore();

    useEffect(() => {
        handleAuthCallback()
            .then(({ accessToken, provider }) => {
                // Use the store's socialLogin function with the tokens
                return socialLogin(accessToken, provider);
            })
            .then(() => {
                // Redirect to home page after successful login
                navigate('/', { replace: true });
            })
            .catch(error => {
                console.error('Auth callback error:', error);
                navigate('/login', { replace: true });
            });
    }, [navigate, socialLogin]);

    // Show a loading indicator while processing the callback
    return <div>Processing authentication, please wait...</div>;
};

// Protected route wrapper component
interface ProtectedRouteProps {
    children: React.ReactNode;
}

const ProtectedRoute = ({ children }: ProtectedRouteProps) => {
    const isAuthenticated = useAuthStore((state) => state.isAuthenticated);

    if (!isAuthenticated) {
        return <Navigate to="/login" replace />;
    }

    return <>{children}</>;
};

function App() {
    const { isAuthenticated, fetchUserProfile } = useAuthStore();

    useEffect(() => {
        if (isAuthenticated) {
            fetchUserProfile();
        }
    }, [isAuthenticated, fetchUserProfile]);

    return (
        <Router>
            <Routes>
                {/* Auth0 callback route */}
                <Route path="/callback" element={<AuthCallback />} />

                {/* Public routes */}
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
                        element={
                            <ViewGradingMethod />
                        }
                    />

                    <Route
                        path="create-interaction/:itemType/:itemId"
                        element={
                            <ProtectedRoute>
                                <CreateInteractionPage />
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