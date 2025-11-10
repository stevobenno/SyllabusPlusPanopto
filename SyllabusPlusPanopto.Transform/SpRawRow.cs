using System;

namespace SyllabusPlusPanopto.Transform;

public record SpRawRow(
    string ModuleCode,
    DateTime StartUtc,
    DateTime EndUtc,
    string Room,
    string Recorder,
    string OwnerEmail);
