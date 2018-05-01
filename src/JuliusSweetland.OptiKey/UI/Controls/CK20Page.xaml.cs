using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Net;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using log4net;

namespace JuliusSweetland.OptiKey.UI.Controls
{
    /// <summary>
    /// Interaction logic for CK20Page.xaml
    /// </summary>
    public partial class CK20Page : UserControl
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private const string ApplicationDataSubPath = @"JuliusSweetland\OptiKey\CommuniKate\";

        public class CKOBF
        {
            public List<Buttons> buttons { get; set; }
            public Grid grid { get; set; }
            public List<Images> images { get; set; }
        }

        public class Buttons
        {
            //[JsonProperty("buttons")]
            public Buttons()
            {
                background_color = "#FFFFFF";
                border_color = null;
                image_id = "";
                sound_id = "";
                label = "";
                vocalization = "";
                load_board = null;
                id = "";
                action = null;
            }
            public string background_color { get; set; }
            public string border_color { get; set; }
            public string id { get; set; }
            public string action { get; set; }
            public string image_id { get; set; }
            public string sound_id { get; set; }
            public string label { get; set; }
            public string vocalization { get; set; }
            public Load_board load_board { get; set; }
        }

        public class Load_board
        {
            public string path { get; set; }
        }

        public class Grid
        {
            //[JsonProperty("grid")]
            public Grid()
            {
                rows = 1;
                columns = 1;
                order = null;
            }
            public int rows { get; set; }
            public int columns { get; set; }
            public List<List<string>> order { get; set; }
        }

        public class Images
        {
            //[JsonProperty("images")]
            public string id { get; set; }
            public string path { get; set; }
            public string url { get; set; }
            public string data { get; set; }
            public string content_type { get; set; }
        }

        public class Pageset
        {
            //[JsonProperty("Pageset")]
            public string format { get; set; }
            public string root { get; set; }
        }
        
        public static DependencyProperty CKPageFileProperty =
        DependencyProperty.Register("CKPageFile", typeof(string), typeof(CK20Page), new PropertyMetadata(default(string), CKPageFileChanged));

        private static void CKPageFileChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                string extractPath = CKpath();

                string pagefile = value;
                Log.DebugFormat("Trying to read page file: {0}.", pagefile);
                if (value == null || !File.Exists(pagefile))
                {
                    string contents = new StreamReader(extractPath + "manifest.json", Encoding.UTF8).ReadToEnd();
                    Pageset manifest = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Pageset>(contents);
                    pagefile = extractPath + manifest.root;
                }

                {
                    Log.DebugFormat("Page file to read: {0}.", pagefile);
                    string contents = new StreamReader(pagefile, Encoding.UTF8).ReadToEnd();
                    CKOBF CKPageOBF = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<CKOBF>(contents);
                    int includesTopRow = CKPageOBF.grid.rows >= 5 ? 1 : 0;
                    key.CKGridRows = CKPageOBF.grid.rows - includesTopRow;
                    key.CKGridColumns = CKPageOBF.grid.columns;
                    int ButtonCount = CKPageOBF.buttons.Count();
                    Log.DebugFormat("Page contains {0} buttons.", ButtonCount - 3 * includesTopRow);
                    string image;
                    string path;
                    string text;

                    List<string> Colours = new List<string>();
                    List<string> Images = new List<string>();
                    List<string> Paths = new List<string>();
                    List<bool> Ismenukeys = new List<bool>();
                    List<string> Labels = new List<string>();
                    List<string> Texts = new List<string>();
                    List<Load_board> Boards = new List<Load_board>();

                    string defaultColour = "rgb(128, 128, 128)";
                    // some alternative default colours:
                    // "rgb(68, 68, 68)"; // "rgb(191, 191, 191)"; // "Transparent"; //"#000000"; //
                    string pageColour = defaultColour;
                    string defaultPath = null;
                    bool defaultIsMenuKey = false;

                    int BlankButtonCount = 0; // CKPageOBF.grid.order.FindAll(x => x.Equals("null")).Count - 2 * includesTopRow; // 20 - ButtonCount + 3 * includesTopRow;
                    for (int row = includesTopRow; row < CKPageOBF.grid.rows; ++row)
                    {
                        var nullEntries = CKPageOBF.grid.order.ElementAt(row).FindAll(x => String.IsNullOrEmpty(x));
                        if (nullEntries.Count > 0)
                            BlankButtonCount += nullEntries.Count();
                    }
                    Log.DebugFormat("There are {0} empty button(s) on this page.", BlankButtonCount);
                    Buttons CurrentButton = new Buttons();

                    // CK 20 pagesets have keys in four rows and five columns 
                    // with an extra row at the top for the scratchpad and two other keys

                    int ButtonNo = includesTopRow * 3;
                    int ButtonIndex;
                    // start at three to skip the scratchpad row keys

                    for (int Row = includesTopRow; Row < 4 + includesTopRow; ++Row)
                    { // start at one to skip the scratchpad row
                        for (int Column = 0; Column < 5; ++Column)
                        { // all five columns are used

                            // assume the current button is blank
                            bool CurrentButtonIsBlank = true;

                            if (ButtonNo < CKPageOBF.buttons.Count && Row < CKPageOBF.grid.rows && Column < CKPageOBF.grid.columns)
                            { // if the stored keys in CKPageOBF total more than the current button number
                                ButtonIndex = CKPageOBF.buttons.FindIndex(x => x.id.Equals(CKPageOBF.grid.order.ElementAt(Row).ElementAt(Column)));
                                if (ButtonIndex != -1)
                                { // check if the position of the current key matches the current position

                                    // if the positions match then it isn't blank
                                    CurrentButtonIsBlank = false;
                                    //CurrentButton = new Buttons();
                                    CurrentButton = CKPageOBF.buttons.ElementAt(ButtonIndex);

                                    if (CurrentButton.load_board == null && pageColour.Equals(defaultColour))
                                    { // if this non-blank key is the first non-menu key then use its background colour for all the subsequent blank keys
                                        pageColour = CurrentButton.background_color;
                                    }
                                }
                            }

                            if (CurrentButtonIsBlank)
                            { // if the current key is blank insert it
                                if (BlankButtonCount == 1)
                                { // if this is the last blank key insert the back button
                                    CurrentButton = new Buttons()
                                    {
                                        background_color = "rgb(204,255,204)",
                                        label = "BACK",
                                        image_id = CKpath() + @"images\back.png",
                                        load_board = new Load_board()
                                        {
                                            path = Settings.Default.CommuniKateKeyboardPrevious1Context
                                        }
                                    };
                                    Log.DebugFormat("Back button added at column {0} row {1} with background colour {2}.", Column + 1, Row, CurrentButton.background_color);
                                }
                                else
                                {
                                    CurrentButton = new Buttons()
                                    {
                                        background_color = pageColour
                                    };
                                    Log.DebugFormat("Blank button number {3} added at column {0} row {1} with background colour {2}.", Column + 1, Row, CurrentButton.background_color, BlankButtonCount - 1);
                                }
                                CurrentButton.id = Column.ToString() + Row.ToString();
                                CKPageOBF.buttons.Insert(ButtonNo, CurrentButton);
                                if (Row < CKPageOBF.grid.rows && Column < CKPageOBF.grid.columns)
                                    --BlankButtonCount;
                            }

                            // store the individual properties of the current button
                            Colours.Add(dec2hex(CurrentButton.background_color));
                            image = CurrentButton.image_id;
                            if (image != "" && image != null && !image.EndsWith(@"images\back.png"))
                            {
                                int imageIndex = CKPageOBF.images.FindIndex(x => x.id.Contains(image));
                                if (imageIndex != -1)
                                {
                                    var imageData = CKPageOBF.images.ElementAt(imageIndex);
                                    if (!String.IsNullOrEmpty(imageData.path)) { image = CKpath() + imageData.path; }
                                    else if (!String.IsNullOrEmpty(imageData.data))
                                    {
                                        image += "." + imageData.content_type.Substring(6);
                                        image = CKpath() + @"images\" + image;
                                        if (!File.Exists(image))
                                                File.WriteAllBytes(image, Convert.FromBase64String(imageData.data.Substring(22)));
                                    }
                                    else if (!String.IsNullOrEmpty(imageData.url))
                                    {
                                        image = CKpath() + @"images\" + Path.GetFileName(imageData.url);
                                        if (!File.Exists(image))
                                            using (WebClient client = new WebClient())
                                            {
                                                client.DownloadFile(new Uri(imageData.url), image);
                                            }
                                    }
                                    else
                                    {
                                        Log.DebugFormat("Insufficient image data for image: {0}.", image);
                                        image = "";
                                    }
                                }
                                else
                                {
                                    Log.DebugFormat("Missing image: {0}.", image);
                                    image = "";
                                }
                            }
                            if (!String.IsNullOrEmpty(image))
                                Log.DebugFormat("Button {0} uses image {1}.", ButtonNo + 1 - 3 * includesTopRow, image);
                            Images.Add(image);
                            Boards.Add(CurrentButton.load_board);
                            Texts.Add(CurrentButton.label);
                            text = CurrentButton.vocalization;
                            if (Boards.Last() != null && Boards.Last().path != null)
                            {
                                path = Boards.Last().path;
                                if (!String.IsNullOrEmpty(text))
                                    Paths.Add(text + "+" + path);
                                else
                                    Paths.Add(path);
                                Ismenukeys.Add(true);
                                Log.DebugFormat("Button {0} is a menu key for board {1}.", ButtonNo + 1 - 3 * includesTopRow, path);
                                Labels.Add(Texts.Last());
                            }
                            else
                            {
                                Paths.Add(defaultPath);
                                Ismenukeys.Add(defaultIsMenuKey);
                                Labels.Add(String.IsNullOrEmpty(text) ? Texts.Last() : text);
                            }
                            ++ButtonNo;
                        }
                    }

                    int buttonid = 0;
                    key.CKBaCo_00 = Colours.ElementAt(buttonid);
                    key.CKLabel_00 = Labels.ElementAt(buttonid);
                    key.CKText_00 = Texts.ElementAt(buttonid);
                    key.CKImSo_00 = Images.ElementAt(buttonid);
                    key.CKKeCo_00 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_00 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_01 = Colours.ElementAt(buttonid);
                    key.CKLabel_01 = Labels.ElementAt(buttonid);
                    key.CKText_01 = Texts.ElementAt(buttonid);
                    key.CKImSo_01 = Images.ElementAt(buttonid);
                    key.CKKeCo_01 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_01 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_02 = Colours.ElementAt(buttonid);
                    key.CKLabel_02 = Labels.ElementAt(buttonid);
                    key.CKText_02 = Texts.ElementAt(buttonid);
                    key.CKImSo_02 = Images.ElementAt(buttonid);
                    key.CKKeCo_02 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_02 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_03 = Colours.ElementAt(buttonid);
                    key.CKLabel_03 = Labels.ElementAt(buttonid);
                    key.CKText_03 = Texts.ElementAt(buttonid);
                    key.CKImSo_03 = Images.ElementAt(buttonid);
                    key.CKKeCo_03 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_03 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_04 = Colours.ElementAt(buttonid);
                    key.CKLabel_04 = Labels.ElementAt(buttonid);
                    key.CKText_04 = Texts.ElementAt(buttonid);
                    key.CKImSo_04 = Images.ElementAt(buttonid);
                    key.CKKeCo_04 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_04 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_10 = Colours.ElementAt(buttonid);
                    key.CKLabel_10 = Labels.ElementAt(buttonid);
                    key.CKText_10 = Texts.ElementAt(buttonid);
                    key.CKImSo_10 = Images.ElementAt(buttonid);
                    key.CKKeCo_10 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_10 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_11 = Colours.ElementAt(buttonid);
                    key.CKLabel_11 = Labels.ElementAt(buttonid);
                    key.CKText_11 = Texts.ElementAt(buttonid);
                    key.CKImSo_11 = Images.ElementAt(buttonid);
                    key.CKKeCo_11 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_11 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_12 = Colours.ElementAt(buttonid);
                    key.CKLabel_12 = Labels.ElementAt(buttonid);
                    key.CKText_12 = Texts.ElementAt(buttonid);
                    key.CKImSo_12 = Images.ElementAt(buttonid);
                    key.CKKeCo_12 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_12 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_13 = Colours.ElementAt(buttonid);
                    key.CKLabel_13 = Labels.ElementAt(buttonid);
                    key.CKText_13 = Texts.ElementAt(buttonid);
                    key.CKImSo_13 = Images.ElementAt(buttonid);
                    key.CKKeCo_13 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_13 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_14 = Colours.ElementAt(buttonid);
                    key.CKLabel_14 = Labels.ElementAt(buttonid);
                    key.CKText_14 = Texts.ElementAt(buttonid);
                    key.CKImSo_14 = Images.ElementAt(buttonid);
                    key.CKKeCo_14 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_14 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_20 = Colours.ElementAt(buttonid);
                    key.CKLabel_20 = Labels.ElementAt(buttonid);
                    key.CKText_20 = Texts.ElementAt(buttonid);
                    key.CKImSo_20 = Images.ElementAt(buttonid);
                    key.CKKeCo_20 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_20 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_21 = Colours.ElementAt(buttonid);
                    key.CKLabel_21 = Labels.ElementAt(buttonid);
                    key.CKText_21 = Texts.ElementAt(buttonid);
                    key.CKImSo_21 = Images.ElementAt(buttonid);
                    key.CKKeCo_21 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_21 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_22 = Colours.ElementAt(buttonid);
                    key.CKLabel_22 = Labels.ElementAt(buttonid);
                    key.CKText_22 = Texts.ElementAt(buttonid);
                    key.CKImSo_22 = Images.ElementAt(buttonid);
                    key.CKKeCo_22 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_22 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_23 = Colours.ElementAt(buttonid);
                    key.CKLabel_23 = Labels.ElementAt(buttonid);
                    key.CKText_23 = Texts.ElementAt(buttonid);
                    key.CKImSo_23 = Images.ElementAt(buttonid);
                    key.CKKeCo_23 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_23 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_24 = Colours.ElementAt(buttonid);
                    key.CKLabel_24 = Labels.ElementAt(buttonid);
                    key.CKText_24 = Texts.ElementAt(buttonid);
                    key.CKImSo_24 = Images.ElementAt(buttonid);
                    key.CKKeCo_24 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_24 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_30 = Colours.ElementAt(buttonid);
                    key.CKLabel_30 = Labels.ElementAt(buttonid);
                    key.CKText_30 = Texts.ElementAt(buttonid);
                    key.CKImSo_30 = Images.ElementAt(buttonid);
                    key.CKKeCo_30 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_30 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_31 = Colours.ElementAt(buttonid);
                    key.CKLabel_31 = Labels.ElementAt(buttonid);
                    key.CKText_31 = Texts.ElementAt(buttonid);
                    key.CKImSo_31 = Images.ElementAt(buttonid);
                    key.CKKeCo_31 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_31 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_32 = Colours.ElementAt(buttonid);
                    key.CKLabel_32 = Labels.ElementAt(buttonid);
                    key.CKText_32 = Texts.ElementAt(buttonid);
                    key.CKImSo_32 = Images.ElementAt(buttonid);
                    key.CKKeCo_32 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_32 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_33 = Colours.ElementAt(buttonid);
                    key.CKLabel_33 = Labels.ElementAt(buttonid);
                    key.CKText_33 = Texts.ElementAt(buttonid);
                    key.CKImSo_33 = Images.ElementAt(buttonid);
                    key.CKKeCo_33 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_33 = Ismenukeys.ElementAt(buttonid);

                    ++buttonid;
                    key.CKBaCo_34 = Colours.ElementAt(buttonid);
                    key.CKLabel_34 = Labels.ElementAt(buttonid);
                    key.CKText_34 = Texts.ElementAt(buttonid);
                    key.CKImSo_34 = Images.ElementAt(buttonid);
                    key.CKKeCo_34 = Paths.ElementAt(buttonid);
                    key.CKMenuKey_34 = Ismenukeys.ElementAt(buttonid);
                }
            }
        }

        public string CKPageFile
        {
            get { return (string)GetValue(CKPageFileProperty); }
            set
            {
                string path = CKpath();
                string pageset = Settings.Default.CommuniKatePagesetLocation;
                bool isOBFarchive = true;
                if (pageset.EndsWith(".obf"))
                {
                    string obfFile = path + Path.GetFileName(pageset);
                    if (File.Exists(obfFile))
                    {
                        isOBFarchive = false;
                        SetValue(CKPageFileProperty, obfFile);
                    }
                    else if (File.Exists(pageset))
                    {
                        isOBFarchive = false;
                        if (!Directory.Exists(path))
                            Directory.CreateDirectory(path);
                        File.Copy(pageset, obfFile);
                        string backImage = path + @"images/back.png";
                        if (!File.Exists(backImage))
                        {
                            if (!Directory.Exists(path + "images"))
                                Directory.CreateDirectory(path + "images");
                            File.Copy(@"./Resources/CommuniKate/back.png", backImage, true);
                        }
                        SetValue(CKPageFileProperty, obfFile);
                    }
                }
                if (isOBFarchive)
                {
                    if (!File.Exists(path + "manifest.json"))
                    {
                        if (!File.Exists(pageset))
                            pageset = @"./Resources/CommuniKate/pageset.obz";

                        using (ZipArchive archive = ZipFile.Open(pageset, ZipArchiveMode.Read))
                        {
                            archive.ExtractToDirectory(path);
                        }
                        string backImage = path + @"images/back.png";
                        if (!File.Exists(backImage))
                        {
                            if (!Directory.Exists(path + "images"))
                                Directory.CreateDirectory(path + "images");
                            File.Copy(@"./Resources/CommuniKate/back.png", backImage, true);
                        }
                    }
                    if (value != null)
                    {
                        SetValue(CKPageFileProperty, path + value);
                    }
                    else
                    {
                        string contents = new StreamReader(path + "manifest.json", Encoding.UTF8).ReadToEnd();
                        Pageset manifest = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<Pageset>(contents);
                        SetValue(CKPageFileProperty, path + manifest.root);
                    }
                }
            }
        }

        public CK20Page()
        {
            CKPageFile = Settings.Default.CommuniKateKeyboardCurrentContext;

            InitializeComponent();
        }

        private static string CKpath()
        {
            var applicationDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ApplicationDataSubPath);
            return applicationDataPath;
        }

        private static string dec2hex(string dec)
        {
            if (string.IsNullOrEmpty(dec))
            {
                return "#000000";
            }
            else if (dec.StartsWith("#"))
            {
                if (dec.Length == 7)
                    return dec;
                else
                    return "#000000";
            }
            else if (dec.Contains("Transparent"))
            {
                return "Transparent";
            }
            else
            {
                if (dec.StartsWith("rgb("))
                    dec = dec.Substring(4);
                else if (dec.StartsWith("rgba("))
                    dec = dec.Substring(5);
                if (dec.Trim().EndsWith(")"))
                    dec = dec.Trim().Substring(0, dec.Trim().Length - 1);
                // Log.DebugFormat("Background colour: {0}.", dec);
                List<string> deccolours = dec.Split(',').ToList<string>();
                if (deccolours.Count != 3 && deccolours.Count != 4)
                    return "#000000";
                int intR = (int)Convert.ToSingle(deccolours.ElementAt(0).Trim());
                int intG = (int)Convert.ToSingle(deccolours.ElementAt(1).Trim());
                int intB = (int)Convert.ToSingle(deccolours.ElementAt(2).Trim());
                byte byteR = Convert.ToByte(intR);
                byte byteG = Convert.ToByte(intG);
                byte byteB = Convert.ToByte(intB);
                return "#" + byteR.ToString("X2") + byteG.ToString("X2") + byteB.ToString("X2");
            }
        }

        public static readonly DependencyProperty CKGridRowsProperty =
            DependencyProperty.Register("CKGridRows", typeof(int), typeof(Key), new PropertyMetadata(default(int)));

        public int CKGridRows
        {
            get { return (int)GetValue(CKGridRowsProperty); }
            set { SetValue(CKGridRowsProperty, value); }
        }

        public static readonly DependencyProperty CKGridColumnsProperty =
            DependencyProperty.Register("CKGridColumns", typeof(int), typeof(Key), new PropertyMetadata(default(int)));

        public int CKGridColumns
        {
            get { return (int)GetValue(CKGridColumnsProperty); }
            set { SetValue(CKGridColumnsProperty, value); }
        }

        public static readonly DependencyProperty CKMenu00Property =
            DependencyProperty.Register("CKMenu00", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu00
        {
            get { return (KeyValue)GetValue(CKMenu00Property); }
            set
            {
                SetValue(CKMenu00Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_00));
            }
        }

        public static readonly DependencyProperty CKLabel_00Property =
            DependencyProperty.Register("CKLabel_00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_00
        {
            get { return (string)GetValue(CKLabel_00Property); }
            set { SetValue(CKLabel_00Property, value); }
        }

        public static readonly DependencyProperty CKText_00Property =
            DependencyProperty.Register("CKText_00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_00
        {
            get { return (string)GetValue(CKText_00Property); }
            set { SetValue(CKText_00Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_00Property =
            DependencyProperty.Register("CKBaCo_00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_00
        {
            get { return (string)GetValue(CKBaCo_00Property); }
            set { SetValue(CKBaCo_00Property, value); }
        }

        public static readonly DependencyProperty CKImSo_00Property =
            DependencyProperty.Register("CKImSo_00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_00
        {
            get { return (string)GetValue(CKImSo_00Property); }
            set { SetValue(CKImSo_00Property, value); }
        }

        public static readonly DependencyProperty CKMenu01Property =
            DependencyProperty.Register("CKMenu01", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu01
        {
            get { return (KeyValue)GetValue(CKMenu01Property); }
            set
            {
                SetValue(CKMenu01Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_01));
            }
        }

        public static readonly DependencyProperty CKLabel_01Property =
            DependencyProperty.Register("CKLabel_01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_01
        {
            get { return (string)GetValue(CKLabel_01Property); }
            set { SetValue(CKLabel_01Property, value); }
        }

        public static readonly DependencyProperty CKText_01Property =
            DependencyProperty.Register("CKText_01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_01
        {
            get { return (string)GetValue(CKText_01Property); }
            set { SetValue(CKText_01Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_01Property =
            DependencyProperty.Register("CKBaCo_01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_01
        {
            get { return (string)GetValue(CKBaCo_01Property); }
            set { SetValue(CKBaCo_01Property, value); }
        }

        public static readonly DependencyProperty CKImSo_01Property =
            DependencyProperty.Register("CKImSo_01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_01
        {
            get { return (string)GetValue(CKImSo_01Property); }
            set { SetValue(CKImSo_01Property, value); }
        }

        public static readonly DependencyProperty CKMenu02Property =
            DependencyProperty.Register("CKMenu02", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu02
        {
            get { return (KeyValue)GetValue(CKMenu02Property); }
            set
            {
                SetValue(CKMenu02Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_02));
            }
        }

        public static readonly DependencyProperty CKLabel_02Property =
            DependencyProperty.Register("CKLabel_02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_02
        {
            get { return (string)GetValue(CKLabel_02Property); }
            set { SetValue(CKLabel_02Property, value); }
        }

        public static readonly DependencyProperty CKText_02Property =
            DependencyProperty.Register("CKText_02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_02
        {
            get { return (string)GetValue(CKText_02Property); }
            set { SetValue(CKText_02Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_02Property =
            DependencyProperty.Register("CKBaCo_02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_02
        {
            get { return (string)GetValue(CKBaCo_02Property); }
            set { SetValue(CKBaCo_02Property, value); }
        }

        public static readonly DependencyProperty CKImSo_02Property =
            DependencyProperty.Register("CKImSo_02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_02
        {
            get { return (string)GetValue(CKImSo_02Property); }
            set { SetValue(CKImSo_02Property, value); }
        }

        public static readonly DependencyProperty CKMenu03Property =
            DependencyProperty.Register("CKMenu03", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu03
        {
            get { return (KeyValue)GetValue(CKMenu03Property); }
            set
            {
                SetValue(CKMenu03Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_03));
            }
        }

        public static readonly DependencyProperty CKLabel_03Property =
            DependencyProperty.Register("CKLabel_03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_03
        {
            get { return (string)GetValue(CKLabel_03Property); }
            set { SetValue(CKLabel_03Property, value); }
        }

        public static readonly DependencyProperty CKText_03Property =
            DependencyProperty.Register("CKText_03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_03
        {
            get { return (string)GetValue(CKText_03Property); }
            set { SetValue(CKText_03Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_03Property =
            DependencyProperty.Register("CKBaCo_03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_03
        {
            get { return (string)GetValue(CKBaCo_03Property); }
            set { SetValue(CKBaCo_03Property, value); }
        }

        public static readonly DependencyProperty CKImSo_03Property =
            DependencyProperty.Register("CKImSo_03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_03
        {
            get { return (string)GetValue(CKImSo_03Property); }
            set { SetValue(CKImSo_03Property, value); }
        }

        public static readonly DependencyProperty CKMenu04Property =
            DependencyProperty.Register("CKMenu04", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu04
        {
            get { return (KeyValue)GetValue(CKMenu04Property); }
            set
            {
                SetValue(CKMenu04Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_04));
            }
        }

        public static readonly DependencyProperty CKLabel_04Property =
            DependencyProperty.Register("CKLabel_04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_04
        {
            get { return (string)GetValue(CKLabel_04Property); }
            set { SetValue(CKLabel_04Property, value); }
        }

        public static readonly DependencyProperty CKText_04Property =
            DependencyProperty.Register("CKText_04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_04
        {
            get { return (string)GetValue(CKText_04Property); }
            set { SetValue(CKText_04Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_04Property =
            DependencyProperty.Register("CKBaCo_04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_04
        {
            get { return (string)GetValue(CKBaCo_04Property); }
            set { SetValue(CKBaCo_04Property, value); }
        }

        public static readonly DependencyProperty CKImSo_04Property =
            DependencyProperty.Register("CKImSo_04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_04
        {
            get { return (string)GetValue(CKImSo_04Property); }
            set { SetValue(CKImSo_04Property, value); }
        }

        public static readonly DependencyProperty CKMenu10Property =
            DependencyProperty.Register("CKMenu10", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu10
        {
            get { return (KeyValue)GetValue(CKMenu10Property); }
            set
            {
                SetValue(CKMenu10Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_10));
            }
        }

        public static readonly DependencyProperty CKLabel_10Property =
            DependencyProperty.Register("CKLabel_10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_10
        {
            get { return (string)GetValue(CKLabel_10Property); }
            set { SetValue(CKLabel_10Property, value); }
        }

        public static readonly DependencyProperty CKText_10Property =
            DependencyProperty.Register("CKText_10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_10
        {
            get { return (string)GetValue(CKText_10Property); }
            set { SetValue(CKText_10Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_10Property =
            DependencyProperty.Register("CKBaCo_10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_10
        {
            get { return (string)GetValue(CKBaCo_10Property); }
            set { SetValue(CKBaCo_10Property, value); }
        }

        public static readonly DependencyProperty CKImSo_10Property =
            DependencyProperty.Register("CKImSo_10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_10
        {
            get { return (string)GetValue(CKImSo_10Property); }
            set { SetValue(CKImSo_10Property, value); }
        }

        public static readonly DependencyProperty CKMenu11Property =
            DependencyProperty.Register("CKMenu11", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu11
        {
            get { return (KeyValue)GetValue(CKMenu11Property); }
            set
            {
                SetValue(CKMenu11Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_11));
            }
        }

        public static readonly DependencyProperty CKLabel_11Property =
            DependencyProperty.Register("CKLabel_11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_11
        {
            get { return (string)GetValue(CKLabel_11Property); }
            set { SetValue(CKLabel_11Property, value); }
        }

        public static readonly DependencyProperty CKText_11Property =
            DependencyProperty.Register("CKText_11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_11
        {
            get { return (string)GetValue(CKText_11Property); }
            set { SetValue(CKText_11Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_11Property =
            DependencyProperty.Register("CKBaCo_11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_11
        {
            get { return (string)GetValue(CKBaCo_11Property); }
            set { SetValue(CKBaCo_11Property, value); }
        }

        public static readonly DependencyProperty CKImSo_11Property =
            DependencyProperty.Register("CKImSo_11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_11
        {
            get { return (string)GetValue(CKImSo_11Property); }
            set { SetValue(CKImSo_11Property, value); }
        }

        public static readonly DependencyProperty CKMenu12Property =
            DependencyProperty.Register("CKMenu12", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu12
        {
            get { return (KeyValue)GetValue(CKMenu12Property); }
            set { SetValue(CKMenu12Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_12)); }
        }

        public static readonly DependencyProperty CKLabel_12Property =
            DependencyProperty.Register("CKLabel_12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_12
        {
            get { return (string)GetValue(CKLabel_12Property); }
            set { SetValue(CKLabel_12Property, value); }
        }

        public static readonly DependencyProperty CKText_12Property =
            DependencyProperty.Register("CKText_12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_12
        {
            get { return (string)GetValue(CKText_12Property); }
            set { SetValue(CKText_12Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_12Property =
            DependencyProperty.Register("CKBaCo_12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_12
        {
            get { return (string)GetValue(CKBaCo_12Property); }
            set { SetValue(CKBaCo_12Property, value); }
        }

        public static readonly DependencyProperty CKImSo_12Property =
            DependencyProperty.Register("CKImSo_12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_12
        {
            get { return (string)GetValue(CKImSo_12Property); }
            set { SetValue(CKImSo_12Property, value); }
        }

        public static readonly DependencyProperty CKMenu13Property =
            DependencyProperty.Register("CKMenu13", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu13
        {
            get { return (KeyValue)GetValue(CKMenu13Property); }
            set { SetValue(CKMenu13Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_13)); }
        }

        public static readonly DependencyProperty CKLabel_13Property =
            DependencyProperty.Register("CKLabel_13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_13
        {
            get { return (string)GetValue(CKLabel_13Property); }
            set { SetValue(CKLabel_13Property, value); }
        }

        public static readonly DependencyProperty CKText_13Property =
            DependencyProperty.Register("CKText_13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_13
        {
            get { return (string)GetValue(CKText_13Property); }
            set { SetValue(CKText_13Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_13Property =
            DependencyProperty.Register("CKBaCo_13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_13
        {
            get { return (string)GetValue(CKBaCo_13Property); }
            set { SetValue(CKBaCo_13Property, value); }
        }

        public static readonly DependencyProperty CKImSo_13Property =
            DependencyProperty.Register("CKImSo_13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_13
        {
            get { return (string)GetValue(CKImSo_13Property); }
            set { SetValue(CKImSo_13Property, value); }
        }

        public static readonly DependencyProperty CKMenu14Property =
            DependencyProperty.Register("CKMenu14", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu14
        {
            get { return (KeyValue)GetValue(CKMenu14Property); }
            set { SetValue(CKMenu14Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_14)); }
        }

        public static readonly DependencyProperty CKLabel_14Property =
            DependencyProperty.Register("CKLabel_14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_14
        {
            get { return (string)GetValue(CKLabel_14Property); }
            set { SetValue(CKLabel_14Property, value); }
        }

        public static readonly DependencyProperty CKText_14Property =
            DependencyProperty.Register("CKText_14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_14
        {
            get { return (string)GetValue(CKText_14Property); }
            set { SetValue(CKText_14Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_14Property =
            DependencyProperty.Register("CKBaCo_14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_14
        {
            get { return (string)GetValue(CKBaCo_14Property); }
            set { SetValue(CKBaCo_14Property, value); }
        }

        public static readonly DependencyProperty CKImSo_14Property =
            DependencyProperty.Register("CKImSo_14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_14
        {
            get { return (string)GetValue(CKImSo_14Property); }
            set { SetValue(CKImSo_14Property, value); }
        }

        public static readonly DependencyProperty CKMenu20Property =
            DependencyProperty.Register("CKMenu20", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu20
        {
            get { return (KeyValue)GetValue(CKMenu20Property); }
            set { SetValue(CKMenu20Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_20)); }
        }

        public static readonly DependencyProperty CKLabel_20Property =
            DependencyProperty.Register("CKLabel_20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_20
        {
            get { return (string)GetValue(CKLabel_20Property); }
            set { SetValue(CKLabel_20Property, value); }
        }

        public static readonly DependencyProperty CKText_20Property =
            DependencyProperty.Register("CKText_20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_20
        {
            get { return (string)GetValue(CKText_20Property); }
            set { SetValue(CKText_20Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_20Property =
            DependencyProperty.Register("CKBaCo_20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_20
        {
            get { return (string)GetValue(CKBaCo_20Property); }
            set { SetValue(CKBaCo_20Property, value); }
        }

        public static readonly DependencyProperty CKImSo_20Property =
            DependencyProperty.Register("CKImSo_20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_20
        {
            get { return (string)GetValue(CKImSo_20Property); }
            set { SetValue(CKImSo_20Property, value); }
        }

        public static readonly DependencyProperty CKMenu21Property =
            DependencyProperty.Register("CKMenu21", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu21
        {
            get { return (KeyValue)GetValue(CKMenu21Property); }
            set { SetValue(CKMenu21Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_21)); }
        }

        public static readonly DependencyProperty CKLabel_21Property =
            DependencyProperty.Register("CKLabel_21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_21
        {
            get { return (string)GetValue(CKLabel_21Property); }
            set { SetValue(CKLabel_21Property, value); }
        }

        public static readonly DependencyProperty CKText_21Property =
            DependencyProperty.Register("CKText_21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_21
        {
            get { return (string)GetValue(CKText_21Property); }
            set { SetValue(CKText_21Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_21Property =
            DependencyProperty.Register("CKBaCo_21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_21
        {
            get { return (string)GetValue(CKBaCo_21Property); }
            set { SetValue(CKBaCo_21Property, value); }
        }

        public static readonly DependencyProperty CKImSo_21Property =
            DependencyProperty.Register("CKImSo_21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_21
        {
            get { return (string)GetValue(CKImSo_21Property); }
            set { SetValue(CKImSo_21Property, value); }
        }

        public static readonly DependencyProperty CKMenu22Property =
            DependencyProperty.Register("CKMenu22", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu22
        {
            get { return (KeyValue)GetValue(CKMenu22Property); }
            set { SetValue(CKMenu22Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_22)); }
        }

        public static readonly DependencyProperty CKLabel_22Property =
            DependencyProperty.Register("CKLabel_22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_22
        {
            get { return (string)GetValue(CKLabel_22Property); }
            set { SetValue(CKLabel_22Property, value); }
        }

        public static readonly DependencyProperty CKText_22Property =
            DependencyProperty.Register("CKText_22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_22
        {
            get { return (string)GetValue(CKText_22Property); }
            set { SetValue(CKText_22Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_22Property =
            DependencyProperty.Register("CKBaCo_22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_22
        {
            get { return (string)GetValue(CKBaCo_22Property); }
            set { SetValue(CKBaCo_22Property, value); }
        }

        public static readonly DependencyProperty CKImSo_22Property =
            DependencyProperty.Register("CKImSo_22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_22
        {
            get { return (string)GetValue(CKImSo_22Property); }
            set { SetValue(CKImSo_22Property, value); }
        }

        public static readonly DependencyProperty CKMenu23Property =
            DependencyProperty.Register("CKMenu23", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu23
        {
            get { return (KeyValue)GetValue(CKMenu23Property); }
            set { SetValue(CKMenu23Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_23)); }
        }

        public static readonly DependencyProperty CKLabel_23Property =
            DependencyProperty.Register("CKLabel_23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_23
        {
            get { return (string)GetValue(CKLabel_23Property); }
            set { SetValue(CKLabel_23Property, value); }
        }

        public static readonly DependencyProperty CKText_23Property =
            DependencyProperty.Register("CKText_23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_23
        {
            get { return (string)GetValue(CKText_23Property); }
            set { SetValue(CKText_23Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_23Property =
            DependencyProperty.Register("CKBaCo_23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_23
        {
            get { return (string)GetValue(CKBaCo_23Property); }
            set { SetValue(CKBaCo_23Property, value); }
        }

        public static readonly DependencyProperty CKImSo_23Property =
            DependencyProperty.Register("CKImSo_23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_23
        {
            get { return (string)GetValue(CKImSo_23Property); }
            set { SetValue(CKImSo_23Property, value); }
        }

        public static readonly DependencyProperty CKMenu24Property =
            DependencyProperty.Register("CKMenu24", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu24
        {
            get { return (KeyValue)GetValue(CKMenu24Property); }
            set { SetValue(CKMenu24Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_24)); }
        }

        public static readonly DependencyProperty CKLabel_24Property =
            DependencyProperty.Register("CKLabel_24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_24
        {
            get { return (string)GetValue(CKLabel_24Property); }
            set { SetValue(CKLabel_24Property, value); }
        }

        public static readonly DependencyProperty CKText_24Property =
            DependencyProperty.Register("CKText_24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_24
        {
            get { return (string)GetValue(CKText_24Property); }
            set { SetValue(CKText_24Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_24Property =
            DependencyProperty.Register("CKBaCo_24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_24
        {
            get { return (string)GetValue(CKBaCo_24Property); }
            set { SetValue(CKBaCo_24Property, value); }
        }

        public static readonly DependencyProperty CKImSo_24Property =
            DependencyProperty.Register("CKImSo_24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_24
        {
            get { return (string)GetValue(CKImSo_24Property); }
            set { SetValue(CKImSo_24Property, value); }
        }

        public static readonly DependencyProperty CKMenu30Property =
            DependencyProperty.Register("CKMenu30", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu30
        {
            get { return (KeyValue)GetValue(CKMenu30Property); }
            set { SetValue(CKMenu30Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_30)); }
        }

        public static readonly DependencyProperty CKLabel_30Property =
            DependencyProperty.Register("CKLabel_30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_30
        {
            get { return (string)GetValue(CKLabel_30Property); }
            set { SetValue(CKLabel_30Property, value); }
        }

        public static readonly DependencyProperty CKText_30Property =
            DependencyProperty.Register("CKText_30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_30
        {
            get { return (string)GetValue(CKText_30Property); }
            set { SetValue(CKText_30Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_30Property =
            DependencyProperty.Register("CKBaCo_30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_30
        {
            get { return (string)GetValue(CKBaCo_30Property); }
            set { SetValue(CKBaCo_30Property, value); }
        }

        public static readonly DependencyProperty CKImSo_30Property =
            DependencyProperty.Register("CKImSo_30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_30
        {
            get { return (string)GetValue(CKImSo_30Property); }
            set { SetValue(CKImSo_30Property, value); }
        }

        public static readonly DependencyProperty CKMenu31Property =
            DependencyProperty.Register("CKMenu31", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu31
        {
            get { return (KeyValue)GetValue(CKMenu31Property); }
            set { SetValue(CKMenu31Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_31)); }
        }

        public static readonly DependencyProperty CKLabel_31Property =
            DependencyProperty.Register("CKLabel_31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_31
        {
            get { return (string)GetValue(CKLabel_31Property); }
            set { SetValue(CKLabel_31Property, value); }
        }

        public static readonly DependencyProperty CKText_31Property =
            DependencyProperty.Register("CKText_31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_31
        {
            get { return (string)GetValue(CKText_31Property); }
            set { SetValue(CKText_31Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_31Property =
            DependencyProperty.Register("CKBaCo_31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_31
        {
            get { return (string)GetValue(CKBaCo_31Property); }
            set { SetValue(CKBaCo_31Property, value); }
        }

        public static readonly DependencyProperty CKImSo_31Property =
            DependencyProperty.Register("CKImSo_31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_31
        {
            get { return (string)GetValue(CKImSo_31Property); }
            set { SetValue(CKImSo_31Property, value); }
        }

        public static readonly DependencyProperty CKMenu32Property =
            DependencyProperty.Register("CKMenu32", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu32
        {
            get { return (KeyValue)GetValue(CKMenu32Property); }
            set { SetValue(CKMenu32Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_32)); }
        }

        public static readonly DependencyProperty CKLabel_32Property =
            DependencyProperty.Register("CKLabel_32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_32
        {
            get { return (string)GetValue(CKLabel_32Property); }
            set { SetValue(CKLabel_32Property, value); }
        }

        public static readonly DependencyProperty CKText_32Property =
            DependencyProperty.Register("CKText_32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_32
        {
            get { return (string)GetValue(CKText_32Property); }
            set { SetValue(CKText_32Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_32Property =
            DependencyProperty.Register("CKBaCo_32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_32
        {
            get { return (string)GetValue(CKBaCo_32Property); }
            set { SetValue(CKBaCo_32Property, value); }
        }

        public static readonly DependencyProperty CKImSo_32Property =
            DependencyProperty.Register("CKImSo_32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_32
        {
            get { return (string)GetValue(CKImSo_32Property); }
            set { SetValue(CKImSo_32Property, value); }
        }

        public static readonly DependencyProperty CKMenu33Property =
            DependencyProperty.Register("CKMenu33", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu33
        {
            get { return (KeyValue)GetValue(CKMenu33Property); }
            set { SetValue(CKMenu33Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_33)); }
        }

        public static readonly DependencyProperty CKLabel_33Property =
            DependencyProperty.Register("CKLabel_33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_33
        {
            get { return (string)GetValue(CKLabel_33Property); }
            set { SetValue(CKLabel_33Property, value); }
        }

        public static readonly DependencyProperty CKText_33Property =
            DependencyProperty.Register("CKText_33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_33
        {
            get { return (string)GetValue(CKText_33Property); }
            set { SetValue(CKText_33Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_33Property =
            DependencyProperty.Register("CKBaCo_33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_33
        {
            get { return (string)GetValue(CKBaCo_33Property); }
            set { SetValue(CKBaCo_33Property, value); }
        }

        public static readonly DependencyProperty CKImSo_33Property =
            DependencyProperty.Register("CKImSo_33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_33
        {
            get { return (string)GetValue(CKImSo_33Property); }
            set { SetValue(CKImSo_33Property, value); }
        }

        public static readonly DependencyProperty CKMenu34Property =
            DependencyProperty.Register("CKMenu34", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu34
        {
            get { return (KeyValue)GetValue(CKMenu34Property); }
            set { SetValue(CKMenu34Property, new KeyValue(Enums.FunctionKeys.CommuniKate, CKKeCo_34)); }
        }

        public static readonly DependencyProperty CKLabel_34Property =
            DependencyProperty.Register("CKLabel_34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKLabel_34
        {
            get { return (string)GetValue(CKLabel_34Property); }
            set { SetValue(CKLabel_34Property, value); }
        }

        public static readonly DependencyProperty CKText_34Property =
            DependencyProperty.Register("CKText_34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKText_34
        {
            get { return (string)GetValue(CKText_34Property); }
            set { SetValue(CKText_34Property, value); }
        }

        public static readonly DependencyProperty CKBaCo_34Property =
            DependencyProperty.Register("CKBaCo_34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKBaCo_34
        {
            get { return (string)GetValue(CKBaCo_34Property); }
            set { SetValue(CKBaCo_34Property, value); }
        }

        public static readonly DependencyProperty CKImSo_34Property =
            DependencyProperty.Register("CKImSo_34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_34
        {
            get { return (string)GetValue(CKImSo_34Property); }
            set { SetValue(CKImSo_34Property, value); }
        }

        public bool CKMenuKey_00
        {
            get { return (bool)GetValue(CKMenuKey_00Property); }
            set { SetValue(CKMenuKey_00Property, value); }
        }

        public bool CKMenuKey_01
        {
            get { return (bool)GetValue(CKMenuKey_01Property); }
            set { SetValue(CKMenuKey_01Property, value); }
        }

        public bool CKMenuKey_02
        {
            get { return (bool)GetValue(CKMenuKey_02Property); }
            set { SetValue(CKMenuKey_02Property, value); }
        }

        public bool CKMenuKey_03
        {
            get { return (bool)GetValue(CKMenuKey_03Property); }
            set { SetValue(CKMenuKey_03Property, value); }
        }

        public bool CKMenuKey_04
        {
            get { return (bool)GetValue(CKMenuKey_04Property); }
            set { SetValue(CKMenuKey_04Property, value); }
        }


        public bool CKMenuKey_10
        {
            get { return (bool)GetValue(CKMenuKey_10Property); }
            set { SetValue(CKMenuKey_10Property, value); }
        }

        public bool CKMenuKey_11
        {
            get { return (bool)GetValue(CKMenuKey_11Property); }
            set { SetValue(CKMenuKey_11Property, value); }
        }

        public bool CKMenuKey_12
        {
            get { return (bool)GetValue(CKMenuKey_12Property); }
            set { SetValue(CKMenuKey_12Property, value); }
        }

        public bool CKMenuKey_13
        {
            get { return (bool)GetValue(CKMenuKey_13Property); }
            set { SetValue(CKMenuKey_13Property, value); }
        }

        public bool CKMenuKey_14
        {
            get { return (bool)GetValue(CKMenuKey_14Property); }
            set { SetValue(CKMenuKey_14Property, value); }
        }


        public bool CKMenuKey_20
        {
            get { return (bool)GetValue(CKMenuKey_20Property); }
            set { SetValue(CKMenuKey_20Property, value); }
        }

        public bool CKMenuKey_21
        {
            get { return (bool)GetValue(CKMenuKey_21Property); }
            set { SetValue(CKMenuKey_21Property, value); }
        }

        public bool CKMenuKey_22
        {
            get { return (bool)GetValue(CKMenuKey_22Property); }
            set { SetValue(CKMenuKey_22Property, value); }
        }

        public bool CKMenuKey_23
        {
            get { return (bool)GetValue(CKMenuKey_23Property); }
            set { SetValue(CKMenuKey_23Property, value); }
        }

        public bool CKMenuKey_24
        {
            get { return (bool)GetValue(CKMenuKey_24Property); }
            set { SetValue(CKMenuKey_24Property, value); }
        }


        public bool CKMenuKey_30
        {
            get { return (bool)GetValue(CKMenuKey_30Property); }
            set { SetValue(CKMenuKey_30Property, value); }
        }

        public bool CKMenuKey_31
        {
            get { return (bool)GetValue(CKMenuKey_31Property); }
            set { SetValue(CKMenuKey_31Property, value); }
        }

        public bool CKMenuKey_32
        {
            get { return (bool)GetValue(CKMenuKey_32Property); }
            set { SetValue(CKMenuKey_32Property, value); }
        }

        public bool CKMenuKey_33
        {
            get { return (bool)GetValue(CKMenuKey_33Property); }
            set { SetValue(CKMenuKey_33Property, value); }
        }

        public bool CKMenuKey_34
        {
            get { return (bool)GetValue(CKMenuKey_34Property); }
            set { SetValue(CKMenuKey_34Property, value); }
        }


        public static readonly DependencyProperty CKMenuKey_00Property =
            DependencyProperty.Register("CKMenuKey_00", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CKMenuKey_01Property =
            DependencyProperty.Register("CKMenuKey_01", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CKMenuKey_02Property =
            DependencyProperty.Register("CKMenuKey_02", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CKMenuKey_03Property =
            DependencyProperty.Register("CKMenuKey_03", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));

        public static readonly DependencyProperty CKMenuKey_04Property =
            DependencyProperty.Register("CKMenuKey_04", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_10Property =
            DependencyProperty.Register("CKMenuKey_10", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_11Property =
            DependencyProperty.Register("CKMenuKey_11", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_12Property =
            DependencyProperty.Register("CKMenuKey_12", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_13Property =
            DependencyProperty.Register("CKMenuKey_13", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_14Property =
            DependencyProperty.Register("CKMenuKey_14", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_20Property =
            DependencyProperty.Register("CKMenuKey_20", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_21Property =
            DependencyProperty.Register("CKMenuKey_21", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_22Property =
            DependencyProperty.Register("CKMenuKey_22", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_23Property =
            DependencyProperty.Register("CKMenuKey_23", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_24Property =
            DependencyProperty.Register("CKMenuKey_24", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_30Property =
            DependencyProperty.Register("CKMenuKey_30", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_31Property =
            DependencyProperty.Register("CKMenuKey_31", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_32Property =
            DependencyProperty.Register("CKMenuKey_32", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_33Property =
            DependencyProperty.Register("CKMenuKey_33", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        
        public static readonly DependencyProperty CKMenuKey_34Property =
            DependencyProperty.Register("CKMenuKey_34", typeof(bool), typeof(Key), new PropertyMetadata(default(bool)));
        

        public static readonly DependencyProperty CKKeCo_00Property =
            DependencyProperty.Register("CKKeCo_00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_00
        {
            get { return (string)GetValue(CKKeCo_00Property); }
            set { SetValue(CKKeCo_00Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_01Property =
            DependencyProperty.Register("CKKeCo_01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_01
        {
            get { return (string)GetValue(CKKeCo_01Property); }
            set { SetValue(CKKeCo_01Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_02Property =
            DependencyProperty.Register("CKKeCo_02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_02
        {
            get { return (string)GetValue(CKKeCo_02Property); }
            set { SetValue(CKKeCo_02Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_03Property =
            DependencyProperty.Register("CKKeCo_03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_03
        {
            get { return (string)GetValue(CKKeCo_03Property); }
            set { SetValue(CKKeCo_03Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_04Property =
            DependencyProperty.Register("CKKeCo_04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_04
        {
            get { return (string)GetValue(CKKeCo_04Property); }
            set { SetValue(CKKeCo_04Property, value); }
        }


        public static readonly DependencyProperty CKKeCo_10Property =
            DependencyProperty.Register("CKKeCo_10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_10
        {
            get { return (string)GetValue(CKKeCo_10Property); }
            set { SetValue(CKKeCo_10Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_11Property =
            DependencyProperty.Register("CKKeCo_11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_11
        {
            get { return (string)GetValue(CKKeCo_11Property); }
            set { SetValue(CKKeCo_11Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_12Property =
            DependencyProperty.Register("CKKeCo_12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_12
        {
            get { return (string)GetValue(CKKeCo_12Property); }
            set { SetValue(CKKeCo_12Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_13Property =
            DependencyProperty.Register("CKKeCo_13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_13
        {
            get { return (string)GetValue(CKKeCo_13Property); }
            set { SetValue(CKKeCo_13Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_14Property =
            DependencyProperty.Register("CKKeCo_14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_14
        {
            get { return (string)GetValue(CKKeCo_14Property); }
            set { SetValue(CKKeCo_14Property, value); }
        }


        public static readonly DependencyProperty CKKeCo_20Property =
            DependencyProperty.Register("CKKeCo_20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_20
        {
            get { return (string)GetValue(CKKeCo_20Property); }
            set { SetValue(CKKeCo_20Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_21Property =
            DependencyProperty.Register("CKKeCo_21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_21
        {
            get { return (string)GetValue(CKKeCo_21Property); }
            set { SetValue(CKKeCo_21Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_22Property =
            DependencyProperty.Register("CKKeCo_22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_22
        {
            get { return (string)GetValue(CKKeCo_22Property); }
            set { SetValue(CKKeCo_22Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_23Property =
            DependencyProperty.Register("CKKeCo_23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_23
        {
            get { return (string)GetValue(CKKeCo_23Property); }
            set { SetValue(CKKeCo_23Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_24Property =
            DependencyProperty.Register("CKKeCo_24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_24
        {
            get { return (string)GetValue(CKKeCo_24Property); }
            set { SetValue(CKKeCo_24Property, value); }
        }


        public static readonly DependencyProperty CKKeCo_30Property =
            DependencyProperty.Register("CKKeCo_30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_30
        {
            get { return (string)GetValue(CKKeCo_30Property); }
            set { SetValue(CKKeCo_30Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_31Property =
            DependencyProperty.Register("CKKeCo_31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_31
        {
            get { return (string)GetValue(CKKeCo_31Property); }
            set { SetValue(CKKeCo_31Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_32Property =
            DependencyProperty.Register("CKKeCo_32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_32
        {
            get { return (string)GetValue(CKKeCo_32Property); }
            set { SetValue(CKKeCo_32Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_33Property =
            DependencyProperty.Register("CKKeCo_33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_33
        {
            get { return (string)GetValue(CKKeCo_33Property); }
            set { SetValue(CKKeCo_33Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_34Property =
            DependencyProperty.Register("CKKeCo_34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKKeCo_34
        {
            get { return (string)GetValue(CKKeCo_34Property); }
            set { SetValue(CKKeCo_34Property, value); }
        }
    }
}
