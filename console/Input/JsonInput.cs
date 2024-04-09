using System.Collections.Generic;
using System.IO;
using console.Dto;
using Newtonsoft.Json;

namespace console.Input
{
    public class JsonInput
    {
        public List<ReportDownloadTask> readJsonFile(string filePath)
        {
            using (StreamReader r = new StreamReader(filePath))
            {
                string json = r.ReadToEnd();
                return JsonConvert.DeserializeObject<List<ReportDownloadTask>>(json);
            }
        }
    }
}