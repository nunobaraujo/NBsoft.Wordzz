using NBsoft.Wordzz.Core.Models;
using System;

namespace NBsoft.Wordzz.Core
{
    public delegate void MatchFoundEventDelegate(object sender, MatchFoundEventArgs e);
    public class MatchFoundEventArgs:EventArgs
    {
        public IGameQueue Queue01 { get; set; }
        public IGameQueue Queue02 { get; set; }
    }
}
