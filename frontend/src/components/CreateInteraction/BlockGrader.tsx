import { useState } from 'react';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { BlockComponent } from "../../api/interaction.ts";
import GradeSlider from "./GradeSlider.tsx";

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

    return (
        <div className="mb-4 bg-gray-50 p-4 rounded-md border border-gray-200">
            {/* Header with toggle */}
            <div
                className="flex justify-between items-center cursor-pointer"
                onClick={() => setIsOpen(!isOpen)}
            >
                <div className="flex items-center">
                    {isOpen ?
                        <ChevronDown className="h-5 w-5 text-gray-500 mr-2" /> :
                        <ChevronRight className="h-5 w-5 text-gray-500 mr-2" />
                    }
                    <h3 className="font-medium text-gray-900">{component.name}</h3>
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