using HRIStwinbeeAPI.DTO;
using System.Globalization;
using System.Text;

namespace HRIStwinbeeAPI.Parsers
{
    public class BiometrixAttLogParser
    {
        private static readonly string[] SupportedDateFormats =
       {
            "yyyy-MM-dd HH:mm:ss",
            "yyyy/MM/dd HH:mm:ss",
            "MM/dd/yyyy HH:mm:ss",
            "M/d/yyyy H:mm:ss"
        };

        public async Task<BiometrixAttLogParserResultDto> ParseAsync(
            Stream fileStream,
            DateTime dateFrom,
            DateTime dateTo,
            CancellationToken cancellationToken = default)
        {
            var result = new BiometrixAttLogParserResultDto();

            var rangeStart = dateFrom.Date;
            var rangeEndInclusiveWithCrossDay = dateTo.Date.AddDays(1).AddHours(6);

            fileStream.Position = 0;

            using var reader = new StreamReader(
                fileStream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: 4096,
                leaveOpen: true);

            string? line;
            var sequenceNo = 0;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                cancellationToken.ThrowIfCancellationRequested();

                sequenceNo++;
                result.RawLineCount++;

                if (string.IsNullOrWhiteSpace(line))
                {
                    result.ParserInvalidLineCount++;
                    result.Punches.Add(new BiometrixParsedPunchDto
                    {
                        SequenceNo = sequenceNo,
                        RawLine = line,
                        IsParserValid = false,
                        ParserError = "Blank line."
                    });
                    continue;
                }

                var parts = line.Split('\t');

                if (parts.Length < 2)
                {
                    result.ParserInvalidLineCount++;
                    result.Punches.Add(new BiometrixParsedPunchDto
                    {
                        SequenceNo = sequenceNo,
                        RawLine = line,
                        IsParserValid = false,
                        ParserError = "Invalid ATTLOG format. Expected at least 2 tab-separated columns."
                    });
                    continue;
                }

                var employeeIdNo = parts[0].Trim();
                var punchDateTimeText = parts[1].Trim();

                if (string.IsNullOrWhiteSpace(employeeIdNo))
                {
                    result.ParserInvalidLineCount++;
                    result.Punches.Add(new BiometrixParsedPunchDto
                    {
                        SequenceNo = sequenceNo,
                        RawLine = line,
                        IsParserValid = false,
                        ParserError = "Missing IDNO."
                    });
                    continue;
                }

                if (!TryParseDateTime(punchDateTimeText, out var punchDateTime))
                {
                    result.ParserInvalidLineCount++;
                    result.Punches.Add(new BiometrixParsedPunchDto
                    {
                        SequenceNo = sequenceNo,
                        EmployeeIdNo = employeeIdNo,
                        RawLine = line,
                        IsParserValid = false,
                        ParserError = $"Invalid PunchDateTime value: {punchDateTimeText}"
                    });
                    continue;
                }

                if (punchDateTime < rangeStart || punchDateTime > rangeEndInclusiveWithCrossDay)
                {
                    result.FilteredOutOfWindowLineCount++;
                    continue;
                }

                result.ParserValidLineCount++;

                result.Punches.Add(new BiometrixParsedPunchDto
                {
                    SequenceNo = sequenceNo,
                    EmployeeIdNo = employeeIdNo,
                    PunchDateTime = punchDateTime,
                    RawColumn3 = parts.Length > 2 ? NullIfBlank(parts[2]) : null,
                    RawColumn4 = parts.Length > 3 ? NullIfBlank(parts[3]) : null,
                    RawColumn5 = parts.Length > 4 ? NullIfBlank(parts[4]) : null,
                    RawColumn6 = parts.Length > 5 ? NullIfBlank(parts[5]) : null,
                    RawLine = line,
                    IsParserValid = true
                });
            }

            return result;
        }

        private static bool TryParseDateTime(string input, out DateTime parsed)
        {
            if (DateTime.TryParseExact(
                    input,
                    SupportedDateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return true;
            }

            return DateTime.TryParse(
                input,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out parsed);
        }

        private static string? NullIfBlank(string value)
        {
            var trimmed = value.Trim();
            return string.IsNullOrWhiteSpace(trimmed) ? null : trimmed;
        }
    }
}
