using System;

namespace MuGet.ViewModels
{
    public enum State
    {
        None,
        Loading,
        Saving,
        Success,
        Error,
        Empty,
        Custom
    }
}
