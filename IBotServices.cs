using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.AI.QnA;

namespace PythonStackBot
{
   
        public interface IBotServices
        {
            QnAMaker QnAMakerService { get; }
            QnAMaker QnAMakerService1 { get; }
           // LuisRecognizer luisRecognizer { get; }
    }
    
}
