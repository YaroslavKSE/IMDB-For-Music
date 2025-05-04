import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Plus, List, RefreshCw, AlertTriangle } from 'lucide-react';
import useAuthStore from '../store/authStore';
import ListsService, { ListOverview } from '../api/lists';
import CreateListModal from '../components/Lists/CreateListModal';
import ListRowItem from '../components/Lists/ListRowItem';

const Lists = () => {
    const navigate = useNavigate();
    const { user, isAuthenticated } = useAuthStore();
    const [lists, setLists] = useState<ListOverview[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [totalLists, setTotalLists] = useState(0);
    const [isCreateModalOpen, setIsCreateModalOpen] = useState(false);
    const [deleteError, setDeleteError] = useState<string | null>(null);
    const [, setIsDeleting] = useState(false);

    // Fetch user's lists
    useEffect(() => {
        const fetchUserLists = async () => {
            if (!isAuthenticated || !user) {
                navigate('/login', { state: { from: '/lists' } });
                return;
            }

            try {
                setLoading(true);
                setError(null);

                const response = await ListsService.getUserLists(user.id, 20, 0);
                setLists(response.lists);
                setTotalLists(response.totalCount);
            } catch (err) {
                console.error('Error fetching lists:', err);
                setError('Failed to load your lists. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchUserLists();
    }, [isAuthenticated, user, navigate]);

    const handleCreateList = () => {
        setIsCreateModalOpen(true);
    };

    const handleListCreated = async () => {
        setIsCreateModalOpen(false);

        // Reload lists after creation
        if (user) {
            try {
                setLoading(true);
                const response = await ListsService.getUserLists(user.id, 20, 0);
                setLists(response.lists);
                setTotalLists(response.totalCount);
            } catch (err) {
                console.error('Error refreshing lists:', err);
            } finally {
                setLoading(false);
            }
        }
    };

    const handleDeleteList = async (listId: string) => {
        if (!user) return;

        setIsDeleting(true);
        setDeleteError(null);

        try {
            const response = await ListsService.deleteList(listId);

            if (response.success) {
                // Update local state to remove the deleted list
                setLists(prevLists => prevLists.filter(list => list.listId !== listId));
                setTotalLists(prev => prev - 1);
            } else {
                setDeleteError(response.errorMessage || 'Failed to delete the list.');
            }
        } catch (err) {
            console.error('Error deleting list:', err);
            setDeleteError('An error occurred while deleting the list. Please try again.');
        } finally {
            setIsDeleting(false);
        }
    };

    // Group lists by type
    const albumLists = lists.filter(list => list.listType === 'Album');
    const trackLists = lists.filter(list => list.listType === 'Track');

    if (!isAuthenticated) {
        return null;
    }

    return (
        <div className="max-w-6xl mx-auto py-8 px-4">
            {/* Header section */}
            <div className="flex justify-between items-center mb-6">
                <div>
                    <h1 className="text-3xl font-bold text-gray-900 mb-2">Your Lists</h1>
                    <p className="text-gray-600">
                        {totalLists > 0
                            ? `You have ${totalLists} ${totalLists === 1 ? 'list' : 'lists'}`
                            : 'Create your first list to organize your music'}
                    </p>
                </div>

                <button
                    onClick={handleCreateList}
                    className="flex items-center px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 transition-colors"
                >
                    <Plus className="h-5 w-5 mr-2" />
                    Create List
                </button>
            </div>

            {/* Error messages */}
            {error && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6">
                    {error}
                    <button
                        onClick={() => window.location.reload()}
                        className="ml-2 underline hover:text-red-900"
                    >
                        Try again
                    </button>
                </div>
            )}

            {deleteError && (
                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md mb-6 flex items-center">
                    <AlertTriangle className="h-5 w-5 mr-2" />
                    {deleteError}
                    <button
                        onClick={() => setDeleteError(null)}
                        className="ml-auto text-red-700 hover:text-red-900"
                    >
                        ×
                    </button>
                </div>
            )}

            {/* Loading state */}
            {loading && (
                <div className="flex justify-center items-center py-20">
                    <RefreshCw className="h-10 w-10 text-primary-600 animate-spin mr-3" />
                    <span className="text-lg text-gray-600">Loading your lists...</span>
                </div>
            )}

            {/* Empty state */}
            {!loading && lists.length === 0 && (
                <div className="bg-white shadow rounded-lg p-8 text-center">
                    <List className="h-16 w-16 text-gray-400 mx-auto mb-4" />
                    <h2 className="text-xl font-semibold text-gray-800 mb-2">You don't have any lists yet</h2>
                    <p className="text-gray-600 mb-6">
                        Create your first list to organize your favorite albums and tracks
                    </p>
                    <button
                        onClick={handleCreateList}
                        className="inline-flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700"
                    >
                        <Plus className="h-5 w-5 mr-2" />
                        Create Your First List
                    </button>
                </div>
            )}

            {/* Lists in row layout */}
            {!loading && lists.length > 0 && (
                <div className="space-y-8">
                    {/* Album Lists */}
                    {albumLists.length > 0 && (
                        <div>
                            <div className="border-b border-gray-200 mb-4 pb-2">
                                <h2 className="text-xl font-semibold text-gray-900">Album Lists</h2>
                            </div>
                            <div className="space-y-2">
                                {albumLists.map((list) => (
                                    <ListRowItem
                                        key={list.listId}
                                        list={list}
                                        onDelete={handleDeleteList}
                                    />
                                ))}
                            </div>
                        </div>
                    )}

                    {/* Track Lists */}
                    {trackLists.length > 0 && (
                        <div>
                            <div className="border-b border-gray-200 mb-4 pb-2">
                                <h2 className="text-xl font-semibold text-gray-900">Track Lists</h2>
                            </div>
                            <div className="space-y-2">
                                {trackLists.map((list) => (
                                    <ListRowItem
                                        key={list.listId}
                                        list={list}
                                        onDelete={handleDeleteList}
                                    />
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            )}

            {/* Create List Modal */}
            <CreateListModal
                isOpen={isCreateModalOpen}
                onClose={() => setIsCreateModalOpen(false)}
                onListCreated={handleListCreated}
            />
        </div>
    );
};

export default Lists;