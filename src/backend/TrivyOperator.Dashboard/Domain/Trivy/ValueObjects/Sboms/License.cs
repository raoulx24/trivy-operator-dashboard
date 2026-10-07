namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record License
{
    public string? Id { get; }
    public string? Name { get; }
    public Uri? Url { get; }

    public License(string? id, string? name, Uri? url)
    {
        Id = string.IsNullOrWhiteSpace(id) ? null : string.Intern(id.Trim());
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Url = url;
    }
}