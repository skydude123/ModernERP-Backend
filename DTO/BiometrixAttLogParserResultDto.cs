namespace HRIStwinbeeAPI.DTO
{
    public class BiometrixAttLogParserResultDto
    {
        public int RawLineCount { get; set; }
        public int ParserValidLineCount { get; set; }
        public int ParserInvalidLineCount { get; set; }
        public int FilteredOutOfWindowLineCount { get; set; }
        public List<BiometrixParsedPunchDto> Punches { get; set; } = new();
    }
}
