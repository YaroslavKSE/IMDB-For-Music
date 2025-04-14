import { AlbumSummary, ArtistSummary, TrackSummary } from '../../api/catalog';

export type SearchTab = 'all' | 'album' | 'track' | 'artist';

export interface SearchState {
    query: string;
    albums: AlbumSummary[];
    tracks: TrackSummary[];
    artists: ArtistSummary[];
    albumsOffset: number;
    tracksOffset: number;
    artistsOffset: number;
    albumsTotal: number;
    tracksTotal: number;
    artistsTotal: number;
    albumsLoaded: boolean;
    tracksLoaded: boolean;
    artistsLoaded: boolean;
}

export interface SearchTabProps {
    searchQuery: string;
    loading: boolean;
    error: string | null;
    onShowMore: () => void;
}

export interface TabButtonProps {
    active: boolean;
    onClick: () => void;
    icon: React.ReactNode;
    label: string;
}

export interface LoadMoreButtonProps {
    isLoading: boolean;
    onClick: (e: React.MouseEvent) => void;
    currentCount: number;
    totalCount: number;
}