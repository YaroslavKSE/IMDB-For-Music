import { useEffect } from 'react';
import { Outlet, useLocation } from 'react-router-dom';
import Header from './Header';

const MainLayout = () => {
    const location = useLocation();

    // Reset scroll position when navigating between pages
    useEffect(() => {
        window.scrollTo(0, 0);
    }, [location.pathname]);

    return (
        <div className="min-h-screen flex flex-col bg-gray-50">
            {/* Header is always included */}
            <Header />

            {/* Main content wrapper is always included, but Home page will render null */}
            <main className="container mx-auto px-4 py-6 md:py-8 flex-grow">
                <Outlet />
            </main>
        </div>
    );
};

export default MainLayout;