import { ChevronLeft } from 'lucide-react';
import { NavigateFunction } from 'react-router-dom';
import {RefObject} from "react";

interface InteractionHeaderProps {
    formattedItemType: string;
    navigate: NavigateFunction;
    audio?: RefObject<HTMLAudioElement | null>;
}

const InteractionHeader = ({ formattedItemType, navigate, audio }: InteractionHeaderProps) => (
    <div className="bg-white shadow rounded-lg mb-6">
        <div className="px-6 py-4 flex justify-between items-center">
            <button
                onClick={() => {
                    if (audio?.current) {
                        audio.current.pause();
                    }
                    navigate(-1);
                }}
                className="flex items-center text-gray-600 hover:text-gray-900"
            >
                <ChevronLeft className="h-5 w-5 mr-1" />
                Back
            </button>
            <h1 className="text-2xl font-bold text-center text-gray-900">
                Rate {formattedItemType}
            </h1>
            <div className="w-6"></div>
        </div>
    </div>
);

export default InteractionHeader;