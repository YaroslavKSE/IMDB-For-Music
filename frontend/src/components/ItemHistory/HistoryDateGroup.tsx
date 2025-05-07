import { Calendar } from 'lucide-react';
import ItemHistoryEntryComponent from './ItemHistoryEntry';
import { GroupedHistoryEntries } from './ItemHistoryTypes';
import {DiaryEntry} from "../Diary/types.ts";

interface HistoryDateGroupProps {
    group: GroupedHistoryEntries;
    onDeleteClick: (e: React.MouseEvent, entry: DiaryEntry) => void;
    isPublic: boolean;
}

const HistoryDateGroup = ({ group, onDeleteClick, isPublic }: HistoryDateGroupProps) => {
    return (
        <div className="bg-white rounded-lg shadow overflow-hidden">
            <div className="bg-primary-50 px-6 py-3 border-b border-primary-100">
                <div className="flex items-center">
                    <Calendar className="h-5 w-5 text-primary-600 mr-2" />
                    <h2 className="text-lg font-medium text-primary-800">{group.date}</h2>
                </div>
            </div>

            <div className="divide-y divide-gray-200">
                {group.entries.map((entry) => (
                    <ItemHistoryEntryComponent
                        key={entry.interaction.aggregateId}
                        entry={entry}
                        onDeleteClick={onDeleteClick}
                        isPublic={isPublic}
                    />
                ))}
            </div>
        </div>
    );
};

export default HistoryDateGroup;