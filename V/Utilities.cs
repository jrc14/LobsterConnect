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

namespace LobsterConnect.V
{
    /// <summary>
    /// Utilities for doing things with app UI
    /// </summary>
    public static class Utilities
    {
        /// <summary>
        /// Set the visual attributes of popup buttons to make them a bit closer to the right look
        /// and feel for the particular kind of device the app is running on
        /// </summary>
        /// <param name="btnOk">The OK button or null if there is no OK button</param>
        /// <param name="btnCancel">The Cancel button or null if there is no Cancel button</param>
        /// <param name="rdefButtons">The grid row containing the OK and Cancel buttons</param>
        /// <param name="buttons">All the other buttons whose appearance you want to adjust</param>
        public static void StylePopupButtons(Button btnOk, Button btnCancel, RowDefinition rdefButtons, params Button[] buttons)
        {
#if ANDROID
            if (btnOk != null)
            {
                btnOk.BackgroundColor = Colors.Transparent;
                btnOk.TextColor = Colors.Black;
                btnOk.HorizontalOptions = LayoutOptions.End;
                btnOk.VerticalOptions = LayoutOptions.End;
                btnOk.FontAttributes = FontAttributes.Bold;
                btnOk.TextTransform = TextTransform.Uppercase;
            }

            if (btnCancel != null)
            {
                btnCancel.BackgroundColor = Colors.Transparent;
                btnCancel.TextColor = Colors.Black;
                btnCancel.HorizontalOptions = LayoutOptions.Start;
                btnCancel.VerticalOptions = LayoutOptions.End;
                btnCancel.FontAttributes = FontAttributes.Bold;
                btnCancel.TextTransform = TextTransform.Uppercase;
            }
            

            if(buttons!=null)
            {
                foreach(Button b in buttons)
                {
                    b.BackgroundColor = Colors.Transparent;
                    b.TextColor = Colors.Black;
                    b.BorderColor = Colors.Black;
                    b.BorderWidth = 1; 
                }
            }
#elif WINDOWS

            if (btnOk != null)
            {
                btnOk.BackgroundColor = Colors.LightGrey;
                btnOk.TextColor = Colors.Black;
                btnOk.HorizontalOptions = LayoutOptions.Center;
                btnOk.VerticalOptions = LayoutOptions.Start;
            }

            if (btnCancel != null)
            {
                btnCancel.BackgroundColor = Colors.LightGrey;
                btnCancel.TextColor = Colors.Black;
                btnCancel.HorizontalOptions = LayoutOptions.Center;
                btnCancel.VerticalOptions = LayoutOptions.Start;
            }
            

            if(rdefButtons!=null)
                rdefButtons.Height = new GridLength(80);

            if (buttons!=null)
            {
                foreach(Button b in buttons)
                {
                    b.BackgroundColor = Colors.LightGrey;
                    b.TextColor = Colors.Black;
                }
            }
#else // Mac, iPad and iPhone

            if (btnOk != null)
            {
                btnOk.BackgroundColor = Colors.LightGrey;
                btnOk.TextColor = Colors.Black;
                btnOk.HorizontalOptions = LayoutOptions.Center;
                btnOk.VerticalOptions = LayoutOptions.Start;
                btnOk.BorderColor = Colors.DarkGray;
                btnOk.BorderWidth = 1;
            }

            if (btnCancel != null)
            {
                btnCancel.BackgroundColor = Colors.LightGrey;
                btnCancel.TextColor = Colors.Black;
                btnCancel.HorizontalOptions = LayoutOptions.Center;
                btnCancel.VerticalOptions = LayoutOptions.Start;
                btnCancel.BorderColor = Colors.DarkGray;
                btnCancel.BorderWidth = 1;
            }
            

            if(rdefButtons!=null)
                rdefButtons.Height = new GridLength(80);

            if (buttons!=null)
            {
                foreach(Button b in buttons)
                {
                    b.BackgroundColor = Colors.LightGrey;
                    b.TextColor = Colors.Black;
                    b.BorderColor = Colors.DarkGray;
                    b.BorderWidth = 1;
                }
            }

#endif
        }
    }
}
