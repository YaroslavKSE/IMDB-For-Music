import { InteractionDetailDTO } from '../../api/interaction';
import { AlbumSummary, TrackSummary } from '../../api/catalog';

// Type to represent a diary entry with catalog item details
export interface DiaryEntry {
    interaction: InteractionDetailDTO;
    catalogItem?: AlbumSummary | TrackSummary;
    isPublic?: boolean; // New property to indicate if the entry is for public view
}

// Group diary entries by date
export interface GroupedEntries {
    date: string;
    entries: DiaryEntry[];
}