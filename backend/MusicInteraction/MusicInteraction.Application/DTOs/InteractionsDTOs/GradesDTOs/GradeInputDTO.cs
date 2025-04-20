public class GradeInputDTO
{
    public string ComponentName { get; set; }
    public float Value { get; set; }

    /// <summary>
    /// For nested components, use dot notation to specify the path:
    /// e.g. "ProductionBlock.Vocals" for the Vocals component inside the ProductionBlock
    /// </summary>
    public GradeInputDTO(string componentName, float value)
    {
        ComponentName = componentName;
        Value = value;
    }
}