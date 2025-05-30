﻿/*
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

using System.ComponentModel;
using System.Runtime.CompilerServices;
using LobsterConnect.Model;

namespace LobsterConnect.VM
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify viewmodel implementation.
    /// </summary>
    public abstract class BindableBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Multicast event for property change notifications.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Checks if a property already matches a desired value.  Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] String propertyName = null)
        {
            if (object.Equals(storage, value)) return false;

            storage = value;
            this.OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners.  This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            if(!Model.DispatcherHelper.UIDispatcherHasThreadAccess)
            {
                Logger.LogMessage(Logger.Level.ERROR, "BindableBase.OnPropertyChanged", "Coding bug: the property '" + propertyName + "' can only be changed on the UI thread");
                return;
            }

            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
            {
                try
                {
                    eventHandler(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (Exception )
                {
                    if (propertyName == null)
                        Logger.LogMessage(Logger.Level.ERROR, "BindableBase.OnPropertyChanged", "exception throw when handling change to NULL");
                    else
                        Logger.LogMessage(Logger.Level.ERROR, "BindableBase.OnPropertyChanged","exception throw when handling change to '" + propertyName.ToString() + "'");
                }
            }
        }
    }
}
