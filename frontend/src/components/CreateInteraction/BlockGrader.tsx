import { useState } from 'react';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { BlockComponent } from "../../api/interaction.ts";
import GradeSlider from "./GradeSlider.tsx";
import { calculateBlockValue } from "./DynamicGradingCalculator";
import { getGradeColorClasses } from "../../utils/GradeColorUtils";

const BlockGrader = ({
                         component,
                         values,
                         onChange,
                         path = ''
                     }: {
    component: BlockComponent,
    values: Record<string, number>,
    onChange: (name: string, value: number) => void,
    path?: string
}) => {
    const [isOpen, setIsOpen] = useState(true);
    const fullPath = path ? `${path}.${component.name}` : component.name;

    // Calculate the current grade for this block
    // Make sure to pass the correct path for nested calculations
    const blockGrade = calculateBlockValue(component, values, path);

    // Get color classes based on percentage of max grade
    const percentage = blockGrade.maxGrade !== 0
        ? blockGrade.currentGrade / blockGrade.maxGrade
        : 0;
    const colorClasses = getGradeColorClasses(percentage);

    return (
        <div className="mb-4 bg-gray-50 p-4 rounded-md border border-gray-200">
            {/* Header with toggle and grade value */}
            <div className="flex justify-between items-center">
                <div
                    className="flex items-center cursor-pointer"
                    onClick={() => setIsOpen(!isOpen)}
                >
                    {isOpen ?
                        <ChevronDown className="h-5 w-5 text-gray-500 mr-2" /> :
                        <ChevronRight className="h-5 w-5 text-gray-500 mr-2" />
                    }
                    <h3 className="font-medium text-gray-900">{component.name}</h3>
                </div>

                {/* Display the current and max grades with dynamic colors */}
                <div className="flex items-center">
                    <span className={`text-sm font-medium rounded-md px-2 py-1 ${colorClasses.background} ${colorClasses.text}`}>
                        {blockGrade.currentGrade.toFixed(1)}/{blockGrade.maxGrade.toFixed(1)}
                    </span>
                </div>
            </div>

            {/* Collapsible content */}
            {isOpen && (
                <div className="space-y-4 mt-3 pl-2 border-l-2 border-gray-200">
                    {component.subComponents.map((subComponent, index) => (
                        <div key={index}>
                            {subComponent.componentType === 'grade' ? (
                                <GradeSlider
                                    component={subComponent}
                                    value={values[`${fullPath}.${subComponent.name}`] ?? null}
                                    onChange={onChange}
                                    path={fullPath}
                                />
                            ) : (
                                <BlockGrader
                                    component={subComponent as BlockComponent}
                                    values={values}
                                    onChange={onChange}
                                    path={fullPath}
                                />
                            )}
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default BlockGrader;