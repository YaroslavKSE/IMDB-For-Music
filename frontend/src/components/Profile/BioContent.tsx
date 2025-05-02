import { useState } from 'react';
import { Edit2, Save, X, AlertCircle, Book } from 'lucide-react';
import useAuthStore from '../../store/authStore';
import { UserProfile } from '../../api/auth';
import { PublicUserProfile } from '../../api/users';
import TabContentWrapper from './TabContentWrapper';

interface BioContentProps {
  user: UserProfile | PublicUserProfile;
  isEditable?: boolean;
}

const BioContent: React.FC<BioContentProps> = ({ user, isEditable = false }) => {
  const { updateBio, deleteBio, error } = useAuthStore();
  const [isEditing, setIsEditing] = useState(false);
  const [bioText, setBioText] = useState(user.bio || '');
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [bioLength, setBioLength] = useState(user.bio?.length || 0);
  const [formError, setFormError] = useState<string | null>(null);
  const [formSuccess, setFormSuccess] = useState<string | null>(null);
  const MAX_BIO_LENGTH = 500;

  const handleEditBio = () => {
    setBioText(user.bio || '');
    setIsEditing(true);
    setFormError(null);
    setFormSuccess(null);
  };

  const handleCancelEdit = () => {
    setIsEditing(false);
    setBioText(user.bio || '');
    setFormError(null);
  };

  const handleBioChange = (e: React.ChangeEvent<HTMLTextAreaElement>) => {
    setBioText(e.target.value);
    setBioLength(e.target.value.length);
  };

  const handleSaveBio = async () => {
    if (bioLength > MAX_BIO_LENGTH) {
      setFormError(`Bio cannot exceed ${MAX_BIO_LENGTH} characters.`);
      return;
    }

    setIsSubmitting(true);
    setFormError(null);

    try {
      // If bio is empty, delete it, otherwise update it
      if (!bioText.trim()) {
        await deleteBio();
      } else {
        await updateBio(bioText);
      }

      setFormSuccess('Bio updated successfully!');

      // Close edit mode after a short delay
      setTimeout(() => {
        setIsEditing(false);
        setFormSuccess(null);
      }, 1500);
    } catch (err) {
      setFormError(error || 'Failed to update bio. Please try again.');
    } finally {
      setIsSubmitting(false);
    }
  };

  return (
    <TabContentWrapper
      title={isEditable ? "Profile Bio" : `About ${user.name}`}
      icon={<Book className="h-5 w-5" />}
    >
      {/* Error message display */}
      {formError && (
        <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-md flex items-start mb-4">
          <AlertCircle className="h-5 w-5 mr-2 flex-shrink-0 mt-0.5" />
          <span>{formError}</span>
        </div>
      )}

      {/* Success message display */}
      {formSuccess && (
        <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-md mb-4">
          {formSuccess}
        </div>
      )}

      {isEditable && !isEditing && (
        <div className="flex justify-end mb-4">
          <button
            onClick={handleEditBio}
            className="px-3 py-1.5 border border-gray-300 rounded-md text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none flex items-center"
          >
            <Edit2 className="h-4 w-4 mr-1.5" />
            {user.bio ? 'Edit Bio' : 'Add Bio'}
          </button>
        </div>
      )}

      {isEditing ? (
        <div className="space-y-4">
          <div>
            <label htmlFor="bio" className="block text-sm font-medium text-gray-700 mb-1 flex items-center">
              <span>About You</span>
              <span className={`ml-2 text-xs ${bioLength > MAX_BIO_LENGTH ? 'text-red-600' : 'text-gray-500'}`}>
                ({bioLength}/{MAX_BIO_LENGTH})
              </span>
            </label>
            <textarea
              id="bio"
              rows={6}
              value={bioText}
              onChange={handleBioChange}
              className={`w-full px-3 py-2 border ${
                bioLength > MAX_BIO_LENGTH ? 'border-red-300' : 'border-gray-300'
              } rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500`}
              placeholder="Tell others about yourself..."
            />
            <p className="mt-1 text-xs text-gray-500">
              Write a short bio about yourself. This will be visible on your public profile.
            </p>
          </div>

          <div className="flex justify-end space-x-3">
            <button
              type="button"
              onClick={handleCancelEdit}
              className="px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
              disabled={isSubmitting}
            >
              <X className="h-4 w-4 inline mr-1" />
              Cancel
            </button>
            <button
              type="button"
              onClick={handleSaveBio}
              className="px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none disabled:bg-primary-400 disabled:cursor-not-allowed flex items-center"
              disabled={isSubmitting || bioLength > MAX_BIO_LENGTH}
            >
              {isSubmitting ? (
                <>
                  <div className="animate-spin rounded-full h-4 w-4 border-t-2 border-b-2 border-white mr-2"></div>
                  Saving...
                </>
              ) : (
                <>
                  <Save className="h-4 w-4 mr-2" />
                  Save Bio
                </>
              )}
            </button>
          </div>
        </div>
      ) : (
        <div className="prose max-w-none">
          {user.bio ? (
            <div className="whitespace-pre-line">{user.bio}</div>
          ) : (
            <p className="text-gray-500 italic">
              {isEditable
                ? "You haven't added a bio yet. Click 'Add Bio' to tell others about yourself."
                : `${user.name} hasn't added a bio yet.`
              }
            </p>
          )}
        </div>
      )}
    </TabContentWrapper>
  );
};

export default BioContent;