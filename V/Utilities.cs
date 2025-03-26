using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LobsterConnect.V
{
    public static class Utilities
    {
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
