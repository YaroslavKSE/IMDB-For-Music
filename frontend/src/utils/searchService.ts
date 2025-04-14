import CatalogService, { SearchResult } from '../api/catalog';
import { SearchTab } from '../components/Search/types';

// Constants
export const INITIAL_LIMIT = 5;    // Initial items shown in 'all' tab
export const EXPANDED_LIMIT = 20;  // Items shown in dedicated tabs

// Cache for search results to avoid redundant API calls
const searchCache: Record<string, SearchResult> = {};

/**
 * Generate a cache key for search results
 */
const getCacheKey = (query: string, searchType: string, limit: number, offset: number): string => {
    return `${query}|${searchType}|${limit}|${offset}`;
};

/**
 * Fetch search results with caching
 */
export const fetchSearchResults = async (
    query: string,
    searchType: string,
    limit: number,
    offset: number
): Promise<SearchResult | null> => {
    if (!query.trim()) return null;

    const cacheKey = getCacheKey(query, searchType, limit, offset);

    // Return cached results if available
    if (searchCache[cacheKey]) {
        return searchCache[cacheKey];
    }

    try {
        const result = await CatalogService.search(query, searchType, limit, offset);

        // Cache the result
        searchCache[cacheKey] = result;

        return result;
    } catch (err) {
        console.error('Error fetching search results:', err);
        throw err;
    }
};

/**
 * Fetch search results for a specific tab with proper limit and offset
 */
export const fetchTabResults = async (
    query: string,
    tab: SearchTab,
    offset: number = 0
): Promise<SearchResult | null> => {
    const isAllTab = tab === 'all';
    const limit = isAllTab ? INITIAL_LIMIT : EXPANDED_LIMIT;
    const searchType = isAllTab ? 'album,track,artist' : tab;

    const result = await fetchSearchResults(query, searchType, limit, offset);

    // Ensure we have the proper total results for each type when in 'all' tab
    if (isAllTab && result) {
        // For the 'all' tab, we need to save the total results for each type
        // Sometimes the API might return only the total for the combined result
        // Make sure each individual type has a reasonable total (at least what we have)
        if (result.albums && !result.totalResults) {
            result.totalResults = Math.max(result.albums.length, result.totalResults || 0);
        }
        if (result.tracks && !result.totalResults) {
            result.totalResults = Math.max(result.tracks.length, result.totalResults || 0);
        }
        if (result.artists && !result.totalResults) {
            result.totalResults = Math.max(result.artists.length, result.totalResults || 0);
        }
    }

    return result;
};

/**
 * Check if results for a tab are cached
 */
export const isTabResultsCached = (
    query: string,
    tab: SearchTab,
    offset: number = 0
): boolean => {
    const isAllTab = tab === 'all';
    const limit = isAllTab ? INITIAL_LIMIT : EXPANDED_LIMIT;
    const searchType = isAllTab ? 'album,track,artist' : tab;

    const cacheKey = getCacheKey(query, searchType, limit, offset);
    return !!searchCache[cacheKey];
};

/**
 * Clear search cache - can be used when needed (e.g., on logout)
 */
export const clearSearchCache = (): void => {
    Object.keys(searchCache).forEach(key => {
        delete searchCache[key];
    });
};