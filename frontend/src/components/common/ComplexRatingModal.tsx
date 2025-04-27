import { useState, useEffect } from 'react';
import { X, ChevronDown, ChevronRight } from 'lucide-react';
import { motion, AnimatePresence } from 'framer-motion';
import InteractionService, { GradedComponentDTO, RatingDetailDTO } from '../../api/interaction.ts';
import NormalizedStarDisplay from '../CreateInteraction/NormalizedStarDisplay.tsx';
import { getGradeGradient } from '../../utils/GradeColorUtils.tsx';

interface ComplexRatingModalProps {
    isOpen: boolean;
    onClose: () => void;
    ratingId: string;
    itemName: string;
    artistName: string;
    date: string;
}

const ComplexRatingModal = ({ isOpen, onClose, ratingId, itemName, artistName, date }: ComplexRatingModalProps) => {
    const [visible, setVisible] = useState(false);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [ratingDetail, setRatingDetail] = useState<RatingDetailDTO | null>(null);
    const [expandedBlocks, setExpandedBlocks] = useState<Record<string, boolean>>({});
    const [initialized, setInitialized] = useState(false);

    useEffect(() => {
        const fetchAndShow = async () => {
            if (isOpen && ratingId) {
                setLoading(true);
                setError(null);
                setRatingDetail(null);
                setExpandedBlocks({});
                setInitialized(false);

                try {
                    const response = await InteractionService.getRatingById(ratingId);
                    if (response && response.ratingId) {
                        setRatingDetail(response);
                        setVisible(true);
                    } else {
                        console.error('Unexpected API response format:', response);
                        setError("Failed to load rating details: Unexpected response format");
                        setVisible(true);
                    }
                } catch (err) {
                    console.error('Error fetching rating details:', err);
                    setError('Something went wrong while loading the rating details');
                    setVisible(true);
                } finally {
                    setLoading(false);
                }
            } else {
                setVisible(false);
            }
        };

        fetchAndShow();
    }, [isOpen, ratingId]);

    // Initialize expandedBlocks when rating details are loaded
    useEffect(() => {
        if (ratingDetail && !initialized) {
            // Initialize all blocks as expanded by default
            const initExpanded: Record<string, boolean> = {};

            const initializeComponents = (component: GradedComponentDTO, path = component.name) => {
                if (component && component.componentType === 'block') {
                    initExpanded[path] = true;

                    if (Array.isArray(component.components)) {
                        component.components.forEach(subComponent => {
                            initializeComponents(subComponent, `${path}.${subComponent.name}`);
                        });
                    }
                }
            };

            if (ratingDetail.gradingComponent) {
                initializeComponents(ratingDetail.gradingComponent);
            }

            setExpandedBlocks(initExpanded);
            setInitialized(true);
        }
    }, [ratingDetail, initialized]);

    const toggleBlock = (path: string) => {
        setExpandedBlocks(prev => ({ ...prev, [path]: !prev[path] }));
    };

    const renderComponent = (component: GradedComponentDTO, level = 0, path = component.name) => {
        if (!component || !component.name) return null;

        const percentage = component.maxPossibleGrade > 0 ? component.currentGrade / component.maxPossibleGrade : 0;
        const gradientClasses = getGradeGradient(percentage);
        const isBlock = component.componentType === 'block';
        const isOpen = expandedBlocks[path] ?? false; // Default to closed if not in state

        return (
            <div
                key={path}
                className="mb-3 p-4 rounded-lg border border-gray-200 bg-white shadow-sm"
            >
                <div className="flex justify-between items-center cursor-pointer" onClick={() => isBlock && toggleBlock(path)}>
                    <div className="flex items-center">
                        {isBlock && (
                            isOpen ? (
                                <ChevronDown className="h-5 w-5 text-gray-500 mr-2" />
                            ) : (
                                <ChevronRight className="h-5 w-5 text-gray-500 mr-2" />
                            )
                        )}
                        <h3 className="font-semibold text-gray-900 text-sm">{component.name}</h3>
                    </div>

                    <span className="text-sm font-medium text-gray-700">
                        {component.currentGrade.toFixed(1)} / {component.maxPossibleGrade.toFixed(1)}
                    </span>
                </div>

                {component.description && (
                    <p className="text-xs text-gray-500 mt-1 mb-2">{component.description}</p>
                )}

                <div className="mt-2 w-full bg-gray-200 rounded-full h-2.5">
                    <div
                        className={`h-2.5 rounded-full bg-gradient-to-r ${gradientClasses}`}
                        style={{ width: `${percentage * 100}%` }}
                    ></div>
                </div>

                {isBlock && Array.isArray(component.components) && component.components.length > 0 && (
                    <AnimatePresence initial={false}>
                        {isOpen && (
                            <motion.div
                                initial={{ height: 0, opacity: 0 }}
                                animate={{ height: 'auto', opacity: 1 }}
                                exit={{ height: 0, opacity: 0 }}
                                transition={{ duration: 0.3, ease: 'easeInOut' }}
                                className="overflow-hidden mt-3 pl-4 border-l-4 border-primary-200 space-y-3"
                            >
                                {component.components.map((subComponent, index) => (
                                    <div key={index}>
                                        {renderComponent(subComponent, level + 1, `${path}.${subComponent.name}`)}
                                        {index < component.components!.length - 1 && component.actions && (
                                            <div className="flex items-center justify-center my-2 text-xs text-gray-500">
                                                <div className="px-2 py-1 bg-gray-100 rounded">
                                                    {component.actions[index]} ↓
                                                </div>
                                            </div>
                                        )}
                                    </div>
                                ))}
                            </motion.div>
                        )}
                    </AnimatePresence>
                )}
            </div>
        );
    };

    if (!visible) return null;

    return (
        <div className="fixed inset-0 z-50 overflow-y-auto">
            <div className="flex items-center justify-center min-h-screen p-4">
                <div
                    className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
                    onClick={onClose}
                ></div>

                <div className="relative bg-white rounded-lg shadow-xl w-full max-w-md z-10">
                    <div className="flex justify-between items-center p-4 border-b border-gray-200">
                        <h2 className="text-lg font-bold text-gray-900">Complex Rating Details</h2>
                        <button
                            onClick={onClose}
                            className="text-gray-400 hover:text-gray-500"
                        >
                            <X className="h-5 w-5" />
                        </button>
                    </div>

                    <div className="p-4 max-h-[70vh] overflow-y-auto">
                        <div className="mb-4">
                            <h3 className="font-medium text-gray-900">{itemName}</h3>
                            <p className="text-sm text-gray-600">{artistName}</p>
                            <p className="text-xs text-gray-500 mt-1">{date}</p>
                        </div>

                        {loading ? (
                            <div className="py-8 flex justify-center">
                                <div className="animate-spin rounded-full h-8 w-8 border-t-2 border-b-2 border-primary-600"></div>
                            </div>
                        ) : error ? (
                            <div className="bg-red-50 border border-red-200 text-red-700 p-4 rounded-md">
                                {error}
                            </div>
                        ) : ratingDetail ? (
                            <div className="space-y-4">
                                <div className="bg-gray-50 p-4 rounded-lg border border-gray-200 mb-4">
                                    <div className="flex flex-col items-center justify-center mb-2">
                                        <div className="mb-1">
                                            <NormalizedStarDisplay
                                                currentGrade={ratingDetail.normalizedGrade}
                                                minGrade={1}
                                                maxGrade={10}
                                                size="lg"
                                            />
                                        </div>
                                        <div className="flex items-center text-center">
                                            <span className="text-l font-bold">
                                                {ratingDetail.overallGrade.toFixed(1)} / {ratingDetail.maxPossibleGrade.toFixed(1)}
                                            </span>
                                        </div>
                                    </div>
                                </div>

                                <div>
                                    <h3 className="text-sm font-medium text-gray-700 mb-2">
                                        Rating Components
                                    </h3>
                                    {ratingDetail.gradingComponent && initialized
                                        ? renderComponent(ratingDetail.gradingComponent)
                                        : <p className="text-gray-500 text-sm">No component details available</p>}
                                </div>
                            </div>
                        ) : (
                            <div className="text-center py-8 text-gray-500">
                                No rating details available
                            </div>
                        )}
                    </div>

                    <div className="bg-gray-50 px-4 py-3 sm:px-6 sm:flex sm:flex-row-reverse border-t border-gray-200">
                        <button
                            type="button"
                            onClick={onClose}
                            className="w-full inline-flex justify-center rounded-md border border-transparent shadow-sm px-4 py-2 bg-primary-600 text-base font-medium text-white hover:bg-primary-700 focus:outline-none sm:ml-3 sm:w-auto sm:text-sm"
                        >
                            Close
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default ComplexRatingModal;