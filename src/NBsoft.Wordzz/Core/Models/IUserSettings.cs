using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NBsoft.Wordzz.Core.Models
{
    interface IUserSettings
    {
        string UserName { get; }
        string MainSettings { get; }
        string WindowsSettings { get; }
        string AndroidSettings { get; }
        string IOSSettings { get; }
    }
}
