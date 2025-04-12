import { GradeComponent, BlockComponent } from '../api/interaction';

/**
 * Checks if a maximum value can be reached from a minimum value using the given step amount
 */
export const canReachMaxValueWithSteps = (min: number, max: number, step: number): boolean => {
    if (step < 0) return false; // Step must be positive
    if (step == 0 && max != min) return false;
    if (step == 0 && max == min) return true;
    // Calculate how many steps needed to go from min to max
    const stepsNeeded = (max - min) / step;

    // Check if stepsNeeded is an integer or close to an integer (floating point precision)
    return Math.abs(Math.round(stepsNeeded) - stepsNeeded) < 0.000001;
};

/**
 * Checks if a component (or its nested components) can produce a zero value
 */
export const canProduceZeroValue = (component: GradeComponent | BlockComponent): boolean => {
    if (component.componentType === 'grade') {
        // Check if the grade component's range includes zero
        return component.minGrade <= 0 && component.maxGrade >= 0;
    } else if (component.componentType === 'block') {
        // For blocks, check if any of its subcomponents can produce zero
        return component.subComponents.some(subComponent => canProduceZeroValue(subComponent));
    }
    return false;
};

/**
 * Checks if there are any potential division by zero operations in the components
 */
export const hasDivisionByZeroRisk = (
    components: (GradeComponent | BlockComponent)[],
    actions: number[]
): boolean => {
    // No division risk if there are no components or just one component
    if (components.length <= 1) return false;

    for (let i = 0; i < actions.length; i++) {
        // Check only division operations (action code 3)
        if (actions[i] === 3) {
            // The component after the division operation (the divisor)
            const divisorComponent = components[i+1];

            // Check if this component can potentially produce a zero value
            if (canProduceZeroValue(divisorComponent)) {
                return true;
            }
        }
    }

    // Also check nested block components for division operations
    for (const component of components) {
        if (component.componentType === 'block') {
            if (hasDivisionByZeroRisk(component.subComponents, component.actions)) {
                return true;
            }
        }
    }

    return false;
};

/**
 * Validates a grade component's configuration
 */
export const validateGradeComponent = (component: GradeComponent): { valid: boolean; error?: string } => {
    // Check if component has a name
    if (!component.name.trim()) {
        return {
            valid: false,
            error: "All grade components must have a name"
        };
    }

    // Check if max is greater than min
    if (component.maxGrade < component.minGrade) {
        return {
            valid: false,
            error: `"${component.name}" has a maximum grade (${component.maxGrade}) that is not greater than its minimum grade (${component.minGrade})`
        };
    }

    // Check if we can reach max from min using step
    if (!canReachMaxValueWithSteps(component.minGrade, component.maxGrade, component.stepAmount)) {
        return {
            valid: false,
            error: `"${component.name}" has a step amount (${component.stepAmount}) that doesn't allow reaching the maximum grade (${component.maxGrade}) from the minimum grade (${component.minGrade})`
        };
    }

    return { valid: true };
};

/**
 * Recursively validates all components and nested components
 */
export const validateAllComponents = (
    components: (GradeComponent | BlockComponent)[]
): { valid: boolean; error?: string } => {
    for (const component of components) {
        if (component.componentType === 'grade') {
            const result = validateGradeComponent(component);
            if (!result.valid) return result;
        } else if (component.componentType === 'block') {
            // Check the name
            if (!component.name.trim()) {
                return {
                    valid: false,
                    error: "All block components must have a name"
                };
            }

            // If it's empty but has a name, it's valid
            if (component.subComponents.length === 0) {
                return {
                    valid: false,
                    error: `Block "${component.name}" must have at least one subcomponent`
                };
            }

            // Validate nested components
            const result = validateAllComponents(component.subComponents);
            if (!result.valid) return result;

            // Check for division by zero in this block
            if (hasDivisionByZeroRisk(component.subComponents, component.actions)) {
                return {
                    valid: false,
                    error: `Block "${component.name}" has a division operation where the divisor could be zero. Please adjust the grade ranges to prevent division by zero.`
                };
            }
        }
    }

    return { valid: true };
};

/**
 * Validates an entire grading method
 */
export const validateGradingMethod = (
    name: string,
    components: (GradeComponent | BlockComponent)[],
    actions: number[]
): { valid: boolean; error?: string } => {
    // Basic validations
    if (!name.trim()) {
        return {
            valid: false,
            error: 'Please provide a name for your grading method'
        };
    }

    if (components.length === 0) {
        return {
            valid: false,
            error: 'Please add at least one component to your grading method'
        };
    }

    // Validate all components
    const componentsValidation = validateAllComponents(components);
    if (!componentsValidation.valid) {
        return componentsValidation;
    }

    // Check for division by zero at the top level
    if (hasDivisionByZeroRisk(components, actions)) {
        return {
            valid: false,
            error: 'Your grading method has a division operation where the divisor could be zero. Please adjust the grade ranges to prevent division by zero.'
        };
    }

    return { valid: true };
};