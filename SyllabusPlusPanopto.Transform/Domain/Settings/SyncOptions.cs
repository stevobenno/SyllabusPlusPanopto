namespace SyllabusPlusPanopto.Integration.Domain.Settings;

public sealed class SyncOptions
{
   
    public int? MinExpectedRows { get; set; }

    public bool AllowDeletions { get; set; } = true;

  
    public int DeleteHorizonDays { get; set; } = 7;

  
    public int LookbackDays { get; set; } = 7;
}
