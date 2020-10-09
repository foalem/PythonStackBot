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
using Microsoft.Bot.Builder.Dialogs.Choices;
using Microsoft.Bot.Schema;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PythonStackBot.Utilities;

namespace PythonStackBot.Dialogs.Operations
{
    public class QnADialog : ComponentDialog
    {

        public const float DefaultThreshold = 0.5F;
        public const int DefaultTopN = 5;
        //public int i = 0;
        public  string Question;
        public const string DefaultNoAnswer = "No QnAMaker answers found.";
        protected readonly IConfiguration Configuration;
        private readonly IBotServices _services;
        //private const string Message = "Humm... Searching...😊";
        public QnADialog(IConfiguration configuration, IBotServices services) : base(nameof(QnADialog))
        {
            //var waterfallSteps = new WaterfallStep[]
            //{
            //    //QuestionsStepAsync,
            //    CallGenerateAnswerAsync,
            //    IntroStepAsync,
            //    QuestionConfirmStepAsync,
            //    //SummaryStepAsync,
            //    //DisplayQnAResult,
            //};
            //AddDialog(new WaterfallDialog(nameof(WaterfallDialog), waterfallSteps));
            AddDialog(new TextPrompt(nameof(TextPrompt)));
            AddDialog(new ConfirmPrompt(nameof(ConfirmPrompt)));
            AddDialog(new NumberPrompt<int>(nameof(NumberPrompt<int>)));
            AddDialog(new ChoicePrompt(nameof(ChoicePrompt)));
            AddDialog(new WaterfallDialog(nameof(WaterfallDialog), new WaterfallStep[]
            {
                CallGenerateAnswerAsync,
                //IntroStepAsync,
                //QuestionConfirmStepAsync,
            }));
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
            //stepContext.Values["Question1"] = (string)stepContext.Result;
            //stepContext.Values["question"] = (string)stepContext.Result;

            var qnaMakerOptions = new QnAMakerOptions
            {
                ScoreThreshold = DefaultThreshold,
                Top = DefaultTopN,
            };
            await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
            await stepContext.Context.SendActivitiesAsync(
                    new Activity[] {
                new Activity { Type = ActivityTypes.Typing },
                new Activity { Type = "delay", Value= 7000 },
                    },
                    cancellationToken);
            //stepContext.Values["Question"] = (string)stepContext.Result;
            var response = await _services.QnAMakerService.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            
            //if ( i < 0.5)
            //{
            //    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry 😞,No QnAMaker answers found for your programming question."), cancellationToken);
            //}

            if (response != null && response.Length > 0)
            {
                float i = response[0].Score;
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("No QnAMaker answers found for your programming question."), cancellationToken);
                if (response.Length >= 1 && i >= 0.9)
                {
                    //for (int j = 0; j < response.Length; j++)
                    //{
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I found the following results from your question"), cancellationToken);
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(response[0].Answer),
                    }, cancellationToken);
                    // }

                }
                 if (response.Length == 1 && i <= 0.9 && i >= 0.5)
                {
                    await stepContext.Context.SendActivityAsync(MessageFactory.Text("I found the following results from your question"), cancellationToken);
                    await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                    {
                        Prompt = MessageFactory.Text(response[0].Answer),
                    }, cancellationToken);
                }
                 if (response.Length > 1 && i > 0.5)
                {
                    var options = response.Select(r => r.Questions[0]).ToList();
                    var herocard = new HeroCard();
                    herocard.Text = "Did you mean:";
                    List<CardAction> buttons = new List<CardAction>();

                    foreach (var item in options)
                    {
                        buttons.Add(new CardAction()
                        {
                            Type = ActionTypes.ImBack,
                            Title = item.ToString(),
                            Value = item.ToString()
                        });
                    }
                    buttons.Add(new CardAction()
                    {
                        Type = ActionTypes.ImBack,
                        Title = "None of the above.",
                        Value = "Cancel."
                    });

                    herocard.Buttons = buttons;
                    var response1 = stepContext.Context.Activity.CreateReply();
                    response1.Attachments = new List<Attachment>() { herocard.ToAttachment() };
                    await stepContext.Context.SendActivityAsync(response1);
                }

                return await stepContext.EndDialogAsync(null, cancellationToken);

            }
            else
            {
                //await stepContext.Context.SendActivityAsync(MessageFactory.Text("Sorry 😞,No QnAMaker answers found for your programming question."), cancellationToken);
                await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
                {
                    Prompt = MessageFactory.Text("Sorry 😞,No QnAMaker answers found for your programming question."),
                }, cancellationToken);
                return await stepContext.EndDialogAsync(null, cancellationToken);
            }



            //switch (response.Length)
            //{
            //    case 0:
            //        await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            //        {
            //            Prompt = MessageFactory.Text("No QnAMaker answers found."),
            //        }, cancellationToken);
            //        break;
            //    case 1:
            //            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            //            {
            //                Prompt = MessageFactory.Text(response[0].Answer),
            //            }, cancellationToken);
            //        break;
            //    //case 2:
            //    //    for (int i = 0; i < response.Length; i++)
            //    //    {
            //    //        await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            //    //        {
            //    //            Prompt = MessageFactory.Text(response[i].Answer),
            //    //        }, cancellationToken);
            //    //    }
            //    //    break;
            //    default:
            //        var options = response.Select(r => r.Questions[0]).ToList();
            //        var herocard = new HeroCard();
            //        herocard.Text = "Did you mean:";
            //        List<CardAction> buttons = new List<CardAction>();

            //        foreach (var item in options)
            //        {
            //            buttons.Add(new CardAction()
            //            {
            //                Type = ActionTypes.ImBack,
            //                Title = item.ToString(),
            //                Value = item.ToString()
            //            });
            //        }
            //        buttons.Add(new CardAction()
            //        {
            //            Type = ActionTypes.ImBack,
            //            Title = "None of the above.",
            //            Value = "None of the above."
            //        });

            //        herocard.Buttons = buttons;
            //        var response1 = stepContext.Context.Activity.CreateReply();
            //        response1.Attachments = new List<Attachment>() { herocard.ToAttachment() };
            //        await stepContext.Context.SendActivityAsync(response1);
            //        break;
            //}


            //return await qnaMaker.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
            // var response = await _services.GetAnswersRawAsync(stepContext.Context, qnaMakerOptions).ConfigureAwait(false);
            //if (response != null && response.Length > 0)
            //{
            //int i = 0;
            //int id = response[0].Id;
            //ID.QuestionID = id;
            //do
            //{
            //        await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text(response[i].Answer),
            //    }, cancellationToken);
            //    //i=i+1;

            //        i++;

            //} while (i < response.Length);

            //await QuestionConfirmStepAsync(stepContext, cancellationToken);
            
                //await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
                //return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
                //QuestionConfirmStepAsync(stepContext, cancellationToken);
                //QuestionConfirmStepAsync(stepContext,cancellationToken);
            //}
            //else
            //{
            //    //return await stepContext.Context.SendActivityAsync(MessageFactory.Text("No QnA Maker answers were found."), cancellationToken);
            //     await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
            //    {
            //        Prompt = MessageFactory.Text("I could not find an answer to your question. Please rephrase your question, so that I can better understand it."),
            //    }, cancellationToken);

            //    //return await stepContext.ContinueDialogAsync();
            //    return await stepContext.EndDialogAsync(null, cancellationToken);

            //}

        }
        //private async Task<DialogTurnResult> IntroStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{

        //    return await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions { }, cancellationToken);
        //}
        //private async Task<DialogTurnResult> QuestionConfirmStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    var qnaMakerOptions = new QnAMakerOptions
        //    {
        //        ScoreThreshold = DefaultThreshold,
        //        Top = DefaultTopN,
        //    };
        //    await stepContext.Context.SendActivityAsync(MessageFactory.Text("Humm... Searching...😊"), cancellationToken);
        //    await stepContext.Context.SendActivitiesAsync(
        //            new Activity[] {
        //        new Activity { Type = ActivityTypes.Typing },
        //        new Activity { Type = "delay", Value= 5000 },
        //            },
        //            cancellationToken);
        //    //stepContext.Values["Question"] = (string)stepContext.Result;
        //    var response = await _services.QnAMakerService.GetAnswersAsync(stepContext.Context, qnaMakerOptions);
        //    if (response != null && response.Length > 0)
        //    {
        //        int i = 0;
        //        int id = response[0].Id;
        //       // ID.QuestionID = id;
        //        do
        //        {
        //            await stepContext.PromptAsync(nameof(TextPrompt), new PromptOptions
        //            {
        //                Prompt = MessageFactory.Text(response[i].Answer),
        //            }, cancellationToken);
        //            //i=i+1;

        //            i++;

        //        } while (i < response.Length);

        //        //await QuestionConfirmStepAsync(stepContext, cancellationToken);
        //        //return await stepContext.ContinueDialogAsync();
        //        return await stepContext.EndDialogAsync(null, cancellationToken);
        //        //QuestionConfirmStepAsync(stepContext, cancellationToken);
        //        //QuestionConfirmStepAsync(stepContext,cancellationToken);
        //    }
        //    return await stepContext.EndDialogAsync(null, cancellationToken);
        //}

        //private async Task<DialogTurnResult> SummaryStepAsync(WaterfallStepContext stepContext, CancellationToken cancellationToken)
        //{
        //    stepContext.Values["question"] = ((FoundChoice)stepContext.Result).Value;
        //    if ((string)stepContext.Values["question"] == "Not helpfull👎")
        //    {
        //        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Thank you for your feedback"), cancellationToken);

        //        // for (int i = 0; i < QnAData.QuestionPhrase.Count; i++)
        //        // {
        //        //     await stepContext.Context.SendActivityAsync(MessageFactory.Text(QnAData.QuestionPhrase[i]), cancellationToken);
        //        // }


        //        //// await stepContext.Context.SendActivityAsync(MessageFactory.Text("Your programming Answer - " + (string)stepContext.Values["Answer"]), cancellationToken);

        //        // await stepContext.Context.SendActivityAsync(MessageFactory.Text("I am updating my Knowledge Base."), cancellationToken);

        //        // await stepContext.Context.SendActivitiesAsync(
        //        //     new Activity[] {
        //        // new Activity { Type = ActivityTypes.Typing },
        //        // new Activity { Type = "delay", Value= 5000 },
        //        //     },
        //        //     cancellationToken);

        //        // var authoringURL = $"https://{Configuration["ResourceName"]}.cognitiveservices.azure.com";

        //        // // <AuthorizationAuthor>
        //        // var client = new QnAMakerClient(new ApiKeyServiceClientCredentials(Configuration["Key"]))
        //        // { Endpoint = authoringURL };
        //        // // </AuthorizationAuthor>

        //        // QnAClient.UpdateKB(client, Configuration["KnowledgeBaseId"], (string)stepContext.Values["Answer"]).Wait();
        //        // QnAClient.PublishKb(client, Configuration["KnowledgeBaseId"]).Wait();

        //        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Done.😇"));

        //        return await stepContext.EndDialogAsync(null, cancellationToken);
        //    }
        //    else
        //    {
        //        await stepContext.Context.SendActivityAsync(MessageFactory.Text("Request Not Confirmed."));
        //        //await stepContext.ContinueDialogAsync(CallGenerateAnswerAsync);
        //        //await stepContext.RepromptDialogAsync(stepContext, CallGenerateAnswerAsync);
        //        // stepContext.ActiveDialog.State["stepIndex"] = (int)stepContext.ActiveDialog.State["stepIndex"] - 2;
        //        //return await CallGenerateAnswerAsync(stepContext, cancellationToken);
        //        return await stepContext.ReplaceDialogAsync(InitialDialogId, stepContext, cancellationToken);
        //        //return await stepContext.EndDialogAsync(cancellationToken: cancellationToken);
        //    }
        //}

    }
}
