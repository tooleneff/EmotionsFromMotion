using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EmotionsFromMotion.Model
{
    public class FileWithFace
    {
        public string Path { get; set; }
        public double SecondsFromBegining { get; set; }
        public Guid FaceID { get; set; }
    }
}
