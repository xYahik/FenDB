[AttributeUsage(AttributeTargets.Property)]
public class DBVariableNameAttribute : Attribute
{
    public string VariableName { get; }

    public DBVariableNameAttribute(string variableName)
    {
        VariableName = variableName;
    }
}
