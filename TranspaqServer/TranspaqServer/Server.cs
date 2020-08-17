using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;

namespace Transpaq
{
    class Server
    {
        private const int PORT_NO = 5001;
        private const string SERVER_IP = "127.0.0.1";

        private const string TRANSPAQ_RENDEZVOUS = "/paq/rendezvous/";
        
        private HttpListener listener;

        public Server()
        {
            this.Initialise();
        }

        private void Initialise()
        {
            this.listener = new HttpListener();

            listener.Prefixes.Add($"http://{Server.SERVER_IP}:{Server.PORT_NO}{Server.TRANSPAQ_RENDEZVOUS}");
        }

        internal void Start()
        {
            this.listener.Start();
            while (true)
            {
                HttpListenerContext context = listener.GetContext();
                HttpListenerRequest request = context.Request;
                HttpListenerResponse response = context.Response;
                try
                {
                    string input = null;
                    using (StreamReader reader = new StreamReader(request.InputStream))
                    {
                        input = reader.ReadToEnd();
                    }

                    NameValueCollection collection = HttpUtility.ParseQueryString(input);
                    
                    switch (request.RawUrl)
                    {
                        case TRANSPAQ_RENDEZVOUS:
                            break;
                    }
                    
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes("Done");
                    response.ContentLength64 = buffer.Length;

                    response.StatusCode = (int)HttpStatusCode.OK;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch (Exception ex)
                {
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(ex.Message);
                    response.ContentLength64 = buffer.Length;

                    Stream output = response.OutputStream;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }

            this.listener.Stop();
        }
    }
}