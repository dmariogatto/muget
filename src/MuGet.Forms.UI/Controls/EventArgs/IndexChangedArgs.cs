using System;

namespace MuGet.Forms.UI.Controls
{
    public class IndexChangedArgs : EventArgs
    {
        public int OldIndex { get; }
        public int NewIndex { get; }

        public IndexChangedArgs(int oldIdx, int newIdx)
        {
            OldIndex = oldIdx;
            NewIndex = newIdx;
        }
    }
}
