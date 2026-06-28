namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public readonly record struct ComponentId(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct ComponentName(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct ComponentVersion(string Value)
{
    public override string ToString() => Value;
}

public readonly record struct ComponentType(string Value)
{
    public override string ToString() => Value;
}
