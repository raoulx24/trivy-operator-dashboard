namespace TrivyOperator.Dashboard.Infrastructure.Abstractions;

public interface IKubernetesContextProvider
{
    string GetCurrentContext();
}