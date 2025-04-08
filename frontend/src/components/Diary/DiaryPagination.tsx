import { ChevronLeft, ChevronRight } from 'lucide-react';

interface DiaryPaginationProps {
    currentPage: number;
    totalPages: number;
    onPageChange: (page: number) => void;
}

const DiaryPagination = ({ currentPage, totalPages, onPageChange }: DiaryPaginationProps) => {
    if (totalPages <= 1) return null;

    return (
        <div className="flex justify-center mt-8">
            <nav className="inline-flex rounded-md shadow">
                <button
                    onClick={() => onPageChange(currentPage - 1)}
                    disabled={currentPage === 1}
                    className="inline-flex items-center px-3 py-2 rounded-l-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <ChevronLeft className="h-5 w-5" />
                </button>
                <div className="px-4 py-2 border-t border-b border-gray-300 bg-white text-gray-700">
                    Page {currentPage} of {totalPages}
                </div>
                <button
                    onClick={() => onPageChange(currentPage + 1)}
                    disabled={currentPage === totalPages}
                    className="inline-flex items-center px-3 py-2 rounded-r-md border border-gray-300 bg-white text-gray-700 hover:bg-gray-50 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <ChevronRight className="h-5 w-5" />
                </button>
            </nav>
        </div>
    );
};

export default DiaryPagination;