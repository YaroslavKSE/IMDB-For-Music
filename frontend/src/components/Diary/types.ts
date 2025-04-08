import { UserInteractionDetail } from '../../api/interaction';
import { AlbumSummary, TrackSummary } from '../../api/catalog';

// Type to represent a diary entry with catalog item details
export interface DiaryEntry {
    interaction: UserInteractionDetail;
    catalogItem?: AlbumSummary | TrackSummary;
}

// Group diary entries by date
export interface GroupedEntries {
    date: string;
    entries: DiaryEntry[];
}