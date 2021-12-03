// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Common
{
    /// <summary>
    /// Interaction logic for Mouse.xaml
    /// </summary>
    public partial class Mouse : KeyboardView
    {
        public Mouse() : base(supportsCollapsedDock:true)
        {
            InitializeComponent();
        }
    }
}
