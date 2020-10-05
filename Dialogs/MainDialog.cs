// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.
//
// Generated with Bot Builder V4 SDK Template for Visual Studio CoreBot v4.9.2

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Luis;
//using Luis;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using PythonStackBot.Dialogs.Operations;

namespace PythonStackBot.Dialogs
{
    public class MainDialog : ComponentDialog
    {
        protected readonly ILogger Logger;
        protected readonly IConfiguration Configuration;
        protected readonly IBotServices Service;
        private readonly LuisRecongnizer _luisRecognizer;
        //private const string InfoMessage = "Here's what I can do to help ... 😊 ";

        // Dependency injection uses this constructor to instantiate MainDialog
        public MainDialog(ILogger<MainDialog> logger, IConfiguration configuration, IBotServices service, LuisRecongnizer luisRecognizer)
            : base(nameof(MainDialog))
        {
            Logger = logger;
            Configuration = configuration;
            Service = service;
            _luisRecognizer = luisRecognizer;
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new AddDialog(configuration));
            AddDialog(new QnADialog(configuration,service));
            AddDialog(new LuisDialog(configuration, luisRecognizer));
            AddDialog(new SmartDialog(configuration, service));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                //IntroStepAsync,
                IntroStepAsync,
                ActStepAsync,
                //LuisStepAsync,
               // FinalStepAsync,
              // FinalStepAsync,
            }));

            // The initial child Dialog to run.
            InitialDialogId = nameof(WaterfallDialog);
        }

        private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {
            await stepContext.Context.SendActivityAsync(
                MessageFactory.Text("Here's what I can do to help ... 😊"), cancellationToken);

            List<string> operationList = new List<string> { "Giving your Python Contribution", "Ask a Question" };
            // Create card
            var card = new AdaptiveCard(new AdaptiveSchemaVersion(1, 0))
            {
                // Use LINQ to turn the choices into submit actions
                Actions = operationList.Select(choice => new AdaptiveSubmitAction
                {
                    Title = choice,
                    Data = choice,  // This will be a string
                }).ToList<AdaptiveAction>(),
            };
            // Prompt
            return await stepContext.PromptAsync(nameof(ChoicePrompt), new PromptOptions
            {
                Prompt = (Activity)MessageFactory.Attachment(new Attachment
                {
                    ContentType = AdaptiveCard.ContentType,
                    // Convert the AdaptiveCard to a JObject
                    Content = JObject.FromObject(card),
                }),
                Choices = ChoiceFactory.ToChoices(operationList),
                // Don't render the choices outside the card
                Style = ListStyle.None,
            },
                cancellationToken);
        }

        private async Task<DialogTurnResult> ActStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        {

            stepContext.Values["Operation"] = ((FoundChoice)stepContext.Result).Value;
            string operation = (string)stepContext.Values["Operation"];

            if (operation.Equals("Giving your Python Contribution"))
            {
                await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please provide following details to add a new python question answer pair."), cancellationToken);
                return await stepContext.BeginDialogAsync(nameof(AddDialog), new User(), cancellationToken);
            }
            else if (operation.Equals("Ask a Question"))
            {
                return await stepContext.BeginDialogAsync(nameof(LuisDialog), new User(), cancellationToken);
                //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                //{
                //    Prompt = MessageFactory.Text("Please you can ask your question.")
                //}, cancellationToken);
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Please you can ask your question."), cancellationToken);


            }
            //else if (operation.Equals("Asking Programming Question about Python"))
            //{
            //    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("What's your programming question ?"), cancellationToken);
            //    return await stepContext.BeginDialogAsync(nameof(QnADialog), new User(), cancellationToken);

            //}
            else
            {

            }
            return await stepContext.EndDialogAsync(null, cancellationToken);
        }
        //private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    if (!_luisRecognizer.IsConfigured)
        //    {
        //        await stepContext.Context.SendActivityAsync(
        //            MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

        //        return await stepContext.NextAsync(null, cancellationToken);
        //    }

        //    // Use the text provided in FinalStepAsync or the default if it is the first time.
        //   // var weekLaterDate = DateTime.Now.AddDays(7).ToString("MMMM d, yyyy");
        //    //var messageText = stepContext.Options?.ToString() ?? $"Please you can ask your question.";
        //    //var promptMessage = MessageFactory.Text(messageText, messageText, InputHints.ExpectingInput);
        //    //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { Prompt = promptMessage }, cancellationToken);
        //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
        //}

        //private async Task<DialogTurnResult> LuisStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //    //    {
        //    //        Prompt = MessageFactory.Text("Please you can ask your question.")
        //    //    }, cancellationToken);

        //    if (!_luisRecognizer.IsConfigured)
        //    {
        //        await stepContext.Context.SendActivityAsync(
        //            MessageFactory.Text("NOTE: LUIS is not configured. To enable all capabilities, add 'LuisAppId', 'LuisAPIKey' and 'LuisAPIHostName' to the appsettings.json file.", inputHint: InputHints.IgnoringInput), cancellationToken);

        //        //return await stepContext.NextAsync(null, cancellationToken);
        //        return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        //    }
        //    var luisResult = await _luisRecognizer.RecognizeAsync<LuisCognitiveHelper>(stepContext.Context, cancellationToken);
        //    switch (luisResult.TopIntent().intent)
        //    {
        //        case LuisCognitiveHelper.Intent.Search:
        //            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
        //             await stepContext.BeginDialogAsync(nameof(QnADialog), new User(), cancellationToken);
        //             break;
        //        case LuisCognitiveHelper.Intent.Joke:
        //            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
        //             await stepContext.BeginDialogAsync(nameof(AddDialog), new User(), cancellationToken);
        //             break;
        //        case LuisCognitiveHelper.Intent.SmallTalk:
        //            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
        //             await stepContext.BeginDialogAsync(nameof(SmartDialog), new User(), cancellationToken);
        //             break;
        //        case LuisCognitiveHelper.Intent.Brain:
        //            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
        //             await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //            {
        //                Prompt = MessageFactory.Text("I'm using Microsoft AI.😊")
        //            }, cancellationToken);
        //             break;
        //        case LuisCognitiveHelper.Intent.None:
        //            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
        //             await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //            {
        //                Prompt = MessageFactory.Text("I don't understang your question. Please rephrase it.")
        //            }, cancellationToken);
        //             break;
        //        default:
        //            //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...humm default😊"), cancellationToken);
        //             await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //            {
        //                Prompt = MessageFactory.Text("Sorry, I didn't get that. Please try asking in a different way")
        //            }, cancellationToken);
        //             break;


        //    }
        //   // return await stepContext.NextAsync(stepContext, cancellationToken);
        //    return await stepContext.ReplaceDialogAsync(InitialDialogId);

        //}

        //private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    //QnAData.QuestionPhrase.Clear();
        //    //var promptMessage = "What else can I do for you?";
        //    //return await stepContext.ReplaceDialogAsync(InitialDialogId, promptMessage, cancellationToken);
        //    //await stepContext.Context.SendActivityAsync(MessageFactory.Text("fd"), cancellationToken);
        //    //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //    //{
        //    //    Prompt = MessageFactory.Text("You can ask other question.")
        //    //}, cancellationToken);
        //    //new DialogTurnResult(DialogTurnStatus.Waiting);
        //    //if (stepContext.Context.Activity.Type == ActivityTypes.Message)
        //    //{ 
        //    //    stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
        //    //}
        //    return await stepContext.ReplaceDialogAsync(InitialDialogId,cancellationToken);
        //}

        //private async Task<DialogTurnResult> FinalStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //   // await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);

        //    return await stepContext.ReplaceDialogAsync(InitialDialogId, cancellationToken);
        //}
    }
}
