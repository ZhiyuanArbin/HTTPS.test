
using System;
using System.IO;
using System.Net.Http;
using System.Diagnostics;
using System.Threading.Tasks;



namespace Client1 // Client1
{
    class Program
    {
        static async Task Main(string[] args)
        {

            string uploadPath = Path.GetTempPath();

            string uploadUrl = $"http://localhost:5000/upload?recipient=client2";
            int numFiles = int.Parse(args[0]);
            int fileSize = int.Parse(args[1]);
            
            byte[][] fileContent = new byte[numFiles][];

            using (HttpClient client = new HttpClient())
            {
                for (int i = 0; i < numFiles; i++)
                {
                    string filePath = Path.Combine(uploadPath, $"dummy{i}.bin");
                    fileContent[i] = new byte[fileSize]; // 1 MB dummy file
                    new Random().NextBytes(fileContent[i]); // Fill with random data

                    File.WriteAllBytes(filePath, fileContent[i]);

                }
                Stopwatch stopwatch = new Stopwatch();
                stopwatch.Start();
                string startTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"); 
                for (int i = 0; i < numFiles; i++) 
                { 
                    using (var content = new MultipartFormDataContent())
                    {
                        string filePath = Path.Combine(uploadPath, $"dummy{i}.bin");
                        var fileContentBytes = new ByteArrayContent(fileContent[i]);
                        content.Add(fileContentBytes, "file", Path.GetFileName(filePath));

                        var response = await client.PostAsync(uploadUrl, content);
                        string result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(result);
                    }
                }
                stopwatch.Stop();
               
                long totalBytes = numFiles * fileSize; 
                double totalMB = totalBytes / (1024.0 * 1024.0);
                double transferRate = totalMB / stopwatch.Elapsed.TotalSeconds;
                Console.WriteLine($"Total transfer rate: {transferRate} MB/s");
                Console.WriteLine($"Files uploaded: {numFiles} files");
                Console.WriteLine($"File size: {fileSize} bytes");

                //File.AppendAllText("log.txt", $"Total transfer rate: {transferRate} MB/s\n");
                Console.WriteLine(); 
                Console.WriteLine($"Client1 started at {startTime}");
                Console.WriteLine($"Time taken for upload: {stopwatch.Elapsed.TotalSeconds} seconds");
                File.AppendAllText("log.txt", $"Client1 started at {startTime}\n");
            }
            Console.WriteLine("Press [Enter] to exit the program.");
            Console.ReadLine();
        }

    }
}