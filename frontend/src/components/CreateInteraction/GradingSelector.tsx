import { Dispatch, SetStateAction } from "react";
import GradeSlider from "./GradeSlider";
import BlockGrader from "./BlockGrader";
import StarRating from "./StarRating";
import { BlockComponent, GradeComponent, GradingMethodDetail, GradingMethodSummary } from "../../api/interaction";
import { calculateOverallGrade } from "./DynamicGradingCalculator";
import { getGradeColorClasses, getGradeGradient } from "../../utils/GradeColorUtils";
import NormalizedStarDisplay from "./NormalizedStarDisplay";

interface GradingSelectorProps {
    useComplexGrading: boolean;
    setUseComplexGrading: Dispatch<SetStateAction<boolean>>;
    gradingMethods: GradingMethodSummary[];
    selectedMethodId: string | null;
    setSelectedMethodId: Dispatch<SetStateAction<string | null>>;
    selectedMethod: GradingMethodDetail | null;
    gradeValues: Record<string, number>;
    handleGradeChange: (name: string, value: number) => void;
    rating: number | null;
    displayRating: number | null;
    setRating: Dispatch<SetStateAction<number | null>>;
    setHoveredRating: Dispatch<SetStateAction<number | null>>;
}

const GradingSelector = ({
                             useComplexGrading,
                             setUseComplexGrading,
                             gradingMethods,
                             selectedMethodId,
                             setSelectedMethodId,
                             selectedMethod,
                             gradeValues,
                             handleGradeChange,
                             rating,
                             displayRating,
                             setRating,
                             setHoveredRating
                         }: GradingSelectorProps) => {
    // Calculate the overall grade if we have a selected method
    // Pass actions to the calculator - handle both string and number action types
    const overallGrade = selectedMethod
        ? calculateOverallGrade(
            selectedMethod.components,
            selectedMethod.actions,
            gradeValues)
        : null;

    // Calculate color classes based on grade percentage
    const percentage = overallGrade && overallGrade.maxGrade > 0
        ? overallGrade.currentGrade / overallGrade.maxGrade
        : 0;
    const colorClasses = getGradeColorClasses(percentage);
    const gradientClasses = getGradeGradient(percentage);

    return (
        <div className="mb-6 p-4 bg-gray-50 rounded-lg border border-gray-200">
            <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-medium text-gray-900">Grading Method</h3>
                <div className="flex gap-2">
                    <button
                        onClick={() => setUseComplexGrading(false)}
                        className={`px-4 py-2 rounded-md text-sm font-medium ${
                            !useComplexGrading ? 'bg-primary-600 text-white' : 'bg-white text-gray-700 border border-gray-300'
                        }`}
                    >
                        Simple
                    </button>
                    <button
                        onClick={() => setUseComplexGrading(true)}
                        className={`px-4 py-2 rounded-md text-sm font-medium ${
                            useComplexGrading ? 'bg-primary-600 text-white' : 'bg-white text-gray-700 border border-gray-300'
                        }`}
                    >
                        Complex
                    </button>
                </div>
            </div>

            {!useComplexGrading && (
                <div className="bg-white p-4 rounded-md border border-gray-200">
                    <h4 className="text-base font-medium text-gray-700 mb-2">Rate with Stars</h4>
                    <div className="flex justify-center items-center">
                        <StarRating
                            displayRating={displayRating}
                            setRating={setRating}
                            setHoveredRating={setHoveredRating}
                        />
                    </div>
                    {rating !== null && (
                        <div className="mt-2 text-center">
                            <button
                                onClick={() => setRating(null)}
                                className="text-sm text-gray-500 hover:text-gray-700"
                            >
                                Clear rating
                            </button>
                        </div>
                    )}
                </div>
            )}

            {useComplexGrading && (
                <div className="bg-white p-4 rounded-md border border-gray-200">
                    <div className="mb-4">
                        <label className="block text-sm font-medium text-gray-700 mb-1">
                            Select Grading Method
                        </label>
                        <select
                            value={selectedMethodId || ''}
                            onChange={(e) => setSelectedMethodId(e.target.value || null)}
                            className="w-full p-2 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
                        >
                            <option value="">Select a grading method...</option>
                            {gradingMethods.map((method) => (
                                <option key={method.id} value={method.id}>
                                    {method.name}
                                </option>
                            ))}
                        </select>
                    </div>

                    {selectedMethod && (
                        <div className="mt-4">
                            <div className="mb-4 p-4 bg-gray-50 rounded-md border border-gray-200">
                                <div className="flex justify-between items-center">
                                    {overallGrade && (
                                        <span className="text-gray-700 font-medium">
                                            <NormalizedStarDisplay
                                            currentGrade={overallGrade.currentGrade}
                                            minGrade={overallGrade.minGrade}
                                            maxGrade={overallGrade.maxGrade}
                                            size="md"/>
                                        </span>
                                    )}

                                    {overallGrade && (
                                        <div className="flex flex-col items-end">
                                            <div className="flex items-center">
                                                {/* Numeric grade display */}
                                                <div className={`px-3 py-1 rounded-md font-medium text-center ${colorClasses.background} ${colorClasses.text}`}>
                                                    <span className="text-lg">{overallGrade.currentGrade.toFixed(1)}</span>
                                                    <span className="text-sm">/{overallGrade.maxGrade.toFixed(1)}</span>
                                                </div>
                                            </div>
                                        </div>
                                    )}
                                </div>

                                {/* Progress bar with purple gradient */}
                                {overallGrade && (
                                    <div className="mt-2 w-full bg-gray-200 rounded-full h-2.5">
                                        <div
                                            className={`h-2.5 rounded-full bg-gradient-to-r ${gradientClasses}`}
                                            style={{
                                                width: `${(overallGrade.currentGrade / overallGrade.maxGrade) * 100}%`
                                            }}
                                        ></div>
                                    </div>
                                )}
                            </div>

                            <div className="space-y-4">
                                {selectedMethod.components.map((component, index) => (
                                    <div key={index}>
                                        {component.componentType === 'grade' ? (
                                            <GradeSlider
                                                component={component as GradeComponent}
                                                value={gradeValues[component.name] ?? null}
                                                onChange={handleGradeChange}
                                            />
                                        ) : (
                                            <BlockGrader
                                                component={component as BlockComponent}
                                                values={gradeValues}
                                                onChange={handleGradeChange}
                                            />
                                        )}
                                    </div>
                                ))}
                            </div>
                        </div>
                    )}
                </div>
            )}
        </div>
    );
};

export default GradingSelector;