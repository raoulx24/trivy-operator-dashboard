namespace TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

public readonly record struct Kind
{
    public string Value { get; }

    public Kind(string value)
    {
        Value = string.Intern(value.ToLowerInvariant());
    }

    public override string ToString() => Value;

    public static string RbacAssessment => "rbacassessmentreport";
}
