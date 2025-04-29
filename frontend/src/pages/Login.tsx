import { useState, useEffect } from 'react';
import { useNavigate, Link, useLocation } from 'react-router-dom';
import { useForm } from 'react-hook-form';
import { LogIn, Mail } from 'lucide-react';
import { TextInput, PasswordInput, FormErrorMessage, FormSuccessMessage } from '../components/common/FormComponents';
import useAuthStore from '../store/authStore';
import { handleAuth0Login } from '../utils/auth0-config';

type LoginFormValues = {
  email: string;
  password: string;
};

const Login = () => {
  const navigate = useNavigate();
  const location = useLocation();
  const from = location.state?.from || '/';
  const { login, isLoading, error, clearError, isAuthenticated } = useAuthStore();
  const [formError, setFormError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [showPassword, setShowPassword] = useState(false);
  const [socialLoading, setSocialLoading] = useState(false);
  const [processingAuth, setProcessingAuth] = useState(false);

  const {
    register,
    handleSubmit,
    formState: { errors }
  } = useForm<LoginFormValues>({
    defaultValues: {
      email: '',
      password: ''
    }
  });

  // Redirect if already authenticated
  useEffect(() => {
    if (isAuthenticated && !isLoading && !processingAuth) {
      navigate(from, { replace: true });
    }
  }, [isAuthenticated, isLoading, navigate, from, processingAuth]);

  // Check for errors passed from auth store
  useEffect(() => {
    if (error) {
      setFormError(error);
    }
  }, [error]);

  const onSubmit = async (data: LoginFormValues) => {
    try {
      clearError();
      setFormError(null);
      setProcessingAuth(true);

      await login(data.email, data.password);

      setSuccess('Login successful! Redirecting...');

      // Navigate will be handled by the useEffect watching isAuthenticated
    } catch (err) {
      console.error('Login error:', err);
      // Error handling is done via the auth store error state
    } finally {
      setProcessingAuth(false);
    }
  };

  const handleGoogleLogin = () => {
    try {
      setSocialLoading(true);
      clearError();
      setFormError(null);
      setProcessingAuth(true);

      // This will redirect to Auth0
      handleAuth0Login('google-oauth2');
    } catch (err) {
      console.error('Google login failed:', err);
      setFormError('An error occurred during Google authentication.');
      setSocialLoading(false);
      setProcessingAuth(false);
    }
  };

  // Show loading overlay for authenticating
  if ((isLoading || processingAuth) && !formError) {
    return (
      <div className="min-h-screen bg-gradient-to-b from-primary-100 to-white flex flex-col justify-center px-4 sm:px-6 lg:px-8">
        <div className="sm:mx-auto sm:w-full sm:max-w-md">
          <div className="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
            <div className="flex flex-col items-center justify-center py-6">
              <div className="animate-spin rounded-full h-10 w-10 border-t-2 border-b-2 border-primary-600 mb-4"></div>
              <h2 className="text-lg font-medium text-gray-700 mb-2">Signing you in</h2>
              <p className="text-sm text-gray-500">Please wait while we authenticate your credentials</p>
            </div>
          </div>
        </div>
      </div>
    );
  }

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
          <h2 className="mb-6 text-center text-2xl font-bold text-gray-900">Sign In</h2>

          {/* Success message */}
          <FormSuccessMessage message={success} />

          {/* Error message */}
          <FormErrorMessage error={formError} />

          <form className="space-y-6" onSubmit={handleSubmit(onSubmit)}>
            {/* Email Field */}
            <TextInput
              id="email"
              label="Email address"
              register={register}
              registerOptions={{
                required: 'Email is required',
                pattern: {
                  value: /^[A-Z0-9._%+-]+@[A-Z0-9.-]+\.[A-Z]{2,}$/i,
                  message: 'Invalid email address'
                }
              }}
              errors={errors}
              icon={<Mail className="h-5 w-5 text-gray-400" />}
              type="email"
              placeholder="you@example.com"
            />

            {/* Password Field */}
            <PasswordInput
              id="password"
              label="Password"
              register={register}
              registerOptions={{ required: 'Password is required' }}
              errors={errors}
              showPassword={showPassword}
              onTogglePassword={() => setShowPassword(!showPassword)}
              placeholder="••••••••"
            />

            {/* Password recovery link */}
            <div className="text-right">
              <Link
                to="/forgot-password"
                className="text-sm font-medium text-primary-600 hover:text-primary-500"
              >
                Forgot your password?
              </Link>
            </div>

            {/* Submit Button */}
            <div>
              <button
                type="submit"
                disabled={isLoading}
                className="group relative w-full flex justify-center py-2 px-4 border border-transparent text-sm font-medium rounded-md text-white bg-primary-600 hover:bg-primary-700 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:bg-primary-400 disabled:cursor-not-allowed"
              >
                <span className="absolute left-0 inset-y-0 flex items-center pl-3">
                  <LogIn className="h-5 w-5 text-primary-500 group-hover:text-primary-400" />
                </span>
                {isLoading ? 'Signing in...' : 'Sign In'}
              </button>
            </div>
          </form>

          <div className="mt-6">
            <div className="relative">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-gray-300" />
              </div>
              <div className="relative flex justify-center text-sm">
                <span className="px-2 bg-white text-gray-500">Or continue with</span>
              </div>
            </div>

            <div className="mt-6">
              <button
                type="button"
                onClick={handleGoogleLogin}
                disabled={socialLoading}
                className="w-full flex justify-center py-2 px-4 border border-gray-300 rounded-md shadow-sm bg-white text-sm font-medium text-gray-700 hover:bg-gray-50 focus:outline-none focus:ring-2 focus:ring-offset-2 focus:ring-primary-500 disabled:opacity-50"
              >
                {socialLoading ? (
                  <span className="flex items-center">
                    <span className="mr-2 h-4 w-4 rounded-full border-2 border-b-transparent border-t-primary-600 animate-spin"></span>
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
                    Sign in with Google
                  </>
                )}
              </button>
            </div>
          </div>

          <div className="mt-6 text-center">
            <p className="text-sm text-gray-600">
              Don't have an account?{' '}
              <Link
                to="/register"
                className="font-medium text-primary-600 hover:text-primary-500"
              >
                Sign up
              </Link>
            </p>
          </div>
        </div>
      </div>
    </div>
  );
};

export default Login;