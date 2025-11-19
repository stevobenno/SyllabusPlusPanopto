CREATE TABLE dbo.SyllabusPlusPanopto_Staging
(
   
    ActivityName    nvarchar(200)  NOT NULL,   -- e.g. "HECS113201_2425/GWK 01 - Exam Revision"
    ModuleCode      nvarchar(50)   NULL,       -- e.g. "HECS113201_2425", "ELU300901_2425"
    ModuleName      nvarchar(200)  NULL,       -- e.g. "Optimising Care"
    ModuleCRN       nvarchar(50)   NULL,       -- e.g. "38641-1", "#SPLUS446B0D"

    StaffName       nvarchar(400)  NULL,       -- STRING_AGG of staff descriptions
                                                 -- e.g. "Sara Montgomery, Helen Finnerty"

    StartDate       date           NULL,       -- from CAST(StartDateTime as date)
    StartTime       varchar(8)     NULL,       -- from FORMAT(..., 't','en-gb') e.g. "9:00", "13:30"
    EndTime         varchar(8)     NULL,       -- same as above

    LocationName    nvarchar(200)  NULL,       -- e.g. "Worsley TR (8.43B)"
    RecorderName    nvarchar(100)  NULL,       -- e.g. "Lyddon_SR1_1_06" or blank

    RecordingFactor int            NULL,       -- ALLACTS.Factor, filtered NOT IN ('3') in source

    StaffUserName   nvarchar(400)  NULL        -- STRING_AGG of usernames, e.g. "smlsm, llchcf"
);
