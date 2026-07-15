namespace TrivyOperator.Dashboard.Infrastructure.Trivy.Mappers.Abstract;

public interface ITrivyReportKeyProvider<in TTrivyReportCr, out TKey>
{
    TKey GetKey(TTrivyReportCr cr);
}
