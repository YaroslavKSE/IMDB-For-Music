import { Save } from 'lucide-react';
import useIsMobile from '../../utils/useIsMobile';

const ResponsiveSubmitButton = ({ onClick, disabled, isSubmitting }: {
  onClick: () => void;
  disabled: boolean;
  isSubmitting: boolean;
}) => {
  const isMobile = useIsMobile();

  if (isMobile) {
    return (
      <button
        onClick={onClick}
        disabled={disabled}
        className="fixed bottom-6 right-6 bg-primary-600 hover:bg-primary-700 text-white p-3 rounded-full shadow-lg disabled:opacity-50"
      >
        {isSubmitting ? (
          <div className="animate-spin rounded-full h-5 w-5 border-t-2 border-b-2 border-white"></div>
        ) : (
          <Save className="h-5 w-5" />
        )}
      </button>
    );
  }

  return (
    <button
      type="button"
      onClick={onClick}
      disabled={disabled}
      className="inline-flex justify-center px-6 py-3 border border-transparent text-base font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:bg-primary-400 disabled:cursor-not-allowed"
    >
      {isSubmitting ? (
        <>
          <div className="animate-spin rounded-full h-5 w-5 border-t-2 border-b-2 border-white mr-3"></div>
          Creating...
        </>
      ) : (
        <>
          <Save className="h-5 w-5 mr-2" />
          Create Grading Method
        </>
      )}
    </button>
  );
};

export default ResponsiveSubmitButton;
