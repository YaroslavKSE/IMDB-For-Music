const InlineDescription = ({
                               description,
                               textColor
                           }: {
    description: string,
    textColor: string
}) => {
    return (
        <div className="mt-2 bg-white bg-opacity-70 rounded-md p-2 border border-gray-200 shadow-sm">
            <div className={`text-xs ${textColor}`}>
                <span className="font-medium">Description: </span>{description}
            </div>
        </div>
    );
};
export default InlineDescription;