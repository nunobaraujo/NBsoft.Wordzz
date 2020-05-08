using System;

namespace NBsoft.Wordzz.Entities
{
    public class GameMoveDataModel
    {
        public uint Id { get; set; }
        public string GameId { get; set; }
        public string PlayerId { get; set; }
        public DateTime PlayStart { get; set; }
        public DateTime? PlayFinish { get; set; }
        public int Score { get; set; }

        public string Letters { get; set; }
        public string Words { get; set; }
        
    }
}
