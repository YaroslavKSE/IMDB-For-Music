import {BlockComponent} from "../../api/interaction.ts";
import GradeSlider from "./GradeSlider.tsx";

const BlockGrader = ({
                         component,
                         values,
                         onChange,
                         path = ''
                     }: {
    component: BlockComponent,
    values: Record<string, number>,
    onChange: (name: string, value: number) => void,
    path?: string
}) => {
    const fullPath = path ? `${path}.${component.name}` : component.name;

    return (
        <div className="mb-4 bg-gray-50 p-4 rounded-md border border-gray-200">
            <h3 className="font-medium text-gray-900 mb-3">{component.name}</h3>

            <div className="space-y-4">
                {component.subComponents.map((subComponent, index) => (
                    <div key={index}>
                        {subComponent.componentType === 'grade' ? (
                            <GradeSlider
                                component={subComponent}
                                value={values[`${fullPath}.${subComponent.name}`] ?? null}
                                onChange={onChange}
                                path={fullPath}
                            />
                        ) : (
                            <BlockGrader
                                component={subComponent as BlockComponent}
                                values={values}
                                onChange={onChange}
                                path={fullPath}
                            />
                        )}
                    </div>
                ))}
            </div>
        </div>
    );
};
export default BlockGrader;