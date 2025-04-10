// src/utils/error-handler.ts

interface ApiErrorResponse {
  code?: string;
  message?: string;
  traceId?: string;
}

interface ErrorWithResponse extends Error {
  response?: {
    data?: ApiErrorResponse;
    status?: number;
  };
}

/**
 * Common error types for reuse throughout the application
 */
export const ErrorCodes = {
  UsernameAlreadyTaken: 'UsernameAlreadyTaken',
  UserAlreadyExists: 'UserAlreadyExists',
  ValidationError: 'ValidationError',
  AuthenticationError: 'AuthenticationError',
  Auth0Error: 'Auth0Error',
  InternalServerError: 'InternalServerError'
};

/**
 * Extracts a user-friendly error message from API error responses
 * @param error The error caught from API request
 * @param defaultMessage Default message to display if error parsing fails
 * @returns A user-friendly error message
 */
export const getErrorMessage = (error: unknown, defaultMessage = 'An unexpected error occurred'): string => {
  // Handle axios error responses
  if (typeof error === 'object' && error !== null) {
    const err = error as ErrorWithResponse;

    // Check for API response with error details
    if (err.response?.data) {
      // Return the API error message if available
      return err.response.data.message || defaultMessage;
    }

    // Return a basic error message if we have an error object with message
    if ('message' in err && typeof err.message === 'string') {
      return err.message;
    }
  }

  // If all else fails, return the default message
  return defaultMessage;
};

/**
 * Get the error code from a thrown error
 * @param error The error to extract the code from
 * @returns The error code or undefined if not found
 */
export const getErrorCode = (error: unknown): string | undefined => {
  if (typeof error === 'object' && error !== null) {
    const err = error as ErrorWithResponse;
    return err.response?.data?.code;
  }
  return undefined;
};

/**
 * Checks if the error is a specific type based on the error code
 * @param error The error to check
 * @param errorCode The error code to check for
 * @returns true if the error matches the code
 */
export const isErrorWithCode = (error: unknown, errorCode: string): boolean => {
  return getErrorCode(error) === errorCode;
};