import auth0 from 'auth0-js';

// Auth0 configuration
const auth0Config = {
  domain: import.meta.env.VITE_AUTH0_DOMAIN || '',
  clientId: import.meta.env.VITE_AUTH0_CLIENT_ID || '',
  redirectUri: `${window.location.origin}/callback`,
  audience: import.meta.env.VITE_AUTH0_AUDIENCE || '',
};

// Initialize Auth0 WebAuth
export const auth0Client = new auth0.WebAuth({
  domain: auth0Config.domain,
  clientID: auth0Config.clientId,
  redirectUri: auth0Config.redirectUri,
  responseType: 'token id_token',
  audience: auth0Config.audience,
  scope: 'openid profile email'
});

// Helper function to handle the Auth0 login
export const handleAuth0Login = (connection: string = 'google-oauth2') => {
  console.log("Starting Auth0 login with connection:", connection);

  auth0Client.authorize({
    connection,
    prompt: 'login'
  });

  // This will redirect the browser, so no return value is needed
};

// Function to handle the authentication callback
export const handleAuthCallback = () => {
  return new Promise<{ accessToken: string, provider: string }>((resolve, reject) => {
    auth0Client.parseHash((err, authResult) => {
      if (err) {
        console.error('Error parsing hash:', err);
        reject(err);
        return;
      }

      if (authResult && authResult.accessToken) {
        console.log('Auth result:', authResult);

        // TypeScript safe version - explicitly check that accessToken is defined
        const accessToken = authResult.accessToken;
        if (!accessToken) {
          reject(new Error('Access token is undefined'));
          return;
        }

        // Get user info to determine provider
        auth0Client.client.userInfo(accessToken, (err, user) => {
          if (err) {
            console.error('Error getting user info:', err);
            reject(err);
            return;
          }

          // Make sure user and user.sub are defined
          if (!user || !user.sub) {
            reject(new Error('User information is incomplete'));
            return;
          }

          // Determine the provider from the user's identities
          const provider = user.sub.startsWith('google-oauth2') ? 'google' : 'auth0';

          resolve({
            accessToken,
            provider
          });
        });
      } else {
        reject(new Error('Invalid authentication result'));
      }
    });
  });
};

// Function to handle Auth0 logout
export const handleAuth0Logout = async () => {
  try {
    auth0Client.logout({
      returnTo: window.location.origin
    });

    return true;
  } catch (error) {
    console.error('Auth0 logout error:', error);
    return false;
  }
};

export default auth0Config;