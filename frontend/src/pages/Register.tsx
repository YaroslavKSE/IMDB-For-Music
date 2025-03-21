import { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { Eye, EyeOff, Mail, Lock, User, UserPlus } from 'lucide-react';
import useAuthStore from '../store/authStore';
import { handleAuth0Login } from '../utils/auth0-config';

interface RegisterFormData {
  name: string;
  surname: string;
  email: string;
  password: string;
  confirmPassword: string;
  acceptTerms: boolean;
}

const Register = () => {
  const navigate = useNavigate();
  const {register: registerUser, isLoading, error: authError, clearError} = useAuthStore();
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [socialLoading, setSocialLoading] = useState(false);

  const {
    register,
    handleSubmit,
    watch,
    formState: {errors},
  } = useForm<RegisterFormData>({
    defaultValues: {
      acceptTerms: false
    }
  });

  const password = watch('password');

  const onSubmit = async (data: RegisterFormData) => {
    try {
      clearError(); // Clear any previous auth errors
      setError(null); // Clear any previous form errors

      await registerUser(data.email, data.password, data.name, data.surname);

      setSuccess('Registration successful! You can now log in.');
      setTimeout(() => navigate('/login'), 2000); // Redirect after 2 seconds
    } catch (err) {
      // Error handling is done by the auth store
      console.error('Registration failed:', err);
    }
  };

  const handleGoogleSignup = async () => {
    try {
      setSocialLoading(true);
      clearError();
      setError(null);

      const success = await handleAuth0Login('google-oauth2');

      if (success) {
        navigate('/');
      } else {
        setError('Google signup failed. Please try again.');
      }
    } catch (err) {
      console.error('Google signup failed:', err);
      setError('An error occurred during Google authentication.');
    } finally {
      setSocialLoading(false);
    }
  };

  return (
      <div
          className="min-h-screen bg-gradient-to-b from-primary-100 to-white flex flex-col justify-center px-4 sm:px-6 lg:px-8">
        <div className="sm:mx-auto sm:w-full sm:max-w-md">
          <div className="text-center">
            <h2 className="text-3xl font-extrabold text-gray-900">MusicEval</h2>
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
            {success && (
                <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded mb-4">
                  {success}
                </div>
            )}

            {/* Error message display */}
            {(error || authError) && (
                <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded mb-4">
                  {error || authError}
                </div>
            )}

            <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
              {/* First Name Field */}
              <div>
                <label htmlFor="name" className="block text-sm font-medium text-gray-700">
                  First Name
                </label>
                <div className="mt-1 relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-gray-400"/>
                  </div>
                  <input
                      id="name"
                      {...register('name', {
                        required: 'First name is required',
                        maxLength: {
                          value: 100,
                          message: 'First name is too long',
                        },
                      })}
                      type="text"
                      className={`block w-full pl-10 pr-3 py-2 border ${
                          errors.name ? 'border-red-300' : 'border-gray-300'
                      } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                      placeholder="John"
                  />
                </div>
                {errors.name && (
                    <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
                )}
              </div>

              {/* Last Name Field */}
              <div>
                <label htmlFor="surname" className="block text-sm font-medium text-gray-700">
                  Last Name
                </label>
                <div className="mt-1 relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <User className="h-5 w-5 text-gray-400"/>
                  </div>
                  <input
                      id="surname"
                      {...register('surname', {
                        required: 'Last name is required',
                        maxLength: {
                          value: 100,
                          message: 'Last name is too long',
                        },
                      })}
                      type="text"
                      className={`block w-full pl-10 pr-3 py-2 border ${
                          errors.surname ? 'border-red-300' : 'border-gray-300'
                      } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                      placeholder="Doe"
                  />
                </div>
                {errors.surname && (
                    <p className="mt-1 text-sm text-red-600">{errors.surname.message}</p>
                )}
              </div>

              {/* Other form fields... */}
              {/* Email Field */}
              <div>
                <label htmlFor="email" className="block text-sm font-medium text-gray-700">
                  Email address
                </label>
                <div className="mt-1 relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Mail className="h-5 w-5 text-gray-400"/>
                  </div>
                  <input
                      id="email"
                      {...register('email', {
                        required: 'Email is required',
                        pattern: {
                          value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                          message: 'Invalid email address',
                        },
                      })}
                      type="email"
                      autoComplete="email"
                      className={`block w-full pl-10 pr-3 py-2 border ${
                          errors.email ? 'border-red-300' : 'border-gray-300'
                      } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                      placeholder="you@example.com"
                  />
                </div>
                {errors.email && (
                    <p className="mt-1 text-sm text-red-600">{errors.email.message}</p>
                )}
              </div>

              {/* Password Field */}
              <div>
                <label htmlFor="password" className="block text-sm font-medium text-gray-700">
                  Password
                </label>
                <div className="mt-1 relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Lock className="h-5 w-5 text-gray-400"/>
                  </div>
                  <input
                      id="password"
                      {...register('password', {
                        required: 'Password is required',
                        minLength: {
                          value: 8,
                          message: 'Password must be at least 8 characters',
                        },
                        pattern: {
                          value: /^(?=.*[A-Z])(?=.*[0-9])(?=.*[^a-zA-Z0-9])/,
                          message: 'Password must include an uppercase letter, a number, and a special character',
                        },
                      })}
                      type={showPassword ? "text" : "password"}
                      autoComplete="new-password"
                      className={`block w-full pl-10 pr-10 py-2 border ${
                          errors.password ? 'border-red-300' : 'border-gray-300'
                      } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                      placeholder="••••••••"
                  />
                  <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
                    <button
                        type="button"
                        onClick={() => setShowPassword(!showPassword)}
                        className="text-gray-400 hover:text-gray-500 focus:outline-none"
                    >
                      {showPassword ? (
                          <EyeOff className="h-5 w-5"/>
                      ) : (
                          <Eye className="h-5 w-5"/>
                      )}
                    </button>
                  </div>
                </div>
                {errors.password ? (
                    <p className="mt-1 text-sm text-red-600">{errors.password.message}</p>
                ) : (
                    <p className="mt-1 text-xs text-gray-500">
                      Must be at least 8 characters with 1 uppercase, 1 number, and 1 special character
                    </p>
                )}
              </div>

              {/* Confirm Password Field */}
              <div>
                <label htmlFor="confirmPassword" className="block text-sm font-medium text-gray-700">
                  Confirm Password
                </label>
                <div className="mt-1 relative rounded-md shadow-sm">
                  <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                    <Lock className="h-5 w-5 text-gray-400"/>
                  </div>
                  <input
                      id="confirmPassword"
                      {...register('confirmPassword', {
                        required: 'Please confirm your password',
                        validate: (value) => value === password || 'Passwords do not match',
                      })}
                      type={showConfirmPassword ? "text" : "password"}
                      autoComplete="new-password"
                      className={`block w-full pl-10 pr-10 py-2 border ${
                          errors.confirmPassword ? 'border-red-300' : 'border-gray-300'
                      } rounded-md leading-5 bg-white placeholder-gray-500 focus:outline-none focus:ring-primary-500 focus:border-primary-500 sm:text-sm`}
                      placeholder="••••••••"
                  />
                  <div className="absolute inset-y-0 right-0 pr-3 flex items-center">
                    <button
                        type="button"
                        onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                        className="text-gray-400 hover:text-gray-500 focus:outline-none"
                    >
                      {showConfirmPassword ? (
                          <EyeOff className="h-5 w-5"/>
                      ) : (
                          <Eye className="h-5 w-5"/>
                      )}
                    </button>
                  </div>
                </div>
                {errors.confirmPassword && (
                    <p className="mt-1 text-sm text-red-600">{errors.confirmPassword.message}</p>
                )}
              </div>

              {/* Terms and Conditions */}
              <div className="flex items-center">
                <input
                    id="acceptTerms"
                    {...register('acceptTerms', {
                      required: 'You must accept the terms and conditions',
                    })}
                    type="checkbox"
                    className="h-4 w-4 text-primary-600 focus:ring-primary-500 border-gray-300 rounded"
                />
                <label htmlFor="acceptTerms" className="ml-2 block text-sm text-gray-900">
                  I agree to the <a href="#" className="text-primary-600 hover:text-primary-500">Terms of
                  Service</a> and <a href="#" className="text-primary-600 hover:text-primary-500">Privacy Policy</a>
                </label>
              </div>
              {errors.acceptTerms && (
                  <p className="text-sm text-red-600">{errors.acceptTerms.message}</p>
              )}

              {/* Submit Button */}
              <div>
                <button
                    type="submit"
                    disabled={isLoading}
                    className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:bg-primary-400"
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

export default Register;