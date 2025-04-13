export const getGradeGradient = (percentage: number): string => {
    // Ensure percentage is between 0 and 1
    const normalizedPercentage = Math.max(0, Math.min(1, percentage));
    const percentValue = normalizedPercentage * 100;

    if (percentValue >= 80) {
        return 'from-purple-700 to-purple-800';
    } else if (percentValue >= 70) {
        return 'from-purple-600 to-purple-700';
    } else if (percentValue >= 60) {
        return 'from-purple-500 to-purple-600';
    } else if (percentValue >= 50) {
        return 'from-purple-400 to-purple-500';
    } else if (percentValue >= 40) {
        return 'from-purple-300 to-purple-400';
    } else {
        return 'from-purple-200 to-purple-300';
    }
};

export const getGradeColorClasses = (percentage: number): { background: string, text: string } => {
    // Ensure percentage is between 0 and 1
    const normalizedPercentage = Math.max(0, Math.min(1, percentage));
    const percentValue = normalizedPercentage * 100;

    if (percentValue >= 80) {
        return { background: 'bg-purple-700', text: 'text-white' };
    } else if (percentValue >= 70) {
        return { background: 'bg-purple-600', text: 'text-white' };
    } else if (percentValue >= 60) {
        return { background: 'bg-purple-500', text: 'text-white' };
    } else if (percentValue >= 50) {
        return { background: 'bg-purple-400', text: 'text-white' };
    } else if (percentValue >= 40) {
        return { background: 'bg-purple-300', text: 'text-white' };
    } else {
        return { background: 'bg-purple-200', text: 'text-purple-800' };
    }
};