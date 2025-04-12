const ReviewTextArea = ({
                            reviewText,
                            setReviewText,
                            formattedItemType
                        }: {
    reviewText: string;
    setReviewText: (value: string) => void;
    formattedItemType: string;
}) => (
    <div className="mb-6">
        <label className="block text-base font-medium text-gray-700 mb-2">
            Your Review
        </label>
        <textarea
            value={reviewText}
            onChange={(e) => setReviewText(e.target.value)}
            className="w-full px-4 py-3 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500"
            rows={6}
            placeholder={`Write your thoughts about this ${formattedItemType.toLowerCase()}...`}
        />
    </div>
);

export default ReviewTextArea;
