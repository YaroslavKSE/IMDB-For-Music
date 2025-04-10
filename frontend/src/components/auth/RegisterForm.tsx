import { useState } from 'react';
import { useNavigate, Link } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { User, UserPlus, AtSign, Mail } from 'lucide-react';
import {
  TextInput,
  PasswordInput,
  CheckboxInput,
  FormErrorMessage,
  FormSuccessMessage
} from '../common/FormComponents';
import {
  nameValidation,
  surnameValidation,
  usernameValidation,
  emailValidation,
  passwordValidation
} from './RegisterFormValidation.ts';
import useAuthStore from '../../store/authStore';
import { handleAuth0Login } from '../../utils/auth0-config';
import { getErrorMessage, getErrorCode, ErrorCodes } from '../../utils/error-handler';

type RegisterFormValues = {
  name: string;
  surname: string;
  username: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
};

const RegisterForm = () => {
  const navigate = useNavigate();
  const { register: registerUser, isLoading, clearError } = useAuthStore();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [fieldErrors, setFieldErrors] = useState<{
    username?: boolean;
    email?: boolean;
  }>({});
  const [socialLoading, setSocialLoading] = useState(false);

  const {
    register,
    handleSubmit,
    // watch,
    formState: { errors }
  } = useForm<RegisterFormValues>({
    defaultValues: {
      name: '',
      surname: '',
      username: '',
      email: '',
      password: '',
      confirmPassword: '',
      acceptTerms: false
    }
  });

  // const password = watch('password');

  const onSubmit = async (data: RegisterFormValues) => {
    // Check password match first
    if (data.password !== data.confirmPassword) {
      setError('Passwords do not match');
      return;
    }

    try {
      clearError();
      setError(null);
      setFieldErrors({});

      await registerUser(data.email, data.password, data.name, data.surname, data.username);

      setSuccess('Registration successful! You can now log in.');
      setTimeout(() => navigate('/login'), 2000);
    } catch (err) {
      const errorCode = getErrorCode(err);
      const errorMessage = getErrorMessage(err, 'Registration failed. Please try again.');

      // Handle specific error types
      switch (errorCode) {
        case ErrorCodes.UsernameAlreadyTaken:
          setFieldErrors({ username: true });
          break;
        case ErrorCodes.UserAlreadyExists:
          setFieldErrors({ email: true });
          break;
        case ErrorCodes.ValidationError:
          // General validation error, no specific field to highlight
          break;
        case ErrorCodes.Auth0Error:
        case ErrorCodes.InternalServerError:
        default:
          // No specific field to highlight
          break;
      }

      setError(errorMessage);
    }
  };

  const handleGoogleSignup = () => {
    try {
      setSocialLoading(true);
      clearError();
      setError(null);
      setFieldErrors({});

      // This will redirect to Auth0
      handleAuth0Login('google-oauth2');
    } catch (err) {
      console.error('Google signup failed:', err);
      setError('An error occurred during Google authentication.');
      setSocialLoading(false);
    }
  };

  return (
    <div className="min-h-screen bg-gradient-to-b from-primary-100 to-white flex flex-col justify-center px-4 sm:px-6 lg:px-8">
      <div className="sm:mx-auto sm:w-full sm:max-w-md">
        <div className="text-center">
          <h2 className="text-3xl font-extrabold text-gray-900">BeatRate</h2>
          <p className="mt-2 text-sm text-gray-600">
            Your personal music discovery & review platform
          </p>
        </div>
      </div>

      <div className="mt-8 sm:mx-auto sm:w-full sm:max-w-md">
        <div className="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          <h2 className="mb-6 text-center text-2xl font-bold text-gray-900">
            Create your account
          </h2>

          {/* Success message */}
          <FormSuccessMessage message={success} />

          {/* Error message */}
          <FormErrorMessage error={error} />

          <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
            {/* First Name Field */}
            <TextInput
              id="name"
              label="First Name"
              register={register}
              registerOptions={nameValidation}
              errors={errors}
              icon={<User className="h-5 w-5 text-gray-400" />}
              placeholder="John"
            />

            {/* Last Name Field */}
            <TextInput
              id="surname"
              label="Last Name"
              register={register}
              registerOptions={surnameValidation}
              errors={errors}
              icon={<User className="h-5 w-5 text-gray-400" />}
              placeholder="Doe"
            />

            {/* Username Field */}
            <TextInput
              id="username"
              label="Username"
              register={register}
              registerOptions={usernameValidation}
              errors={errors}
              icon={<AtSign className="h-5 w-5 text-gray-400" />}
              placeholder="johndoe"
              errorHighlight={fieldErrors.username}
              description="Choose a unique username for your profile. This will be your public identifier."
            />

            {/* Email Field */}
            <TextInput
              id="email"
              label="Email address"
              register={register}
              registerOptions={emailValidation}
              errors={errors}
              icon={<Mail className="h-5 w-5 text-gray-400" />}
              type="email"
              placeholder="you@example.com"
              errorHighlight={fieldErrors.email}
            />

            {/* Password Field */}
            <PasswordInput
              id="password"
              label="Password"
              register={register}
              registerOptions={passwordValidation}
              errors={errors}
              showPassword={showPassword}
              onTogglePassword={() => setShowPassword(!showPassword)}
              placeholder="••••••••"
              description="Must be at least 8 characters with 1 uppercase, 1 number, and 1 special character"
            />

            {/* Confirm Password Field */}
            <PasswordInput
              id="confirmPassword"
              label="Confirm Password"
              register={register}
              registerOptions={{ required: 'Please confirm your password' }}
              errors={errors}
              showPassword={showConfirmPassword}
              onTogglePassword={() => setShowConfirmPassword(!showConfirmPassword)}
              placeholder="••••••••"
            />

            {/* Terms and Conditions Checkbox */}
            <CheckboxInput
              id="acceptTerms"
              label={
                <>
                  I agree to the <a href="#" className="text-primary-600 hover:text-primary-500">Terms of Service</a> and <a href="#" className="text-primary-600 hover:text-primary-500">Privacy Policy</a>
                </>
              }
              register={register}
              registerOptions={{ required: 'You must accept the terms and conditions' }}
              errors={errors}
            />

            {/* Submit Button */}
            <div>
              <button
                type="submit"
                disabled={isLoading}
                className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:bg-primary-400 disabled:cursor-not-allowed"
              >
                <span className="absolute left-0 inset-y-0 flex items-center pl-3">
                  <UserPlus className="h-5 w-5 text-primary-500 group-hover:text-primary-400"/>
                </span>
                {isLoading ? (
                  <span className="flex items-center">
                    <svg
                      className="animate-spin -ml-1 mr-3 h-5 w-5 text-white"
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      ></circle>
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      ></path>
                    </svg>
                    Creating account...
                  </span>
                ) : (
                  'Create Account'
                )}
              </button>
            </div>
          </form>

          <div className="mt-6">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300"/>
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-500">Or sign up with</span>
              </div>
            </div>

            <div className="mt-6">
              <button
                type="button"
                onClick={handleGoogleSignup}
                disabled={socialLoading}
                className="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
              >
                {socialLoading ? (
                  <span className="flex items-center">
                    <svg
                      className="animate-spin -ml-1 mr-3 h-5 w-5 text-gray-700"
                      xmlns="http://www.w3.org/2000/svg"
                      fill="none"
                      viewBox="0 0 24 24"
                    >
                      <circle
                        className="opacity-25"
                        cx="12"
                        cy="12"
                        r="10"
                        stroke="currentColor"
                        strokeWidth="4"
                      ></circle>
                      <path
                        className="opacity-75"
                        fill="currentColor"
                        d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                      ></path>
                    </svg>
                    Connecting...
                  </span>
                ) : (
                  <>
                    <svg className="h-5 w-5 mr-2" viewBox="0 0 24 24">
                      <path
                        d="M12.545,10.239v3.818h5.445c-0.712,2.315-2.647,3.972-5.445,3.972c-3.332,0-6.033-2.701-6.033-6.032s2.701-6.032,6.033-6.032c1.498,0,2.866,0.549,3.921,1.453l2.814-2.814C17.503,2.988,15.139,2,12.545,2C7.021,2,2.543,6.477,2.543,12s4.478,10,10.002,10c8.396,0,10.249-7.85,9.426-11.748L12.545,10.239z"
                        fill="#4285F4"
                      />
                      <path
                        d="M2.543,12c0-5.523,4.478-10,10.002-10c2.594,0,4.958,0.988,6.735,2.604l-2.814,2.814c-1.055-0.904-2.423-1.453-3.921-1.453c-3.332,0-6.033,2.701-6.033,6.032s2.701,6.032,6.033,6.032c2.798,0,4.733-1.657,5.445-3.972h-5.445V10.239l9.426,0.013c0.823,3.898-1.03,11.748-9.426,11.748C7.021,22,2.543,17.523,2.543,12z"
                        fill="#34A853"
                      />
                    </svg>
                    Sign up with Google
                  </>
                )}
              </button>
            </div>
          </div>

          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              Already have an account?{' '}
              <Link to="/login" className="font-medium text-primary-600 hover:text-primary-500">
                Sign in instead
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default RegisterForm;