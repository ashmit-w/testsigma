using System.Security.Cryptography;
using System.Text;

namespace Puppet.Core;

/// <summary>
/// Deterministic element ids (MB-9): same automationId + controlType +
/// structural path hashes to the same id on every run.
/// </summary>
public static class ElementIdHasher
{
    public static string ComputeId(string? automationId, string controlType, IReadOnlyList<string> path)
    {
        var canonical = $"{automationId}|{controlType}|{string.Join("/", path)}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(canonical));
        return "el_" + Convert.ToHexString(hash.AsSpan(0, 8)).ToLowerInvariant();
    }
}
