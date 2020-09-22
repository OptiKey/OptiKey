// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.UI.Controls;

namespace JuliusSweetland.OptiKey.UI.Views.Keyboards.Hindi
{
    /// <summary>
    /// Interaction logic for Alpha2.xaml
    /// </summary>
    public partial class Alpha2 : KeyboardView
    {
        public Alpha2() : base(shiftAware: false)
        {
            InitializeComponent();
        }

        public KeyValue अKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "अ");
        public KeyValue आKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "आ");
        public KeyValue इKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "इ");
        public KeyValue ईKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ई");
        public KeyValue उKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "उ");
        public KeyValue ऊKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ऊ");
        public KeyValue एKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ए");
        public KeyValue ऐKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ऐ");
        public KeyValue ओKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ओ");
        public KeyValue औKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "औ");
        public KeyValue अँKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "अँ");
        public KeyValue अःKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "अः");
        public KeyValue DevanagariVowelSignCandraEKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "\u0945");
        public KeyValue मKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "म");
        public KeyValue यKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "य");
        public KeyValue रKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "र");
        public KeyValue लKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ल");
        public KeyValue वKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "व");
        public KeyValue सKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "स");
        public KeyValue शKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "श");
        public KeyValue षKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ष");
        public KeyValue हKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ह");
        public KeyValue ऋKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ऋ");
        public KeyValue DevanagariVowelSignCandraOKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "\u0949");
        public KeyValue ङKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ङ");
        public KeyValue ञKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ञ");
        public KeyValue णKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ण");
        public KeyValue श्रKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "श्र");
        public KeyValue क्षKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "क्ष");
        public KeyValue त्रKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "त्र");
        public KeyValue ज्ञKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ज्ञ");
        public KeyValue ऍKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ऍ");
        public KeyValue ऑKey => new KeyValue(FunctionKeys.Alpha1Keyboard, "ऑ");
    }
}
