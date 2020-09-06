using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PythonStackBot.Utilities;

namespace PythonStackBot.Dialogs.Operations
{
    public class QnADialog : ComponentDialog
    {

        public const float DefaultThreshold = 0.5F;
        public const int DefaultTopN = 25;
        public int i = 0;
        public  string Question;
        public const string DefaultNoAnswer = "No QnAMaker answers found.";
        protected readonly IConfiguration Configuration;
        private readonly IBotServices _services;
        //private const string Message = "Humm... Searching...😊";
        public QnADialog(IConfiguration configuration, IBotServices services) : base(nameof(QnADialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                QuestionsStepAsync,
                CallGenerateAnswerAsync,
                QuestionConfirmStepAsync,
                SummaryStepAsync,
                //DisplayQnAResult,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            // The initial child Dialog to run.
            _services = services ?? throw new ArgumentNullException(nameof(services));
            InitialDialogId = nameof(WaterfallDialog);
            //Step = nameof(CallGenerateAnswerAsync);

        }

        private async Task<DialogTurnResult> QuestionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter your programming question.")
            }, cancellationToken);
        }
        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var qnaMakerOptions = new QnAMakerOptions
            {
                ScoreThreshold = DefaultThreshold,
                Top = DefaultTopN,
            };
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
            await stepContext.Context.SendActivitiesAsync(
                    new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 5000 },
                    },
                    cancellationToken);
            //stepContext.Values["Question"] = (string)stepContext.Result;
            var response = await _services.QnAMakerService.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            //return await qnaMaker.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            // var response = await _services.GetAnswersRawAsync(stepContext.Context, qnaMakerOptions).ConfigureAwait(false);
            QnAQuestionID ID = new QnAQuestionID();
            if (response != null && response.Length > 0)
            {
                int id = response[0].Id;
                ID.QuestionID = id;
                 await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text(response[i].Answer),
                }, cancellationToken);
                i++;
                return await stepContext.ContinueDialogAsync();
                //QuestionConfirmStepAsync(stepContext, cancellationToken);
                //QuestionConfirmStepAsync(stepContext,cancellationToken);
            }
            else
            {
                //return await stepContext.Context.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("I could not find an answer to your question. Please rephrase it, so that I can better understand it."),
                }, cancellationToken);

            }

        }
        private async Task<DialogTurnResult> QuestionConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Question"] = (string)stepContext.Result;
            QnAData.QuestionPhrase.Add((string)stepContext.Values["Question"]);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Did the answer helpful?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you for your feedback"), cancellationToken);

               // for (int i = 0; i < QnAData.QuestionPhrase.Count; i++)
               // {
               //     await stepContext.Context.SendActivityAsync(MessageFactory.Text(QnAData.QuestionPhrase[i]), cancellationToken);
               // }


               //// await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your programming Answer - " + (string)stepContext.Values["Answer"]), cancellationToken);

               // await stepContext.Context.SendActivityAsync(MessageFactory.Text("I am updating my Knowledge Base."), cancellationToken);

               // await stepContext.Context.SendActivitiesAsync(
               //     new Activity[] {
               // new Activity { Type = ActivityTypes.Typing },
               // new Activity { Type = "delay", Value= 5000 },
               //     },
               //     cancellationToken);

               // var authoringURL = $"https://{Configuration["ResourceName"]}.cognitiveservices.azure.com";

               // // <AuthorizationAuthor>
               // var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(Configuration["Key"]))
               // { Endpoint = authoringURL };
               // // </AuthorizationAuthor>

               // QnAClient.UpdateKB(client, Configuration["KnowledgeBaseId"], (string)stepContext.Values["Answer"]).Wait();
               // QnAClient.PublishKb(client, Configuration["KnowledgeBaseId"]).Wait();

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Done.😇"));

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Request Not Confirmed."));
                //await stepContext.ContinueDialogAsync(CallGenerateAnswerAsync);
                //await stepContext.RepromptDialogAsync(stepContext, CallGenerateAnswerAsync);
                 stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
                return await CallGenerateAnswerAsync(stepContext, cancellationToken);
                //return await stepContext.ReplaceDialogAsync(InitialDialogId, stepContext.Values["Question"], cancellationToken);
                //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }

    }
}
