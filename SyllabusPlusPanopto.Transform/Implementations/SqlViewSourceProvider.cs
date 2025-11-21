using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Data.SqlClient;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Implementations
{
    public sealed class SqlViewSourceProvider : ISourceDataProvider
    {
        private readonly string _conn;
        private readonly string _view;

        public SqlViewSourceProvider(string conn, string view)
        {
            _conn = conn ?? throw new ArgumentNullException(nameof(conn));
            _view = view ?? throw new ArgumentNullException(nameof(view));
        }

        public async IAsyncEnumerable<SourceEvent> ReadAsync(
            [EnumeratorCancellation] CancellationToken ct = default)
        {
            await using var con = new SqlConnection(_conn);
            await con.OpenAsync(ct);

            // View name is assumed trusted / whitelisted in config.
            var sql = $"SELECT top 500(*) FROM {_view}"; // todo
            await using var cmd = new SqlCommand(sql, con)
            {
                CommandType = CommandType.Text
            };

            await using var rdr = await cmd.ExecuteReaderAsync(
                CommandBehavior.SequentialAccess, ct);

            var ordActivityName = rdr.GetOrdinal("ActivityName");
            var ordModuleCode = rdr.GetOrdinal("ModuleCode");
            var ordModuleName = rdr.GetOrdinal("ModuleName");
            var ordModuleCRN = rdr.GetOrdinal("ModuleCRN");
            var ordStaffName = rdr.GetOrdinal("StaffName");
            var ordStartDate = rdr.GetOrdinal("StartDate");
            var ordStartTime = rdr.GetOrdinal("StartTime");
            var ordEndTime = rdr.GetOrdinal("EndTime");
            var ordLocationName = rdr.GetOrdinal("LocationName");
            var ordRecorderName = rdr.GetOrdinal("RecorderName");
            var ordRecordingFactor = rdr.GetOrdinal("RecordingFactor");
            var ordStaffUserName = rdr.GetOrdinal("StaffUserName");

            while (await rdr.ReadAsync(ct))
            {
                if (ct.IsCancellationRequested)
                    yield break;

                string GetString(int ord) =>
                    rdr.IsDBNull(ord) ? string.Empty : rdr.GetString(ord);

                DateTime GetDate(int ord) =>
                    rdr.IsDBNull(ord) ? DateTime.MinValue : rdr.GetDateTime(ord);

                TimeSpan GetTime(int ord)
                {
                    if (rdr.IsDBNull(ord)) return TimeSpan.Zero;
                    return rdr.GetTimeSpan(ord); // assumes SQL type 'time'
                }

                int GetInt(int ord) =>
                    rdr.IsDBNull(ord) ? 0 : rdr.GetInt32(ord);

                yield return new SourceEvent(
                    ActivityName: GetString(ordActivityName),
                    ModuleCode: GetString(ordModuleCode),
                    ModuleName: GetString(ordModuleName),
                    ModuleCRN: GetString(ordModuleCRN),
                    StaffName: GetString(ordStaffName),
                    StartDate: GetDate(ordStartDate),
                    StartTime: GetTime(ordStartTime),
                    EndTime: GetTime(ordEndTime),
                    LocationName: GetString(ordLocationName),
                    RecorderName: GetString(ordRecorderName),
                    RecordingFactor: GetInt(ordRecordingFactor),
                    StaffUserName: GetString(ordStaffUserName)
                );
            }
        }
    }
}
