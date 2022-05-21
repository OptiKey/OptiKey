// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Chinese
{
    /// <summary>
    /// Interaction logic for Cangjie.xaml
    /// </summary>
    public partial class Cangjie : KeyboardView
    {
        public Cangjie() : base(shiftAware: false)
        {
            InitializeComponent();
        }
    }
}
