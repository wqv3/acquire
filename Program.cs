using System;
using System.IO;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;

class Program
{
    static void Main(string[] args)
    {
        string operationalFolder = Path.Combine(Directory.GetCurrentDirectory(), "operational");

        if (!Directory.Exists(operationalFolder))
        {
            Console.WriteLine("casting operational folder");
            Directory.CreateDirectory(operationalFolder);
        }

        string outputFolder = Path.Combine(operationalFolder, "output");

        if (!Directory.Exists(outputFolder))
        {
            Directory.CreateDirectory(outputFolder);
        }

        Console.WriteLine("path to .har or just simply drag it here:");
        string harPath = Console.ReadLine();

        if (harPath.StartsWith("\"") && harPath.EndsWith("\""))
        {
            harPath = harPath.Substring(1, harPath.Length - 2);
        }

        if (!File.Exists(harPath) || Path.GetExtension(harPath).ToLower() != ".har")
        {
            Console.WriteLine("tf is this");
            return;
        }

        string fileName = Path.GetFileNameWithoutExtension(harPath);
        string dateTime = DateTime.Now.ToString("HH-mm dd.MM.yyyy");
        string outputDir = Path.Combine(outputFolder, $"{dateTime} {fileName}");

        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        Console.WriteLine($"acquiring: {harPath}");

        var harJson = File.ReadAllText(harPath);
        var harData = JObject.Parse(harJson);

        var entries = harData["log"]["entries"];
        int index = 1;

        // attach
        var morphmeData = new JObject();
        var requestsArray = new JArray();

        foreach (var entry in entries)
        {
            string url = entry["request"]["url"].ToString();
            string method = entry["request"]["method"].ToString();
            string status = entry["response"]["status"].ToString();
            long size = (long)entry["response"]["bodySize"];

            string folderName = status == "200"
                ? $"[{method}] [{status}] request_{index}"
                : $"[{method}] request_{index}";

            string folderPath = Path.Combine(outputDir, folderName);

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            string requestHeaders = FormatHeaders(entry["request"]["headers"]);
            File.WriteAllText(Path.Combine(folderPath, "request_headers.txt"), requestHeaders);

            string responseHeaders = FormatHeaders(entry["response"]["headers"]);
            File.WriteAllText(Path.Combine(folderPath, "response_headers.txt"), responseHeaders);

            string infoText = $"URL: {url}\nResource size: {size} bytes";
            File.WriteAllText(Path.Combine(folderPath, "info.txt"), infoText);

            Console.WriteLine($"acquired request {index}: {method} {url}, status: {status}, size: {size} bytes");

            // jsoning
            var requestObject = new JObject();
            requestObject[$"request_{index}"] = new JObject
            {
                { "url-to-morph", url },
                { "headers-to-morph", FormatHeadersAsObject(entry["request"]["headers"]) }
            };

            requestsArray.Add(requestObject);

            index++;
        }

        // morphing
        morphmeData["requests"] = requestsArray;
        string jsonFilePath = Path.Combine(outputDir, $"{fileName}.json");
        File.WriteAllText(jsonFilePath, morphmeData.ToString(Formatting.Indented));

        Console.WriteLine($"acquiring of {fileName} completed.");
        Console.WriteLine("probably done!");
    }

    static string FormatHeaders(JToken headers)
    {
        string result = "";
        foreach (var header in headers)
        {
            result += $"{header["name"]}: {header["value"]}\n";
        }
        return result;
    }

    static JObject FormatHeadersAsObject(JToken headers)
    {
        JObject headersObject = new JObject();
        foreach (var header in headers)
        {
            headersObject[header["name"].ToString()] = header["value"].ToString();
        }
        return headersObject;
    }
}
