/*
    Copyright (C) 2025 Turnipsoft Ltd, Jim Chapman

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.
 */

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
