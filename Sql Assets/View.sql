USE [MockSyllabusPlus];
GO

/********************************************************************************************
 View:        dbo.vwPanoptoIntegration
 Author:      Stevie Bennett
 Created:     <insert original creation date>
 Modified:    14/11/2025

 Purpose:
 --------
 This view provides a simplified and consumable dataset for the Panopto → Syllabus Plus 
 integration pipeline. It reads from the staging table (PanoptoTestData2) and filters
 events to a two-week rolling window starting from the current date.

 Key Behaviour:
 --------------
 • Returns ONLY rows with StartDate >= today.
 • Returns ONLY rows with StartDate < today + 14 days.
 • Acts as a stable integration surface for the Panopto ingestion process.
 • Ignores all historical or far-future events.

 Notes:
 ------
 • SQL Server views cannot accept parameters, so the 14-day window is hard-coded.
 • If a variable range becomes necessary, consider replacing this with an inline 
   table-valued function (ITVF) that accepts @DaysAhead as a parameter.

********************************************************************************************/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

ALTER VIEW [dbo].[vwPanoptoIntegration]
AS
    SELECT *
    FROM dbo.PanoptoTestData2
    WHERE 
        StartDate >= CAST(GETDATE() AS date)
        AND StartDate < DATEADD(day, 14, CAST(GETDATE() AS date));
GO
