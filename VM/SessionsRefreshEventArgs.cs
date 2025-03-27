using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.VM
{
    /// <summary>
    /// Event arguments for handling a 'sessions must be refreshed event' (which is triggered when the viewmodel
    /// detects a state change that necessitates a refresh of the session grid in the main UI.
    /// At present it has no parameters, but in the future we might allow it to contain attributes
    /// to specify a more, or less, comprehensive refresh of the UI,
    /// </summary>
    public class SessionsRefreshEventArgs : EventArgs
    {
        public SessionsRefreshEventArgs() : base()
        {

        }

    }
}
