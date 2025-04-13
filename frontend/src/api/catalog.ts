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
  genres : string[];
  externalUrls?: string[];
}

export interface ArtistDetail extends ArtistSummary {
  followersCount: number;
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
  previewUrl?: string;
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

export interface NewReleasesResult {
  limit: number;
  offset: number;
  totalResults: number;
  next: string | null;
  previous: string | null;
  albums: AlbumSummary[];
}

export interface ArtistTopTracksResult {
  artistId: string;
  artistName: string;
  market: string;
  tracks: TrackSummary[];
}

export interface ArtistAlbumsResult {
  artistId: string;
  artistName: string;
  limit: number;
  offset: number;
  totalResults: number;
  next: string | null;
  previous: string | null;
  albums: AlbumSummary[];
}

export interface BatchItemsResponse {
  tracks?: TrackSummary[];
  albums?: AlbumSummary[];
  count: number;
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

  getArtist: async (spotifyId: string): Promise<ArtistDetail> => {
    const response = await catalogApi.get(`/artists/spotify/${spotifyId}`);
    return response.data;
  },

  getArtistTopTracks: async (artistId: string): Promise<ArtistTopTracksResult> => {
    const response = await catalogApi.get(`/artists/spotify/${artistId}/top-tracks`);
    return response.data;
  },

  getArtistAlbums: async (
      artistId: string,
      limit: number = 20,
      offset: number = 0,
      includeGroups: string = 'album'
  ): Promise<ArtistAlbumsResult> => {
    const response = await catalogApi.get(`/artists/spotify/${artistId}/albums`, {
      params: {
        limit,
        offset,
        include_groups: includeGroups
      }
    });
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

  getNewReleases: async (limit: number = 10, offset: number = 0): Promise<NewReleasesResult> => {
    const response = await catalogApi.get('/search/new-releases', {
      params: {
        limit,
        offset
      }
    });
    return response.data;
  },

  getBatchAlbums: async (albumIds: string[]): Promise<BatchItemsResponse> => {
    if (albumIds.length === 0) return { albums: [], count: 0 };

    // Only process up to 20 IDs at a time
    const idsToFetch = albumIds.slice(0, 20);
    const idsParam = idsToFetch.join(',');

    const response = await catalogApi.get('/albums/spotify', {
      params: {
        ids: idsParam
      }
    });
    return response.data;
  },

  getBatchTracks: async (trackIds: string[]): Promise<BatchItemsResponse> => {
    if (trackIds.length === 0) return { tracks: [], count: 0 };

    // Only process up to 20 IDs at a time
    const idsToFetch = trackIds.slice(0, 20);
    const idsParam = idsToFetch.join(',');

    const response = await catalogApi.get('/tracks/spotify', {
      params: {
        ids: idsParam
      }
    });
    return response.data;
  },
};

export default CatalogService;