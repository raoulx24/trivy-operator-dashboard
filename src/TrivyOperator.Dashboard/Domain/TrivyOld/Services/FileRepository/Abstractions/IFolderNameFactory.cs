namespace TrivyOperator.Dashboard.Domain.Trivy.Services.FileRepository.Abstractions;

public interface IFolderNameFactory
{
    string Get<TKubernetesObject>();
}
