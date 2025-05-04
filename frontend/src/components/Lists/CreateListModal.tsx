import React, { useState } from 'react';
import { X, Save, Music, Disc } from 'lucide-react';
import useAuthStore from '../../store/authStore';
import ListsService, { CreateListRequest } from '../../api/lists';

interface CreateListModalProps {
    isOpen: boolean;
    onClose: () => void;
    onListCreated: () => void;
}

const CreateListModal: React.FC<CreateListModalProps> = ({ isOpen, onClose, onListCreated }) => {
    const { user } = useAuthStore();
    const [listName, setListName] = useState('');
    const [listDescription, setListDescription] = useState('');
    const [listType, setListType] = useState<string>('Album');
    const [isRanked, setIsRanked] = useState(false);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);

    if (!isOpen) return null;

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!user) {
            setError('You must be logged in to create a list');
            return;
        }

        if (!listName.trim()) {
            setError('Please provide a name for your list');
            return;
        }

        try {
            setIsSubmitting(true);
            setError(null);

            const createListRequest: CreateListRequest = {
                userId: user.id,
                listType,
                listName: listName.trim(),
                listDescription: listDescription.trim(),
                isRanked,
                items: [] // Start with an empty list
            };

            const response = await ListsService.createList(createListRequest);

            if (response.success) {
                setListName('');
                setListDescription('');
                setListType('Album');
                setIsRanked(false);
                onListCreated();
            } else {
                setError(response.errorMessage || 'Failed to create list');
            }
        } catch (err) {
            console.error('Error creating list:', err);
            setError('An error occurred while creating the list');
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen p-4">
                {/* Backdrop */}
                <div
                    className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
                    onClick={onClose}
                ></div>

                {/* Modal */}
                <div className="relative bg-white rounded-lg shadow-xl max-w-md w-full z-10">
                    {/* Header */}
                    <div className="flex justify-between items-center p-4 border-b border-gray-200">
                        <h2 className="text-lg font-bold text-gray-900">Create New List</h2>
                        <button
                            onClick={onClose}
                            className="text-gray-400 hover:text-gray-500 focus:outline-none"
                        >
                            <X className="h-5 w-5" />
                        </button>
                    </div>

                    {/* Form Content */}
                    <form onSubmit={handleSubmit}>
                        <div className="p-4 space-y-4">
                            {/* Error message */}
                            {error && (
                                <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md">
                                    {error}
                                </div>
                            )}

                            {/* List Name */}
                            <div>
                                <label htmlFor="listName" className="block text-sm font-medium text-gray-700 mb-1">
                                    List Name*
                                </label>
                                <input
                                    type="text"
                                    id="listName"
                                    value={listName}
                                    onChange={(e) => setListName(e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                                    placeholder="My Favorite Albums"
                                    required
                                />
                            </div>

                            {/* List Description */}
                            <div>
                                <label htmlFor="listDescription" className="block text-sm font-medium text-gray-700 mb-1">
                                    Description
                                </label>
                                <textarea
                                    id="listDescription"
                                    value={listDescription}
                                    onChange={(e) => setListDescription(e.target.value)}
                                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                                    placeholder="A collection of my all-time favorites..."
                                    rows={3}
                                />
                            </div>

                            {/* List Type - Styled as buttons similar to Artist page */}
                            <div>
                                <label className="block text-sm font-medium text-gray-700 mb-3">
                                    List Type
                                </label>
                                <div className="flex space-x-2">
                                    <button
                                        type="button"
                                        onClick={() => setListType('Album')}
                                        className={`px-4 py-2 rounded-md font-medium text-sm flex items-center ${
                                            listType === 'Album'
                                                ? 'bg-primary-600 text-white'
                                                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                                        }`}
                                    >
                                        <Disc className="h-4 w-4 mr-2" />
                                        Albums
                                    </button>
                                    <button
                                        type="button"
                                        onClick={() => setListType('Track')}
                                        className={`px-4 py-2 rounded-md font-medium text-sm flex items-center ${
                                            listType === 'Track'
                                                ? 'bg-primary-600 text-white'
                                                : 'bg-gray-100 text-gray-800 hover:bg-gray-200'
                                        }`}
                                    >
                                        <Music className="h-4 w-4 mr-2" />
                                        Tracks
                                    </button>
                                </div>
                            </div>

                            {/* Ranked Option */}
                            <div className="flex items-center">
                                <input
                                    id="isRanked"
                                    type="checkbox"
                                    checked={isRanked}
                                    onChange={(e) => setIsRanked(e.target.checked)}
                                    className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                                />
                                <label htmlFor="isRanked" className="ml-2 block text-sm text-gray-900">
                                    Make this a ranked list
                                </label>
                            </div>
                        </div>

                        {/* Footer */}
                        <div className="p-4 border-t border-gray-200 flex justify-end space-x-2">
                            <button
                                type="button"
                                onClick={onClose}
                                className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
                            >
                                Cancel
                            </button>
                            <button
                                type="submit"
                                disabled={isSubmitting || !listName.trim()}
                                className="flex items-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed"
                            >
                                {isSubmitting ? (
                                    <>
                                        <span className="mr-2 h-4 w-4 border-2 border-white border-t-transparent rounded-full animate-spin"></span>
                                        Creating...
                                    </>
                                ) : (
                                    <>
                                        <Save className="h-4 w-4 mr-2" />
                                        Create List
                                    </>
                                )}
                            </button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default CreateListModal;