namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Shared;

public sealed record ImageMeta(
    ImageRegistry Registry,
    ImageRepository Repo,
    ImageName Name,
    ImageTag Tag
);

public readonly record struct ImageRegistry
{
    private const string Sentinel = "n/a";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ImageRegistry(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public ImageRegistry() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ImageRepository
{
    private const string Sentinel = "n/a";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ImageRepository(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public ImageRepository() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ImageName
{
    private const string Sentinel = "n/a";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ImageName(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim().ToLowerInvariant();
    }

    public ImageName() : this(Sentinel) { }

    public override string ToString() => Value;
}

public readonly record struct ImageTag
{
    private const string Sentinel = "n/a";
    
    public string Value { get; }
    public bool IsValid => Value != Sentinel;

    public ImageTag(string value)
    {
        Value = string.IsNullOrWhiteSpace(value) ? Sentinel : value.Trim();
    }

    public ImageTag() : this(Sentinel) { }

    public override string ToString() => Value;
}
