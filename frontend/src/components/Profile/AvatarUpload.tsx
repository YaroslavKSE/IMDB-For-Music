import { useState, useRef, ChangeEvent } from 'react';
import { Camera, Trash2, Upload, Loader, Check, X } from 'lucide-react';
import AuthService from '../../api/auth';
import useAuthStore from '../../store/authStore';

interface AvatarUploadProps {
  currentAvatarUrl?: string;
  onUploadSuccess?: () => void;
}

const AvatarUpload = ({ currentAvatarUrl, onUploadSuccess }: AvatarUploadProps) => {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [isUploading, setIsUploading] = useState(false);
  const [uploadProgress, setUploadProgress] = useState(0);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState(false);
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

  const handleFileChange = async (e: ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (!file) return;

    // Reset states
    setIsUploading(true);
    setUploadProgress(0);
    setError(null);
    setSuccess(false);

    try {
      // Validate file size (max 5MB)
      if (file.size > 5 * 1024 * 1024) {
        throw new Error('File size exceeds 5MB limit');
      }

      // Validate file type
      if (!['image/jpeg', 'image/png', 'image/gif'].includes(file.type)) {
        throw new Error('Only JPEG, PNG, and GIF formats are supported');
      }

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
          setSuccess(true);

          // Notify parent component if callback provided
          if (onUploadSuccess) onUploadSuccess();
        }
      } catch (presignedError) {
        console.error('Error with presigned URL upload:', presignedError);

        // Fallback to traditional upload if presigned URL method fails
        console.log('Falling back to traditional upload method');
        const updatedUser = await AuthService.uploadAvatar(file);

        // Update user in store with new avatar URL
        setUser(updatedUser);
        setSuccess(true);

        // Notify parent component if callback provided
        if (onUploadSuccess) onUploadSuccess();
      }
    } catch (err: unknown) {
      console.error('Avatar upload error:', err);
      if (err instanceof Error) {
        setError(err.message || 'Failed to upload avatar. Please try again.');
      } else {
        setError('Failed to upload avatar. Please try again.');
      }
    } finally {
      setIsUploading(false);
      // Reset file input so same file can be selected again
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
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
        if (onUploadSuccess) onUploadSuccess();
      } catch (err: unknown) {
        console.error('Error deleting avatar:', err);
        if (err instanceof Error) {
          setError(err.message || 'Failed to delete avatar. Please try again.');
        } else {
          setError('Failed to delete avatar. Please try again.');
        }
      } finally {
        setIsUploading(false);
      }
    }
  };

  return (
    <div className="relative">
      {/* Avatar Display */}
      <div 
        className="h-24 w-24 rounded-full overflow-hidden bg-primary-100 flex items-center justify-center relative group"
        style={{ border: '4px solid white' }}
      >
        {currentAvatarUrl ? (
          <img 
            src={currentAvatarUrl} 
            alt="User avatar" 
            className="h-full w-full object-cover"
          />
        ) : (
          <Camera className="h-10 w-10 text-primary-700" />
        )}
        
        {/* Overlay with controls */}
        <div className="absolute inset-0 bg-black bg-opacity-50 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity">
          <div className="flex flex-col items-center">
            <button
              onClick={() => fileInputRef.current?.click()}
              className="text-white mb-1 p-1 rounded-full hover:bg-white hover:bg-opacity-20"
              disabled={isUploading}
            >
              <Upload className="h-6 w-6" />
            </button>
            
            {currentAvatarUrl && (
              <button
                onClick={handleDeleteAvatar}
                className="text-white p-1 rounded-full hover:bg-white hover:bg-opacity-20"
                disabled={isUploading}
              >
                <Trash2 className="h-6 w-6" />
              </button>
            )}
          </div>
        </div>
        
        {/* Upload progress indicator */}
        {isUploading && (
          <div className="absolute inset-0 flex items-center justify-center bg-black bg-opacity-70">
            <div className="text-center">
              <Loader className="h-8 w-8 text-white animate-spin mx-auto mb-1" />
              <span className="text-white text-xs">{uploadProgress}%</span>
            </div>
          </div>
        )}
        
        {/* Success indicator */}
        {success && !isUploading && (
          <div className="absolute bottom-0 right-0 bg-green-500 rounded-full p-1">
            <Check className="h-4 w-4 text-white" />
          </div>
        )}
      </div>
      
      {/* Hidden file input */}
      <input
        type="file"
        ref={fileInputRef}
        onChange={handleFileChange}
        className="hidden"
        accept="image/jpeg,image/png,image/gif"
      />
      
      {/* Error message */}
      {error && (
        <div className="absolute top-full mt-2 left-0 right-0 bg-red-100 text-red-700 p-2 rounded-md text-sm flex items-start">
          <X className="h-4 w-4 mr-1 flex-shrink-0 mt-0.5" />
          <span>{error}</span>
        </div>
      )}
    </div>
  );
};

export default AvatarUpload;