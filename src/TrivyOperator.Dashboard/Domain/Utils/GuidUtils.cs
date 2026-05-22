using System.Security.Cryptography;
using System.Text;

namespace TrivyOperator.Dashboard.Domain.Utils;

public static class GuidUtils
{
    public static Guid GetDeterministicGuid(params object[] inputs)
    {
        IncrementalHash hasher = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);

        foreach (object input in inputs)
        {
            switch (input)
            {
                case string s:
                    AppendString(hasher, s);
                    break;

                case string[] arr:
                    foreach (string s2 in arr)
                        AppendString(hasher, s2);
                    break;

                case string[][] matrix:
                    foreach (string[] row in matrix)
                    {
                        foreach (string cell in row)
                        {
                            AppendString(hasher, cell);
                            hasher.AppendData(new byte[] { 0x1F, }); // field sep - 0x1F - Unit Separator (US)
                        }
                        hasher.AppendData(new byte[] { 0x1E, });     // row sep - 0x1E - Record Separator (RS)
                    }
                    break;

                default:
                    AppendString(hasher, input?.ToString() ?? string.Empty);
                    break;
            }
        }

        byte[] hash = hasher.GetHashAndReset();
        return new Guid(hash.AsSpan(0, 16));
    }

    private static void AppendString(IncrementalHash hasher, string s)
    {
        byte[] bytes = Encoding.UTF8.GetBytes(s);
        hasher.AppendData(bytes);
        hasher.AppendData(new byte[] { 0x1F });
    }
}
