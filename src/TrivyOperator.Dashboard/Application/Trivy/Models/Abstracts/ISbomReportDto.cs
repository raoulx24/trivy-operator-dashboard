namespace TrivyOperator.Dashboard.Application.Trivy.Models.Abstracts;

public interface ISbomReportDto<TSBomReportDetailDto>
    where TSBomReportDetailDto : ISBomReportDetailDto
{
    string RootNodeBomRef { get; set; }
    TSBomReportDetailDto[] Details { get; set; }
}

public interface ISBomReportDetailDto
{
    string BomRef { get; set; }
    string[] DependsOn { get; set; }
}
