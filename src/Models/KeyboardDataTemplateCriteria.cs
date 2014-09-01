using System.Windows;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;

namespace JuliusSweetland.ETTA.Models
{
    public class KeyboardDataTemplateCriteria
    {
        public Languages Language { get; set; }
        public IKeyboard Keyboard { get; set; }
    }
}
