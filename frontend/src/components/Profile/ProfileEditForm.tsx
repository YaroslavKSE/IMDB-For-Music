import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { UserCheck, X, Save, AlertCircle } from 'lucide-react';
import useAuthStore from '../../store/authStore';
import { UpdateProfileParams } from '../../api/auth';

interface ProfileEditFormProps {
  onCancel: () => void;
  onSuccess: () => void;
}

interface ProfileFormData {
  name: string;
  surname: string;
  username: string;
}

const ProfileEditForm = ({ onCancel, onSuccess }: ProfileEditFormProps) => {
  const { user, updateProfile, error, clearError } = useAuthStore();
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [formError, setFormError] = useState<string | null>(null);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ProfileFormData>({
    defaultValues: {
      name: user?.name || '',
      surname: user?.surname || '',
      username: user?.username || '',
    },
  });

  const onSubmit = async (data: ProfileFormData) => {
    try {
      setIsSubmitting(true);
      setFormError(null);
      clearError();

      // Only include fields that have changed
      const updateData: UpdateProfileParams = {};
      if (data.name !== user?.name) updateData.name = data.name;
      if (data.surname !== user?.surname) updateData.surname = data.surname;
      if (data.username !== user?.username) updateData.username = data.username;

      // No changes, just return
      if (Object.keys(updateData).length === 0) {
        setFormSuccess('No changes to save');
        setTimeout(() => {
          setFormSuccess(null);
          onSuccess();
        }, 1500);
        return;
      }

      await updateProfile(updateData);

      setFormSuccess('Profile updated successfully!');

      // Slight delay before closing the form to show success message
      setTimeout(() => {
        setFormSuccess(null);
        onSuccess();
      }, 1500);
    } catch (err) {
      console.error('Error updating profile:', err);
      // The error is already set in the auth store, so we can just read it from there
      setFormError(error || 'An unexpected error occurred. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <div className="bg-white shadow rounded-lg overflow-hidden">
      <div className="px-6 py-4 bg-primary-50 border-b border-primary-100 flex justify-between items-center">
        <h3 className="text-lg font-medium text-primary-800 flex items-center">
          <UserCheck className="h-5 w-5 mr-2" />
          Edit Profile
        </h3>
        <button
          onClick={onCancel}
          className="text-gray-500 hover:text-gray-700 transition-colors"
        >
          <X className="h-5 w-5" />
        </button>
      </div>

      <form onSubmit={handleSubmit(onSubmit)} className="p-6 space-y-4">
        {/* Error message display */}
        {formError && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md flex items-start">
            <AlertCircle className="h-5 w-5 mr-2 flex-shrink-0 mt-0.5" />
            <span>{formError}</span>
          </div>
        )}

        {/* Success message display */}
        {formSuccess && (
          <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-md">
            {formSuccess}
          </div>
        )}

        {/* First Name Field */}
        <div>
          <label htmlFor="name" className="block text-sm font-medium text-gray-700 mb-1">
            First Name
          </label>
          <input
            id="name"
            type="text"
            {...register('name', {
              required: 'First name is required',
              maxLength: {
                value: 50,
                message: 'First name cannot exceed 50 characters',
              },
            })}
            className={`w-full px-3 py-2 border ${
              errors.name ? 'border-red-300' : 'border-gray-300'
            } rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
          />
          {errors.name && (
            <p className="mt-1 text-sm text-red-600">{errors.name.message}</p>
          )}
        </div>

        {/* Last Name Field */}
        <div>
          <label htmlFor="surname" className="block text-sm font-medium text-gray-700 mb-1">
            Last Name
          </label>
          <input
            id="surname"
            type="text"
            {...register('surname', {
              required: 'Last name is required',
              maxLength: {
                value: 50,
                message: 'Last name cannot exceed 50 characters',
              },
            })}
            className={`w-full px-3 py-2 border ${
              errors.surname ? 'border-red-300' : 'border-gray-300'
            } rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
          />
          {errors.surname && (
            <p className="mt-1 text-sm text-red-600">{errors.surname.message}</p>
          )}
        </div>

        {/* Username Field */}
        <div>
          <label htmlFor="username" className="block text-sm font-medium text-gray-700 mb-1">
            Username
          </label>
          <input
            id="username"
            type="text"
            {...register('username', {
              required: 'Username is required',
              pattern: {
                value: /^[a-zA-Z0-9_-]+$/,
                message: 'Username can only contain letters, numbers, underscores, and hyphens',
              },
              minLength: {
                value: 3,
                message: 'Username must be at least 3 characters',
              },
              maxLength: {
                value: 20,
                message: 'Username cannot exceed 20 characters',
              },
            })}
            className={`w-full px-3 py-2 border ${
              errors.username ? 'border-red-300' : 'border-gray-300'
            } rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
          />
          {errors.username && (
            <p className="mt-1 text-sm text-red-600">{errors.username.message}</p>
          )}
          <p className="mt-1 text-xs text-gray-500">
            This will be your public username visible to other users.
          </p>
        </div>

        {/* Form Actions */}
        <div className="flex justify-end space-x-3 pt-4 border-t border-gray-200">
          <button
            type="button"
            onClick={onCancel}
            className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
            disabled={isSubmitting}
          >
            Cancel
          </button>
          <button
            type="submit"
            className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed flex items-center"
            disabled={isSubmitting}
          >
            {isSubmitting ? (
              <>
                <div className="animate-spin rounded-full h-4 w-4 border-t-2 border-b-2 border-white mr-2"></div>
                Saving...
              </>
            ) : (
              <>
                <Save className="h-4 w-4 mr-2" />
                Save Changes
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};

export default ProfileEditForm;