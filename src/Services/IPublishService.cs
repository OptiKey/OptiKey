using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public interface IPublishService
    {
        void PublishKeyStroke(VirtualKeyCodeSet virtualKeyCodeSet);
        void PublishText(string text);
    }
}
