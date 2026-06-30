using TrivyOperator.Dashboard.Domain.K8s.ValueObjects;

namespace TrivyOperator.Dashboard.Domain.Trivy.Entities.Factories;

public static class ReportKinds
{
    public static readonly Kind ClusterCompliance =  new("ClusterComplianceReport");
    public static readonly Kind ClusterConfigAudit = new("ClusterConfigAuditReport");
    public static readonly Kind ClusterInfraAssessment = new("ClusterInfraAssessmentReport");
    public static readonly Kind ClusterRbacAssessment = new("ClusterRbacAssessmentReport");
    public static readonly Kind ClusterVulnerability = new("ClusterVulnerabilityReport");
    
    
    public static readonly Kind ConfigAudit = new("ConfigAuditReport");
    public static readonly Kind ExposedSecret = new("ExposedSecretReport");
    public static readonly Kind InfraAssessment = new("InfraAssessmentReport");
    public static readonly Kind RbacAssessment = new("RbacAssessmentReport");
    public static readonly Kind Sbom =  new("SbomReport");
    public static readonly Kind Vulnerability = new("VulnerabilityReport");
}
