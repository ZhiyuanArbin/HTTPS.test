using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;
using System.ComponentModel;
namespace WinFormsApp1
{

    public partial class Form1 : Form
    {
        private Task serverTask;
        private HttpListener listener;
        static Dictionary<string, List<string>> fileStore = new Dictionary<string, List<string>>();
        private int numFiles;
        private int fileSize;
        public Form1()
        {
            InitializeComponent();
        }

        private void serverbtn_Click(object sender, EventArgs e)
        {
            //default values
            numFiles = 1000;
            fileSize = 1024; 
            
            if (int.TryParse(textBoxNumFiles.Text, out numFiles) && int.TryParse(textBoxFileSize.Text, out fileSize))
            {
                fileSize = fileSize * 1024;
                MessageBox.Show($"Number of files: {numFiles}\nFile size: {fileSize} kB");
            }
            serverTask = Task.Run(() => StartServer());
            serverbtn.Enabled = false;
        }
        private void StartServer()
        {
            string prefix = "http://localhost:5000/";
            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            MessageBox.Show("Server started");
            // string consoleappPath = @"C:\Users\Zhiyuan.Y\testing\HTTPS.test\ConsoleApp1\bin\Debug\net7.0\ConsoleApp1.exe";

            // Process.Start(consoleappPath); // Start the console app
            while (true)
            {
                var context = listener.GetContext();
                Task.Run(() => HandleRequest(context));
            }
        }
        private async Task HandleRequest(HttpListenerContext context)
        {
            var request = context.Request;
            var response = context.Response;

            if (request.HttpMethod == "POST" && request.Url.AbsolutePath == "/upload")
            {
                string recipient = request.QueryString["recipient"];
                if (string.IsNullOrEmpty(recipient))
                {
                    response.StatusCode = 400; // Bad Request
                    await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Recipient not specified"));
                    response.Close();
                    return;
                }
               
                using (var ms = new MemoryStream())
                {
                    await request.InputStream.CopyToAsync(ms);
                    var fileData = ms.ToArray();
                    string filePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                    File.WriteAllBytes(filePath, fileData);

                    if (!fileStore.ContainsKey(recipient))
                    {
                        fileStore[recipient] = new List<string>();
                    }
                    fileStore[recipient].Add(filePath);
                }

                response.StatusCode = 200; // OK
                await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("File uploaded"));
                response.Close();
            }
            else if (request.HttpMethod == "GET" && request.Url.AbsolutePath == "/download")
            {
                string recipient = request.QueryString["recipient"];
                if (string.IsNullOrEmpty(recipient) || !fileStore.ContainsKey(recipient) || fileStore[recipient].Count == 0)
                {
                    response.StatusCode = 404; // Not Found
                    await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("No files available for recipient"));
                    response.Close();
                    return;
                }

                string filePath = fileStore[recipient][0];
                fileStore[recipient].RemoveAt(0);

                byte[] fileBytes = File.ReadAllBytes(filePath);
                response.ContentType = "application/octet-stream";
                response.ContentLength64 = fileBytes.Length;
                await response.OutputStream.WriteAsync(fileBytes, 0, fileBytes.Length);
                response.Close();
              
        

                File.Delete(filePath); // Clean up

            }
            else
            {
                response.StatusCode = 404; // Not Found
                await response.OutputStream.WriteAsync(System.Text.Encoding.UTF8.GetBytes("Invalid endpoint"));
                response.Close();
            }
        }

        private void client1btn_Click(object sender, EventArgs e)
        {
            string client1path = @"C:\Users\Zhiyuan.Y\testing\HTTPS.test\ConsoleApp1\bin\Debug\net7.0\Client1.exe";
            Task.Run(() => System.Diagnostics.Process.Start(client1path, $"{numFiles} {fileSize}")); // Start the client1 app
        }

        private void client2btn_Click(object sender, EventArgs e)
        {
            string client2path = @"C:\Users\Zhiyuan.Y\testing\HTTPS.test\Client2\bin\Debug\net7.0\Client2.exe";
            Task.Run(() => System.Diagnostics.Process.Start(client2path, $"{numFiles}")); // Start the client2 app
        }
    }
}
