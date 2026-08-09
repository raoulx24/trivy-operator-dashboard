namespace TrivyOperator.Dashboard.Application.Alerts.Models;

public readonly record struct EmitterKey : IEquatable<EmitterKey>
{
    public EmitterKey(IEnumerable<string> value)
    {
        Value = [.. value,];
    }

    public IReadOnlyList<string> Value { get; }

    public bool Equals(EmitterKey other) =>
        Value.SequenceEqual(other.Value);

    public override int GetHashCode()
    {
        HashCode hash = new();

        foreach (string item in Value)
            hash.Add(item);

        return hash.ToHashCode();
    }
}
