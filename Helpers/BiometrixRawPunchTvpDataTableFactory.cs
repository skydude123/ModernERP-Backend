using HRIStwinbeeAPI.DTO;
using System.Data;

namespace HRIStwinbeeAPI.Helpers
{
    public class BiometrixRawPunchTvpDataTableFactory
    {
        public static DataTable Create(IEnumerable<BiometrixParsedPunchDto> punches)
        {
            var table = new DataTable();
            table.Columns.Add("SequenceNo", typeof(int));
            table.Columns.Add("EmployeeIdNo", typeof(string));
            table.Columns.Add("PunchDateTime", typeof(DateTime));
            table.Columns.Add("RawColumn3", typeof(string));
            table.Columns.Add("RawColumn4", typeof(string));
            table.Columns.Add("RawColumn5", typeof(string));
            table.Columns.Add("RawColumn6", typeof(string));
            table.Columns.Add("RawLine", typeof(string));
            table.Columns.Add("IsParserValid", typeof(bool));
            table.Columns.Add("ParserError", typeof(string));

            foreach (var punch in punches)
            {
                var row = table.NewRow();
                row["SequenceNo"] = punch.SequenceNo;
                row["EmployeeIdNo"] = (object?)punch.EmployeeIdNo ?? DBNull.Value;
                row["PunchDateTime"] = punch.PunchDateTime.HasValue ? punch.PunchDateTime.Value : DBNull.Value;
                row["RawColumn3"] = (object?)punch.RawColumn3 ?? DBNull.Value;
                row["RawColumn4"] = (object?)punch.RawColumn4 ?? DBNull.Value;
                row["RawColumn5"] = (object?)punch.RawColumn5 ?? DBNull.Value;
                row["RawColumn6"] = (object?)punch.RawColumn6 ?? DBNull.Value;
                row["RawLine"] = punch.RawLine;
                row["IsParserValid"] = punch.IsParserValid;
                row["ParserError"] = (object?)punch.ParserError ?? DBNull.Value;
                table.Rows.Add(row);
            }

            return table;
        }
    }
}
