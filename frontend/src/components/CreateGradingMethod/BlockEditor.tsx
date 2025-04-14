import {BlockComponent, GradeComponent} from "../../api/interaction.ts";
import {MoveDown, MoveUp, Plus, Trash2} from "lucide-react";
import GradeEditor from "./GradeEditor.tsx";
import {getOperationSymbol} from "../../utils/grading-utils.ts";
import OperationSelector from "./OperationSelector.tsx";

interface BlockEditorProps {
    component: BlockComponent;
    onChange: (component: BlockComponent) => void;
    onRemove?: () => void;
    level?: number;
}

const BlockEditor = ({
                         component,
                         onChange,
                         onRemove,
                         level = 0
                     }: BlockEditorProps) => {
    const addGradeComponent = () => {
        const newSubComponents = [...component.subComponents];
        newSubComponents.push({
            componentType: 'grade',
            name: '',
            minGrade: 1,
            maxGrade: 10,
            stepAmount: 0.5,
            description: ''
        });

        onChange({
            ...component,
            subComponents: newSubComponents,
            // Ensure we have enough actions
            actions: newSubComponents.length > 1
                ? [...component.actions, 0].slice(0, newSubComponents.length - 1) as number[]
                : component.actions as number[]
        });
    };

    const addBlockComponent = () => {
        const newSubComponents = [...component.subComponents];
        newSubComponents.push({
            componentType: 'block',
            name: '',
            subComponents: [],
            actions: []
        });

        onChange({
            ...component,
            subComponents: newSubComponents,
            // Ensure we have enough actions
            actions: newSubComponents.length > 1
                ? [...component.actions, 0].slice(0, newSubComponents.length - 1) as number[]
                : component.actions as number[]
        });
    };

    const updateSubComponent = (index: number, updatedComponent: GradeComponent | BlockComponent) => {
        const newSubComponents = [...component.subComponents];
        newSubComponents[index] = updatedComponent;
        onChange({
            ...component,
            subComponents: newSubComponents
        });
    };

    const removeSubComponent = (index: number) => {
        const newSubComponents = [...component.subComponents];
        newSubComponents.splice(index, 1);

        // Also update actions if needed
        const newActions = [...component.actions];
        if (index < newActions.length) {
            newActions.splice(index, 1);
        } else if (index === newSubComponents.length && newActions.length > 0) {
            newActions.pop();
        }

        onChange({
            ...component,
            subComponents: newSubComponents,
            actions: newActions as number[]
        });
    };

    const moveSubComponentUp = (index: number) => {
        if (index === 0) return;

        const newSubComponents = [...component.subComponents];
        const temp = newSubComponents[index];
        newSubComponents[index] = newSubComponents[index - 1];
        newSubComponents[index - 1] = temp;

        // Also move the action
        const newActions = [...component.actions];
        if (index > 0 && index <= newActions.length) {
            const tempAction = newActions[index - 1];
            newActions[index - 1] = newActions[index > newActions.length - 1 ? newActions.length - 1 : index];
            if (index < newActions.length) {
                newActions[index] = tempAction;
            }
        }

        onChange({
            ...component,
            subComponents: newSubComponents,
            actions: newActions as number[]
        });
    };

    const moveSubComponentDown = (index: number) => {
        if (index === component.subComponents.length - 1) return;

        const newSubComponents = [...component.subComponents];
        const temp = newSubComponents[index];
        newSubComponents[index] = newSubComponents[index + 1];
        newSubComponents[index + 1] = temp;

        // Also move the action
        const newActions = [...component.actions];
        if (index < newActions.length) {
            const tempAction = newActions[index];
            newActions[index] = newActions[Math.min(index + 1, newActions.length - 1)];
            if (index + 1 < newActions.length) {
                newActions[index + 1] = tempAction;
            }
        }

        onChange({
            ...component,
            subComponents: newSubComponents,
            actions: newActions as number[]
        });
    };

    const updateOperation = (index: number, value: number) => {
        const newActions = [...component.actions];
        newActions[index] = value;
        onChange({
            ...component,
            actions: newActions as number[]
        });
    };

    const borderColor = level === 0
        ? 'border-purple-200'
        : level === 1
            ? 'border-blue-200'
            : 'border-teal-200';

    const bgColor = level === 0
        ? 'bg-purple-50'
        : level === 1
            ? 'bg-blue-50'
            : 'bg-teal-50';

    const textColor = level === 0
        ? 'text-purple-700'
        : level === 1
            ? 'text-blue-700'
            : 'text-teal-700';

    return (
        <div className={`p-4 rounded-md ${bgColor} border ${borderColor} mb-4`}>
            <div className="flex justify-between items-center mb-4">
                <div className={`font-medium ${textColor}`}>
                    {onRemove ? 'Nested Block' : 'Block Component'}
                </div>
                {onRemove && (
                    <button
                        type="button"
                        onClick={onRemove}
                        className="p-1 text-red-500 hover:text-red-700"
                    >
                        <Trash2 className="h-4 w-4" />
                    </button>
                )}
            </div>

            <div className="mb-4">
                <label className="block text-sm font-medium text-gray-700 mb-1">
                    Block Name
                </label>
                <input
                    type="text"
                    value={component.name}
                    onChange={(e) => onChange({ ...component, name: e.target.value })}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                    placeholder="e.g., Production Quality"
                    required
                />
            </div>

            <div className="mb-4">
                <div className="flex justify-between items-center mb-2">
                    <h5 className={`text-sm font-medium ${textColor}`}>Subcomponents</h5>
                    <div className="flex space-x-2">
                        <button
                            type="button"
                            onClick={addGradeComponent}
                            className={`px-2 py-1 text-xs ${
                                level === 0
                                    ? 'bg-blue-100 text-blue-700 hover:bg-blue-200'
                                    : level === 1
                                        ? 'bg-teal-100 text-teal-700 hover:bg-teal-200'
                                        : 'bg-green-100 text-green-700 hover:bg-green-200'
                            } rounded-md`}
                        >
                            <Plus className="h-3 w-3 inline mr-1" />
                            Add Grade
                        </button>
                        {level < 2 && (
                            <button
                                type="button"
                                onClick={addBlockComponent}
                                className={`px-2 py-1 text-xs ${
                                    level === 0
                                        ? 'bg-teal-100 text-teal-700 hover:bg-teal-200'
                                        : 'bg-green-100 text-green-700 hover:bg-green-200'
                                } rounded-md`}
                            >
                                <Plus className="h-3 w-3 inline mr-1" />
                                Add Block
                            </button>
                        )}
                    </div>
                </div>

                {component.subComponents.length === 0 ? (
                    <div className={`text-center py-4 px-3 border border-dashed ${borderColor} rounded-md mb-4`}>
                        <p className={`${textColor} text-sm`}>
                            Add subcomponents to this block.
                        </p>
                    </div>
                ) : (
                    <div className="space-y-4">
                        {component.subComponents.map((subcomp, index) => {
                            const isLast = index === component.subComponents.length - 1;

                            return (
                                <div key={index}>
                                    <div className="flex items-start mb-2">
                                        <div className="flex flex-col items-center mr-2 pt-2">
                                            <button
                                                type="button"
                                                onClick={() => moveSubComponentUp(index)}
                                                disabled={index === 0}
                                                className={`p-1 text-gray-500 hover:text-gray-700 ${
                                                    index === 0 ? 'opacity-50 cursor-not-allowed' : ''
                                                }`}
                                            >
                                                <MoveUp className="h-4 w-4" />
                                            </button>
                                            <button
                                                type="button"
                                                onClick={() => moveSubComponentDown(index)}
                                                disabled={isLast}
                                                className={`p-1 text-gray-500 hover:text-gray-700 ${
                                                    isLast ? 'opacity-50 cursor-not-allowed' : ''
                                                }`}
                                            >
                                                <MoveDown className="h-4 w-4" />
                                            </button>
                                        </div>

                                        <div className="flex-grow">
                                            {subcomp.componentType === 'grade' ? (
                                                <div className="bg-white p-4 rounded-md border border-gray-200">
                                                    <div className="flex justify-between mb-3">
                                                        <h6 className="text-sm font-medium text-gray-700">
                                                            Grade Component
                                                        </h6>
                                                        <button
                                                            type="button"
                                                            onClick={() => removeSubComponent(index)}
                                                            className="p-1 text-red-500 hover:text-red-700"
                                                        >
                                                            <Trash2 className="h-4 w-4" />
                                                        </button>
                                                    </div>

                                                    <div className="mb-3">
                                                        <label className="block text-sm font-medium text-gray-700 mb-1">
                                                            Name
                                                        </label>
                                                        <input
                                                            type="text"
                                                            value={subcomp.name}
                                                            onChange={(e) => updateSubComponent(
                                                                index,
                                                                { ...subcomp, name: e.target.value }
                                                            )}
                                                            className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                                                            placeholder="e.g., Mixing Quality"
                                                            required
                                                        />
                                                    </div>

                                                    <GradeEditor
                                                        component={subcomp}
                                                        onChange={(updated) => updateSubComponent(index, updated)}
                                                    />
                                                </div>
                                            ) : (
                                                <BlockEditor
                                                    component={subcomp as BlockComponent}
                                                    onChange={(updated) => updateSubComponent(index, updated)}
                                                    onRemove={() => removeSubComponent(index)}
                                                    level={level + 1}
                                                />
                                            )}
                                        </div>
                                    </div>

                                    {!isLast && (
                                        <div className="ml-8 mb-4 flex items-center">
                                            <div className="w-8 h-8 rounded-full border-2 border-gray-300 flex items-center justify-center font-bold text-gray-500">
                                                {getOperationSymbol(component.actions[index] as number || 0)}
                                            </div>
                                            <div className="ml-2 flex-grow">
                                                <OperationSelector
                                                    value={component.actions[index] as number || 0}
                                                    onChange={(value) => updateOperation(index, value)}
                                                    label="Operation with next component:"
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
        </div>
    );
};
export default BlockEditor;