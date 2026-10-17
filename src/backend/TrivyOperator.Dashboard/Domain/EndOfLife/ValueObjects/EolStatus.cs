namespace TrivyOperator.Dashboard.Domain.EndOfLife.ValueObjects;

public enum EolStatus
{
    // seen on Trivy side, not processed yet
    Pending,
    Ok,
    NotApplicable,
    Error,
}

public enum EolNotApplicableReason
{
    // the subject name has no endoflife.date product in mappings
    NoMapping,
    // endoflife.date does not know the mapped product
    ProductNotFound,
    // the product exists, but no release cycle matches the subject version
    CycleNotFound,
}
