using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

using log4net;


namespace TeamCityPlug
{
    internal static class TeamCityBuild
    {
        internal static bool CheckConnection(HttpClient httpClient)
        {
            HttpResponseMessage response = null;
            try
            {
                response = httpClient.GetAsync(QUEUE_BUILD_URI).Result;
            }
            catch (Exception ex)
            {
                LogException(ex);
                return false;
            }

            return response.IsSuccessStatusCode;
        }

        internal static async Task<string> QueueBuildAsync(
            string projectPlanKey,
            string plasticUpdateToSpec,
            string comments,
            Dictionary<string, string> botRequestProperties,
            HttpClient httpClient)
        {
            TeamCityBuildConfig tcBuildConf = new TeamCityBuildConfig();
            tcBuildConf.buildType.id = projectPlanKey;
            tcBuildConf.comment.text = comments;

            BuildProperty switchToSpecProperty = new BuildProperty();
            switchToSpecProperty.name = PLASTIC_PROPERTY_UPDATE_SPEC;
            switchToSpecProperty.value = plasticUpdateToSpec;

            List<BuildProperty> payloadProperties = new List<BuildProperty>();
            payloadProperties.Add(switchToSpecProperty);

            AddPropertiesToTeamcityRequest(payloadProperties, botRequestProperties);

            tcBuildConf.properties = payloadProperties.ToArray();

            var payLoad = new StringContent(tcBuildConf.SerializeToXml(), Encoding.UTF8, "application/xml");
            payLoad.Headers.Add("Origin", httpClient.BaseAddress.AbsoluteUri);

            HttpResponseMessage response = await httpClient.PostAsync(QUEUE_BUILD_URI, payLoad);

            if (!response.IsSuccessStatusCode)
                return string.Empty;

            string responseStr = await response.Content.ReadAsStringAsync();

            return LoadBuildId(responseStr);
        }

        internal static async Task<BuildStatus> QueryStatusAsync(
            string buildNumberId, HttpClient httpClient)
        {
            string endPoint = QUEUE_BUILD_URI + "/" + buildNumberId;
            HttpResponseMessage response = await httpClient.GetAsync(endPoint);

            if (!response.IsSuccessStatusCode)
                return null;

            string responseStr = await response.Content.ReadAsStringAsync();

            return LoadBuildStatus(responseStr);
        }

        static string LoadBuildId(string responseStr)
        {
            if (string.IsNullOrEmpty(responseStr))
                return string.Empty;

            XmlDocument xmlOutput = new XmlDocument();
            xmlOutput.LoadXml(responseStr);

            return GetBuildAttribute(xmlOutput, "id");
        }

        static BuildStatus LoadBuildStatus(string responseStr)
        {
            XmlDocument xmlOutput = new XmlDocument();
            xmlOutput.LoadXml(responseStr);

            BuildStatus buildStatus = new BuildStatus();
            buildStatus.Progress = GetBuildAttribute(xmlOutput, "state");
            buildStatus.BuildResult = GetBuildAttribute(xmlOutput, "status");
            return buildStatus;
        }

        static string GetBuildAttribute(XmlDocument xmlOutput, string attrName)
        {
            if (xmlOutput == null)
                return string.Empty;

            XmlNode buildNode = xmlOutput.SelectSingleNode("/build");

            if (buildNode == null)
                return string.Empty;

            XmlAttribute attr = buildNode.Attributes[attrName];

            if (attr == null)
                return string.Empty;

            return attr.Value;
        }

        static void AddPropertiesToTeamcityRequest(
            List<BuildProperty> payloadProperties,
            Dictionary<string, string> botRequestProperties)
        {
            if (botRequestProperties == null || botRequestProperties.Count == 0)
                return;

            BuildProperty botProperty = null;

            foreach (string key in botRequestProperties.Keys)
            {
                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(botRequestProperties[key]))
                    continue;

                botProperty = new BuildProperty();
                botProperty.name = BOT_BUILD_PROPERTY_PREFIX + key;
                botProperty.value = botRequestProperties[key];

                payloadProperties.Add(botProperty);
            }
        }

        internal static void LogException(Exception ex)
        {
            string exceptionErrorMsg = GetErrorMessage(ex);
            string innerExceptionErrorMsg = GetErrorMessage(ex == null ? null : ex.InnerException);

            bool bHasInnerEx = !string.IsNullOrEmpty(innerExceptionErrorMsg);

            mLog.ErrorFormat("{0}{1}{2}{3}",
                exceptionErrorMsg,
                bHasInnerEx ? " - [" : string.Empty,
                innerExceptionErrorMsg,
                bHasInnerEx ? "]" : string.Empty);

            mLog.Debug(ex.StackTrace);
        }

        static string GetErrorMessage(Exception ex)
        {
            return ex == null || string.IsNullOrEmpty(ex.Message) ? string.Empty : ex.Message;
        }

        const string QUEUE_BUILD_URI = "httpAuth/app/rest/buildQueue";
        const string BOT_BUILD_PROPERTY_PREFIX = "plasticscm.mergebot.";
        const string PLASTIC_PROPERTY_UPDATE_SPEC = BOT_BUILD_PROPERTY_PREFIX + "update.spec";

        static readonly ILog mLog = LogManager.GetLogger("teamcityplug");
    }
}
