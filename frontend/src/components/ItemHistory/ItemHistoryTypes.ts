import { InteractionDetailDTO } from '../../api/interaction';
import { AlbumSummary, TrackSummary } from '../../api/catalog';
import { PublicUserProfile } from '../../api/users';

// Type to represent a history entry with catalog item details and user profile
export interface ItemHistoryEntry {
    interaction: InteractionDetailDTO;
    catalogItem?: AlbumSummary | TrackSummary;
    userProfile?: PublicUserProfile;
}

// Group history entries by date
export interface GroupedHistoryEntries {
    date: string;
    entries: ItemHistoryEntry[];
}