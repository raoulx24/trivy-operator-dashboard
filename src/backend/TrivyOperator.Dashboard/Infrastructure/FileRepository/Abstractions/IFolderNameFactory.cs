namespace TrivyOperator.Dashboard.Infrastructure.FileRepository.Abstractions;

public interface IFolderNameFactory
{
    string Get<TKubernetesObject>();
}
