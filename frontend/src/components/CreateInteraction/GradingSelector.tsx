import { Dispatch, SetStateAction, Fragment } from 'react';
import { Listbox, Transition } from '@headlessui/react';
import { Check, ChevronDown, Scale } from 'lucide-react';
import GradeSlider from './GradeSlider';
import BlockGrader from './BlockGrader';
import StarRating from './StarRating';
import NormalizedStarDisplay from './NormalizedStarDisplay';
import {
    BlockComponent,
    GradeComponent,
    GradingMethodDetail,
    GradingMethodSummary
} from '../../api/interaction';
import { calculateOverallGrade } from './DynamicGradingCalculator';
import { getGradeGradient } from '../../utils/GradeColorUtils';

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
    const overallGrade = selectedMethod
        ? calculateOverallGrade(selectedMethod.components, selectedMethod.actions, gradeValues)
        : null;

    const percentage = overallGrade && overallGrade.maxGrade > 0
        ? overallGrade.currentGrade / overallGrade.maxGrade
        : 0;
    const gradientClasses = getGradeGradient(percentage);

    return (
        <div className="mb-6 p-4 bg-gray-50 rounded-xl border border-gray-200">
            <div className="flex items-center justify-between mb-4">
                <h3 className="text-lg font-semibold text-gray-900 flex items-center gap-2">
                    <Scale className="w-5 h-5 text-gray-400" /> Grading Method
                </h3>
                <div className="flex gap-2">
                    <button
                        onClick={() => setUseComplexGrading(false)}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                            !useComplexGrading
                                ? 'bg-primary-600 text-white shadow'
                                : 'bg-white text-gray-700 border border-gray-300'
                        }`}
                    >
                        Simple
                    </button>
                    <button
                        onClick={() => setUseComplexGrading(true)}
                        className={`px-4 py-2 rounded-lg text-sm font-medium transition ${
                            useComplexGrading
                                ? 'bg-primary-600 text-white shadow'
                                : 'bg-white text-gray-700 border border-gray-300'
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
                                className="text-sm text-gray-400 hover:text-gray-600 transition"
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
                        <label className="block text-sm font-medium text-gray-700 mb-1">Select Grading Method</label>
                        <Listbox value={selectedMethodId || ''} onChange={setSelectedMethodId}>
                            {() => (
                                <div className="relative">
                                    <Listbox.Button className="relative w-full bg-white border border-gray-300 rounded-md shadow-sm pl-3 pr-10 py-2 text-left cursor-default focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500 sm:text-sm">
                                        <span className="block truncate">
                                            {selectedMethod?.name || 'Select a grading method...'}
                                        </span>
                                        <span className="absolute inset-y-0 right-0 flex items-center pr-2 pointer-events-none">
                                            <ChevronDown className="h-4 w-4 text-gray-400" />
                                        </span>
                                    </Listbox.Button>
                                    <Transition
                                        as={Fragment}
                                        leave="transition ease-in duration-100"
                                        leaveFrom="opacity-100"
                                        leaveTo="opacity-0"
                                    >
                                        <Listbox.Options className="absolute z-10 mt-1 w-full bg-white shadow-lg max-h-60 rounded-md py-1 text-base ring-1 ring-black ring-opacity-5 overflow-auto sm:text-sm">
                                            {gradingMethods.map((method) => (
                                                <Listbox.Option
                                                    key={method.id}
                                                    value={method.id}
                                                    className={({ active }) =>
                                                        `cursor-pointer select-none relative py-2 pl-10 pr-4 ${
                                                            active ? 'bg-primary-100 text-primary-900' : 'text-gray-900'
                                                        }`
                                                    }
                                                >
                                                    {({ selected }) => (
                                                        <>
                                                            <span className={`block truncate ${selected ? 'font-medium' : 'font-normal'}`}>
                                                                {method.name}
                                                            </span>
                                                            {selected && (
                                                                <span className="absolute inset-y-0 left-0 flex items-center pl-3 text-primary-600">
                                                                    <Check className="w-5 h-5" />
                                                                </span>
                                                            )}
                                                        </>
                                                    )}
                                                </Listbox.Option>
                                            ))}
                                        </Listbox.Options>
                                    </Transition>
                                </div>
                            )}
                        </Listbox>
                    </div>

                    {selectedMethod && (
                        <div className="mt-4">
                            <div className="mb-4 p-4 bg-gray-50 rounded-md border border-gray-200">
                                <div className="flex justify-between items-center">
                                    <NormalizedStarDisplay
                                        currentGrade={overallGrade!.currentGrade}
                                        minGrade={overallGrade!.minGrade}
                                        maxGrade={overallGrade!.maxGrade}
                                        size="md"
                                    />

                                    <div className="flex flex-col items-end">
                                        <div className="px-3 py-1 rounded-md font-medium text-center text-sm ${colorClasses.background} ${colorClasses.text}">
                                            <span className="text-lg">{overallGrade!.currentGrade.toFixed(1)}</span>
                                            <span>/{overallGrade!.maxGrade.toFixed(1)}</span>
                                        </div>
                                    </div>
                                </div>

                                <div className="mt-2 w-full bg-gray-200 rounded-full h-2.5">
                                    <div
                                        className={`h-2.5 rounded-full bg-gradient-to-r ${gradientClasses}`}
                                        style={{ width: `${(overallGrade!.currentGrade / overallGrade!.maxGrade) * 100}%` }}
                                    ></div>
                                </div>
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
