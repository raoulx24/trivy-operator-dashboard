namespace TrivyOperator.Dashboard.Domain.TrivyOld.Services.FileRepository.Abstractions;

public interface IFolderNameFactory
{
    string Get<TKubernetesObject>();
}
