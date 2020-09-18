using Acr.UserDialogs;
using MuGet.Services;

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
