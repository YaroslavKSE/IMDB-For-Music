// src/utils/grading-utils.ts
export const getOperationSymbol = (operation: number): string => {
    switch (operation) {
        case 0:
            return '+';
        case 1:
            return '−';
        case 2:
            return '×';
        case 3:
            return '÷';
        default:
            return '+';
    }
};

export const getOperationName = (operation: number): string => {
    switch (operation) {
        case 0:
            return 'Addition';
        case 1:
            return 'Subtraction';
        case 2:
            return 'Multiplication';
        case 3:
            return 'Division';
        default:
            return 'Addition';
    }
};

export const getOperationDescription = (operation: number): string => {
    switch (operation) {
        case 0:
            return 'Components are added together (A + B)';
        case 1:
            return 'Second component is subtracted from the first (A - B)';
        case 2:
            return 'Components are multiplied together (A × B)';
        case 3:
            return 'First component is divided by the second (A ÷ B)';
        default:
            return 'Components are added together (A + B)';
    }
};