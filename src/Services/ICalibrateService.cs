using System;

namespace JuliusSweetland.ETTA.Services
{
    public interface ICalibrateService
    {
        event EventHandler<string> Info;
        event EventHandler<Exception> Error;

        void Calibrate(int retryNumber, Action onResultAction);
    }
}
