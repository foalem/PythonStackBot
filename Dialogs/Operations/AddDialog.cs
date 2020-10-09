using Microsoft.Azure.CognitiveServices.Knowledge.QnAMaker;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using PythonStackBot.Utilities;
using System.Threading;
using System.Threading.Tasks;

namespace PythonStackBot.Dialogs.Operations
{
    public class AddDialog : ComponentDialog
    {
        protected readonly IConfiguration Configuration;
        public AddDialog(IConfiguration configuration) : base(nameof(AddDialog))
        {
            Configuration = configuration;
            var waterfallSteps = new WaterfallStep[]
            {
                QuestionStepAsync,
                QuestionPhraseStepAsync,
                ActStepAsync,
                AnswerStepAsync,
                ConfirmStepAsync,
                SummaryStepAsync,
            };

            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new QuestionPhraseDialog());

            InitialDialogId = nameof(WaterfallDialog);
        }



        private async Task<DialogTurnResult> QuestionStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("What's your programming question ?")
            }, cancellationToken);
        }
        private async Task<DialogTurnResult> QuestionPhraseStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Question"] = (string)stepContext.Result;
            QnAData.QuestionPhrase.Add((string)stepContext.Values["Question"]);
            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to Add more phrases for your programming question?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                return await stepContext.BeginDialogAsync(nameof(QuestionPhraseDialog), new User(), cancellationToken);
            }
            else
            {
                return await stepContext.NextAsync(stepContext, cancellationToken);
            }
        }

        private async Task<DialogTurnResult> AnswerStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Please enter the answer for your python programming question.")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> ConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            stepContext.Values["Answer"] = (string)stepContext.Result;

            return await stepContext.PromptAsync(nameof(ConfirmPrompt), new PromptOptions
            {
                Prompt = MessageFactory.Text("Would you like to Confirm?")
            }, cancellationToken);
        }

        private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if ((bool)stepContext.Result)
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Here are the details you provided."), cancellationToken);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Python programming questions - "), cancellationToken);

                for (int i = 0; i < QnAData.QuestionPhrase.Count; i++)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text(QnAData.QuestionPhrase[i]), cancellationToken);
                }

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your programming Answer - "), cancellationToken);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text((string)stepContext.Values["Answer"]), cancellationToken);

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please wait while I update my Knowledge Base."), cancellationToken);

                await stepContext.Context.SendActivitiesAsync(
                    new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 5000 },
                    },
                    cancellationToken);

                var authoringURL = $"https://{Configuration["ResourceName"]}.cognitiveservices.azure.com";

                // <AuthorizationAuthor>
                var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(Configuration["Key"]))
                { Endpoint = authoringURL };
                // </AuthorizationAuthor>

                QnAClient.UpdateKB(client, Configuration["KnowledgeBaseId"], (string)stepContext.Values["Answer"]).Wait();
                QnAClient.PublishKb(client, Configuration["KnowledgeBaseId"]).Wait();

                await stepContext.Context.SendActivityAsync(MessageFactory.Text("I have update my Knowledge Base. Thank you for your contribution 😇"));

                return await stepContext.EndDialogAsync(null, cancellationToken);
            }
            else
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Request Not Confirmed."));
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
        }
    }
}
