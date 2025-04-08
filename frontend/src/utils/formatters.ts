/**
 * Format milliseconds duration to MM:SS format
 */
export const formatDuration = (durationMs: number): string => {
    const minutes = Math.floor(durationMs / 60000);
    const seconds = Math.floor((durationMs % 60000) / 1000);
    return `${minutes}:${seconds.toString().padStart(2, '0')}`;
};

/**
 * Format a date string to YYYY-MM-DD format
 */
export const formatDate = (dateString: string): string => {
    if (!dateString) return 'Unknown';

    try {
        const date = new Date(dateString);
        return date.toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric'
        });
    } catch (error) {
        return error as string; // Return the original string if parsing fails
    }
};

/**
 * Truncate a string to a specified length
 */
export const truncateString = (str: string, maxLength: number = 100): string => {
    if (!str || str.length <= maxLength) return str;
    return `${str.substring(0, maxLength)}...`;
};

/**
 * Format a number with commas (e.g., 1,234,567)
 */
export const formatNumberWithCommas = (num: number): string => {
    return new Intl.NumberFormat('en-US').format(num);
};

/**
 * Format a large number to a more readable format (e.g., 1.2M, 4.5K)
 */
export const formatLargeNumber = (num: number): string => {
    if (num >= 1000000) {
        return `${(num / 1000000).toFixed(1)}M`;
    } else if (num >= 1000) {
        return `${(num / 1000).toFixed(1)}K`;
    }
    return num.toString();
};