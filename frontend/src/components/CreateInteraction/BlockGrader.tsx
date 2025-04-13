// BlockGrader.tsx with framer-motion animation
import { useState } from 'react';
import { ChevronDown, ChevronRight } from 'lucide-react';
import { BlockComponent } from "../../api/interaction.ts";
import GradeSlider from "./GradeSlider.tsx";
import { calculateBlockValue } from "./DynamicGradingCalculator";
import { getGradeColorClasses } from "../../utils/GradeColorUtils";
import { motion, AnimatePresence } from 'framer-motion';

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

    const blockGrade = calculateBlockValue(component, values, path);
    const percentage = blockGrade.maxGrade !== 0 ? blockGrade.currentGrade / blockGrade.maxGrade : 0;
    const colorClasses = getGradeColorClasses(percentage);

    return (
        <div className="mb-4 bg-white p-4 rounded-xl border border-gray-200 shadow-sm">
            <div className="flex justify-between items-center cursor-pointer" onClick={() => setIsOpen(!isOpen)}>
                <div className="flex items-center">
                    {isOpen ? (
                        <ChevronDown className="h-5 w-5 text-gray-500 mr-2" />
                    ) : (
                        <ChevronRight className="h-5 w-5 text-gray-500 mr-2" />
                    )}
                    <h3 className="font-semibold text-gray-900">{component.name}</h3>
                </div>

                <span className={`text-sm font-medium rounded-md px-2 py-1 ${colorClasses.background} ${colorClasses.text}`}>
          {blockGrade.currentGrade.toFixed(1)} / {blockGrade.maxGrade.toFixed(1)}
        </span>
            </div>

            <AnimatePresence initial={false}>
                {isOpen && (
                    <motion.div
                        key="content"
                        initial={{ height: 0, opacity: 0 }}
                        animate={{ height: 'auto', opacity: 1 }}
                        exit={{ height: 0, opacity: 0 }}
                        transition={{ duration: 0.3, ease: 'easeInOut' }}
                        className="overflow-hidden border-l-4 border-primary-200 pl-3 mt-3"
                    >
                        <div className="space-y-4 pt-2">
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
                    </motion.div>
                )}
            </AnimatePresence>
        </div>
    );
};

export default BlockGrader;
