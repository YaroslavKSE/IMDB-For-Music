import {ReactNode} from "react";

interface TabButtonProps {
    active: boolean;
    onClick: () => void;
    icon: ReactNode;
    label: string;
}

const TabButton = ({ active, onClick, icon, label }: TabButtonProps) => {
    return (
        <button
            onClick={onClick}
            className={`mr-8 py-4 px-6 border-b-2 font-medium text-sm flex items-center ${
                active
                    ? 'border-primary-600 text-primary-600'
                    : 'border-transparent text-gray-500 hover:text-gray-700 hover:border-gray-300'
            }`}
        >
            {icon}
            {label}
        </button>
    );
};

export default TabButton;