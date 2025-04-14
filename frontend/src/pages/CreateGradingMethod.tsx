import { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import {
    ChevronLeft, Save, Plus, Trash2, MoveUp, MoveDown, HelpCircle
} from 'lucide-react';
import useAuthStore from '../store/authStore';
import InteractionService, {
    GradeComponent, BlockComponent, GradingMethodCreate
} from '../api/interaction';
import { getOperationSymbol } from '../utils/grading-utils';
import { validateGradingMethod } from '../utils/validation-utils';
import OperationSelector from "../components/CreateGradingMethod/OperationSelector";
import HelpModal from "../components/CreateGradingMethod/HelpModal";
import GradeEditor from "../components/CreateGradingMethod/GradeEditor";
import BlockEditor from "../components/CreateGradingMethod/BlockEditor";

const CreateGradingMethod = () => {
    const navigate = useNavigate();
    const { user } = useAuthStore();

    const [name, setName] = useState('');
    const [isPublic, setIsPublic] = useState(false);
    const [components, setComponents] = useState<(GradeComponent | BlockComponent)[]>([]);
    const [actions, setActions] = useState<number[]>([]);
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [error, setError] = useState<string | null>(null);
    const [success, setSuccess] = useState<string | null>(null);
    const [isHelpModalOpen, setIsHelpModalOpen] = useState(false);

    useEffect(() => {
        // Make sure user is logged in
        if (!user) {
            navigate('/login', { state: { from: '/grading-methods/create' } });
        }
    }, [user, navigate]);

    // Add a grade component to the top level
    const addGradeComponent = () => {
        const newGrade: GradeComponent = {
            componentType: 'grade',
            name: '',
            minGrade: 1,
            maxGrade: 10,
            stepAmount: 0.5,
            description: ''
        };
        setComponents([...components, newGrade]);

        // Update actions for the new component
        if (components.length > 0) {
            setActions([...actions, 0]);
        }
    };

    // Add a block component to the top level
    const addBlockComponent = () => {
        const newBlock: BlockComponent = {
            componentType: 'block',
            name: '',
            subComponents: [],
            actions: []
        };
        setComponents([...components, newBlock]);

        // Update actions for the new component
        if (components.length > 0) {
            setActions([...actions, 0]);
        }
    };

    // Update component at a specific index
    const updateComponent = (index: number, updated: GradeComponent | BlockComponent) => {
        const newComponents = [...components];
        newComponents[index] = updated;
        setComponents(newComponents);
    };

    // Remove component at a specific index
    const removeComponent = (index: number) => {
        const newComponents = [...components];
        newComponents.splice(index, 1);
        setComponents(newComponents);

        // Update actions
        const newActions = [...actions];
        if (index < newActions.length) {
            newActions.splice(index, 1);
        } else if (index === newComponents.length && newActions.length > 0) {
            newActions.pop();
        }
        setActions(newActions);
    };

    // Move component up in the list
    const moveComponentUp = (index: number) => {
        if (index === 0) return;

        // Swap components
        const newComponents = [...components];
        const temp = newComponents[index];
        newComponents[index] = newComponents[index - 1];
        newComponents[index - 1] = temp;
        setComponents(newComponents);

        // Swap actions if needed
        if (index > 0 && index <= actions.length) {
            const newActions = [...actions];
            const tempAction = newActions[index - 1];
            newActions[index - 1] = newActions[index > newActions.length - 1 ? newActions.length - 1 : index];
            if (index < newActions.length) {
                newActions[index] = tempAction;
            }
            setActions(newActions);
        }
    };

    // Move component down in the list
    const moveComponentDown = (index: number) => {
        if (index === components.length - 1) return;

        // Swap components
        const newComponents = [...components];
        const temp = newComponents[index];
        newComponents[index] = newComponents[index + 1];
        newComponents[index + 1] = temp;
        setComponents(newComponents);

        // Swap actions if needed
        if (index < actions.length) {
            const newActions = [...actions];
            const tempAction = newActions[index];
            newActions[index] = newActions[Math.min(index + 1, newActions.length - 1)];
            if (index + 1 < newActions.length) {
                newActions[index + 1] = tempAction;
            }
            setActions(newActions);
        }
    };

    // Update operation between components
    const updateOperation = (index: number, value: number) => {
        const newActions = [...actions];
        newActions[index] = value;
        setActions(newActions);
    };

    // Handle form submission
    const handleSubmit = async () => {
        if (!user?.id) {
            setError('User information is missing. Please log in again.');
            return;
        }

        // Validate the grading method
        const validation = validateGradingMethod(name, components, actions);
        if (!validation.valid && validation.error) {
            setError(validation.error);
            return;
        }

        try {
            setIsSubmitting(true);
            setError(null);

            const gradingMethod: GradingMethodCreate = {
                name,
                userId: user.id,
                isPublic,
                components,
                actions
            };

            const response = await InteractionService.createGradingMethod(gradingMethod);

            if (response.success) {
                setSuccess('Grading method created successfully!');
                // Redirect after success
                setTimeout(() => {
                    navigate('/profile');
                }, 1500);
            } else {
                setError(response.errorMessage || 'Failed to create grading method.');
                setIsSubmitting(false);
            }
        } catch (err) {
            console.error('Error creating grading method:', err);
            setError('An error occurred while creating the grading method. Please try again.');
            setIsSubmitting(false);
        }
    };

    if (!user) {
        return (
            <div className="flex justify-center items-center h-64">
                <div className="animate-spin rounded-full h-12 w-12 border-t-2 border-b-2 border-primary-600"></div>
            </div>
        );
    }

    return (
        <div className="max-w-6xl mx-auto pb-12">
            {/* Header */}
            <div className="bg-white shadow rounded-lg mb-6">
                <div className="px-6 py-4 flex justify-between items-center">
                    <button
                        onClick={() => navigate(-1)}
                        className="flex items-center text-gray-600 hover:text-gray-900"
                    >
                        <ChevronLeft className="h-5 w-5 mr-1" />
                        Back
                    </button>
                    <h1 className="text-2xl font-bold text-center text-gray-900">Create Grading Method</h1>
                    <button
                        onClick={() => setIsHelpModalOpen(true)}
                        className="text-primary-600 hover:text-primary-800 flex items-center"
                    >
                        <HelpCircle className="h-5 w-5 mr-1" />
                        Help
                    </button>
                </div>
            </div>

            {/* Help Modal */}
            <HelpModal isOpen={isHelpModalOpen} onClose={() => setIsHelpModalOpen(false)} />

            {/* Main content */}
            <div className="bg-white shadow rounded-lg overflow-hidden mb-6">
                <div className="p-6">
                    {error && (
                        <div className="mb-6 p-4 bg-red-50 border border-red-200 text-red-700 rounded-md">
                            {error}
                        </div>
                    )}

                    {success && (
                        <div className="mb-6 p-4 bg-green-50 border border-green-200 text-green-700 rounded-md">
                            {success}
                        </div>
                    )}

                    <div className="mb-6">
                        <label htmlFor="methodName" className="block text-lg font-medium text-gray-700 mb-2">
                            Method Name
                        </label>
                        <input
                            type="text"
                            id="methodName"
                            value={name}
                            onChange={(e) => setName(e.target.value)}
                            className="w-full px-4 py-3 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                            placeholder="e.g., Comprehensive Album Rating"
                            required
                        />
                    </div>

                    <div className="mb-6">
                        <div className="flex items-center">
                            <input
                                type="checkbox"
                                id="isPublic"
                                checked={isPublic}
                                onChange={(e) => setIsPublic(e.target.checked)}
                                className="h-5 w-5 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                            />
                            <label htmlFor="isPublic" className="ml-2 block text-base text-gray-700">
                                Make this grading method public (other users can use it)
                            </label>
                        </div>
                    </div>

                    <div className="mb-4">
                        <div className="flex justify-between items-center mb-4">
                            <h3 className="text-lg font-medium text-gray-900">Components</h3>
                            <div className="flex space-x-2">
                                <button
                                    type="button"
                                    onClick={addGradeComponent}
                                    className="px-3 py-2 bg-blue-100 text-blue-700 rounded-md hover:bg-blue-200"
                                >
                                    <Plus className="h-4 w-4 inline mr-1" />
                                    Add Grade
                                </button>
                                <button
                                    type="button"
                                    onClick={addBlockComponent}
                                    className="px-3 py-2 bg-purple-100 text-purple-700 rounded-md hover:bg-purple-200"
                                >
                                    <Plus className="h-4 w-4 inline mr-1" />
                                    Add Block
                                </button>
                            </div>
                        </div>

                        {components.length === 0 ? (
                            <div className="text-center py-12 px-6 border-2 border-dashed border-gray-300 rounded-md">
                                <div className="mx-auto h-16 w-16 text-gray-400 mb-4">
                                    <HelpCircle className="h-full w-full" />
                                </div>
                                <h3 className="text-lg font-medium text-gray-900 mb-2">Start Building Your Grading Method</h3>
                                <p className="text-gray-500 mb-6 max-w-md mx-auto">
                                    Add components to build your grading method.
                                    Use <strong>Grades</strong> for individual rating criteria and <strong>Blocks</strong> to group related grades.
                                </p>
                                <div className="flex justify-center space-x-4">
                                    <button
                                        type="button"
                                        onClick={addGradeComponent}
                                        className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-blue-600 hover:bg-blue-700"
                                    >
                                        <Plus className="h-4 w-4 inline mr-1" />
                                        Add Grade
                                    </button>
                                    <button
                                        type="button"
                                        onClick={addBlockComponent}
                                        className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-purple-600 hover:bg-purple-700"
                                    >
                                        <Plus className="h-4 w-4 inline mr-1" />
                                        Add Block
                                    </button>
                                </div>
                            </div>
                        ) : (
                            <div className="space-y-6">
                                {components.map((component, index) => {
                                    const isLast = index === components.length - 1;

                                    return (
                                        <div key={index}>
                                            <div className="flex items-start">
                                                <div className="flex flex-col items-center mr-2 pt-2">
                                                    <button
                                                        type="button"
                                                        onClick={() => moveComponentUp(index)}
                                                        disabled={index === 0}
                                                        className={`p-1 text-gray-500 hover:text-gray-700 ${
                                                            index === 0 ? 'opacity-50 cursor-not-allowed' : ''
                                                        }`}
                                                    >
                                                        <MoveUp className="h-5 w-5" />
                                                    </button>
                                                    <button
                                                        type="button"
                                                        onClick={() => moveComponentDown(index)}
                                                        disabled={isLast}
                                                        className={`p-1 text-gray-500 hover:text-gray-700 ${
                                                            isLast ? 'opacity-50 cursor-not-allowed' : ''
                                                        }`}
                                                    >
                                                        <MoveDown className="h-5 w-5" />
                                                    </button>
                                                    <button
                                                        type="button"
                                                        onClick={() => removeComponent(index)}
                                                        className="p-1 text-red-500 hover:text-red-700"
                                                    >
                                                        <Trash2 className="h-5 w-5" />
                                                    </button>
                                                </div>

                                                <div className="flex-grow">
                                                    {component.componentType === 'grade' ? (
                                                        <div className="bg-blue-50 p-4 rounded-md border border-blue-200">
                                                            <div className="mb-3">
                                                                <h4 className="text-md font-medium text-blue-700 mb-2">
                                                                    Grade Component
                                                                </h4>
                                                                <label className="block text-sm font-medium text-gray-700 mb-1">
                                                                    Name
                                                                </label>
                                                                <input
                                                                    type="text"
                                                                    value={component.name}
                                                                    onChange={(e) => updateComponent(
                                                                        index,
                                                                        { ...component, name: e.target.value }
                                                                    )}
                                                                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                                                                    placeholder="e.g., Lyrics"
                                                                    required
                                                                />
                                                            </div>

                                                            <GradeEditor
                                                                component={component}
                                                                onChange={(updated) => updateComponent(index, updated)}
                                                            />
                                                        </div>
                                                    ) : (
                                                        <BlockEditor
                                                            component={component as BlockComponent}
                                                            onChange={(updated) => updateComponent(index, updated)}
                                                            level={0}
                                                        />
                                                    )}
                                                </div>
                                            </div>

                                            {!isLast && (
                                                <div className="ml-12 my-4 flex items-center">
                                                    <div className="w-10 h-10 rounded-full border-2 border-gray-300 flex items-center justify-center font-bold text-xl text-gray-500">
                                                        {getOperationSymbol(actions[index] || 0)}
                                                    </div>
                                                    <div className="ml-4 flex-grow">
                                                        <OperationSelector
                                                            value={actions[index] || 0}
                                                            onChange={(value) => updateOperation(index, value)}
                                                        />
                                                    </div>
                                                </div>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        )}
                    </div>

                    <div className="mt-8 text-right">
                        <button
                            type="button"
                            onClick={handleSubmit}
                            disabled={isSubmitting}
                            className="inline-flex justify-center px-6 py-3 border border-transparent text-base font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:bg-primary-400 disabled:cursor-not-allowed"
                        >
                            {isSubmitting ? (
                                <>
                                    <div className="animate-spin rounded-full h-5 w-5 border-t-2 border-b-2 border-white mr-3"></div>
                                    Creating...
                                </>
                            ) : (
                                <>
                                    <Save className="h-5 w-5 mr-2" />
                                    Create Grading Method
                                </>
                            )}
                        </button>
                    </div>
                </div>
            </div>
        </div>
    );
};

export default CreateGradingMethod;