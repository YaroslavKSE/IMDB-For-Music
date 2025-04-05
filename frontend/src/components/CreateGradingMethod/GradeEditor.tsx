import {GradeComponent} from "../../api/interaction.ts";

interface GradeEditorProps {
    component: GradeComponent;
    onChange: (component: GradeComponent) => void;
}

const GradeEditor = ({ component, onChange }: GradeEditorProps) => {
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
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
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
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
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
                        className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                        step="0.1"
                        required
                    />
                </div>
            </div>
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
export default GradeEditor;