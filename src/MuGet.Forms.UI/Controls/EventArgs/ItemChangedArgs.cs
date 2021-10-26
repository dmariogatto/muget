using System;

namespace MuGet.Forms.UI.Controls
{
    public class ItemChangedArgs : EventArgs
    {
        public object OldItem { get; }
        public object NewItem { get; }

        public ItemChangedArgs(object oldItem, object newItem)
        {
            OldItem = oldItem;
            NewItem = newItem;
        }
    }
}