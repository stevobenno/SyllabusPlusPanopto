using System;
using System.Collections.Generic;
using System.Data;
using System.Runtime.CompilerServices;
using System.Threading;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using SyllabusPlusPanopto.Integration.Domain;
using SyllabusPlusPanopto.Integration.Domain.Settings;
using SyllabusPlusPanopto.Integration.Interfaces;

namespace SyllabusPlusPanopto.Integration.Implementations
{
    public sealed class SqlViewSourceProvider : ISourceDataProvider
    {
        private readonly SourceOptions _options;

        public SqlViewSourceProvider(IOptions<SourceOptions> opts)
        {
            _options = opts.Value ?? throw new ArgumentNullException(nameof(opts));

            if (string.IsNullOrWhiteSpace(_options.SqlConnectionString))
                throw new InvalidOperationException("Source.SqlConnectionString is required.");

            if (string.IsNullOrWhiteSpace(_options.SqlViewName))
                throw new InvalidOperationException("Source.SqlViewName is required.");
        }

        public async IAsyncEnumerable<SourceEvent> ReadAsync([EnumeratorCancellation] CancellationToken ct = default)
        {
            await using var con = new SqlConnection(_options.SqlConnectionString);
            await con.OpenAsync(ct);

            var sql = $"SELECT * FROM {_options.SqlViewName}";
            await using var cmd = new SqlCommand(sql, con);

            await using var rdr = await cmd.ExecuteReaderAsync(CommandBehavior.SequentialAccess, ct);

            // Cache ordinals
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

                yield return new SourceEvent(
                    ActivityName: rdr.IsDBNull(ordActivityName) ? "" : rdr.GetString(ordActivityName),
                    ModuleCode: rdr.IsDBNull(ordModuleCode) ? "" : rdr.GetString(ordModuleCode),
                    ModuleName: rdr.IsDBNull(ordModuleName) ? "" : rdr.GetString(ordModuleName),
                    ModuleCRN: rdr.IsDBNull(ordModuleCRN) ? "" : rdr.GetString(ordModuleCRN),
                    StaffName: rdr.IsDBNull(ordStaffName) ? "" : rdr.GetString(ordStaffName),
                    StartDate: rdr.IsDBNull(ordStartDate) ? DateTime.MinValue : rdr.GetDateTime(ordStartDate),
                    StartTime: rdr.IsDBNull(ordStartTime) ? TimeSpan.Zero : rdr.GetTimeSpan(ordStartTime),
                    EndTime: rdr.IsDBNull(ordEndTime) ? TimeSpan.Zero : rdr.GetTimeSpan(ordEndTime),
                    LocationName: rdr.IsDBNull(ordLocationName) ? "" : rdr.GetString(ordLocationName),
                    RecorderName: rdr.IsDBNull(ordRecorderName) ? "" : rdr.GetString(ordRecorderName),
                    RecordingFactor: rdr.IsDBNull(ordRecordingFactor) ? 0 : rdr.GetInt32(ordRecordingFactor),
                    StaffUserName: rdr.IsDBNull(ordStaffUserName) ? "" : rdr.GetString(ordStaffUserName)
                );
            }
        }
    }
}
