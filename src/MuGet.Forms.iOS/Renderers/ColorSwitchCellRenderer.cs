using MuGet.Forms.Controls;
using MuGet.Forms.iOS.Renderers;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(ColorSwitchCell), typeof(ColorSwitchCellRenderer))]
namespace MuGet.Forms.iOS.Renderers
{
    [Preserve(AllMembers = true)]
    public class ColorSwitchCellRenderer : SwitchCellRenderer
    {
        public override UITableViewCell GetCell(Cell item, UITableViewCell reusableCell, UITableView tv)
        {
            var menuCell = item as ColorSwitchCell;

            var cell = base.GetCell(menuCell, reusableCell, tv);

            cell.TextLabel.Text = menuCell.Text;
            cell.TextLabel.TextColor = menuCell.TextColor.ToUIColor();
            cell.BackgroundColor = UIColor.Clear;

            return cell;
        }
    }
}