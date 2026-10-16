using CommunityToolkit.Mvvm.ComponentModel;
using MuMuSDK.Core.Extensions;
using MuMuSDK.Core.Interfaces;
using System.Runtime.CompilerServices;

namespace MuMuSDK.Desktop.ViewModels;
public class ViewModelBase : ObservableObject, ILogSource
{
    /// <summary>
    /// Localized interceptor method that guarantees this.Log calls route directly 
    /// to the MuMu logging framework with zero compile-time namespace ambiguity.
    /// </summary>
    protected void Log(MuMuLogLevel level, string message, [CallerFilePath] string sourceFile = "", [CallerLineNumber] int lineNumber = 0)
    {
        LogExtensions.Log(this, level, message, sourceFile, lineNumber);
    }
}