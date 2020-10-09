using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder;
using System.Threading;
//using System.Threading.Tasks;
using Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PythonStackBot.Dialogs.Operations
{
    public class LuisDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;
        protected readonly IBotServices Service;
        private readonly LuisRecongnizer _luisRecognizer;


        public LuisDialog(IConfiguration configuration, LuisRecongnizer luisRecognizer)
            : base(nameof(LuisDialog))
        {
            //Logger = logger;
            Configuration = configuration;
            //Service = service;
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                //IntroStepAsync,
                IntroStepAsync,
                //ActStepAsync,
                LuisStepAsync,
                // FinalStepAsync,
                // FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }
        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                return await stepContext.NextAsync(null, cancellationToken);
            }

            // Use the text provided in FinalStepAsync or the default if it is the first time.
            // var weekLaterDate = DateTime.Now.AddDays(7).ToString("MMMM d, yyyy");
            //var messageText = stepContext.Options?.ToString() ?? $"Please you can ask your question.";
            //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
            //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
            return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
        }

        private async Task<DialogTurnResult> LuisStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text("Please you can ask your question.")
            //    }, cancellationToken);

            if (!_luisRecognizer.IsConfigured)
            {
                await stepContext.Context.SendActivityAsync(
                    MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

                //return await stepContext.NextAsync(null, cancellationToken);
                return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
            }
            var luisResult = await _luisRecognizer.RecognizeAsync<LuisCognitiveHelper>(stepContext.Context, cancellationToken);
            switch (luisResult.TopIntent().intent)
            {
                case LuisCognitiveHelper.Intent.Search:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
                    await stepContext.BeginDialogAsync(nameof(QnADialog), new User(), cancellationToken);
                    break;
                case LuisCognitiveHelper.Intent.SmallTalk:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
                    await stepContext.BeginDialogAsync(nameof(SmartDialog), new User(), cancellationToken);
                    break;
                case LuisCognitiveHelper.Intent.Help:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
                    await stepContext.BeginDialogAsync(nameof(SmartDialog), new User(), cancellationToken);
                    break;
                case LuisCognitiveHelper.Intent.Joke:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
                     return await stepContext.EndDialogAsync();
                   // break;
                case LuisCognitiveHelper.Intent.Brain:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("I'm using Microsoft AI.😊")
                    }, cancellationToken);
                    break;
                case LuisCognitiveHelper.Intent.None:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Sorry😊,this question is not in my area of ​​expertise.")
                    }, cancellationToken);
                    break;
                default:
                    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...humm default😊"), cancellationToken);
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text("Sorry😊,this question is not in my area of ​​expertise.")
                    }, cancellationToken);
                    break;


            }
            // return await stepContext.NextAsync(stepContext, cancellationToken);
            return await stepContext.ReplaceDialogAsync(InitialDialogId);

        }
    }
    


}
