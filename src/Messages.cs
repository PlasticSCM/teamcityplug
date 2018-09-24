using System.Collections.Generic;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TeamCityPlug
{
    static class Messages
    {
        internal static string BuildRegisterPlugMessage(string name, string type)
        {
            JObject obj = new JObject(
                new JProperty("action", "register"),
                new JProperty("type", type),
                new JProperty("name", name));

            return obj.ToString();
        }

        internal static string BuildLoginMessage(string token)
        {
            JObject obj = new JObject(
                new JProperty("action", "login"),
                new JProperty("key", token));

            return obj.ToString();
        }

        internal static string GetActionType(string message)
        {
            return ReadProperty(message, "action").ToLower();
        }

        internal static LaunchPlanMessage ReadLaunchPlanMessage(string message)
        {
            return JsonConvert.DeserializeObject<LaunchPlanMessage>(message);
        }

        internal static GetStatusMessage ReadGetStatusMessage(string message)
        {
            return JsonConvert.DeserializeObject<GetStatusMessage>(message);
        }

        internal static string BuildLaunchPlanResponse(string requestId, string buildId)
        {
            return new JObject(
                new JProperty("requestId", requestId),
                new JProperty("value", buildId)).ToString();
        }

        internal static string BuildGetStatusResponse(
            string requestId, bool isFinished, bool succeeded, string explanation)
        {
            return new JObject(
                new JProperty("requestId", requestId),
                new JProperty("isFinished", isFinished),
                new JProperty("succeeded", succeeded),
                new JProperty("explanation", explanation)).ToString();
        }

        internal static string BuildWrongGetStatusResponse(string requestId)
        {
            return BuildGetStatusResponse(requestId, true, false, "unknown id");
        }

        internal static string BuildErrorResponse(string requestId, string message)
        {
            return new JObject(
                new JProperty("requestId", requestId),
                new JProperty("error", message)).ToString();
        }

        internal static string GetRequestId(string message)
        {
            return ReadProperty(message, "requestId");
        }

        static string ReadProperty(string message, string name)
        {
            try
            {
                return JObject.Parse(message).Value<string>(name);
            }
            catch
            {
                return string.Empty;
            }
        }
    }

    public class LaunchPlanMessage
    {
        public string PlanName;
        public string ObjectSpec;
        public string Comment;
        public Dictionary<string, string> Properties;
    }

    public class GetStatusMessage
    {
        public string PlanName;
        public string ExecutionId;
    }
}
