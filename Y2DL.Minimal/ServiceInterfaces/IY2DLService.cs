using Y2DL.Minimal.Models;

namespace Y2DL.Minimal.ServiceInterfaces;

public interface IY2DLService<T>
{
    /// <summary>
    ///     Runs the Y2DL service, asynchronously.
    /// </summary>
    Task RunAsync(T data);
}