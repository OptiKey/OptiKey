// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

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

        public KeyValue अKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "अ");
        public KeyValue आKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "आ");
        public KeyValue इKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "इ");
        public KeyValue ईKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ई");
        public KeyValue उKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "उ");
        public KeyValue ऊKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ऊ");
        public KeyValue एKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ए");
        public KeyValue ऐKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ऐ");
        public KeyValue ओKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ओ");
        public KeyValue औKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "औ");
        public KeyValue अँKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "अँ");
        public KeyValue अःKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "अः");
        public KeyValue DevanagariVowelSignCandraEKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "\u0945");
        public KeyValue मKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "म");
        public KeyValue यKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "य");
        public KeyValue रKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "र");
        public KeyValue लKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ल");
        public KeyValue वKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "व");
        public KeyValue सKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "स");
        public KeyValue शKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "श");
        public KeyValue षKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ष");
        public KeyValue हKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ह");
        public KeyValue ऋKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ऋ");
        public KeyValue DevanagariVowelSignCandraOKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "\u0949");
        public KeyValue ङKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ङ");
        public KeyValue ञKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ञ");
        public KeyValue णKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ण");
        public KeyValue श्रKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "श्र");
        public KeyValue क्षKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "क्ष");
        public KeyValue त्रKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "त्र");
        public KeyValue ज्ञKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ज्ञ");
        public KeyValue ऍKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ऍ");
        public KeyValue ऑKey => new KeyValue(FunctionKeys.ConversationAlpha1Keyboard, "ऑ");
    }
}
