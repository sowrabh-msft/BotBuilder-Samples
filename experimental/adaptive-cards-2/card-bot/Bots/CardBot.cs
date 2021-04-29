// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveCards;
using Cache;
using Microsoft.Bot.AdaptiveCards;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Schema;


namespace Microsoft.BotBuilderSamples.Bots
{
    public class CardBot : ActivityHandler
    {
        protected override async Task OnMessageActivityAsync(ITurnContext<IMessageActivity> turnContext, CancellationToken cancellationToken)
        {
            object adaptiveCardData = getAdaptiveCardData(turnContext.Activity.From.Id);
            var adaptiveCardAttachment = new Attachment()
            {
                ContentType = "application/vnd.microsoft.card.adaptive",
                Content = new CardResource(Path.Combine(".", "Cards", "CardOne.json")).AsJObject(adaptiveCardData),
            };

            await turnContext.SendActivityAsync(MessageFactory.Attachment(adaptiveCardAttachment, "Hello " + turnContext.Activity.From.Name + "!"), cancellationToken);
        }

        protected override async Task<InvokeResponse> OnInvokeActivityAsync(ITurnContext<IInvokeActivity> turnContext, CancellationToken cancellationToken)
        {
            if (AdaptiveCardInvokeValidator.IsAdaptiveCardAction(turnContext))
            {
                try
                {
                    AdaptiveCardInvoke request = AdaptiveCardInvokeValidator.ValidateRequest(turnContext);
                    if (request.Action.Verb == "send")
                    {
                        CardDB.updateCardData(request);
                        var responseBody = await ProcessSend(turnContext);
                        return CreateInvokeResponse(HttpStatusCode.OK, responseBody);
                    }
                    else if (request.Action.Verb == "testAgain")
                    {
                        CardDB.updateCardData(request);
                        var responseBody = await ProcessTestAgain(turnContext);
                        return CreateInvokeResponse(HttpStatusCode.OK, responseBody);
                    }
                    else if (request.Action.Verb == "refresh")
                    {
                        var responseBody = await ProcessRefresh(turnContext);
                        return CreateInvokeResponse(HttpStatusCode.OK, responseBody);
                    }
                    else
                    {
                        AdaptiveCardActionException.VerbNotSupported(request.Action.Type);
                    }
                }
                catch (AdaptiveCardActionException e)
                {
                    return CreateInvokeResponse(HttpStatusCode.OK, e.Response);
                }
            }

            return null;
        }

        private Task<AdaptiveCardInvokeResponse> ProcessSend(ITurnContext<IInvokeActivity> turnContext)
        {
            object adaptiveCardData = getAdaptiveCardData(turnContext.Activity.From.Id);
            return Task.FromResult(new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(Path.Combine(".", "Cards", "CardTwo.json")).AsJObject(adaptiveCardData)
            });
        }

        private Task<AdaptiveCardInvokeResponse> ProcessTestAgain(ITurnContext<IInvokeActivity> turnContext)
        {
            object adaptiveCardData = getAdaptiveCardData(turnContext.Activity.From.Id);
            return Task.FromResult(new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(Path.Combine(".", "Cards", "CardOne.json")).AsJObject(adaptiveCardData)
            });
        }

        private Task<AdaptiveCardInvokeResponse> ProcessRefresh(ITurnContext<IInvokeActivity> turnContext)
        {
            object adaptiveCardData = getAdaptiveCardData(turnContext.Activity.From.Id);
            string adaptiveCardFileName = CardDB.getCurrentFileName();
            return Task.FromResult(new AdaptiveCardInvokeResponse()
            {
                StatusCode = 200,
                Type = AdaptiveCard.ContentType,
                Value = new CardResource(Path.Combine(".", "Cards", adaptiveCardFileName)).AsJObject(adaptiveCardData)
            });
        }

        private object getAdaptiveCardData(string userId)
        {
            object CardData = new
            {
                CardId = CardDB.getCardId(),
                Text = CardDB.getCardText(),
                UserId = new string[] { userId },
            };
            return CardData;
        }

        private static InvokeResponse CreateInvokeResponse(HttpStatusCode statusCode, object body = null)
        {
            return new InvokeResponse()
            {
                Status = (int)statusCode,
                Body = body
            };
        }
    }
}
