using System;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public interface IPublishService
    {
        event EventHandler<Exception> Error;

        void PublishModifiedKeyStroke(VirtualKeyCodeSet virtualKeyCodeSet);
        void PublishText(string text);
    }
}
