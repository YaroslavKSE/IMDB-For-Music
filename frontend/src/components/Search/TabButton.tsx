import React from 'react';
import { TabButtonProps } from './types';

const TabButton: React.FC<TabButtonProps> = ({
                                                 active,
                                                 onClick,
                                                 icon,
                                                 label
                                             }) => {
    return (
        <button
            type="button"
            onClick={onClick}
            className={`mr-8 py-4 px-1 border-b-2 font-medium text-sm flex items-center ${
                active
                    ? 'border-primary-600 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
        >
            {icon && <span className="mr-1">{icon}</span>}
            {label}
        </button>
    );
};

export default TabButton;