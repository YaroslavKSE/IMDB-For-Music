const OperationSelector = ({
                               value,
                               onChange,
                               label = "Operation for combining with next component:"
                           }: {
    value: number,
    onChange: (value: number) => void,
    label?: string
}) => (
    <div className="mt-2 border-t border-dashed border-gray-200 pt-2">
        <label className="block text-xs font-medium text-gray-700 mb-1">
            {label}
        </label>
        <select
            value={value}
            onChange={(e) => onChange(parseInt(e.target.value))}
            className="w-full px-2 py-1 border border-gray-300 rounded-md focus:outline-none focus:ring-primary-500 focus:border-primary-500 text-sm"
        >
            <option value={0}>Add (+)</option>
            <option value={1}>Subtract (-)</option>
            <option value={2}>Multiply (×)</option>
            <option value={3}>Divide (÷)</option>
        </select>
    </div>
);
export default OperationSelector;