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
            }

            if (btnCancel != null)
            {
                btnCancel.BackgroundColor = Colors.Transparent;
                btnCancel.TextColor = Colors.Black;
                btnCancel.HorizontalOptions = LayoutOptions.Start;
                btnCancel.VerticalOptions = LayoutOptions.End;
                btnCancel.FontAttributes = FontAttributes.Bold;
            }
            

            if(buttons!=null)
            {
                foreach(Button b in buttons)
                {
                    b.BackgroundColor = Colors.Transparent;
                    b.TextColor = Colors.Black;
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
                btnCancel.VerticalOptions = LayoutOptions.End;
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
#endif
        }
    }
}
