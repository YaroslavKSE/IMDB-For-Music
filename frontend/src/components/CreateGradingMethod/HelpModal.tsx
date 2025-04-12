import { X, AlertTriangle } from "lucide-react";

const HelpModal = ({ isOpen, onClose }: { isOpen: boolean, onClose: () => void }) => {
    if (!isOpen) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen pt-4 px-4 pb-20 text-center sm:block sm:p-0">
                <div className="fixed inset-0 transition-opacity" onClick={onClose}>
                    <div className="absolute inset-0 bg-gray-500 opacity-75"></div>
                </div>
                <span className="hidden sm:inline-block sm:align-middle sm:h-screen">&#8203;</span>
                <div className="inline-block align-bottom bg-white rounded-lg text-left overflow-hidden shadow-xl transform transition-all sm:my-8 sm:align-middle sm:max-w-lg sm:w-full">
                    <div className="bg-white px-4 pt-5 pb-4 sm:p-6 sm:pb-4">
                        <div className="flex justify-between items-start">
                            <h3 className="text-lg font-medium text-gray-900 mb-4">How to Create a Grading Method</h3>
                            <button onClick={onClose} className="text-gray-400 hover:text-gray-500">
                                <X className="h-5 w-5" />
                            </button>
                        </div>

                        <div className="space-y-4 text-sm text-gray-600">
                            <div>
                                <h4 className="font-medium text-gray-900 mb-1">Grading Method Name</h4>
                                <p>Choose a descriptive name for your grading method (e.g., "Album Production Rating System").</p>
                            </div>

                            <div>
                                <h4 className="font-medium text-gray-900 mb-1">Visibility</h4>
                                <p>Public methods can be used by other users. Private methods are only visible to you.</p>
                            </div>

                            <div>
                                <h4 className="font-medium text-gray-900 mb-1">Grade Components</h4>
                                <p>Add individual grading criteria like "Lyrics" or "Vocal Performance". Each grade can have its own range and step size.</p>
                            </div>

                            <div>
                                <h4 className="font-medium text-gray-900 mb-1">Block Components</h4>
                                <p>Group related grades into a block (e.g., "Production" might contain "Mixing", "Mastering", etc.).</p>
                            </div>

                            <div>
                                <h4 className="font-medium text-gray-900 mb-1">Nested Blocks</h4>
                                <p>You can create blocks within blocks for complex hierarchical ratings (up to 3 levels deep).</p>
                            </div>

                            <div>
                                <h4 className="font-medium text-gray-900 mb-1">Operations</h4>
                                <p>Choose how components are combined mathematically (addition, subtraction, multiplication, or division).</p>
                            </div>

                            {/* Validation Requirements Section */}
                            <div className="pt-2 border-t border-gray-200 mt-4">
                                <div className="font-medium text-gray-900 mb-2 flex items-center">
                                    <AlertTriangle className="h-4 w-4 mr-1 text-amber-500" />
                                    Validation Requirements:
                                </div>
                                <ul className="list-disc pl-5 space-y-2">
                                    <li>
                                        <strong>Min/Max Values:</strong> Maximum grade must always be greater than the minimum grade.
                                    </li>
                                    <li>
                                        <strong>Step Amounts:</strong> You must be able to reach the maximum value from the minimum value using the step amount. For example, with min=1, max=10, and step=0.5, the valid values are 1, 1.5, 2, ..., 9.5, 10.
                                    </li>
                                    <li>
                                        <strong>Division Operations:</strong> When using division, ensure that the divisor component cannot produce a value of zero, as this would result in division by zero.
                                    </li>
                                </ul>
                            </div>

                            <div className="pt-2 border-t border-gray-200 mt-4">
                                <div className="font-medium text-gray-900 mb-1">Example Use Cases:</div>
                                <ul className="list-disc pl-5 space-y-1">
                                    <li>Rate album production quality</li>
                                    <li>Evaluate vocal performances</li>
                                    <li>Create a comprehensive music review system</li>
                                    <li>Build a custom scoring system for specific genres</li>
                                </ul>
                            </div>
                        </div>
                    </div>
                    <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse">
                        <button
                            type="button"
                            onClick={onClose}
                            className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-primary-600 text-base font-medium text-white hover:bg-primary-700 focus:outline-none sm:ml-3 sm:w-auto sm:text-sm"
                        >
                            Got it
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};
export default HelpModal;