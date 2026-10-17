using TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;
using TrivyOperator.Dashboard.Domain.Shared.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.EndOfLife.Entities;

// the resolved end of life info for a subject; Product and Release are set only when they could be determined
public sealed record EolInfo(
    EolSubject Subject,
    EolStatus Status,
    EolNotApplicableReason? NotApplicableReason,
    string? ErrorMessage,
    EolProduct? Product,
    EolRelease? Release,
    Timestamp ResolvedAt)
{
    public static EolInfo Pending(EolSubject subject) =>
        new(subject, EolStatus.Pending, null, null, null, null, Timestamp.Now());

    public static EolInfo Resolved(EolSubject subject, EolProduct product, EolRelease release) =>
        new(subject, EolStatus.Ok, null, null, product, release, Timestamp.Now());

    public static EolInfo NotApplicable(EolSubject subject, EolNotApplicableReason reason, EolProduct? product = null) =>
        new(subject, EolStatus.NotApplicable, reason, null, product, null, Timestamp.Now());

    public static EolInfo Failed(EolSubject subject, string? errorMessage) =>
        new(subject, EolStatus.Error, null, errorMessage, null, null, Timestamp.Now());
}
