using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Entities
{
    public class GameDataModel
    {
        public string Id { get; set; }
        public string BoardId { get; set; }
        public string Language { get; set; }
        public DateTime CreationDate { get; set; }
        public string Player01 { get; set; }
        public string Player01Rack { get; set; }
        public string Player02 { get; set; }
        public string Player02Rack { get; set; }
        public int Status { get; set; }
        public string CurrentPlayer { get; set; }
        public DateTime CurrentStart { get; set; }
        public DateTime? CurrentPauseStart { get; set; }
        public string LetterBag { get; set; }
        public string Winner { get; set; }
        public int? FinishReason { get; set; }
        public int ConsecutivePasses { get; set; }
        public DateTime? FinishDate { get; set; }
        public int P1FinalScore { get; set; }
        public int P2FinalScore { get; set; }        
    }
}
