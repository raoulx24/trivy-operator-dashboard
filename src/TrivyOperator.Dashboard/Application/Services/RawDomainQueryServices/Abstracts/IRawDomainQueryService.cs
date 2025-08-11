﻿namespace TrivyOperator.Dashboard.Application.Services.RawDomainQueryServices.Abstracts;

public interface IRawDomainQueryService
{
    Task<IReadOnlyList<object>> GetAllAsync(Type valueType, string key, CancellationToken ct = default);
}