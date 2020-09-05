using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PythonStackBot
{
    public class QnADialogResponseOptions
    {
        public string NoAnswer { get; set; }
        public string ActiveLearningCardTitle { get; set; }
        public string CardNoMatchText { get; set; }
        public string CardNoMatchResponse { get; set; }
    }
}
