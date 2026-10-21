namespace TrivyOperator.Dashboard.Application.Types;

public static class TypeExtensions
{
    public static string GetReadableName(this Type type)
    {
        var name = type.Name.Split('`')[0];

        if (!type.IsGenericType)
            return name;

        var arguments = string.Join(", ",
            type.GetGenericArguments().Select(t => t.GetReadableName()));

        return $"{name}<{arguments}>";
    }
}
