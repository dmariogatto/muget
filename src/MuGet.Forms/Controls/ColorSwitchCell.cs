using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace MuGet.Forms.Controls
{
    public class ColorSwitchCell : SwitchCell
    {
        public Color TextColor
        {
            get { return (Color)GetValue(TextColorProperty); }
            set { SetValue(TextColorProperty, value); }
        }

        public static readonly BindableProperty TextColorProperty =
          BindableProperty.Create(propertyName: nameof(TextColor),
              returnType: typeof(Color),
              declaringType: typeof(ColorSwitchCell),
              defaultValue: Color.Black);
    }
}
