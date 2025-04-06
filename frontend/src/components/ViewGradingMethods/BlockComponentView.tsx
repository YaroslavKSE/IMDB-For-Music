import type {BlockComponent as BlockComponentType} from "../../api/interaction.ts";
import {useState} from "react";
import {ChevronDown, ChevronRight} from "lucide-react";
import GradeComponent from "./GradeComponent.tsx";
import {getOperationName, getOperationSymbol} from "../../utils/grading-utils.ts";

const BlockComponentView = ({
                                component,
                                level = 0
                            }: {
    component: BlockComponentType,
    level?: number
}) => {
    const [expanded, setExpanded] = useState(true);

    // Gradient backgrounds based on level
    const getBgGradient = (level: number) => {
        const gradients = [
            'bg-gradient-to-br from-purple-50 to-purple-100',
            'bg-gradient-to-br from-indigo-50 to-indigo-100',
            'bg-gradient-to-br from-violet-50 to-violet-100'
        ];
        return gradients[level % gradients.length];
    };

    const getBorderColor = (level: number) => {
        const colors = ['border-purple-200', 'border-indigo-200', 'border-violet-200'];
        return colors[level % colors.length];
    };

    const getTextColor = (level: number) => {
        const colors = ['text-purple-800', 'text-indigo-800', 'text-violet-800'];
        return colors[level % colors.length];
    };

    const getAccentColor = (level: number) => {
        const colors = ['bg-purple-500', 'bg-indigo-500', 'bg-violet-500'];
        return colors[level % colors.length];
    };

    const getShadowColor = (level: number) => {
        const shadows = ['shadow-purple-100', 'shadow-indigo-100', 'shadow-violet-100'];
        return shadows[level % shadows.length];
    };

    return (
        <div className={`p-3 rounded-md ${getBgGradient(level)} border ${getBorderColor(level)} mb-3 shadow-sm ${getShadowColor(level)} transition-all duration-200 hover:shadow`}>
            <div className="flex justify-between items-center mb-2">
                <div className="flex items-center cursor-pointer" onClick={() => setExpanded(!expanded)}>
                    <div className={`w-1.5 h-10 rounded-full ${getAccentColor(level)} mr-2`}></div>
                    <div>
                        <div className="flex items-center">
                            <h3 className={`font-medium text-sm ${getTextColor(level)}`}>
                                {component.name}
                            </h3>
                            <button className={`ml-1 ${getTextColor(level)}`}>
                                {expanded ? <ChevronDown className="h-3.5 w-3.5" /> : <ChevronRight className="h-3.5 w-3.5" />}
                            </button>
                        </div>
                        <p className="text-xs text-gray-500">Components: {component.subComponents.length}</p>
                    </div>
                </div>

                <div className={`${getAccentColor(level)} bg-opacity-10 px-2 py-1 rounded-full text-xs font-medium ${getTextColor(level)}`}>
                    {component.minGrade} - {component.maxGrade}
                </div>
            </div>

            {expanded && (
                <div className="space-y-2 mt-2 pl-2 border-l border-gray-200">
                    {component.subComponents.map((subComponent, index) => {
                        const isLast = index === component.subComponents.length - 1;

                        return (
                            <div key={index}>
                                {subComponent.componentType === 'grade' ? (
                                    <GradeComponent component={subComponent} level={level + 1} />
                                ) : (
                                    <BlockComponentView component={subComponent} level={level + 1} />
                                )}

                                {!isLast && (
                                    <div className="flex items-center my-2 pl-6">
                                        <div className={`flex items-center justify-center px-3 py-1 rounded-md ${getAccentColor(level)} bg-opacity-10 text-xs ${getTextColor(level)}`}>
                      <span className="mr-1.5">{getOperationName(
                          typeof component.actions[index] === 'string'
                              ? (component.actions[index] === 'Add' ? 0 :
                                  component.actions[index] === 'Subtract' ? 1 :
                                      component.actions[index] === 'Multiply' ? 2 : 3)
                              : component.actions[index]
                      )}</span>
                                            <div className="w-5 h-5 rounded-full bg-white flex items-center justify-center font-medium text-gray-700 shadow-sm">
                                                {getOperationSymbol(
                                                    typeof component.actions[index] === 'string'
                                                        ? (component.actions[index] === 'Add' ? 0 :
                                                            component.actions[index] === 'Subtract' ? 1 :
                                                                component.actions[index] === 'Multiply' ? 2 : 3)
                                                        : component.actions[index]
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                )}
                            </div>
                        );
                    })}
                </div>
            )}
        </div>
    );
};
export default BlockComponentView;