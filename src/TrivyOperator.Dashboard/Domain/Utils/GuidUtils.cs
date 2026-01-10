using System.Security.Cryptography;
using System.Text;

namespace TrivyOperator.Dashboard.Domain.Utils;

public static class GuidUtils
{
    public static Guid GetDeterministicGuid(params string[] inputs)
    {
        string inputStr = string.Join("#", inputs);
        byte[] hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(inputStr));
        byte[] guidBytes = new byte[16];
        Array.Copy(hashBytes, guidBytes, 16);
        return new Guid(guidBytes);
    }
}
