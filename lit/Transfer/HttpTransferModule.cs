using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Newtonsoft.Json;

namespace lit
{
    class HttpTransferModule : ITransferModule
    {
        public const string ConnectionRequest = "subscribe";
        public const string StatusRequest = "getstatus";
        public const string StatusReport = "status";

        private List<string> connections = new List<string>();
        private IDictionary<string, string> myRecord;

        private readonly HttpListener myListener = new HttpListener();

        public List<string> Prefixes { get; set; }

        public HttpTransferModule(IConfiguration configuration)
        {
            if (!HttpListener.IsSupported)
            {
                throw new NotSupportedException("Needs Windows XP SP2, Server 2003 or later.");
            }

            // URI prefixes are required, for example 
            // "http://localhost:8080/index/".
            if (string.IsNullOrEmpty(configuration.Transfer.Prefix))
            {
                throw new ArgumentException("Prefix");
            }
            Console.WriteLine("using prefix {0}", configuration.Transfer.Prefix);
            myListener.Prefixes.Add(configuration.Transfer.Prefix);

        }

        public void Start()
        {
            myListener.Start();
            ThreadPool.QueueUserWorkItem((o) =>
            {
                Console.WriteLine("Webserver is running...");
                try
                {
                    while (myListener.IsListening)
                    {
                        ThreadPool.QueueUserWorkItem((c) =>
                        {
                            var ctx = c as HttpListenerContext;
                            try
                            {
                                var simpleResponse = Response(ctx.Request);
                                var buf = Encoding.UTF8.GetBytes(simpleResponse.Content);
                                ctx.Response.ContentLength64 = buf.Length;
                                ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                ctx.Response.StatusCode = (int)simpleResponse.StatusCode;
                            }
                            catch { } // suppress any exceptions
                            finally
                            {
                                // always close the stream
                                ctx.Response.OutputStream.Close();
                            }
                        }, myListener.GetContext());
                    }
                }
                catch { } // suppress any exceptions
            });
        }

        public void ReceiveChanges(IDictionary<string, string> record)
        {
            myRecord = record;
            Console.WriteLine(string.Join(", ",
                new List<string>() { "TimeStamp", "Build", "Assembly", "TC", "Status" }.Select(f => record.ContainsKey(f) ? record[f] : "")));
        }

        public void Stop()
        {
            myListener.Stop();
            myListener.Close();
        }

        public void Dispose()
        {
            Stop();
        }

        private HttpSimpleResponse Response(HttpListenerRequest httpRequest)
        {
            if (null == httpRequest)
            {
                return new HttpSimpleResponse(HttpStatusCode.BadRequest, "null");
            }
            var request = httpRequest.Url.LocalPath.Trim('/');
            var client = httpRequest.RemoteEndPoint.Address.ToString();
            Console.WriteLine("http {0} request received from {1}: {2}", httpRequest.HttpMethod, client, request);
            switch (request)
            {
                case ConnectionRequest:
                    if (connections.All(c => c != client))
                    {
                        connections.Add(client);
                        Console.WriteLine("connection request accepted from {0}", client);
                    }
                    return new HttpSimpleResponse(HttpStatusCode.OK, RecordAsJson);
                case StatusRequest:
                    return new HttpSimpleResponse(HttpStatusCode.OK, RecordAsJson);
                default:
                    return new HttpSimpleResponse(HttpStatusCode.BadRequest, "he?!");
            }
        }

        private string RecordAsJson
        {
            get
            {
                return JsonConvert.SerializeObject(myRecord, typeof(Dictionary<string, string>), Formatting.None,
                    new JsonSerializerSettings()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        DefaultValueHandling = DefaultValueHandling.Ignore,
                    });
            }
        }
    }
}
