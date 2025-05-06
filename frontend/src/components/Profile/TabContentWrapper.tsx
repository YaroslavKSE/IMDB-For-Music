import React, { ReactNode } from 'react';

interface TabContentWrapperProps {
  children: ReactNode;
  title?: string;
  icon?: ReactNode;
  className?: string;
  backgroundColorClass?: string;
}

const TabContentWrapper: React.FC<TabContentWrapperProps> = ({
  children,
  title,
  icon,
  className = '',
  backgroundColorClass = 'bg-primary-50'
}) => {
  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      {title && (
        <div className={`px-6 py-4 ${backgroundColorClass} border-b border-primary-100`}>
          <h3 className="text-lg font-medium text-primary-800 flex items-center">
            {icon && <span className="mr-2">{icon}</span>}
            {title}
          </h3>
        </div>
      )}
      <div className={`p-6 ${className}`}>
        {children}
      </div>
    </div>
  );
};

export default TabContentWrapper;