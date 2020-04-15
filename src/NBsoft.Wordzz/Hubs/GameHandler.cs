using NBsoft.Wordzz.Contracts;
using System.Collections.Generic;

namespace NBsoft.Wordzz.Hubs
{
    public static class GameHandler
    {   
        public static Dictionary<string, IGame> ActiveGames = new Dictionary<string, IGame>();
    }
}
