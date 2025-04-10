import {RegisterOptions, ValidateResult} from "react-hook-form";

type RegisterFormValues = {
  name: string;
  surname: string;
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
};

export const nameValidation = {
  required: 'First name is required',
  maxLength: {
    value: 100,
    message: 'First name is too long',
  }
};

export const surnameValidation = {
  required: 'Last name is required',
  maxLength: {
    value: 100,
    message: 'Last name is too long',
  }
};

export const usernameValidation = {
  required: 'Username is required',
  minLength: {
    value: 3,
    message: 'Username must be at least 3 characters',
  },
  maxLength: {
    value: 50,
    message: 'Username is too long',
  },
  pattern: {
    value: /^[a-zA-Z0-9_\-.]+$/,
    message: 'Username can only contain letters, numbers, underscores, hyphens, and periods',
  }
};

export const emailValidation = {
  required: 'Email is required',
  pattern: {
    value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
    message: 'Invalid email address',
  }
};

export const passwordValidation = {
  required: 'Password is required',
  minLength: {
    value: 8,
    message: 'Password must be at least 8 characters',
  },
  pattern: {
    value: /^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])/,
    message: 'Password must include an uppercase letter, a number, and a special character',
  }
};



export const createConfirmPasswordValidation = (password: string): RegisterOptions<RegisterFormValues, 'confirmPassword'> => ({
  required: 'Please confirm your password',
  validate: (value: string): ValidateResult =>
    value === password || 'Passwords do not match'
});