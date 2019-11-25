using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace BasicWebServer
{
    public class Server
    {
        private HttpListener listener;

        private Router router;

        public static int maxSimultaneousConnections = 20;
        
        private static Semaphore sem = new Semaphore(maxSimultaneousConnections, maxSimultaneousConnections);

        public void Start()
        {
            this.router = new Router(GetWebsitePath());
            InitializeListener();
            this.listener.Start();
            Task.Run(() => RunServer(listener));
        }

        private List<IPAddress> GetLocalHostIPs()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork).ToList();
        }

        private void InitializeListener()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost/");

            GetLocalHostIPs().ForEach(ip =>
            {
                Console.WriteLine("Listening on IP " + "http://" + ip.ToString() + "/");
                listener.Prefixes.Add("http://" + ip.ToString() + "/");
            });

            this.listener = listener;
        }

        private void RunServer(HttpListener listener)
        {
            while (true)
            {
                sem.WaitOne();
                StartConnectionListener(listener);
            }
        }

        private async void StartConnectionListener(HttpListener listener)
        {
            var context = await listener.GetContextAsync();
            sem.Release();
            Log(context.Request);
            var request = context.Request;
            var path = request.RawUrl;
            var method = request.HttpMethod;
            //var parms = request.RawUrl.RightOf("?");
            //var kvParams = GetKeyValues(parms);
            //router.Route(verb, path, kvParams);
            var response = router.Route(request.HttpMethod, path, new Dictionary<string, string>());
            context.Response.ContentLength64 = response.Data.Length;
            context.Response.OutputStream.Write(response.Data, 0, response.Data.Length);
            context.Response.OutputStream.Close();
        }

        private void Log(HttpListenerRequest request)
        {
            Console.WriteLine(request.RemoteEndPoint + " " + request.HttpMethod + " /" + request.Url.AbsoluteUri);
        }

        private string GetWebsitePath()
        {
            return ConfigurationManager.AppSettings.Get("websiteRoot");
        }
    }
}
