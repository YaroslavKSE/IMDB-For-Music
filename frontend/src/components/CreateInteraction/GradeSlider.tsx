import { GradeComponent } from "../../api/interaction.ts";
import { useState } from "react";

const GradeSlider = ({
                         component,
                         value,
                         onChange,
                         path = ''
                     }: {
    component: GradeComponent,
    value: number | null,
    onChange: (name: string, value: number) => void,
    path?: string
}) => {
    const [sliderValue, setSliderValue] = useState<number>(value ?? component.minGrade);
    const [inputValue, setInputValue] = useState<string>(value?.toString() ?? component.minGrade.toString());
    const fullPath = path ? `${path}.${component.name}` : component.name;

    const handleSliderChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const newValue = parseFloat(e.target.value);
        setSliderValue(newValue);
        setInputValue(newValue.toString());
        onChange(fullPath, newValue);
    };

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const newValue = e.target.value;
        setInputValue(newValue);

        const numValue = parseFloat(newValue);
        if (!isNaN(numValue) && numValue >= component.minGrade && numValue <= component.maxGrade) {
            const stepsFromMin = (numValue - component.minGrade) / component.stepAmount;
            const isValidStep = Math.abs(Math.round(stepsFromMin) - stepsFromMin) < 0.0001;

            if (isValidStep) {
                setSliderValue(numValue);
                onChange(fullPath, numValue);
            }
        }
    };

    const handleBlur = () => {
        const stepsFromMin = (sliderValue - component.minGrade) / component.stepAmount;
        const roundedSteps = Math.round(stepsFromMin);
        const validValue = component.minGrade + (roundedSteps * component.stepAmount);
        const fixedValue = Number(validValue.toFixed(2));

        setSliderValue(fixedValue);
        setInputValue(fixedValue.toString());
        onChange(fullPath, fixedValue);
    };

    // Calculate percentage for dynamic styling
    const percentage = ((sliderValue - component.minGrade) / (component.maxGrade - component.minGrade)) * 100;

    return (
        <div className="mb-4 bg-white p-4 rounded-md shadow-sm border border-gray-200">
            <div className="flex justify-between items-center mb-2">
                <label className="block text-sm font-medium text-gray-700">
                    {component.name}
                </label>
                <div className="flex items-center">
                    <input
                        min={component.minGrade}
                        max={component.maxGrade}
                        value={inputValue}
                        onChange={handleInputChange}
                        onBlur={handleBlur}
                        className="w-16 px-2 py-1 text-right border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 text-sm"
                    />
                    <span className="ml-1 text-sm text-gray-600">
          </span>
                </div>
            </div>

            {component.description && (
                <p className="text-xs text-gray-500 mb-2">{component.description}</p>
            )}

            <div className="flex items-center">
                <span className="text-xs text-gray-500 mr-2">{component.minGrade}</span>
                <div className="relative w-full">
                    <input
                        type="range"
                        min={component.minGrade}
                        max={component.maxGrade}
                        step={component.stepAmount}
                        value={sliderValue}
                        onChange={handleSliderChange}
                        className="w-full h-2 appearance-none bg-transparent cursor-pointer"
                        style={{
                            background: `linear-gradient(to right, #7a24ec 0%, #7a24ec ${percentage}%, #e5e7eb ${percentage}%, #e5e7eb 100%)`,
                            borderRadius: '9999px'
                        }}
                    />
                </div>
                <span className="text-xs text-gray-500 ml-2">{component.maxGrade}</span>
            </div>
        </div>
    );
};

export default GradeSlider;