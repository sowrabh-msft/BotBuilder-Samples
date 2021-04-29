using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.AdaptiveCards;
using Newtonsoft.Json.Linq;
using System.Runtime.InteropServices;

namespace Cache
{
    public enum CardState
    {
        EnterText,
        DisplayText
    }
    public class CardDB
    {
        private static string cardText = "";
        private static CardState cardState = CardState.EnterText;
        private static string cardId = "";
        public static void updateCardData(AdaptiveCardInvoke request)
        {
            object data = request.Action.Data;
            if (request.Action.Verb == "send")
            {
                cardText = JObject.FromObject(data)["text"]?.ToString();
                cardState = CardState.DisplayText;
            }
            else if (request.Action.Verb == "testAgain")
            {
                cardText = "";
                cardState = CardState.EnterText;
            }
            else
                return;
        }

        public static string getCardText()
        {
            return cardText;
        }

        public static string getCardId()
        {
            if (cardId == "")
                cardId = generateGUID();
            return cardId;
        }

        private static string generateGUID()
        {
            return Marshal.GenerateGuidForType(typeof(string)).ToString();
        }

        public static string getCurrentFileName()
        {
            if (cardState == CardState.EnterText)
                return "CardOne.json";
            return "CardTwo.json";
        }
    }
}
