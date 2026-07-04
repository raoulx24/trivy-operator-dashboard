namespace TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

public readonly record struct Timestamp : IComparable<Timestamp>
{
    public DateTime Value { get; }

    public Timestamp(DateTime value)
    {
        Value = value.Kind switch
        {
            DateTimeKind.Utc => value,
            DateTimeKind.Local => value.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(value, DateTimeKind.Utc),
            _ => throw new ArgumentOutOfRangeException(nameof(value)),
        };
    }

    public static Timestamp Now() => new(DateTime.UtcNow);
    
    public int CompareTo(Timestamp other)
        => Value.CompareTo(other.Value);

    public static bool operator >(Timestamp left, Timestamp right)
        => left.Value > right.Value;

    public static bool operator <(Timestamp left, Timestamp right)
        => left.Value < right.Value;
}
