using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Abstracts;

public interface ITrivyReport<TSelf, out TId> : IEntity<TId>, ITrivyReport
where TSelf : ITrivyReport<TSelf, TId>
{
    TSelf MergeFrom(TSelf other);

    bool IsOtherNewer(TSelf other);
}

public interface ITrivyReport : IEntity;
