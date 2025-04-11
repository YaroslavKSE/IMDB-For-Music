import { useState, useEffect } from 'react';
import { useParams } from 'react-router-dom';
import CatalogService, { ArtistDetail } from '../api/catalog';
import ArtistHeader from '../components/Artist/ArtistHeader';
import ArtistContentTabs from '../components/Artist/ArtistContentTabs';
import ArtistLoadingState from '../components/Artist/ArtistLoadingState';
import ArtistErrorState from '../components/Artist/ArtistErrorState';
import ArtistNotFoundState from '../components/Artist/ArtistNotFoundState';

const Artist = () => {
    const { id } = useParams<{ id: string }>();
    const [artist, setArtist] = useState<ArtistDetail | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);
    const [activeTab, setActiveTab] = useState<'overview' | 'albums' | 'top-tracks'>('overview');

    useEffect(() => {
        const fetchArtistDetails = async () => {
            if (!id) return;

            setLoading(true);
            setError(null);

            try {
                const artistData = await CatalogService.getArtist(id);
                setArtist(artistData);
            } catch (err) {
                console.error('Error fetching artist details:', err);
                setError('Failed to load artist information. Please try again later.');
            } finally {
                setLoading(false);
            }
        };

        fetchArtistDetails();
    }, [id]);

    if (loading) {
        return <ArtistLoadingState />;
    }

    if (error) {
        return <ArtistErrorState error={error} />;
    }

    if (!artist) {
        return <ArtistNotFoundState />;
    }

    return (
        <div className="max-w-6xl mx-auto pb-12">
            <ArtistHeader artist={artist} />

            <ArtistContentTabs
                activeTab={activeTab}
                setActiveTab={setActiveTab}
                artist={artist}
            />
        </div>
    );
};

export default Artist;