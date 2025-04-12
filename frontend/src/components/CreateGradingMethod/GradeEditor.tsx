import { useState, useEffect } from 'react';
import { AlertTriangle } from 'lucide-react';
import { GradeComponent } from "../../api/interaction";
import { canReachMaxValueWithSteps } from "../../utils/validation-utils";

interface GradeEditorProps {
    component: GradeComponent;
    onChange: (component: GradeComponent) => void;
}

const GradeEditor = ({ component, onChange }: GradeEditorProps) => {
    const [minMaxError, setMinMaxError] = useState<string | null>(null);
    const [stepError, setStepError] = useState<string | null>(null);

    // Check for validation errors whenever the component changes
    useEffect(() => {
        // Check min/max relationship
        if (component.maxGrade < component.minGrade) {
            setMinMaxError("Maximum grade must be greater or equal to minimum grade");
        } else {
            setMinMaxError(null);
        }

        // Check step allows reaching max from min
        if (component.stepAmount < 0) {
            setStepError("Step amount must be greater than zero");
        } else if (!canReachMaxValueWithSteps(component.minGrade, component.maxGrade, component.stepAmount)) {
            setStepError(`Step of ${component.stepAmount} can't reach ${component.maxGrade} from ${component.minGrade}`);
        } else {
            setStepError(null);
        }
    }, [component.minGrade, component.maxGrade, component.stepAmount]);

    const updateField = <K extends keyof GradeComponent>(
        field: K,
        value: GradeComponent[K]
    ) => {
        onChange({ ...component, [field]: value });
    };

    return (
        <>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4 mb-2">
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Min Grade
                    </label>
                    <input
                        type="number"
                        value={component.minGrade}
                        onChange={(e) => updateField('minGrade', parseFloat(e.target.value))}
                        className={`w-full px-3 py-2 border ${minMaxError ? 'border-red-300 bg-red-50' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
                        step="0.1"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Max Grade
                    </label>
                    <input
                        type="number"
                        value={component.maxGrade}
                        onChange={(e) => updateField('maxGrade', parseFloat(e.target.value))}
                        className={`w-full px-3 py-2 border ${minMaxError ? 'border-red-300 bg-red-50' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
                        step="0.1"
                        required
                    />
                </div>
                <div>
                    <label className="block text-sm font-medium text-gray-700 mb-1">
                        Step Amount
                    </label>
                    <input
                        type="number"
                        value={component.stepAmount}
                        onChange={(e) => updateField('stepAmount', parseFloat(e.target.value))}
                        className={`w-full px-3 py-2 border ${stepError ? 'border-red-300 bg-red-50' : 'border-gray-300'} rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
                        step="0.1"
                        required
                    />
                </div>
            </div>

            {/* Show validation errors */}
            {(minMaxError || stepError) && (
                <div className="mb-3 text-xs text-red-700 bg-red-50 p-3 rounded-md border border-red-200">
                    <div className="flex items-start mb-1">
                        <AlertTriangle className="h-4 w-4 mr-1 flex-shrink-0 mt-0.5" />
                        <strong>Please fix the following errors:</strong>
                    </div>
                    <ul className="pl-5 list-disc">
                        {minMaxError && <li>{minMaxError}</li>}
                        {stepError && <li>{stepError}</li>}
                    </ul>
                </div>
            )}

            {/* Valid range calculator */}
            {!minMaxError && !stepError && (
                <div className="mb-3 text-xs text-green-700 bg-green-50 p-2 rounded-md border border-green-200">
                    <strong>Valid grades:</strong>{' '}
                    {calculateValidValues(component.minGrade, component.maxGrade, component.stepAmount)}
                </div>
            )}

            <div>
                <label className="block text-sm font-medium text-gray-700 mb-1">
                    Description (optional)
                </label>
                <textarea
                    value={component.description || ''}
                    onChange={(e) => updateField('description', e.target.value)}
                    className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                    rows={2}
                    placeholder="Describe what this component measures"
                />
            </div>
        </>
    );
};

// Helper function to display the valid values users can enter based on min, max, and step
function calculateValidValues(min: number, max: number, step: number): string {
    if (step < 0 || max < min) return "Invalid range";
    // Only show a limited number of values to not overwhelm the user
    const allValues = [];
    let current = min;
    let count = 0;
    const MAX_VALUES_TO_SHOW = 5;
    if(min == max){
        allValues.push(min);
        return allValues.join(', ')
    }

    while (current <= max && count < MAX_VALUES_TO_SHOW) {
        allValues.push(current);
        current = Math.round((current + step) * 100) / 100; // Round to avoid floating point issues
        count++;
    }

    // Check if we've displayed all values or need to add an ellipsis
    if (current <= max) {
        // There are more values, add ellipsis
        return `${allValues.join(', ')}, ... ${Math.round(max * 100) / 100}`;
    } else {
        // We've displayed all values
        return allValues.join(', ');
    }
}

export default GradeEditor;