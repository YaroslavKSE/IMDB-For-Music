import React, { useState } from 'react';
import { Search } from 'lucide-react';

interface SearchBarProps {
    initialQuery: string;
    onSearch: (query: string) => void;
}

const SearchBar: React.FC<SearchBarProps> = ({ initialQuery, onSearch }) => {
    const [searchQuery, setSearchQuery] = useState(initialQuery);

    const handleSearchSubmit = (e: React.FormEvent) => {
        e.preventDefault();
        if (searchQuery.trim()) {
            onSearch(searchQuery.trim());
        }
    };

    return (
        <div className="max-w-3xl mb-8">
            <form onSubmit={handleSearchSubmit} className="relative">
                <div className="flex items-center">
                    <input
                        type="text"
                        placeholder="Search for artists, albums, or tracks..."
                        value={searchQuery}
                        onChange={(e) => setSearchQuery(e.target.value)}
                        className="w-full py-3 px-5 pl-12 rounded-full text-base focus:outline-none border border-gray-300 focus:border-primary-500 focus:ring-2 focus:ring-primary-200 shadow-sm"
                    />
                    <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                        <Search className="h-5 w-5 text-gray-400" />
                    </div>
                    <button
                        type="submit"
                        className="absolute right-3 bg-primary-600 text-white p-2 rounded-full hover:bg-primary-700 transition-colors"
                    >
                        <Search className="h-5 w-5" />
                    </button>
                </div>
            </form>
        </div>
    );
};

export default SearchBar;