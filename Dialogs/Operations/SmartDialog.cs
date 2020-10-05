using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;

namespace PythonStackBot.Dialogs.Operations
{
    public class SmartDialog : ComponentDialog
    {
        public const float DefaultThreshold = 0.3F;
        public const int DefaultTopN = 25;
        //public int i = 0;
        public string Question;
        public const string DefaultNoAnswer = "No QnAMaker answers found.";
        protected readonly IConfiguration Configuration;
        private readonly IBotServices _services;

        public SmartDialog(IConfiguration configuration, IBotServices services) : base(nameof(SmartDialog))
        {
            var waterfallSteps = new WaterfallStep[]
            {
                //QuestionsStepAsync,
                CallGenerateAnswerAsync,
                //QuestionConfirmStepAsync,
                //SummaryStepAsync,
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
        //private async Task<DialogTurnResult> QuestionsStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //    {
        //        Prompt = MessageFactory.Text("Please enter your programming question.")
        //    }, cancellationToken);
        //}

        private async Task<DialogTurnResult> CallGenerateAnswerAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            var qnaMakerOptions = new QnAMakerOptions
            {
                ScoreThreshold = DefaultThreshold,
                Top = DefaultTopN,
            };
            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
            await stepContext.Context.SendActivitiesAsync(
                    new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 5000 },
                    },
                    cancellationToken);
            //stepContext.Values["Question"] = (string)stepContext.Result;
            var response = await _services.QnAMakerService1.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            //return await qnaMaker.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            // var response = await _services.GetAnswersRawAsync(stepContext.Context, qnaMakerOptions).ConfigureAwait(false);
            //QnAQuestionID ID = new QnAQuestionID();
            if (response != null && response.Length > 0)
            {
                //int id = response[0].Id;
                //ID.QuestionID = id;
                 await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text(response[0].Answer),
                }, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
                //i++;
                //return await stepContext.ContinueDialogAsync();
                //QuestionConfirmStepAsync(stepContext, cancellationToken);
                //QuestionConfirmStepAsync(stepContext,cancellationToken);
            }
            else
            {
                //return await stepContext.Context.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                 await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("I don't know how to response."),
                }, cancellationToken);

                return await stepContext.EndDialogAsync(null, cancellationToken);

            }

        }
    }
}
