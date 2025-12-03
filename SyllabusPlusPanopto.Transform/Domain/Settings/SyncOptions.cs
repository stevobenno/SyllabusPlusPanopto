namespace SyllabusPlusPanopto.Integration.Domain.Settings;

public sealed class SyncOptions
{
   
    public int? MinExpectedRows { get; set; }

    public bool AllowDeletions { get; set; } = true;

  
    public int SyncHorizonDays { get; set; } = 140; // TODO: Config

  
    public int LookbackDays { get; set; } = -14;// TODO: Config

    /// <summary>
    /// If true, any scheduled Panopto session in the horizon with no ExternalId
    /// is treated as non–S+ ("alien") and deleted before reconciliation.
    /// </summary>
    public bool EnableAlienPurge { get; set; } = true;
}
