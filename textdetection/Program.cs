using System;
using System.IO;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace GoogleCloudSamples
{
    public class TextDetection
    {
        static string ApplicationId = "HackatonAbby";
        static string Password = "77DKgcN+f4fAcvIcuwB6JjbN";


        static string FilePath { get; set; }

        static protected ICredentials Credentials
        {
            get
            {
                if (_credentials == null)
                {
                    _credentials = string.IsNullOrEmpty(ApplicationId) || string.IsNullOrEmpty(Password)
                        ? CredentialCache.DefaultNetworkCredentials
                        : new NetworkCredential(ApplicationId, Password);
                }
                return _credentials;
            }
        }

        static protected IWebProxy Proxy
        {
            get
            {
                if (_proxy == null)
                {
                    _proxy = HttpWebRequest.DefaultWebProxy;
                    _proxy.Credentials = CredentialCache.DefaultCredentials;
                }
                return _proxy;
            }
        }

        public String photo2string(string filePath)
        {
            string language = "Russian";
            string exportFormat = "txt";

            var url = string.Format("http://cloud.ocrsdk.com/processImage?language={0}&exportFormat={1}", language, exportFormat);
            var request = CreateRequest(url, "POST", Credentials, Proxy);
            FillRequestWithContent(request, filePath);

            var response = GetResponse(request);
            var taskId = GetTaskId(response);

            url = string.Format("http://cloud.ocrsdk.com/getTaskStatus?taskId={0}", taskId);
            Console.WriteLine(taskId);
            var resultUrl = string.Empty;
            var status = string.Empty;
            while (status != "Completed")
            {
                System.Threading.Thread.Sleep(1000);
                request = CreateRequest(url, "GET", Credentials, Proxy);
                response = GetResponse(request);
                status = GetStatus(response);
                resultUrl = GetResultUrl(response);
                Console.WriteLine(status);
            }

            string html = null;
            request = (HttpWebRequest)HttpWebRequest.Create(resultUrl);
            using (var responses = request.GetResponse())
            {
                using (var streams = responses.GetResponseStream())
                using (var readers = new StreamReader(streams))
                {
                    html = readers.ReadToEnd();
                }
            }
            return html;
        }

        protected static HttpWebRequest CreateRequest(string url, string method, ICredentials credentials, IWebProxy proxy)
        {
            var request = (HttpWebRequest)HttpWebRequest.Create(url);
            request.ContentType = "application/octet-stream";
            request.Credentials = credentials;
            request.Method = method;
            request.Proxy = proxy;
            return request;
        }

        protected static void FillRequestWithContent(HttpWebRequest request, string contentPath)
        {
            using (BinaryReader reader = new BinaryReader(File.OpenRead(contentPath)))
            {
                request.ContentLength = reader.BaseStream.Length;
                using (Stream stream = request.GetRequestStream())
                {
                    byte[] buffer = new byte[reader.BaseStream.Length];
                    while (true)
                    {
                        int bytesRead = reader.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0)
                        {
                            break;
                        }
                        stream.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }

        protected static XDocument GetResponse(HttpWebRequest request)
        {
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                using (Stream stream = response.GetResponseStream())
                {
                    return XDocument.Load(new XmlTextReader(stream));
                }
            }
        }

        protected static string GetTaskId(XDocument doc)
        {
            var id = string.Empty;
            var task = doc.Root.Element("task");
            if (task != null)
            {
                id = task.Attribute("id").Value;
            }
            return id;
        }

        protected static string GetStatus(XDocument doc)
        {
            var status = string.Empty;
            var task = doc.Root.Element("task");
            if (task != null)
            {
                status = task.Attribute("status").Value;
            }
            return status;
        }

        protected static string GetResultUrl(XDocument doc)
        {
            var resultUrl = string.Empty;
            var task = doc.Root.Element("task");
            if (task != null)
            {
                resultUrl = task.Attribute("resultUrl") != null ? task.Attribute("resultUrl").Value : string.Empty;
            }
            return resultUrl;
        }

        protected static string GetExtension(string exportFormat)
        {
            return "txt";
        }

        static private IWebProxy _proxy;
        static private ICredentials _credentials;

        public static void Main(string[] args)
        {
            ApplicationId = "HackatonAbby";
            Password = "77DKgcN+f4fAcvIcuwB6JjbN";
        }
    }
}
