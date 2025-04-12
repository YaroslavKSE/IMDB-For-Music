import { Save } from 'lucide-react';

const InteractionSubmitButton = ({
                                     handleSubmit,
                                     isSubmitting,
                                     hasListened
                                 }: {
    handleSubmit: () => void;
    isSubmitting: boolean;
    hasListened: boolean;
}) => (
    <div className="flex justify-end">
        <button
            type="button"
            onClick={handleSubmit}
            disabled={isSubmitting || !hasListened}
            className="px-6 py-3 flex items-center justify-center border border-transparent text-base font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed"
        >
            {isSubmitting ? (
                <>
                    <svg
                        className="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
                        xmlns="http://www.w3.org/2000/svg"
                        fill="none"
                        viewBox="0 0 24 24"
                    >
                        <circle
                            className="opacity-25"
                            cx="12"
                            cy="12"
                            r="10"
                            stroke="currentColor"
                            strokeWidth="4"
                        ></circle>
                        <path
                            className="opacity-75"
                            fill="currentColor"
                            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                        ></path>
                    </svg>
                    Submitting...
                </>
            ) : (
                <>
                    <Save className="h-5 w-5 mr-2" />
                    Submit Interaction
                </>
            )}
        </button>
    </div>
);

export default InteractionSubmitButton;
