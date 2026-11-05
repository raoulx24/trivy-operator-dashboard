using k8s.Models;
using TrivyOperator.Dashboard.Domain.Kubernetes.ValueObjects;

namespace TrivyOperator.Dashboard.Infrastructure.Kubernetes.Mappers.Extensions;

// the uid is set by the api server; objects read from files may come without one
// - a missing uid is made up (a new guid) so the object stays usable; it changes on every load of the files
// - it is written back to the object, so everything mapping the same object (e.g. the key provider and the mapper)
//   gets the same uid
public static class UidMappingExtensions
{
    public static Uid ToUid(this V1ObjectMeta metadata)
    {
        if (string.IsNullOrWhiteSpace(metadata.Uid))
        {
            metadata.Uid = Guid.NewGuid().ToString();
        }

        return new Uid(metadata.Uid);
    }

    public static Uid ToUid(this V1OwnerReference ownerReference)
    {
        if (string.IsNullOrWhiteSpace(ownerReference.Uid))
        {
            ownerReference.Uid = Guid.NewGuid().ToString();
        }

        return new Uid(ownerReference.Uid);
    }
}
