using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.Models;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;

namespace JuliusSweetland.ETTA.UI.DataTemplateSelectors
{
    public class KeyboardDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate ErrorTemplate { get; set; }

        public DataTemplate EnglishAlphaTemplate { get; set; }
        public DataTemplate EnglishNumericAndSymbols1Template { get; set; }
        public DataTemplate EnglishSymbols2Template { get; set; }
        public DataTemplate EnglishPublishTemplate { get; set; }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var criteria = item as KeyboardDataTemplateCriteria;

            if (criteria != null)
            {
                switch (criteria.Language)
                {
                    case Languages.AmericanEnglish:
                    case Languages.BritishEnglish:
                    case Languages.CanadianEnglish:
                        if (criteria.Keyboard is Alpha
                            && EnglishAlphaTemplate != null)
                        {
                            return EnglishAlphaTemplate;
                        }
                        
                        if (criteria.Keyboard is NumericAndSymbols1
                            && EnglishNumericAndSymbols1Template != null)
                        {
                            return EnglishNumericAndSymbols1Template;
                        }
                        
                        if (criteria.Keyboard is Symbols2
                            && EnglishSymbols2Template != null)
                        {
                            return EnglishSymbols2Template;
                        }
                        
                        if (criteria.Keyboard is Publish
                            && EnglishPublishTemplate != null)
                        {
                            return EnglishPublishTemplate;
                        }
                        break;
                }
            }

            return ErrorTemplate;
        }
    }
}
