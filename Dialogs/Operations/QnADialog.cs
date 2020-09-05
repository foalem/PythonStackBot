using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.AI.QnA;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PythonStackBot.Dialogs.Operations
{
    public class QnADialog : ComponentDialog
    {

        public const float DefaultThreshold = 0.5F;
        public const int DefaultTopN = 25;
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
                //DisplayQnAResult,
            };
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            // The initial child Dialog to run.
            _services = services ?? throw new ArgumentNullException(nameof(services));
            InitialDialogId = nameof(WaterfallDialog);
 
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
            var response = await _services.QnAMakerService.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            //return await qnaMaker.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
           // var response = await _services.GetAnswersRawAsync(stepContext.Context, qnaMakerOptions).ConfigureAwait(false);
            if (response != null && response.Length > 0)
            {
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text(response[0].Answer),
                }, cancellationToken);
                //QuestionConfirmStepAsync(stepContext, cancellationToken);
                //QuestionConfirmStepAsync(stepContext,cancellationToken);
            }
            else
            {
                //return await stepContext.Context.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
                return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("No QnA Maker answers were found."),
                }, cancellationToken);

            }

        }
        private async Task<DialogTurnResult> QuestionConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //stepContext.Values["Question"] = (string)stepContext.Result;
            //QnAData.QuestionPhrase.Add((string)stepContext.Values["Question"]);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Did the answer helpful?")
            }, cancellationToken);
        }

    }
}
