# ReceivingReport.trdp design

Create `ReceivingReport.trdp` with the licensed Telerik Report Designer 2026 Q2
(`20.1.26.707`) and save it beside this file.

- Page: A4 portrait. Use landscape only if the final item columns do not fit legibly.
- Hidden string report parameter: `rr_no`.
- SqlDataSource connection: named connection `TwinBeeErpReporting`.
- Select command: stored procedure `dbo.usp_Receiving_ReportData`.
- Parameter mapping: `@rr_no = Parameters.rr_no.Value`.
- Header: company identity, report title, RR number, RR date, status, type, branch,
  warehouse, supplier, reference, user, and remarks.
- Detail table: sequence, PO number, item code, description, received quantity,
  PO U/M, base U/M, conversion, unit price, and line amount.
- Footer: line count, total quantity, total amount, signature blocks, page numbering,
  and print timestamp.
- Add a conditional `REVERSED` watermark when `Fields.Status = "R"`.

Do not place database credentials in the TRDP. The API emits a clear 503 report-status
response until the licensed `ReceivingReport.trdp` file is installed.
