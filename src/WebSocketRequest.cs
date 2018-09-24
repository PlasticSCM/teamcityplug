using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

using log4net;

namespace TeamCityPlug
{
    internal class WebSocketRequest
    {
        internal WebSocketRequest(HttpClient httpClient)
        {
            mHttpClient = httpClient;
        }

        internal async Task<string> ProcessMessage(string message)
        {
            string requestId = Messages.GetRequestId(message);
            string type = string.Empty;
            try
            {
                type = Messages.GetActionType(message);
                switch (type)
                {
                    case "launchplan":
                        return await ProcessLaunchPlanMessage(
                            requestId,
                            Messages.ReadLaunchPlanMessage(message),
                            mHttpClient);

                    case "getstatus":
                        return await ProcessGetStatusMessage(
                            requestId,
                            Messages.ReadGetStatusMessage(message),
                            mHttpClient);

                    default:
                        return Messages.BuildErrorResponse(requestId,
                            string.Format("The action '{0}' is not supported", type));
                }
            }
            catch (Exception ex)
            {
                mLog.ErrorFormat("Error processing message {0}: \nMessage:{1}. Error: {2}",
                    type, message, ex.Message);
                TeamCityBuild.LogException(ex);
                return Messages.BuildErrorResponse(requestId, ex.Message);
            }
        }

        static async Task<string> ProcessLaunchPlanMessage(
            string requestId,
            LaunchPlanMessage message,
            HttpClient httpClient)
        {
            LogLaunchPlanMessage(message);

            string buildId = await TeamCityBuild.QueueBuildAsync(
                message.PlanName,
                message.ObjectSpec,
                message.Comment,
                message.Properties,
                httpClient);

            return Messages.BuildLaunchPlanResponse(requestId, buildId);
        }

        static async Task<string> ProcessGetStatusMessage(
            string requestId,
            GetStatusMessage message,
            HttpClient httpClient)
        {
            LogGetStatusMessage(message);

            BuildStatus status = await TeamCityBuild.QueryStatusAsync(
                message.ExecutionId, httpClient);

            bool bIsFinished;
            bool bIsSuccessful;
            ParseStatus(status, out bIsFinished, out bIsSuccessful);

#warning teamcity API wrapper does not retrieve an explanation yet.
            return Messages.BuildGetStatusResponse(
                requestId, bIsFinished, bIsSuccessful, string.Empty);
        }

        internal static void LogException(Exception exception)
        {
            if (exception.InnerException != null)
                exception = exception.InnerException;

            Console.WriteLine("Unexpected error: {0}", exception.Message);
            Console.WriteLine("Stack trace: {0}", exception.StackTrace);
        }

        static void ParseStatus(BuildStatus status, out bool bIsFinished, out bool bIsSuccessful)
        {
            if (status == null)
            {
                bIsFinished = true;
                bIsSuccessful = false;
                return;
            }

            bIsFinished = status.Progress.Equals(
                FINISHED_BUILD_TAG, StringComparison.InvariantCultureIgnoreCase);

            bIsSuccessful = status.BuildResult.Equals(
                SUCESSFUL_BUILD_TAG, StringComparison.InvariantCultureIgnoreCase);
        }

        static void LogLaunchPlanMessage(LaunchPlanMessage message)
        {
            mLog.Info("Launch plan was requested. Fields:");
            mLog.Info("\tPlanName: " + message.PlanName);
            mLog.Info("\tObjectSpec: " + message.ObjectSpec);
            mLog.Info("\tComment: " + message.Comment);
            mLog.Info("\tProperties:");

            foreach (KeyValuePair<string, string> pair in message.Properties)
                mLog.InfoFormat("\t\t{0}: {1}", pair.Key, pair.Value);
        }

        static void LogGetStatusMessage(GetStatusMessage message)
        {
            mLog.Info("Plan status requested. Fields:");
            mLog.Info("\tPlanName: " + message.PlanName);
            mLog.Info("\tExecutionId: " + message.ExecutionId);
        }

        readonly HttpClient mHttpClient;

        const string FINISHED_BUILD_TAG = "finished";
        const string SUCESSFUL_BUILD_TAG = "SUCCESS";

        static readonly ILog mLog = LogManager.GetLogger("teamcityplug");
    }
}
