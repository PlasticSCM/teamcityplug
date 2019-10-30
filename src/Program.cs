using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

using log4net;
using log4net.Config;

using TeamCityPlug.Configuration;

namespace TeamCityPlug
{
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                PlugArguments plugArgs = new PlugArguments(args);

                bool bValidArgs = plugArgs.Parse();

                ConfigureLogging(plugArgs.BotName);

                mLog.InfoFormat("TeamCityPlug [{0}] started. Version [{1}]",
                    plugArgs.BotName,
                    System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);

                string argsStr = args == null ? string.Empty : string.Join(" ", args);
                mLog.DebugFormat("Args: [{0}]. Are valid args?: [{1}]", argsStr, bValidArgs);

                if (!bValidArgs || plugArgs.ShowUsage)
                {
                    PrintUsage();
                    return 0;
                }

                CheckArguments(plugArgs);

                Config config = ReadConfigFromFile(plugArgs.ConfigFilePath);

                string authToken = HttpClientBuilder.GetAuthToken(
                    config.User, config.Password);

                using (HttpClient httpClient = HttpClientBuilder.Build(
                    config.Url, authToken))
                {
                    httpClient.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/xml"));

                    CheckConnection(httpClient, config.Url);

                    LaunchTeamCityPlug(plugArgs.WebSocketUrl, httpClient,
                        plugArgs.BotName, plugArgs.ApiKey);
                }

                return 0;

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                mLog.ErrorFormat("Error: {0}", ex.Message);
                mLog.DebugFormat("StackTrace: {0}", ex.StackTrace);
                return 1;
            }
        }

        static void LaunchTeamCityPlug(string serverUrl, HttpClient httpClient,
            string plugName, string apiKey)
        {
            WebSocketClient ws = new WebSocketClient(
                serverUrl,
                "ciPlug",
                plugName,
                apiKey,
                new WebSocketRequest(httpClient).ProcessMessage);

            ws.ConnectWithRetries();

            Task.Delay(-1).Wait();
        }

        static void CheckConnection(HttpClient httpClient, string url)
        {
            if (TeamCityBuild.CheckConnection(httpClient))
                return;

            mLog.ErrorFormat(
                "Unable to contact teamcity server [{0}] " +
                "with specified credentials in configuration.", url);
        }

        static void ConfigureLogging(string plugName)
        {
            if (string.IsNullOrEmpty(plugName))
                plugName = DateTime.Now.ToString("yyyy_MM_dd_HH_mm");

            try
            {
                string log4netpath = LogConfig.GetLogConfigFile();
                log4net.GlobalContext.Properties["Name"] = plugName;
                XmlConfigurator.Configure(new FileInfo(log4netpath));
            }
            catch
            {
                //it failed configuring the logging info; nothing to do.
            }
        }

        static void CheckArguments(PlugArguments plugArgs)
        {
            CheckAgumentIsNotEmpty(
                "Plastic web socket url endpoint",
                plugArgs.WebSocketUrl,
                "web socket url",
                "--server wss://blackmore:7111/plug");

            CheckAgumentIsNotEmpty("name for this bot", plugArgs.BotName, "name", "--name teamcity");
            CheckAgumentIsNotEmpty("connection API key", plugArgs.ApiKey, "api key",
                "--apikey 014B6147A6391E9F4F9AE67501ED690DC2D814FECBA0C1687D016575D4673EE3");
            CheckAgumentIsNotEmpty("JSON config file", plugArgs.ConfigFilePath, "file path",
                "--config teamcity-config.conf");
        }

        static Config ReadConfigFromFile(string file)
        {
            try
            {
                string fileContent = System.IO.File.ReadAllText(file);
                Config result = Newtonsoft.Json.JsonConvert.DeserializeObject<Config>(fileContent);

                if (result == null)
                    throw new Exception(string.Format(
                        "Config file {0} is not valid", file));

                CheckFieldIsNotEmpty("serverUrl", result.Url);
                CheckFieldIsNotEmpty("user", result.User);
                CheckFieldIsNotEmpty("password", result.Password);

                return result;
            }
            catch (Exception e)
            {
                throw new Exception("The config cannot be loaded. Error: " + e.Message);
            }
        }

        static void CheckAgumentIsNotEmpty(
            string fielName, string fieldValue, string type, string example)
        {
            if (!string.IsNullOrEmpty(fieldValue))
                return;
            string message = string.Format("teamcityplug can't start without specifying a {0}.{1}" +
                "Please type a valid {2}. Example:  \"{3}\"",
                fielName, Environment.NewLine, type, example);
            throw new Exception(message);
        }

        static void CheckFieldIsNotEmpty(string fieldName, string fieldValue)
        {
            if (!string.IsNullOrEmpty(fieldValue))
                return;

            throw BuildFieldNotDefinedException(fieldName);
        }

        static Exception BuildFieldNotDefinedException(string fieldName)
        {
            throw new Exception(string.Format(
                "The field '{0}' must be defined in the config", fieldName));
        }

        static void PrintUsage()
        {
            Console.WriteLine("Usage:");
            Console.WriteLine("\tteamcityplug.exe --server <WEB_SOCKET_URL> --config <JSON_CONFIG_FILE_PATH>");
            Console.WriteLine("\t                 --apikey <WEB_SOCKET_CONN_KEY> --name <PLUG_NAME>");
            Console.WriteLine();
            Console.WriteLine("Example:");
            Console.WriteLine("\tteamcityplug.exe --server wss://blackmore:7111/plug --config teamcity-config.conf ");
            Console.WriteLine("\t                 --apikey x2fjk28fda --name teamcity");
            Console.WriteLine();

        }

        static class HttpClientBuilder
        {
            internal static string GetAuthToken(string user, string password)
            {
                return Convert.ToBase64String(
                    System.Text.ASCIIEncoding.ASCII.GetBytes(user + ":" + password));
            }

            internal static HttpClient Build(string host, string authHeader)
            {
                HttpClient httpClient = new HttpClient();
                httpClient.BaseAddress = new Uri(host);

                httpClient.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", authHeader);

                httpClient.DefaultRequestHeaders.ConnectionClose = false;
                return httpClient;
            }
        }

        static readonly ILog mLog = LogManager.GetLogger("teamcityplug");
    }
}
