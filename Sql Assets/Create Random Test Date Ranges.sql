/********************************************************************************************
 Script:      Update StartDate for First 2,000 Rows (No PK / No Identity)
 Author:      Stevie Bennett
 Date:        <insert date here>

 Purpose:
 --------
 This script simulates a rolling two-week schedule window for the Syllabus Plus → Panopto
 test data, without relying on any primary key or identity column.

 Behaviour:
 ----------
 • Assigns a row number to every row in dbo.PanoptoTestData2 using ROW_NUMBER().
 • Updates ONLY the first 2,000 rows (RowNum <= 2000).
 • For each updated row, StartDate is set to today's date plus a random offset:
       StartDate = today + N days, where N is between 1 and 14 (inclusive).
 • Uses NEWID() + CHECKSUM() to generate a different random offset per row.

 Assumptions:
 ------------
 • Table name: dbo.PanoptoTestData2
 • Column: StartDate is of type DATE (or can be implicitly converted to DATE).
 • This is non-production data used for integration testing / experimentation.

********************************************************************************************/

;WITH TargetRows AS
(
    SELECT
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum,
        *
    FROM dbo.PanoptoTestData2
)
UPDATE TargetRows
SET StartDate = DATEADD(
                    day,
                    ABS(CHECKSUM(NEWID())) % 14 + 1,   -- random 1–14
                    CAST(GETDATE() AS date)            -- base = today
                )
WHERE RowNum <= 2000;        -- limit to first 2,000 rows

-- Optional: quick sanity check
SELECT TOP (50)
       RowNum,
       StartDate
FROM
(
    SELECT
        ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS RowNum,
        StartDate
    FROM dbo.PanoptoTestData2
) x
ORDER BY RowNum;
