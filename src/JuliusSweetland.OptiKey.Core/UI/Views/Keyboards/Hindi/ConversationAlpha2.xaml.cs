// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Hindi
{
    /// <summary>
    /// Interaction logic for ConversationAlpha2.xaml
    /// </summary>
    public partial class ConversationAlpha2 : KeyboardView
    {
        public ConversationAlpha2() : base(shiftAware: false)
        {
            InitializeComponent();
        }

        /*
         Example of key with value which records a string value AND a function key (to return to Alpha1 keyboard)
         <controls:Key Grid.Row="2" Grid.Column="0" Grid.ColumnSpan="2"
                      Text="अ"
                      UseUnicodeCompatibilityFont="True"
                      SharedSizeGroup="KeyWithSingleLetter"
                      Value="{Binding Path=अKey, RelativeSource={RelativeSource AncestorType={x:Type controls:KeyboardView}}}" />
        //This is stored in ConversationAlpha2.xaml.cs:
        public KeyValue अKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "अ");
         */
    }
}
