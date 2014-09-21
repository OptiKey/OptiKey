using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using JuliusSweetland.ETTA.Enums;
using JuliusSweetland.ETTA.UI.Controls;
using JuliusSweetland.ETTA.UI.ViewModels.Keyboards;

namespace JuliusSweetland.ETTA.Models
{
    public class KeyboardDataTemplateCriteria : Control
    {
        public KeyboardDataTemplateCriteria()
        {
            Initialized += (sender, args) => Debug.Print("*** KeyboardDataTemplateCriteria: Initialized");
            Loaded += (sender, args) => Debug.Print("*** KeyboardDataTemplateCriteria: Loaded");
            DependencyPropertyDescriptor.FromProperty(TemplateProperty, typeof(KeyboardHost)).AddValueChanged(this, TemplateChanged);
        }

        private void TemplateChanged(object sender, EventArgs e)
        {
            Debug.Print("*** KeyboardDataTemplateCriteria: TemplateChanged");
        }

        public override void OnApplyTemplate()
        {
            Debug.Print("*** KeyboardDataTemplateCriteria: OnApplyTemplate");
            base.OnApplyTemplate();
        }

        protected override void OnTemplateChanged(ControlTemplate oldTemplate, ControlTemplate newTemplate)
        {
            Debug.Print("*** KeyboardDataTemplateCriteria: OnTemplateChanged");
            base.OnTemplateChanged(oldTemplate, newTemplate);
        }

        public new Languages Language { get; set; }
        public IKeyboard Keyboard { get; set; }
    }
}
