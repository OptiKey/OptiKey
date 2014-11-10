using JuliusSweetland.ETTA.Models;

namespace JuliusSweetland.ETTA.Services
{
    public interface IPublishService
    {
        void PublishModifiedKeyStroke(VirtualKeyCodeSet virtualKeyCodeSet);
        void PublishText(string text);
    }
}
