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
            btnOk.BackgroundColor = Colors.Transparent;
            btnCancel.BackgroundColor = Colors.Transparent;
            btnOk.TextColor = Colors.Black;
            btnCancel.TextColor = Colors.Black;
            btnCancel.HorizontalOptions = LayoutOptions.Start;
            btnOk.HorizontalOptions = LayoutOptions.End;
            btnCancel.VerticalOptions = LayoutOptions.End;
            btnOk.VerticalOptions = LayoutOptions.End;
            btnCancel.FontAttributes = FontAttributes.Bold;
            btnOk.FontAttributes = FontAttributes.Bold;

            if(buttons!=null)
            {
                foreach(Button b in buttons)
                {
                    b.BackgroundColor = Colors.Transparent;
                    b.TextColor = Colors.Black;
                }
            }
#elif WINDOWS
            btnOk.BackgroundColor = Colors.LightGrey;
            btnCancel.BackgroundColor = Colors.LightGrey;
            btnOk.TextColor = Colors.Black;
            btnCancel.TextColor = Colors.Black;
            rdefButtons.Height = new GridLength(80);
            btnCancel.HorizontalOptions = LayoutOptions.Center;
            btnOk.HorizontalOptions = LayoutOptions.Center;
            btnCancel.VerticalOptions = LayoutOptions.Center;
            btnOk.VerticalOptions = LayoutOptions.Center;

            if(buttons!=null)
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
