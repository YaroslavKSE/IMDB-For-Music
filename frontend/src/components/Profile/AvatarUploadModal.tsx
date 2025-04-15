import { useState, useRef, ChangeEvent } from 'react';
import { X, Upload, Trash2, Camera, Loader, Check } from 'lucide-react';
import AuthService from '../../api/auth';
import useAuthStore from '../../store/authStore';

interface AvatarUploadModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSuccess: () => void;
  currentAvatarUrl?: string;
}

const AvatarUploadModal = ({ isOpen, onClose, onSuccess, currentAvatarUrl }: AvatarUploadModalProps) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const { setUser } = useAuthStore();

  // Function to handle direct-to-S3 upload
  const uploadToS3 = async (file: File, presignedUrl: string): Promise<boolean> => {
    try {
      const xhr = new XMLHttpRequest();

      // Set up progress monitoring
      xhr.upload.addEventListener('progress', (event) => {
        if (event.lengthComputable) {
          const percentComplete = Math.round((event.loaded / event.total) * 100);
          setUploadProgress(percentComplete);
        }
      });

      // Promise-based XHR request
      return new Promise((resolve, reject) => {
        xhr.open('PUT', presignedUrl, true);
        xhr.setRequestHeader('Content-Type', file.type);

        xhr.onload = () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            resolve(true);
          } else {
            reject(new Error(`Upload failed with status ${xhr.status}`));
          }
        };

        xhr.onerror = () => reject(new Error('Network error during upload'));
        xhr.onabort = () => reject(new Error('Upload aborted'));

        // Send the file
        xhr.send(file);
      });
    } catch (err) {
      console.error('Error uploading to S3:', err);
      throw err;
    }
  };

  const handleFileSelect = (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Reset states
    setError(null);

    // Validate file size (max 5MB)
    if (file.size > 5 * 1024 * 1024) {
      setError('File size exceeds 5MB limit');
      return;
    }

    // Validate file type
    if (!['image/jpeg', 'image/png', 'image/gif'].includes(file.type)) {
      setError('Only JPEG, PNG, and GIF formats are supported');
      return;
    }

    // Create a preview URL
    const objectUrl = URL.createObjectURL(file);
    setPreviewUrl(objectUrl);

    // Clean up the object URL when no longer needed
    return () => URL.revokeObjectURL(objectUrl);
  };

  const handleUpload = async () => {
    if (!previewUrl || !fileInputRef.current?.files?.[0]) {
      return;
    }

    const file = fileInputRef.current.files[0];

    setIsUploading(true);
    setUploadProgress(0);
    setError(null);

    try {
      // Method 1: Direct upload to S3 with presigned URL
      try {
        // Get presigned URL from the server
        const presignedData = await AuthService.getAvatarUploadUrl(file.type);

        // Upload directly to S3
        const uploadSuccess = await uploadToS3(file, presignedData.url);

        if (uploadSuccess) {
          // Notify the server that upload is complete
          const updatedUser = await AuthService.completeAvatarUpload(
            presignedData.objectKey,
            presignedData.avatarUrl
          );

          // Update user in store with new avatar URL
          setUser(updatedUser);

          // Notify parent component
          onSuccess();
        }
      } catch (presignedError) {
        console.error('Error with presigned URL upload:', presignedError);

        // Fallback to traditional upload if presigned URL method fails
        console.log('Falling back to traditional upload method');
        const updatedUser = await AuthService.uploadAvatar(file);

        // Update user in store with new avatar URL
        setUser(updatedUser);

        // Notify parent component
        onSuccess();
      }
    } catch (err: unknown) {
      console.error('Avatar upload error:', err);
      if (err instanceof Error) {
        setError(err.message || 'Failed to upload avatar. Please try again.');
      } else {
        setError('Failed to upload avatar. Please try again.');
      }
      setIsUploading(false);
    }
  };

  const handleDeleteAvatar = async () => {
    if (!currentAvatarUrl) return;

    if (confirm('Are you sure you want to remove your avatar?')) {
      setIsUploading(true);
      setError(null);

      try {
        const updatedUser = await AuthService.deleteAvatar();
        setUser(updatedUser);
        onSuccess();
      } catch (err: unknown) {
        console.error('Error deleting avatar:', err);
        if (err instanceof Error) {
          setError(err.message || 'Failed to delete avatar. Please try again.');
        } else {
          setError('Failed to delete avatar. Please try again.');
        }
        setIsUploading(false);
      }
    }
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto">
      {/* Backdrop */}
      <div className="fixed inset-0 bg-black bg-opacity-50 transition-opacity" onClick={onClose}></div>

      {/* Modal */}
      <div className="flex min-h-full items-center justify-center p-4">
        <div className="relative w-full max-w-md transform overflow-hidden rounded-lg bg-white p-6 shadow-xl transition-all">
          {/* Close button */}
          <button
            className="absolute top-3 right-3 text-gray-400 hover:text-gray-500"
            onClick={onClose}
          >
            <X className="h-5 w-5" />
          </button>

          {/* Modal header */}
          <div className="mb-5 text-center">
            <h3 className="text-lg font-medium text-gray-900">Update Profile Picture</h3>
            <p className="mt-1 text-sm text-gray-500">Upload a new profile picture or remove your current one.</p>
          </div>

          {/* Current/Preview Image */}
          <div className="flex justify-center my-5">
            <div className="h-32 w-32 rounded-full overflow-hidden bg-primary-100 flex items-center justify-center relative">
              {isUploading ? (
                <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-60">
                  <div className="text-center">
                    <Loader className="h-10 w-10 text-white animate-spin mx-auto mb-2" />
                    <span className="text-white text-sm">{uploadProgress}%</span>
                  </div>
                </div>
              ) : previewUrl ? (
                <img src={previewUrl} alt="Preview" className="h-full w-full object-cover" />
              ) : currentAvatarUrl ? (
                <img src={currentAvatarUrl} alt="Current avatar" className="h-full w-full object-cover" />
              ) : (
                <Camera className="h-12 w-12 text-primary-500" />
              )}
            </div>
          </div>

          {/* Error message */}
          {error && (
            <div className="mb-4 text-sm text-red-600 p-2 bg-red-50 rounded">
              {error}
            </div>
          )}

          {/* Actions */}
          <div className="flex flex-col gap-3">
            {/* Choose File */}
            <input
              ref={fileInputRef}
              type="file"
              accept="image/jpeg,image/png,image/gif"
              className="hidden"
              onChange={handleFileSelect}
            />

            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              className="flex items-center justify-center px-4 py-2 border border-gray-300 rounded-md shadow-sm text-sm font-medium text-gray-700 bg-white hover:bg-gray-50 focus:outline-none"
              disabled={isUploading}
            >
              <Upload className="h-4 w-4 mr-2" />
              Choose a file
            </button>

            {/* Upload */}
            {previewUrl && (
              <button
                type="button"
                onClick={handleUpload}
                className="flex items-center justify-center px-4 py-2 border border-transparent rounded-md shadow-sm text-sm font-medium text-white bg-primary-600 hover:bg-primary-700 focus:outline-none"
                disabled={isUploading}
              >
                <Check className="h-4 w-4 mr-2" />
                Upload
              </button>
            )}

            {/* Delete */}
            {currentAvatarUrl && !previewUrl && (
              <button
                type="button"
                onClick={handleDeleteAvatar}
                className="flex items-center justify-center px-4 py-2 border border-red-300 rounded-md shadow-sm text-sm font-medium text-red-700 bg-white hover:bg-red-50 focus:outline-none"
                disabled={isUploading}
              >
                <Trash2 className="h-4 w-4 mr-2" />
                Remove Current Picture
              </button>
            )}
          </div>

          {/* Help text */}
          <p className="mt-4 text-xs text-gray-500 text-center">
            Recommended: Square image, at least 200x200 pixels.<br />
            Max file size: 5MB. Formats: JPEG, PNG, GIF.
          </p>
        </div>
      </div>
    </div>
  );
};

export default AvatarUploadModal;