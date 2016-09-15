using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ProjectOxford.Emotion.Contract;

namespace EmotionsFromMotion.Model
{
    public class EmotionAndTime
    {
        public double SecondsFromBegining { get; set; }
        public Guid PersonID { get; set; }
        public string PersonGroupID { get; set; }
        public Emotion EmotionResult { get; set; }
    }
}
