namespace TrivyOperator.Dashboard.Domain.Trivy.ValueObjects.Sboms;

public sealed record Supplier
{
    public string? Name { get; }
    public string? Email { get; }
    public string? Phone { get; }

    public Supplier(string? name, string? email, string? phone)
    {
        Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim();
        Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();
        Phone = string.IsNullOrWhiteSpace(phone) ? null : phone.Trim();
    }
}
