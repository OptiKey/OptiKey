using System;

namespace JuliusSweetland.OptiKey.Services
{
    public interface INotifyErrors
    {
        event EventHandler<Exception> Error;
    }
}
