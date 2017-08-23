using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using JuliusSweetland.OptiKey.Models;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    /// <summary>
    /// Interaction logic for CKPage.xaml
    /// </summary>
    public partial class CKPage : UserControl
    {
        public CKPage()
        {
            InitializeComponent();
        }

        public string dec2hex(string dec)
        {
            if (dec == null || dec == "")
            {
                return "#000000";
            }
            else if (dec.StartsWith("#"))
            {
                return dec;
            }
            else
            {
                List<string> deccolours = dec.Split(',').ToList<string>();
                int intR = (int)Convert.ToSingle(deccolours.ElementAt(0));
                int intG = (int)Convert.ToSingle(deccolours.ElementAt(1));
                int intB = (int)Convert.ToSingle(deccolours.ElementAt(2));
                byte byteR = Convert.ToByte(intR);
                byte byteG = Convert.ToByte(intG);
                byte byteB = Convert.ToByte(intB);
                return "#" + byteR.ToString("X2") + byteG.ToString("X2") + byteB.ToString("X2");
            }
        }
        /*
        public static readonly DependencyProperty CKMenu00Property =
            DependencyProperty.Register("CKMenu00", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu00
        {
            get { return (KeyValue)GetValue(CKMenu00Property); }
            set { SetValue(CKMenu00Property, value); }
        }

        public static readonly DependencyProperty CKText00Property =
            DependencyProperty.Register("CKText00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText00
        {
            get { return (string)GetValue(CKText00Property); }
            set { SetValue(CKText00Property, value); }
        }

        public static readonly DependencyProperty CKBaCo00Property =
            DependencyProperty.Register("CKBaCo00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo00
        {
            get { return (string)GetValue(CKBaCo00Property); }
            set { SetValue(CKBaCo00Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo00Property =
            DependencyProperty.Register("CKImSo00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo00
        {
            get { return (string)GetValue(CKImSo00Property); }
            set { SetValue(CKImSo00Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu01Property =
            DependencyProperty.Register("CKMenu01", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu01
        {
            get { return (KeyValue)GetValue(CKMenu01Property); }
            set { SetValue(CKMenu01Property, value); }
        }

        public static readonly DependencyProperty CKText01Property =
            DependencyProperty.Register("CKText01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText01
        {
            get { return (string)GetValue(CKText01Property); }
            set { SetValue(CKText01Property, value); }
        }

        public static readonly DependencyProperty CKBaCo01Property =
            DependencyProperty.Register("CKBaCo01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo01
        {
            get { return (string)GetValue(CKBaCo01Property); }
            set { SetValue(CKBaCo01Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo01Property =
            DependencyProperty.Register("CKImSo01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo01
        {
            get { return (string)GetValue(CKImSo01Property); }
            set { SetValue(CKImSo01Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu02Property =
            DependencyProperty.Register("CKMenu02", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu02
        {
            get { return (KeyValue)GetValue(CKMenu02Property); }
            set { SetValue(CKMenu02Property, value); }
        }

        public static readonly DependencyProperty CKText02Property =
            DependencyProperty.Register("CKText02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText02
        {
            get { return (string)GetValue(CKText02Property); }
            set { SetValue(CKText02Property, value); }
        }

        public static readonly DependencyProperty CKBaCo02Property =
            DependencyProperty.Register("CKBaCo02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo02
        {
            get { return (string)GetValue(CKBaCo02Property); }
            set { SetValue(CKBaCo02Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo02Property =
            DependencyProperty.Register("CKImSo02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo02
        {
            get { return (string)GetValue(CKImSo02Property); }
            set { SetValue(CKImSo02Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu03Property =
            DependencyProperty.Register("CKMenu03", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu03
        {
            get { return (KeyValue)GetValue(CKMenu03Property); }
            set { SetValue(CKMenu03Property, value); }
        }

        public static readonly DependencyProperty CKText03Property =
            DependencyProperty.Register("CKText03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText03
        {
            get { return (string)GetValue(CKText03Property); }
            set { SetValue(CKText03Property, value); }
        }

        public static readonly DependencyProperty CKBaCo03Property =
            DependencyProperty.Register("CKBaCo03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo03
        {
            get { return (string)GetValue(CKBaCo03Property); }
            set { SetValue(CKBaCo03Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo03Property =
            DependencyProperty.Register("CKImSo03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo03
        {
            get { return (string)GetValue(CKImSo03Property); }
            set { SetValue(CKImSo03Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu04Property =
            DependencyProperty.Register("CKMenu04", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu04
        {
            get { return (KeyValue)GetValue(CKMenu04Property); }
            set { SetValue(CKMenu04Property, value); }
        }

        public static readonly DependencyProperty CKText04Property =
            DependencyProperty.Register("CKText04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText04
        {
            get { return (string)GetValue(CKText04Property); }
            set { SetValue(CKText04Property, value); }
        }

        public static readonly DependencyProperty CKBaCo04Property =
            DependencyProperty.Register("CKBaCo04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo04
        {
            get { return (string)GetValue(CKBaCo04Property); }
            set { SetValue(CKBaCo04Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo04Property =
            DependencyProperty.Register("CKImSo04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo04
        {
            get { return (string)GetValue(CKImSo04Property); }
            set { SetValue(CKImSo04Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu10Property =
            DependencyProperty.Register("CKMenu10", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu10
        {
            get { return (KeyValue)GetValue(CKMenu10Property); }
            set { SetValue(CKMenu10Property, value); }
        }

        public static readonly DependencyProperty CKText10Property =
            DependencyProperty.Register("CKText10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText10
        {
            get { return (string)GetValue(CKText10Property); }
            set { SetValue(CKText10Property, value); }
        }

        public static readonly DependencyProperty CKBaCo10Property =
            DependencyProperty.Register("CKBaCo10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo10
        {
            get { return (string)GetValue(CKBaCo10Property); }
            set { SetValue(CKBaCo10Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo10Property =
            DependencyProperty.Register("CKImSo10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo10
        {
            get { return (string)GetValue(CKImSo10Property); }
            set { SetValue(CKImSo10Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu11Property =
            DependencyProperty.Register("CKMenu11", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu11
        {
            get { return (KeyValue)GetValue(CKMenu11Property); }
            set { SetValue(CKMenu11Property, value); }
        }

        public static readonly DependencyProperty CKText11Property =
            DependencyProperty.Register("CKText11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText11
        {
            get { return (string)GetValue(CKText11Property); }
            set { SetValue(CKText11Property, value); }
        }

        public static readonly DependencyProperty CKBaCo11Property =
            DependencyProperty.Register("CKBaCo11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo11
        {
            get { return (string)GetValue(CKBaCo11Property); }
            set { SetValue(CKBaCo11Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo11Property =
            DependencyProperty.Register("CKImSo11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo11
        {
            get { return (string)GetValue(CKImSo11Property); }
            set { SetValue(CKImSo11Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu12Property =
            DependencyProperty.Register("CKMenu12", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu12
        {
            get { return (KeyValue)GetValue(CKMenu12Property); }
            set { SetValue(CKMenu12Property, value); }
        }

        public static readonly DependencyProperty CKText12Property =
            DependencyProperty.Register("CKText12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText12
        {
            get { return (string)GetValue(CKText12Property); }
            set { SetValue(CKText12Property, value); }
        }

        public static readonly DependencyProperty CKBaCo12Property =
            DependencyProperty.Register("CKBaCo12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo12
        {
            get { return (string)GetValue(CKBaCo12Property); }
            set { SetValue(CKBaCo12Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo12Property =
            DependencyProperty.Register("CKImSo12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo12
        {
            get { return (string)GetValue(CKImSo12Property); }
            set { SetValue(CKImSo12Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu13Property =
            DependencyProperty.Register("CKMenu13", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu13
        {
            get { return (KeyValue)GetValue(CKMenu13Property); }
            set { SetValue(CKMenu13Property, value); }
        }

        public static readonly DependencyProperty CKText13Property =
            DependencyProperty.Register("CKText13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText13
        {
            get { return (string)GetValue(CKText13Property); }
            set { SetValue(CKText13Property, value); }
        }

        public static readonly DependencyProperty CKBaCo13Property =
            DependencyProperty.Register("CKBaCo13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo13
        {
            get { return (string)GetValue(CKBaCo13Property); }
            set { SetValue(CKBaCo13Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo13Property =
            DependencyProperty.Register("CKImSo13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo13
        {
            get { return (string)GetValue(CKImSo13Property); }
            set { SetValue(CKImSo13Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu14Property =
            DependencyProperty.Register("CKMenu14", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu14
        {
            get { return (KeyValue)GetValue(CKMenu14Property); }
            set { SetValue(CKMenu14Property, value); }
        }

        public static readonly DependencyProperty CKText14Property =
            DependencyProperty.Register("CKText14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText14
        {
            get { return (string)GetValue(CKText14Property); }
            set { SetValue(CKText14Property, value); }
        }

        public static readonly DependencyProperty CKBaCo14Property =
            DependencyProperty.Register("CKBaCo14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo14
        {
            get { return (string)GetValue(CKBaCo14Property); }
            set { SetValue(CKBaCo14Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo14Property =
            DependencyProperty.Register("CKImSo14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo14
        {
            get { return (string)GetValue(CKImSo14Property); }
            set { SetValue(CKImSo14Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu20Property =
            DependencyProperty.Register("CKMenu20", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu20
        {
            get { return (KeyValue)GetValue(CKMenu20Property); }
            set { SetValue(CKMenu20Property, value); }
        }

        public static readonly DependencyProperty CKText20Property =
            DependencyProperty.Register("CKText20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText20
        {
            get { return (string)GetValue(CKText20Property); }
            set { SetValue(CKText20Property, value); }
        }

        public static readonly DependencyProperty CKBaCo20Property =
            DependencyProperty.Register("CKBaCo20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo20
        {
            get { return (string)GetValue(CKBaCo20Property); }
            set { SetValue(CKBaCo20Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo20Property =
            DependencyProperty.Register("CKImSo20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo20
        {
            get { return (string)GetValue(CKImSo20Property); }
            set { SetValue(CKImSo20Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu21Property =
            DependencyProperty.Register("CKMenu21", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu21
        {
            get { return (KeyValue)GetValue(CKMenu21Property); }
            set { SetValue(CKMenu21Property, value); }
        }

        public static readonly DependencyProperty CKText21Property =
            DependencyProperty.Register("CKText21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText21
        {
            get { return (string)GetValue(CKText21Property); }
            set { SetValue(CKText21Property, value); }
        }

        public static readonly DependencyProperty CKBaCo21Property =
            DependencyProperty.Register("CKBaCo21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo21
        {
            get { return (string)GetValue(CKBaCo21Property); }
            set { SetValue(CKBaCo21Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo21Property =
            DependencyProperty.Register("CKImSo21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo21
        {
            get { return (string)GetValue(CKImSo21Property); }
            set { SetValue(CKImSo21Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu22Property =
            DependencyProperty.Register("CKMenu22", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu22
        {
            get { return (KeyValue)GetValue(CKMenu22Property); }
            set { SetValue(CKMenu22Property, value); }
        }

        public static readonly DependencyProperty CKText22Property =
            DependencyProperty.Register("CKText22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText22
        {
            get { return (string)GetValue(CKText22Property); }
            set { SetValue(CKText22Property, value); }
        }

        public static readonly DependencyProperty CKBaCo22Property =
            DependencyProperty.Register("CKBaCo22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo22
        {
            get { return (string)GetValue(CKBaCo22Property); }
            set { SetValue(CKBaCo22Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo22Property =
            DependencyProperty.Register("CKImSo22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo22
        {
            get { return (string)GetValue(CKImSo22Property); }
            set { SetValue(CKImSo22Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu23Property =
            DependencyProperty.Register("CKMenu23", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu23
        {
            get { return (KeyValue)GetValue(CKMenu23Property); }
            set { SetValue(CKMenu23Property, value); }
        }

        public static readonly DependencyProperty CKText23Property =
            DependencyProperty.Register("CKText23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText23
        {
            get { return (string)GetValue(CKText23Property); }
            set { SetValue(CKText23Property, value); }
        }

        public static readonly DependencyProperty CKBaCo23Property =
            DependencyProperty.Register("CKBaCo23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo23
        {
            get { return (string)GetValue(CKBaCo23Property); }
            set { SetValue(CKBaCo23Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo23Property =
            DependencyProperty.Register("CKImSo23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo23
        {
            get { return (string)GetValue(CKImSo23Property); }
            set { SetValue(CKImSo23Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu24Property =
            DependencyProperty.Register("CKMenu24", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu24
        {
            get { return (KeyValue)GetValue(CKMenu24Property); }
            set { SetValue(CKMenu24Property, value); }
        }

        public static readonly DependencyProperty CKText24Property =
            DependencyProperty.Register("CKText24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText24
        {
            get { return (string)GetValue(CKText24Property); }
            set { SetValue(CKText24Property, value); }
        }

        public static readonly DependencyProperty CKBaCo24Property =
            DependencyProperty.Register("CKBaCo24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo24
        {
            get { return (string)GetValue(CKBaCo24Property); }
            set { SetValue(CKBaCo24Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo24Property =
            DependencyProperty.Register("CKImSo24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo24
        {
            get { return (string)GetValue(CKImSo24Property); }
            set { SetValue(CKImSo24Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu30Property =
            DependencyProperty.Register("CKMenu30", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu30
        {
            get { return (KeyValue)GetValue(CKMenu30Property); }
            set { SetValue(CKMenu30Property, value); }
        }

        public static readonly DependencyProperty CKText30Property =
            DependencyProperty.Register("CKText30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText30
        {
            get { return (string)GetValue(CKText30Property); }
            set { SetValue(CKText30Property, value); }
        }

        public static readonly DependencyProperty CKBaCo30Property =
            DependencyProperty.Register("CKBaCo30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo30
        {
            get { return (string)GetValue(CKBaCo30Property); }
            set { SetValue(CKBaCo30Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo30Property =
            DependencyProperty.Register("CKImSo30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo30
        {
            get { return (string)GetValue(CKImSo30Property); }
            set { SetValue(CKImSo30Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu31Property =
            DependencyProperty.Register("CKMenu31", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu31
        {
            get { return (KeyValue)GetValue(CKMenu31Property); }
            set { SetValue(CKMenu31Property, value); }
        }

        public static readonly DependencyProperty CKText31Property =
            DependencyProperty.Register("CKText31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText31
        {
            get { return (string)GetValue(CKText31Property); }
            set { SetValue(CKText31Property, value); }
        }

        public static readonly DependencyProperty CKBaCo31Property =
            DependencyProperty.Register("CKBaCo31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo31
        {
            get { return (string)GetValue(CKBaCo31Property); }
            set { SetValue(CKBaCo31Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo31Property =
            DependencyProperty.Register("CKImSo31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo31
        {
            get { return (string)GetValue(CKImSo31Property); }
            set { SetValue(CKImSo31Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu32Property =
            DependencyProperty.Register("CKMenu32", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu32
        {
            get { return (KeyValue)GetValue(CKMenu32Property); }
            set { SetValue(CKMenu32Property, value); }
        }

        public static readonly DependencyProperty CKText32Property =
            DependencyProperty.Register("CKText32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText32
        {
            get { return (string)GetValue(CKText32Property); }
            set { SetValue(CKText32Property, value); }
        }

        public static readonly DependencyProperty CKBaCo32Property =
            DependencyProperty.Register("CKBaCo32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo32
        {
            get { return (string)GetValue(CKBaCo32Property); }
            set { SetValue(CKBaCo32Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo32Property =
            DependencyProperty.Register("CKImSo32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo32
        {
            get { return (string)GetValue(CKImSo32Property); }
            set { SetValue(CKImSo32Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu33Property =
            DependencyProperty.Register("CKMenu33", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu33
        {
            get { return (KeyValue)GetValue(CKMenu33Property); }
            set { SetValue(CKMenu33Property, value); }
        }

        public static readonly DependencyProperty CKText33Property =
            DependencyProperty.Register("CKText33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText33
        {
            get { return (string)GetValue(CKText33Property); }
            set { SetValue(CKText33Property, value); }
        }

        public static readonly DependencyProperty CKBaCo33Property =
            DependencyProperty.Register("CKBaCo33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo33
        {
            get { return (string)GetValue(CKBaCo33Property); }
            set { SetValue(CKBaCo33Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo33Property =
            DependencyProperty.Register("CKImSo33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo33
        {
            get { return (string)GetValue(CKImSo33Property); }
            set { SetValue(CKImSo33Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu34Property =
            DependencyProperty.Register("CKMenu34", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu34
        {
            get { return (KeyValue)GetValue(CKMenu34Property); }
            set { SetValue(CKMenu34Property, value); }
        }

        public static readonly DependencyProperty CKText34Property =
            DependencyProperty.Register("CKText34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText34
        {
            get { return (string)GetValue(CKText34Property); }
            set { SetValue(CKText34Property, value); }
        }

        public static readonly DependencyProperty CKBaCo34Property =
            DependencyProperty.Register("CKBaCo34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo34
        {
            get { return (string)GetValue(CKBaCo34Property); }
            set { SetValue(CKBaCo34Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo34Property =
            DependencyProperty.Register("CKImSo34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo34
        {
            get { return (string)GetValue(CKImSo34Property); }
            set { SetValue(CKImSo34Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenuKey00Property =
            DependencyProperty.Register("CKMenuKey00", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey00 { get { return CKMenu00 != null && CKMenu00 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey01Property =
            DependencyProperty.Register("CKMenuKey01", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey01 { get { return CKMenu01 != null && CKMenu01 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey02Property =
            DependencyProperty.Register("CKMenuKey02", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey02 { get { return CKMenu02 != null && CKMenu02 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey03Property =
            DependencyProperty.Register("CKMenuKey03", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey03 { get { return CKMenu03 != null && CKMenu03 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey04Property =
            DependencyProperty.Register("CKMenuKey04", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey04 { get { return CKMenu04 != null && CKMenu04 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey10Property =
            DependencyProperty.Register("CKMenuKey10", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey10 { get { return CKMenu10 != null && CKMenu10 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey11Property =
            DependencyProperty.Register("CKMenuKey11", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey11 { get { return CKMenu11 != null && CKMenu11 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey12Property =
            DependencyProperty.Register("CKMenuKey12", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey12 { get { return CKMenu12 != null && CKMenu12 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey13Property =
            DependencyProperty.Register("CKMenuKey13", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey13 { get { return CKMenu13 != null && CKMenu13 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey14Property =
            DependencyProperty.Register("CKMenuKey14", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey14 { get { return CKMenu14 != null && CKMenu14 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey20Property =
            DependencyProperty.Register("CKMenuKey20", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey20 { get { return CKMenu20 != null && CKMenu20 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey21Property =
            DependencyProperty.Register("CKMenuKey21", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey21 { get { return CKMenu21 != null && CKMenu21 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey22Property =
            DependencyProperty.Register("CKMenuKey22", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey22 { get { return CKMenu22 != null && CKMenu22 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey23Property =
            DependencyProperty.Register("CKMenuKey23", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey23 { get { return CKMenu23 != null && CKMenu23 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey24Property =
            DependencyProperty.Register("CKMenuKey24", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey24 { get { return CKMenu24 != null && CKMenu24 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey30Property =
            DependencyProperty.Register("CKMenuKey30", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey30 { get { return CKMenu30 != null && CKMenu30 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey31Property =
            DependencyProperty.Register("CKMenuKey31", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey31 { get { return CKMenu31 != null && CKMenu31 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey32Property =
            DependencyProperty.Register("CKMenuKey32", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey32 { get { return CKMenu32 != null && CKMenu32 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey33Property =
            DependencyProperty.Register("CKMenuKey33", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey33 { get { return CKMenu33 != null && CKMenu33 != KeyValues.CommuniKate_; } }

        public static readonly DependencyProperty CKMenuKey34Property =
            DependencyProperty.Register("CKMenuKey34", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public bool CKMenuKey34 { get { return CKMenu34 != null && CKMenu34 != KeyValues.CommuniKate_; } }
        */
    }
}
