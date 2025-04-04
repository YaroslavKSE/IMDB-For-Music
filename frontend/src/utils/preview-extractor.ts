export async function getPreviewUrl(spotifyTrackId: string): Promise<string | null> {
    try {
        // Use Vite's proxy to avoid CORS issues
        const embedUrl = `/spotify/embed/track/${spotifyTrackId}`;
        const response = await fetch(embedUrl);
        const html = await response.text();

        const matches = html.match(/"audioPreview":\s*{\s*"url":\s*"([^"]+)"/);
        return matches ? matches[1] : null;
    } catch (error) {
        console.error("Failed to fetch Spotify preview URL:", error);
        return null;
    }
}