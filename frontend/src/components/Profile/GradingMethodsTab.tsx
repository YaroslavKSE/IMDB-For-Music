import { useState, useEffect, useCallback } from 'react';
import { Plus, Trash2, Eye } from 'lucide-react';
import { useNavigate } from 'react-router-dom';
import useAuthStore from '../../store/authStore';
import InteractionService, { GradingMethodSummary } from '../../api/interaction';
import { formatDate } from '../../utils/formatters';

const GradingMethodsTab = () => {
    const navigate = useNavigate();
    const { user } = useAuthStore();
    const [gradingMethods, setGradingMethods] = useState<GradingMethodSummary[]>([]);
    const [isLoading, setIsLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    // Wrap the fetchGradingMethods in useCallback to prevent unnecessary recreations
    const fetchGradingMethods = useCallback(async () => {
        if (!user?.id) return;

        setIsLoading(true);
        setError(null);

        try {
            const methods = await InteractionService.getUserGradingMethods(user.id);
            setGradingMethods(methods);
        } catch (err) {
            console.error('Error fetching grading methods:', err);
            setError('Failed to load your grading methods. Please try again later.');
        } finally {
            setIsLoading(false);
        }
    }, [user?.id]); // Add user?.id as a dependency

    useEffect(() => {
        fetchGradingMethods();
    }, [fetchGradingMethods]); // Now fetchGradingMethods is a dependency

    const handleDeleteMethod = async (id: string) => {
        if (confirm('Are you sure you want to delete this grading method?')) {
            try {
                await InteractionService.deleteGradingMethod(id);
                // Refresh the list
                fetchGradingMethods();
            } catch (err) {
                console.error('Error deleting grading method:', err);
                alert('Failed to delete the grading method. Please try again.');
            }
        }
    };

    return (
        <div className="bg-white shadow rounded-lg overflow-hidden">
            <div className="p-6">
                <div className="flex justify-between items-center mb-6">
                    <h2 className="text-xl font-bold text-gray-900">Your Grading Methods</h2>
                    <button
                        onClick={() => navigate('/grading-methods/create')}
                        className="bg-primary-600 text-white px-4 py-2 rounded-md flex items-center hover:bg-primary-700 transition-colors"
                    >
                        <Plus className="h-4 w-4 mr-2" />
                        Create New
                    </button>
                </div>

                {isLoading ? (
                    <div className="flex justify-center py-8">
                        <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary-600"></div>
                    </div>
                ) : error ? (
                    <div className="bg-red-50 border border-red-200 rounded-md p-4 text-red-700">
                        {error}
                    </div>
                ) : gradingMethods.length === 0 ? (
                    <div className="text-center py-8 px-4">
                        <div className="mx-auto h-12 w-12 text-gray-400 mb-4">
                            <svg
                                xmlns="http://www.w3.org/2000/svg"
                                fill="none"
                                viewBox="0 0 24 24"
                                stroke="currentColor"
                            >
                                <path
                                    strokeLinecap="round"
                                    strokeLinejoin="round"
                                    strokeWidth={2}
                                    d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                                />
                            </svg>
                        </div>
                        <h3 className="text-lg font-medium text-gray-900 mb-2">No grading methods yet</h3>
                        <p className="text-gray-500 mb-4">
                            Create your first grading method to start rating music in your unique way.
                        </p>
                        <button
                            onClick={() => navigate('/grading-methods/create')}
                            className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700"
                        >
                            <Plus className="h-4 w-4 mr-2" />
                            Create Your First Grading Method
                        </button>
                    </div>
                ) : (
                    <div className="overflow-hidden rounded-md border border-gray-200">
                        <table className="min-w-full divide-y divide-gray-200">
                            <thead className="bg-gray-50">
                            <tr>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Name
                                </th>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Created
                                </th>
                                <th scope="col" className="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Visibility
                                </th>
                                <th scope="col" className="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">
                                    Actions
                                </th>
                            </tr>
                            </thead>
                            <tbody className="bg-white divide-y divide-gray-200">
                            {gradingMethods.map((method) => (
                                <tr key={method.id} className="hover:bg-gray-50">
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="font-medium text-gray-900">{method.name}</div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                                        <div className="text-sm text-gray-500">{formatDate(method.createdAt)}</div>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`px-2 inline-flex text-xs leading-5 font-semibold rounded-full ${
                          method.isPublic
                              ? 'bg-green-100 text-green-800'
                              : 'bg-gray-100 text-gray-800'
                      }`}>
                        {method.isPublic ? 'Public' : 'Private'}
                      </span>
                                    </td>
                                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                                        <div className="flex justify-end space-x-2">
                                            <button
                                                onClick={() => window.open(`/grading-methods/${method.id}`, '_blank')}
                                                className="text-primary-600 hover:text-primary-900"
                                            >
                                                <Eye className="h-5 w-5" />
                                            </button>
                                            <button
                                                onClick={() => handleDeleteMethod(method.id)}
                                                className="text-red-600 hover:text-red-900"
                                            >
                                                <Trash2 className="h-5 w-5" />
                                            </button>
                                        </div>
                                    </td>
                                </tr>
                            ))}
                            </tbody>
                        </table>
                    </div>
                )}
            </div>
        </div>
    );
};

export default GradingMethodsTab;