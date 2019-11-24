using System;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MuGet.Forms.Android.Renderers;
using MuGet.Forms.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using Color = Xamarin.Forms.Color;
using View = Android.Views.View;

[assembly: ExportRenderer(typeof(ColorSwitchCell), typeof(ColorSwitchCellRenderer))]
namespace MuGet.Forms.Android.Renderers
{
    [Preserve(AllMembers = true)]
    public class ColorSwitchCellRenderer : SwitchCellRenderer
    {
        protected override View GetCellCore(Cell item, View convertView, ViewGroup parent, Context context)
        {
            var colorSwitchCell = Cell as ColorSwitchCell;

            var cell = base.GetCellCore(item, convertView, parent, context);

            if (cell is LinearLayout ly &&
                ly.ChildCount > 1 &&
                ly.GetChildAt(1) is LinearLayout ly2 &&
                ly2.ChildCount > 0 &&
                ly2.GetChildAt(0) is TextView text)
            {
                text.SetTextColor((colorSwitchCell?.TextColor ?? Color.Black).ToAndroid());
            }

            return cell;
        }
    }
}