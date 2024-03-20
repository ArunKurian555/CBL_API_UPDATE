using Crestron.SimplSharp;
using Crestron.SimplSharp.Net;
using Crestron.SimplSharp.WebScripting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Serialization;



namespace CBL2
{

    public class Config
    {
        public static Configuration Setting;  //Static?  YES!  This makes sure we have only ONE as a static member meaning it can exist only once.

        private HttpCwsRoute myRoute;
        private HttpCwsServer myServer;

              public Config(string Filename)
        {
            Setting = new Configuration();                              // Create an instance of our Configuration class that will
            Cws("app", "settings");
        }

        public Configuration getSetting()
        {
            return Setting;
        }
        public void putSetting(Configuration newSetting)
        {
            Setting = newSetting;
        }


        public void Cws(string path, string route)
        {
            myServer = new HttpCwsServer("/" + path);
            myRoute = new HttpCwsRoute(route + "/{REQUEST}");
            myRoute.RouteHandler = new ConfigRequestHandler();
            myServer.AddRoute(myRoute);
            if (myServer.Register())
            {
                CrestronConsole.PrintLine("API registered");
            }
        }
        public class ConfigRequestHandler : IHttpCwsHandler
        {

            private HttpCwsContext _context;
            public void ProcessRequest(HttpCwsContext context)
            {
                _context = context;

                var requestMethod = _context.Request.HttpMethod;
                var requestContents = "";

                using (var myreader = new Crestron.SimplSharp.CrestronIO.StreamReader(_context.Request.InputStream))
                {
                    requestContents = myreader.ReadToEnd().Trim();
                }
                switch (requestMethod)
                {
                    case "GET":
                        {
                            if (_context.Request.RouteData.Values.ContainsKey("REQUEST"))
                            {
                                if (_context.Request.RouteData.Values["REQUEST"].ToString().ToLower() == "levels")
                                {
                                    string response = JsonConvert.SerializeObject(Config.Setting.zoneLevels);
                                    GenerateResponseHeader();
                                    _context.Response.Write(response, true);
                                }
                                else if (_context.Request.RouteData.Values["REQUEST"].ToString().ToLower() == "names")
                                {
                                    string response = JsonConvert.SerializeObject(Config.Setting.zoneNames);
                                    GenerateResponseHeader();
                                    _context.Response.Write(response, true);
                                }
                                else if (_context.Request.RouteData.Values["REQUEST"].ToString().ToLower() == "all")
                                {
                                    string response = JsonConvert.SerializeObject(Config.Setting);
                                    GenerateResponseHeader();
                                    _context.Response.Write(response, true);
                                }
                                else
                                {
                                    _context.Response.Write("Error in the request", true);
                                }
                            }
                            return;
                        }
                    case "POST":
                        {
                           
                            if (_context.Request.RouteData.Values.ContainsKey("REQUEST"))
                            {
                                if (_context.Request.RouteData.Values["REQUEST"].ToString().ToLower() == "levels")
                                {
                                    using (var myreader =
                                           new Crestron.SimplSharp.CrestronIO.StreamReader(_context.Request.InputStream))
                                    {
                                        // We only want to update the endpoint list so we have to reference it properly as a List<NVX>
                                        // This will overwrite the whole list, not append to it.

                                        Config.Setting.zoneLevels = JsonConvert.DeserializeObject<ushort[]>(requestContents);
                                    }
                                    _context.Response.Write("OK", true);
                                }
                                else if (_context.Request.RouteData.Values["REQUEST"].ToString().ToLower() == "names")
                                {
                                    using (var myreader =
                                           new Crestron.SimplSharp.CrestronIO.StreamReader(_context.Request.InputStream))
                                    {
                                        Config.Setting.zoneNames = JsonConvert.DeserializeObject<string[]>(requestContents);
                                    }
                                    _context.Response.Write("Endpoint OK", true);
                                }

                                else if (_context.Request.RouteData.Values["REQUEST"].ToString().ToLower() == "all")
                                {
                                    using (var myreader =
                                           new Crestron.SimplSharp.CrestronIO.StreamReader(_context.Request.InputStream))
                                    {
                                        Config.Setting = JsonConvert.DeserializeObject<Configuration>(requestContents);
                                    }
                                    _context.Response.Write("Endpoint OK", true);
                                }

                                else
                                {
                                    _context.Response.Write($"ERROR: {_context.Request.RouteData.Values["REQUEST"].ToString()} is unrecognized", true);
                                }
                            }

                            GenerateResponseHeader();
                            return;
                        }
                }
            }

            private void GenerateResponseHeader()
            {
                _context.Response.StatusCode = 200;
                _context.Response.ContentType = "text/plain";
            }


        }


        public class Configuration
        {
            public ushort[] zoneLevels = new ushort[300];
            public string[] zoneNames = new string[300];

            public Configuration()
            {
                for (int i = 0; i < zoneNames.Length; i++)
                {
                    zoneNames[i] = "Default Zone Name";
                }
                for (int i = 0; i < zoneLevels.Length; i++)
                {
                    zoneLevels[i] = 100;
                }
            }
        }






    }
}
