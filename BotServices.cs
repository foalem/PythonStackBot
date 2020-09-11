using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Extensions.Configuration;

namespace PythonStackBot
{
    public class BotServices : IBotServices
    {
        //private readonly LuisRecognizer luisRecognizer;
        public BotServices(IConfiguration configuration)
        {
            //var luisIsConfigured = !string.IsNullOrEmpty(configuration["LuisAppId"]) && !string.IsNullOrEmpty(configuration["LuisAPIKey"]) && !string.IsNullOrEmpty(configuration["LuisAPIHostName"]);
            //if (luisIsConfigured)
            //{
            //    var luisApplication = new LuisApplication(
            //        configuration["LuisAppId"],
            //        configuration["LuisAPIKey"],
            //        "https://" + configuration["LuisAPIHostName"]);
            //    // Set the recognizer options depending on which endpoint version you want to use.
            //    // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
            //    var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
            //    {
            //        PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
            //        {
            //            IncludeInstanceData = true,
            //        }
            //    };
            //    luisRecognizer = new LuisRecognizer(recognizerOptions);
            //}
                //var luisApplication = new LuisApplication(
                //    configuration["LuisAppId"],
                //    configuration["LuisAPIKey"],
                //   configuration["LuisAPIHostName"]);

                // luisRecognizer = new LuisRecognizer((luisApplication),
                //new LuisPredictionOptions
                //    {
                //        IncludeAllIntents = true,
                //        IncludeInstanceData = true
                //    },
                //    true);
                //var luisApplication = new LuisApplication(
                //        configuration["LuisAppId"],
                //        configuration["LuisAPIKey"],
                //        "https://" + configuration["LuisAPIHostName"]);
                //// Set the recognizer options depending on which endpoint version you want to use.
                //// More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
                //var recognizerOptions = new LuisRecognizerOptionsV3(luisApplication)
                //{
                //    PredictionOptions = new Microsoft.Bot.Builder.AI.LuisV3.LuisPredictionOptions
                //    {
                //        IncludeInstanceData = true,
                //    }
                //};

           //     var luisApplication = new LuisApplication(
           //    configuration["LuisAppId"],
           //    configuration["LuisAPIKey"],
           //   $"https://{configuration["LuisAPIHostName"]}.api.cognitive.microsoft.com");

           //// Set the recognizer options depending on which endpoint version you want to use.
           // // More details can be found in https://docs.microsoft.com/en-gb/azure/cognitive-services/luis/luis-migration-api-v3
           // var recognizerOptions = new LuisRecognizerOptionsV2(luisApplication)
           // {
           //     IncludeAPIResults = true,
           //     PredictionOptions = new LuisPredictionOptions()
           //     {
           //         IncludeAllIntents = true,
           //         IncludeInstanceData = true
           //     }
           // };
            //luisRecognizer = new LuisRecognizer("https://westus.api.cognitive.microsoft.com/luis/v3.0/apps/12cf74a0-3668-4a1c-8e6a-7164b85e85d0?subscription-key=d03aa41c5f194f8382dc5e8c5ffd1bbb");


            //luisRecognizer = new LuisRecognizer(recognizerOptions);

            QnAMakerService = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["KnowledgeBaseId"],
                Host = GetHostname(configuration["Host"]),
                EndpointKey = GetEndpointKey(configuration)
            });
            QnAMakerService1 = new QnAMaker(new QnAMakerEndpoint
            {
                KnowledgeBaseId = configuration["QnAKnowledgeBaseId"],
                Host = GetHostname1(configuration["QnAHost"]),
                EndpointKey = GetEndpointKey1(configuration)
            });
        }

        public QnAMaker QnAMakerService { get; private set; }
        //public LuisRecognizer luisRecognizer { get; private set; }
        public QnAMaker QnAMakerService1 { get; private set; }

        private static string GetHostname(string hostname)
        {
            if (!hostname.StartsWith("https://"))
            {
                hostname = string.Concat("https://", hostname);
            }

            if (!hostname.Contains("/v5.0") && !hostname.EndsWith("/qnamaker"))
            {
                hostname = string.Concat(hostname, "/qnamaker");
            }

            return hostname;
        }
        private static string GetHostname1(string hostname)
        {
            if (!hostname.StartsWith("https://"))
            {
                hostname = string.Concat("https://", hostname);
            }

            if (!hostname.Contains("/v5.0") && !hostname.EndsWith("/qnamaker"))
            {
                hostname = string.Concat(hostname, "/qnamaker");
            }

            return hostname;
        }

        private static string GetEndpointKey(IConfiguration configuration)
        {
            var endpointKey = configuration["EndpointKey"];

            if (string.IsNullOrWhiteSpace(endpointKey))
            {
                // This features sample is copied as is for "azure bot service" default "createbot" template.
                // Post this sample change merged into "azure bot service" template repo, "Azure Bot Service"
                // will make the web app config change to use "QnAEndpointKey".But, the the old "QnAAuthkey"
                // required for backward compact. This is a requirement from docs to keep app setting name
                // consistent with "QnAEndpointKey". This is tracked in Github issue:
                // https://github.com/microsoft/BotBuilder-Samples/issues/2532

                endpointKey = configuration["Key"];
            }

            return endpointKey;

        }
        private static string GetEndpointKey1(IConfiguration configuration)
        {
            var endpointKey = configuration["QnAEndpointKey"];

            if (string.IsNullOrWhiteSpace(endpointKey))
            {
                // This features sample is copied as is for "azure bot service" default "createbot" template.
                // Post this sample change merged into "azure bot service" template repo, "Azure Bot Service"
                // will make the web app config change to use "QnAEndpointKey".But, the the old "QnAAuthkey"
                // required for backward compact. This is a requirement from docs to keep app setting name
                // consistent with "QnAEndpointKey". This is tracked in Github issue:
                // https://github.com/microsoft/BotBuilder-Samples/issues/2532

                endpointKey = configuration["QnAKey"];
            }

            return endpointKey;

        }
    }
}
