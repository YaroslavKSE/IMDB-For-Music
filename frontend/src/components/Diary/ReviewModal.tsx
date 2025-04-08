import { X } from 'lucide-react';

// Review modal props
interface ReviewModalProps {
    isOpen: boolean;
    onClose: () => void;
    review: {
        reviewId: string;
        reviewText: string;
    };
    itemName: string;
    artistName: string;
    date: string;
}

// Review Modal Component
const ReviewModal = ({ isOpen, onClose, review, itemName, artistName, date }: ReviewModalProps) => {
    if (!isOpen) return null;

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
                        <h2 className="text-lg font-bold text-gray-900">Review</h2>
                        <button
                            onClick={onClose}
                            className="text-gray-400 hover:text-gray-500 focus:outline-none"
                        >
                            <X className="h-5 w-5" />
                        </button>
                    </div>

                    {/* Content */}
                    <div className="p-4">
                        <div className="mb-3">
                            <h3 className="font-medium text-gray-900">{itemName}</h3>
                            <p className="text-sm text-gray-600">{artistName}</p>
                            <p className="text-xs text-gray-500 mt-1">{date}</p>
                        </div>

                        <div className="bg-gray-50 p-4 rounded-md border border-gray-200">
                            <p className="text-gray-800 whitespace-pre-wrap">{review.reviewText}</p>
                        </div>
                    </div>

                    {/* Footer */}
                    <div className="p-4 border-t border-gray-200 flex justify-end">
                        <button
                            onClick={onClose}
                            className="px-4 py-2 bg-primary-600 text-white rounded-md hover:bg-primary-700 focus:outline-none"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};
export default ReviewModal;