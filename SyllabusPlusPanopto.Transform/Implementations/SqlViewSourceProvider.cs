using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Data.SqlClient;
using SyllabusPlusPanopto.Domain;
using SyllabusPlusPanopto.Transform.Interfaces;
using SpRawRow = SyllabusPlusPanopto.Transform.SpRawRow;

public sealed class SqlViewSourceProvider : ISourceDataProvider
{
    private readonly string _conn;
    private readonly string _view;
    public SqlViewSourceProvider(string conn, string view) { _conn = conn; _view = view; }

    public async IAsyncEnumerable<SpRawRow> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
    {
        await using var con = new SqlConnection(_conn);
        await con.OpenAsync(ct);
        await using var cmd = new SqlCommand($"SELECT * FROM {_view}", con) { CommandType = CommandType.Text };
        await using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);

        while (await rdr.ReadAsync(ct))
        {
            yield return new SpRawRow(
                ModuleCode: rdr.GetString(rdr.GetOrdinal("ModuleCode")),
                StartUtc: rdr.GetDateTime(rdr.GetOrdinal("StartUtc")),
                EndUtc: rdr.GetDateTime(rdr.GetOrdinal("EndUtc")),
                Room: rdr.GetString(rdr.GetOrdinal("Room")),
                Recorder: rdr.GetString(rdr.GetOrdinal("Recorder")),
                OwnerEmail: rdr.GetString(rdr.GetOrdinal("OwnerEmail")));
        }
    }
}
