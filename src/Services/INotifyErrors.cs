using System;

namespace JuliusSweetland.ETTA.Services
{
    public interface INotifyErrors
    {
        event EventHandler<Exception> Error;
    }
}
