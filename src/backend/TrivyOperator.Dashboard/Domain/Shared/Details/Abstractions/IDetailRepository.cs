using TrivyOperator.Dashboard.Domain.Shared.Abstractions;

namespace TrivyOperator.Dashboard.Domain.Shared.Details.Abstractions;

public interface IDetailRepository<TDetail, TId> : IDetailStore<TDetail, TId>, IDetailProvider<TDetail, TId>
    where TDetail : IDetail<TId>
    where TId : notnull;
