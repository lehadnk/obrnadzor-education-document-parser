using Newtonsoft.Json;

namespace console.Dto
{
    public class ReportDownloadTask
    {
        [JsonProperty("Фамилия")]
        public string lastName;
        [JsonProperty("Номер бланка")]
        public string documentNumber;
        [JsonProperty("Серия бланка")]
        public string documentSeries;
        [JsonProperty("Регистрационный номер")]
        public string registrationNumber;
        [JsonProperty("Дата выдачи")]
        public string issuedAt;
        [JsonProperty("ОГРН")]
        public string organizationOgrn;
        [JsonProperty("Название организации")]
        public string organizationName;
    }
}