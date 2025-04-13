import { BlockComponent, GradeComponent } from "../../api/interaction";

export interface BlockGradeResult {
    name: string;
    currentGrade: number;
    maxGrade: number;
    minGrade: number;
}

export const calculateGradeComponentValue = (
    component: GradeComponent,
    values: Record<string, number>,
    path = ''
): number => {
    const fullPath = path ? `${path}.${component.name}` : component.name;
    return values[fullPath] || component.minGrade;
};

export const calculateBlockValue = (
    component: BlockComponent,
    values: Record<string, number>,
    path = ''
): BlockGradeResult => {
    const fullPath = path ? `${path}.${component.name}` : component.name;

    const result: BlockGradeResult = {
        name: component.name,
        currentGrade: 0,
        maxGrade: 0,
        minGrade: 0
    };

    if (component.subComponents.length === 0) {
        return result;
    }

    let currentValue: number;
    let minValue: number;
    let maxValue: number;

    const firstComponent = component.subComponents[0];

    if (firstComponent.componentType === 'grade') {
        currentValue = calculateGradeComponentValue(firstComponent, values, fullPath);
        minValue = firstComponent.minGrade;
        maxValue = firstComponent.maxGrade;
    } else {
        const blockResult = calculateBlockValue(firstComponent as BlockComponent, values, fullPath);
        currentValue = blockResult.currentGrade;
        minValue = blockResult.minGrade;
        maxValue = blockResult.maxGrade;
    }

    for (let i = 1; i < component.subComponents.length; i++) {
        const subComponent = component.subComponents[i];
        const action = component.actions[i - 1];
        const operationCode = getOperation(action);

        let nextCurrentValue: number;
        let nextMinValue: number;
        let nextMaxValue: number;

        if (subComponent.componentType === 'grade') {
            nextCurrentValue = calculateGradeComponentValue(subComponent, values, fullPath);
            nextMinValue = subComponent.minGrade;
            nextMaxValue = subComponent.maxGrade;
        } else {
            const blockResult = calculateBlockValue(subComponent as BlockComponent, values, fullPath);
            nextCurrentValue = blockResult.currentGrade;
            nextMinValue = blockResult.minGrade;
            nextMaxValue = blockResult.maxGrade;
        }

        [currentValue, minValue, maxValue] = applyOperation(
            operationCode,
            currentValue,
            minValue,
            maxValue,
            nextCurrentValue,
            nextMinValue,
            nextMaxValue
        );
    }

    result.currentGrade = Number(currentValue.toFixed(2));
    result.minGrade = Number(minValue.toFixed(2));
    result.maxGrade = Number(maxValue.toFixed(2));

    return result;
};

export const calculateOverallGrade = (
    components: (GradeComponent | BlockComponent)[],
    actions: number[] | string[],
    values: Record<string, number>
): BlockGradeResult => {
    const result: BlockGradeResult = {
        name: "Overall",
        currentGrade: 0,
        maxGrade: 0,
        minGrade: 0
    };

    if (components.length === 0) {
        return result;
    }

    let currentValue: number;
    let minValue: number;
    let maxValue: number;

    const firstComponent = components[0];

    if (firstComponent.componentType === 'grade') {
        currentValue = calculateGradeComponentValue(firstComponent, values);
        minValue = firstComponent.minGrade;
        maxValue = firstComponent.maxGrade;
    } else {
        const blockResult = calculateBlockValue(firstComponent as BlockComponent, values);
        currentValue = blockResult.currentGrade;
        minValue = blockResult.minGrade;
        maxValue = blockResult.maxGrade;
    }

    for (let i = 1; i < components.length; i++) {
        const component = components[i];
        const action = actions[i - 1];
        const operationCode = getOperation(action);

        let nextCurrentValue: number;
        let nextMinValue: number;
        let nextMaxValue: number;

        if (component.componentType === 'grade') {
            nextCurrentValue = calculateGradeComponentValue(component, values);
            nextMinValue = component.minGrade;
            nextMaxValue = component.maxGrade;
        } else {
            const blockResult = calculateBlockValue(component as BlockComponent, values);
            nextCurrentValue = blockResult.currentGrade;
            nextMinValue = blockResult.minGrade;
            nextMaxValue = blockResult.maxGrade;
        }

        [currentValue, minValue, maxValue] = applyOperation(
            operationCode,
            currentValue,
            minValue,
            maxValue,
            nextCurrentValue,
            nextMinValue,
            nextMaxValue
        );
    }

    result.currentGrade = Number(currentValue.toFixed(2));
    result.minGrade = Number(minValue.toFixed(2));
    result.maxGrade = Number(maxValue.toFixed(2));

    return result;
};

const getOperation = (action: number | string): number => {
    if (typeof action === 'number') {
        return action;
    } else {
        if (action === 'Add') return 0;
        else if (action === 'Subtract') return 1;
        else if (action === 'Multiply') return 2;
        else if (action === 'Divide') return 3;
    }
    return 0;
};

const applyOperation = (
    operationCode: number,
    currentValue: number,
    minValue: number,
    maxValue: number,
    nextCurrentValue: number,
    nextMinValue: number,
    nextMaxValue: number
): [number, number, number] => {
    switch (operationCode) {
        case 0: // Addition
            return [
                currentValue + nextCurrentValue,
                minValue + nextMinValue,
                maxValue + nextMaxValue
            ];
        case 1: // Subtraction
            return [
                currentValue - nextCurrentValue,
                minValue - nextMaxValue,
                maxValue - nextMinValue
            ];
        case 2: { // Multiplication
            const newCurrent = currentValue * nextCurrentValue;
            const products = [
                minValue * nextMinValue,
                minValue * nextMaxValue,
                maxValue * nextMinValue,
                maxValue * nextMaxValue
            ];
            return [newCurrent, Math.min(...products), Math.max(...products)];
        }
        case 3: { // Division
            if (nextCurrentValue !== 0) {
                const newCurrent = currentValue / nextCurrentValue;
                const nextMinNonZero = nextMinValue === 0 ? 0.00001 : nextMinValue;
                const nextMaxNonZero = nextMaxValue === 0 ? 0.00001 : nextMaxValue;
                const quotients = [
                    minValue / nextMinNonZero,
                    minValue / nextMaxNonZero,
                    maxValue / nextMinNonZero,
                    maxValue / nextMaxNonZero
                ];
                return [newCurrent, Math.min(...quotients), Math.max(...quotients)];
            } else {
                return [maxValue, minValue, maxValue];
            }
        }
        default: // Default to addition
            return [
                currentValue + nextCurrentValue,
                minValue + nextMinValue,
                maxValue + nextMaxValue
            ];
    }
};
