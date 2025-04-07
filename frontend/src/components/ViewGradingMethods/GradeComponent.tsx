import type {GradeComponent as GradeComponentType} from "../../api/interaction.ts";
import {useState} from "react";
import {HelpCircle} from "lucide-react";
import InlineDescription from "./InlineDescription.tsx";

const GradeComponent = ({
                            component,
                            level = 0
                        }: {
    component: GradeComponentType,
    level?: number
}) => {
    const [showDescription, setShowDescription] = useState(false);

    // Gradient backgrounds based on level
    const getBgGradient = (level: number) => {
        const gradients = [
            'bg-gradient-to-br from-blue-50 to-blue-100',
            'bg-gradient-to-br from-teal-50 to-teal-100',
            'bg-gradient-to-br from-emerald-50 to-emerald-100'
        ];
        return gradients[level % gradients.length];
    };

    const getBorderColor = (level: number) => {
        const colors = ['border-blue-200', 'border-teal-200', 'border-emerald-200'];
        return colors[level % colors.length];
    };

    const getTextColor = (level: number) => {
        const colors = ['text-blue-800', 'text-teal-800', 'text-emerald-800'];
        return colors[level % colors.length];
    };

    const getAccentColor = (level: number) => {
        const colors = ['bg-blue-500', 'bg-teal-500', 'bg-emerald-500'];
        return colors[level % colors.length];
    };

    const getShadowColor = (level: number) => {
        const shadows = ['shadow-blue-100', 'shadow-teal-100', 'shadow-emerald-100'];
        return shadows[level % shadows.length];
    };

    return (
        <div className={`p-3 rounded-md ${getBgGradient(level)} border ${getBorderColor(level)} mb-2 shadow-sm ${getShadowColor(level)} transition-all duration-200 hover:shadow`}>
            <div className="flex justify-between items-center mb-2">
                <div className="flex items-center">
                    <div className={`w-1.5 h-9 rounded-full ${getAccentColor(level)} mr-2`}></div>
                    <h4 className={`font-medium text-sm ${getTextColor(level)}`}>
                        {component.name}
                        {component.description && (
                            <button
                                onClick={() => setShowDescription(!showDescription)}
                                className={`ml-1.5 ${getTextColor(level)} hover:opacity-75 focus:outline-none`}
                                title="Toggle description"
                            >
                                <HelpCircle className="h-3 w-3 inline" />
                            </button>
                        )}
                    </h4>
                </div>

                <div className="flex items-center">
          <span className={`${getAccentColor(level)} bg-opacity-10 px-2 py-1 rounded-full text-xs font-medium ${getTextColor(level)}`}>
            {component.minGrade} - {component.maxGrade}
          </span>
                </div>
            </div>

            {showDescription && component.description && (
                <InlineDescription description={component.description} textColor={getTextColor(level)} />
            )}

            <div className="mt-2 flex flex-wrap gap-2 items-start text-xs">
        <span className="bg-white px-2 py-0.5 rounded-full border border-gray-200 text-gray-700 flex items-center">
          <span className="w-1.5 h-1.5 rounded-full bg-red-500 mr-1.5"></span>
          Min: {component.minGrade}
        </span>
                <span className="bg-white px-2 py-0.5 rounded-full border border-gray-200 text-gray-700 flex items-center">
          <span className="w-1.5 h-1.5 rounded-full bg-green-500 mr-1.5"></span>
          Max: {component.maxGrade}
        </span>
                <span className="bg-white px-2 py-0.5 rounded-full border border-gray-200 text-gray-700 flex items-center">
          <span className="w-1.5 h-1.5 rounded-full bg-yellow-500 mr-1.5"></span>
          Step: {component.stepAmount}
        </span>
            </div>
        </div>
    );
};
export default GradeComponent;