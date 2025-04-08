import { createApiClient } from '../utils/axios-factory';

// Create the API client specifically for catalog service
const catalogApi = createApiClient('/catalog');

export interface Image {
  url: string;
  height?: number;
  width?: number;
}

export interface ArtistSummary {
  catalogItemId: string;
  spotifyId: string;
  name: string;
  imageUrl?: string;
  images?: Image[];
  popularity?: number;
  externalUrls?: string[];
}

export interface TrackSummary {
  catalogItemId: string;
  spotifyId: string;
  name: string;
  artistName: string;
  imageUrl?: string;
  images?: Image[];
  durationMs: number;
  isExplicit: boolean;
  trackNumber?: number;
  albumId: string;
  popularity?: number;
  externalUrls?: string[];
}

export interface AlbumSummary {
  catalogItemId: string;
  spotifyId: string;
  name: string;
  artistName: string;
  imageUrl?: string;
  images?: Image[];
  releaseDate?: string;
  albumType?: string;
  totalTracks: number;
  externalUrls?: string[];
}

export interface TrackDetail extends TrackSummary {
  artists: ArtistSummary[];
  album: AlbumSummary;
  discNumber: number;
  isrc?: string;
  previewUrl?: string;
  duration?: string;
}

export interface AlbumDetail extends AlbumSummary {
  artists: ArtistSummary[];
  tracks: TrackSummary[];
  releaseDatePrecision?: string;
  label?: string;
  copyright?: string;
}

export interface SearchResult {
  query: string;
  type: string;
  limit: number;
  offset: number;
  totalResults: number;
  albums?: AlbumSummary[];
  tracks?: TrackSummary[];
  artists?: ArtistSummary[];
}

const CatalogService = {
  getTrack: async (spotifyId: string): Promise<TrackDetail> => {
    const response = await catalogApi.get(`/tracks/spotify/${spotifyId}`);
    return response.data;
  },

  getAlbum: async (spotifyId: string): Promise<AlbumDetail> => {
    const response = await catalogApi.get(`/albums/spotify/${spotifyId}`);
    return response.data;
  },

  search: async (
    query: string,
    type: string = 'album,artist,track',
    limit: number = 20,
    offset: number = 0
  ): Promise<SearchResult> => {
    const response = await catalogApi.get('/search', {
      params: {
        q: query,
        type,
        limit,
        offset,
      },
    });
    return response.data;
  },
};

export default CatalogService;