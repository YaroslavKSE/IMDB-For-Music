import React, { ReactNode } from 'react';
import { Eye, EyeOff, Lock } from 'lucide-react';
import {
  UseFormRegister,
  FieldErrors,
  RegisterOptions,
  FieldValues,
  Path
} from 'react-hook-form';
type RegisterFormValues = {
  name: string;
  surname: string;
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
};

interface FormErrorMessageProps {
  error: string | null;
}

export const FormErrorMessage = ({ error }: FormErrorMessageProps) => {
  if (!error) return null;

  return (
    <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
      {error}
    </div>
  );
};

interface FormSuccessMessageProps {
  message: string | null;
}

export const FormSuccessMessage = ({ message }: FormSuccessMessageProps) => {
  if (!message) return null;

  return (
    <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
      {message}
    </div>
  );
};

interface TextInputProps<T extends FieldValues> {
  id: Path<T>;
  label: string;
  register: UseFormRegister<T>;
  registerOptions?: RegisterOptions<T>;
  errors: FieldErrors<T>;
  icon: React.ReactNode;
  type?: string;
  placeholder?: string;
  errorHighlight?: boolean;
  description?: string;
}

export const TextInput = <T extends FieldValues>({
  id,
  label,
  register,
  registerOptions,
  errors,
  icon,
  type = 'text',
  placeholder = '',
  errorHighlight = false,
  description
}: TextInputProps<T>) => {
  const fieldError = errors[id];

  return (
    <div>
      <label htmlFor={id} className="block text-sm font-medium text-gray-700">
        {label}
      </label>
      <div className="mt-1 relative rounded-md shadow-sm">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          {icon}
        </div>
        <input
          id={id}
          {...register(id, registerOptions)}
          type={type}
          className={`block w-full pl-10 pr-3 py-2 border ${
            errorHighlight || fieldError ? 'border-red-300 bg-red-50' : 'border-gray-300'
          } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
          placeholder={placeholder}
        />
      </div>
      {fieldError ? (
        <p className="mt-1 text-sm text-red-600">{fieldError.message as string}</p>
      ) : description ? (
        <p className="mt-1 text-xs text-gray-500">{description}</p>
      ) : null}
    </div>
  );
};

interface PasswordInputProps<T extends FieldValues> extends Omit<TextInputProps<T>, 'type' | 'icon'> {
  showPassword: boolean;
  onTogglePassword: () => void;
}
export const PasswordInput = <T extends FieldValues>({
  id,
  label,
  register,
  registerOptions,
  errors,
  showPassword,
  onTogglePassword,
  placeholder = '',
  errorHighlight = false,
  description
}: PasswordInputProps<T>) => {
  const fieldError = errors[id];

  return (
    <div>
      <label htmlFor={id} className="block text-sm font-medium text-gray-700">
        {label}
      </label>
      <div className="mt-1 relative rounded-md shadow-sm">
        <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
          <Lock className="h-5 w-5 text-gray-400" />
        </div>
        <input
          id={id}
          {...register(id, registerOptions)}
          type={showPassword ? 'text' : 'password'}
          className={`block w-full pl-10 pr-10 py-2 border ${
            errorHighlight || fieldError ? 'border-red-300 bg-red-50' : 'border-gray-300'
          } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
          placeholder={placeholder}
        />
        <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
          <button
            type="button"
            onClick={onTogglePassword}
            className="text-gray-400 hover:text-gray-500 focus:outline-none"
          >
            {showPassword ? <EyeOff className="h-5 w-5" /> : <Eye className="h-5 w-5" />}
          </button>
        </div>
      </div>
      {fieldError ? (
        <p className="mt-1 text-sm text-red-600">{fieldError.message as string}</p>
      ) : description ? (
        <p className="mt-1 text-xs text-gray-500">{description}</p>
      ) : null}
    </div>
  );
};

interface CheckboxInputProps {
  id: string;
  label: ReactNode;
  register: UseFormRegister<RegisterFormValues>;
  registerOptions?: RegisterOptions<RegisterFormValues>;
  errors: FieldErrors<RegisterFormValues>;
}

export const CheckboxInput = ({
  id,
  label,
  register,
  registerOptions,
  errors
}: CheckboxInputProps) => {
  const fieldName = id as keyof RegisterFormValues;
  const fieldError = errors[fieldName];

  return (
    <div>
      <div className="flex items-center">
        <input
          id={id}
          type="checkbox"
          {...register(fieldName, registerOptions)}
          className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
        />
        <label htmlFor={id} className="ml-2 block text-sm text-gray-900">
          {label}
        </label>
      </div>
      {fieldError && (
        <p className="text-sm text-red-600">{fieldError.message as string}</p>
      )}
    </div>
  );
};