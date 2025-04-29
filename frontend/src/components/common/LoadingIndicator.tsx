import React from 'react';

interface LoadingIndicatorProps {
  size?: 'small' | 'medium' | 'large';
  type?: 'spinner' | 'bar' | 'pulse';
  text?: string;
  fullScreen?: boolean;
  className?: string;
  color?: 'primary' | 'secondary' | 'gray';
}

const LoadingIndicator: React.FC<LoadingIndicatorProps> = ({
  size = 'medium',
  type = 'spinner',
  text,
  fullScreen = false,
  className = '',
  color = 'primary'
}) => {
  // Size mappings
  const sizeMap = {
    small: {
      spinner: 'h-4 w-4',
      bar: 'h-0.5',
      text: 'text-xs'
    },
    medium: {
      spinner: 'h-8 w-8',
      bar: 'h-1',
      text: 'text-sm'
    },
    large: {
      spinner: 'h-12 w-12',
      bar: 'h-1.5',
      text: 'text-base'
    }
  };

  // Color mappings
  const colorMap = {
    primary: {
      spinner: 'border-primary-600',
      bar: 'bg-primary-600',
      pulse: 'bg-primary-100',
      text: 'text-primary-600',
      muted: 'text-primary-500'
    },
    secondary: {
      spinner: 'border-secondary-600',
      bar: 'bg-secondary-600',
      pulse: 'bg-secondary-100',
      text: 'text-secondary-600',
      muted: 'text-secondary-500'
    },
    gray: {
      spinner: 'border-gray-600',
      bar: 'bg-gray-600',
      pulse: 'bg-gray-100',
      text: 'text-gray-600',
      muted: 'text-gray-500'
    }
  };

  // Wrapper
  const wrapperClasses = fullScreen
    ? 'fixed inset-0 flex items-center justify-center bg-white bg-opacity-80 z-50'
    : 'flex flex-col items-center justify-center';

  // Render the appropriate loading indicator based on type
  const renderLoader = () => {
    switch (type) {
      case 'spinner':
        return (
          <div className={`${sizeMap[size].spinner} rounded-full border-2 border-b-transparent ${colorMap[color].spinner} animate-spin ${className}`}></div>
        );
      case 'bar':
        return (
          <div className={`w-full ${sizeMap[size].bar} ${colorMap[color].pulse} rounded overflow-hidden ${className}`}>
            <div
              className={`${sizeMap[size].bar} ${colorMap[color].bar} rounded animate-pulse`}
              style={{ width: '30%', animation: 'progressBar 1.5s ease-in-out infinite' }}
            ></div>
          </div>
        );
      case 'pulse':
        return (
          <div className={`${sizeMap[size].spinner} ${colorMap[color].pulse} rounded-full animate-pulse ${className}`}></div>
        );
      default:
        return (
          <div className={`${sizeMap[size].spinner} rounded-full border-2 border-b-transparent ${colorMap[color].spinner} animate-spin ${className}`}></div>
        );
    }
  };

  return (
    <div className={wrapperClasses}>
      {renderLoader()}
      {text && (
        <div className={`mt-3 ${sizeMap[size].text} ${colorMap[color].muted}`}>
          {text}
        </div>
      )}
      <style>
        {`
          @keyframes progressBar {
            0% { margin-left: -30%; }
            100% { margin-left: 100%; }
          }
        `}
      </style>
    </div>
  );
};

export default LoadingIndicator;