import React, { ReactNode } from 'react';
import { Search } from 'lucide-react';

interface EmptyStateProps {
    title: string;
    message: string;
    icon?: ReactNode;
    action?: {
        label: string;
        onClick: () => void;
    };
}

const EmptyState: React.FC<EmptyStateProps> = ({
                                                   title,
                                                   message,
                                                   icon,
                                                   action
                                               }) => {
    return (
        <div className="text-center py-12 px-4 rounded-lg border border-gray-200 bg-white">
            <div className="mx-auto flex justify-center">
                {icon || <Search className="h-12 w-12 text-gray-400" />}
            </div>
            <h3 className="mt-4 text-lg font-medium text-gray-900">{title}</h3>
            <p className="mt-2 text-sm text-gray-500 max-w-md mx-auto">{message}</p>
            {action && (
                <div className="mt-6">
                    <button
                        type="button"
                        onClick={action.onClick}
                        className="inline-flex items-center px-4 py-2 border border-transparent text-sm font-medium rounded-md shadow-sm text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500"
                    >
                        {action.label}
                    </button>
                </div>
            )}
        </div>
    );
};

export default EmptyState;