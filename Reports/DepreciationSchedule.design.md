# DEPRECIATION SCHEDULE report design

Create `DepreciationSchedule.trdp` with Telerik Report Designer 2026 Q2
(`20.1.26.707`) and save it in this folder.

- Visible title: `DEPRECIATION SCHEDULE`.
- Hidden string parameter: `dep_run_no`.
- Named connection: `TwinBeeErpReporting`.
- Data source: stored procedure `dbo.usp_Depreciation_ReportData`.
- Parameter mapping: `@dep_run_no = Parameters.dep_run_no.Value`.
- Header: company identity, run number, status, period, run date, filters,
  convention, journal and reversal references.
- Detail: asset code and description, category, branch, method, cost, residual,
  useful life, opening/current/closing depreciation, NBV, and posting accounts.
- Footer: asset count, financial totals, prepared/submitted/approved/posted
  users, page number, and print timestamp.
- Add a `REVERSED` watermark when the header status is `REVERSED`.

Do not embed credentials in the report. The core module remains buildable until
the licensed designer artifact is installed.
