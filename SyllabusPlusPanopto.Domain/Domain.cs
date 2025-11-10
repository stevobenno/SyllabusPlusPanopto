
using System;
using System.Linq;

namespace SyllabusPlusPanopto.Domain;

public record SpRawRow(string ModuleCode, DateTime StartUtc, DateTime EndUtc, string Room, string RecorderId, string OwnerEmail);

public record PanoptoScheduleCandidate(
    string Hash,
    string ModuleCode,
    DateTime StartUtc,
    DateTime EndUtc,
    string Room,
    string RecorderId,
    string OwnerEmail);

public static class RowHash
{
    public static string Compute(params string[] parts)
    {
        static string N(string? s) => (s ?? string.Empty).Trim().ToUpperInvariant();
        using var sha = System.Security.Cryptography.SHA256.Create();
        var joined = string.Join("|", parts.Select(N));
        var bytes = System.Text.Encoding.UTF8.GetBytes(joined);
        var hash = sha.ComputeHash(bytes);
        return Convert.ToHexString(hash);
    }
}
