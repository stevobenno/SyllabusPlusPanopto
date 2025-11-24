namespace SyllabusPlusPanopto.Integration.Domain.Settings;

public sealed class SyncOptions
{
   
    public int? MinExpectedRows { get; set; }

    public bool AllowDeletions { get; set; } = true;

  
    public int SyncHorizonDays { get; set; } = 140; // TODO: Config

  
    public int LookbackDays { get; set; } = 140;// TODO: Config
}
