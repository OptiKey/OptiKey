using System;
using System.Windows;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
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
        //public string CKPageFile = @"F:\soft\eye tracking\osk\CommuniKate\pageset\boards\toppage.obf";

        public class CKOBF
        {
            public List<Buttons> buttons { get; set; }
        }

        public class Buttons
        {
            //[JsonProperty("buttons")]
            public string background_color { get; set; }
            public string border_color { get; set; }
            public string id { get; set; }
            public string image_id { get; set; }
            public string label { get; set; }
            public Load_board load_board { get; set; }
        }

        public class Load_board
        {
            public string path { get; set; }
        }
        //*
        public static readonly DependencyProperty CKPageFileProperty =
        DependencyProperty.Register("CKPageFile", typeof(string), typeof(CK20Page), new PropertyMetadata(default(string), CKPageFileChanged));

        private static void CKPageFileChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                string pagefile = value;
                //Depending on parametrized case, change the value
                Log.InfoFormat("Trying to read page file: {0}.", pagefile);
                if (pagefile == null)
                    pagefile = "./Resources/CommuniKateBoards/toppage.obf"; //questions.obf";
                else if (!pagefile.StartsWith("./Resources/CommuniKateBoards/"))
                    pagefile = "./Resources/CommuniKateBoards/" + pagefile + ".obf";
                {
                    //value = "./Resources/CommuniKateBoards/" + value + ".obf";
                    Log.InfoFormat("Page file to read: {0}.", pagefile);
                    string contents = new StreamReader(pagefile, Encoding.UTF8).ReadToEnd();
                    CKOBF CKPageOBF = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<CKOBF>(contents);
                    //Log.InfoFormat("raw json file:\n{0}", contents);
                    int ButtonCount = CKPageOBF.buttons.Count();
                    Log.InfoFormat("Page contains {0} buttons.", ButtonCount -3);
                    /*
                    //Buttons BlankButton;// = new Button(bac);
                    //BlankButton.background_color = "rgb(0,0,0)";
                    for (int i = 0; i < 20 - ButtonCount; ++i)
                    {
                        CKPageOBF.buttons.Add(BlankButton);
                    }
                    */
                    //Log.InfoFormat("Page:{0}\n", CKPageOBF);
                    int buttonid = 3;
                    string colour;
                    string image;
                    string path;
                    bool ismenukey;
                    string text;
                    Load_board board;
                    string defaultcolour = "#000000";
                    string defaultimage = ".png";
                    string defaultpath = null;
                    bool defaultismenukey = false;
                    string defaulttext = "";
                    Load_board defaultboard = null;
                    //Settings.Default.CommuniKateKeyboardCurrentContext = "toppage,toppage".Split(',').ToList();
                    //if (Settings.Default.CommuniKateKeyboardCurrentContext != null)
                    //    Settings.Default.CommuniKateKeyboardCurrentContext.Clear();
                    //else
                    //    Settings.Default.CommuniKateKeyboardCurrentContext = new List<string>;
                    /*
                    colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                    key.CKBaCo_00 = colour.Substring(0, colour.Length - 1);
                    key.CKText_00 = CKPageOBF.buttons.ElementAt(buttonid).label;
                    image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                    key.CKImSo_00 = image.Substring(0, image.Length - 4);
                    board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        Log.InfoFormat("Button {0} is a menu key for board {1}.", buttonid - 2, path);
                        key.CKMenuKey_00 = true;
                        key.CKKeCo_00 = path;
                        //Settings.Default.CommuniKateKeyboardCurrentContext00 = path;
                    }
                    //Settings.Default.CommuniKateKeyboardCurrentContext.Add(context.ElementAt(0).path);
                    //Settings.Default.CommuniKateKeyboardCurrentContext.Add(path);

                    ++buttonid;
                    colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                    key.CKBaCo_01 = colour.Substring(0, colour.Length - 1);
                    key.CKText_01 = CKPageOBF.buttons.ElementAt(buttonid).label;
                    image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                    key.CKImSo_01 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7); //, context.path.Length - 4);
                        key.CKMenuKey_01 = true;
                        key.CKKeCo_01 = path;
                        //Settings.Default.CommuniKateKeyboardCurrentContext01 = path;
                    }
                    //Settings.Default.CommuniKateKeyboardCurrentContext.Add(path);
                    //Settings.Default.CommuniKateKeyboardCurrentContext = paths.Split(',').ToList();
                    */

                    //++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_00 = colour.Substring(0, colour.Length - 1);
                    key.CKText_00 = text;
                    key.CKImSo_00 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_00 = ismenukey;
                    key.CKKeCo_00 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_01 = colour.Substring(0, colour.Length - 1);
                    key.CKText_01 = text;
                    key.CKImSo_01 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_01 = ismenukey;
                    key.CKKeCo_01 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_02 = colour.Substring(0, colour.Length - 1);
                    key.CKText_02 = text;
                    key.CKImSo_02 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_02 = ismenukey;
                        key.CKKeCo_02 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_03 = colour.Substring(0, colour.Length - 1);
                    key.CKText_03 = text;
                    key.CKImSo_03 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_03 = ismenukey;
                    key.CKKeCo_03 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_04 = colour.Substring(0, colour.Length - 1);
                    key.CKText_04 = text;
                    key.CKImSo_04 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_04 = ismenukey;
                        key.CKKeCo_04 = path;


                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_10 = colour.Substring(0, colour.Length - 1);
                    key.CKText_10 = text;
                    key.CKImSo_10 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_10 = ismenukey;
                        key.CKKeCo_10 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_11 = colour.Substring(0, colour.Length - 1);
                    key.CKText_11 = text;
                    key.CKImSo_11 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_11 = ismenukey;
                        key.CKKeCo_11 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_12 = colour.Substring(0, colour.Length - 1);
                    key.CKText_12 = text;
                    key.CKImSo_12 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_12 = ismenukey;
                        key.CKKeCo_12 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_13 = colour.Substring(0, colour.Length - 1);
                    key.CKText_13 = text;
                    key.CKImSo_13 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_13 = ismenukey;
                        key.CKKeCo_13 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_14 = colour.Substring(0, colour.Length - 1);
                    key.CKText_14 = text;
                    key.CKImSo_14 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_14 = ismenukey;
                        key.CKKeCo_14 = path;


                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_20 = colour.Substring(0, colour.Length - 1);
                    key.CKText_20 = text;
                    key.CKImSo_20 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_20 = ismenukey;
                        key.CKKeCo_20 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_21 = colour.Substring(0, colour.Length - 1);
                    key.CKText_21 = text;
                    key.CKImSo_21 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_21 = ismenukey;
                        key.CKKeCo_21 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_22 = colour.Substring(0, colour.Length - 1);
                    key.CKText_22 = text;
                    key.CKImSo_22 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_22 = ismenukey;
                        key.CKKeCo_22 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_23 = colour.Substring(0, colour.Length - 1);
                    key.CKText_23 = text;
                    key.CKImSo_23 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_23 = ismenukey;
                        key.CKKeCo_23 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_24 = colour.Substring(0, colour.Length - 1);
                    key.CKText_24 = text;
                    key.CKImSo_24 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_24 = ismenukey;
                        key.CKKeCo_24 = path;


                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_30 = colour.Substring(0, colour.Length - 1);
                    key.CKText_30 = text;
                    key.CKImSo_30 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_30 = ismenukey;
                        key.CKKeCo_30 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_31 = colour.Substring(0, colour.Length - 1);
                    key.CKText_31 = text;
                    key.CKImSo_31 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_31 = ismenukey;
                        key.CKKeCo_31 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_32 = colour.Substring(0, colour.Length - 1);
                    key.CKText_32 = text;
                    key.CKImSo_32 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_32 = ismenukey;
                        key.CKKeCo_32 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_33 = colour.Substring(0, colour.Length - 1);
                    key.CKText_33 = text;
                    key.CKImSo_33 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_33 = ismenukey;
                        key.CKKeCo_33 = path;

                    ++buttonid;
                    if (buttonid < ButtonCount)
                    {
                        colour = CKPageOBF.buttons.ElementAt(buttonid).background_color.Substring(4);
                        image = CKPageOBF.buttons.ElementAt(buttonid).image_id;
                        board = CKPageOBF.buttons.ElementAt(buttonid).load_board;
                        text = CKPageOBF.buttons.ElementAt(buttonid).label;
                    }
                    else
                    {
                        colour = defaultcolour;
                        image = defaultimage;
                        board = defaultboard;
                        text = defaulttext;
                    }
                    key.CKBaCo_34 = colour.Substring(0, colour.Length - 1);
                    key.CKText_34 = text;
                    key.CKImSo_34 = image.Substring(0, image.Length - 4);
                    if (board != null && board.path != null)
                    {
                        path = board.path.Substring(7);
                        ismenukey = true;
                    }
                    else
                    {
                        path = defaultpath;
                        ismenukey = defaultismenukey;
                    }
                    key.CKMenuKey_34 = ismenukey;
                        key.CKKeCo_34 = path;
                }
                //else
                {
                    //Log.Error("Page json file to read is null.");
                }
            }
        }

        public string CKPageFile
        {
            get { return (string)GetValue(CKPageFileProperty); }
            set { SetValue(CKPageFileProperty,// value); }
                                (value != null) ? "./Resources/CommuniKateBoards/" + value + ".obf" 
                                : "./Resources/CommuniKateBoards/toppage.obf"); }
        }//*/

        public CK20Page()
        {
            CKPageFile = "toppage"; // Settings.Default.KeyboardCurrentContext;// "./Resources/CommuniKateBoards/questions.obf"; 
            // "{Binding KeyboardCurrentContext, Mode=Default}"
            //if (Settings.Default.CommuniKateKeyboardCurrentContext == null)
            //    Settings.Default.CommuniKateKeyboardCurrentContext = "questions";

            InitializeComponent();
            //OnLoaded();
        }

        #region On Loaded

        private void OnLoaded()
        {
            //onUnloaded = new CompositeDisposable();

        }

        #endregion

        public CKOBF readpage(string file)
        {
            if (file == null || file == "")
                file = "./Resources/CommuniKateBoards/toppage.obf";
            string contents = new StreamReader(file, Encoding.UTF8).ReadToEnd();
            CKOBF page = new System.Web.Script.Serialization.JavaScriptSerializer().Deserialize<CKOBF>(contents);
            //Log.InfoFormat("raw json file:\n{0}", contents);
            Log.InfoFormat("Page contains {0} buttons.", page.buttons.Count());
            return page;
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

        public static readonly DependencyProperty CKMenu00Property =
            DependencyProperty.Register("CKMenu00", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu00
        {
            get { return (KeyValue)GetValue(CKMenu00Property); }
            set { SetValue(CKMenu00Property, value); }
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
            set { SetValue(CKBaCo_00Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_00Property =
            DependencyProperty.Register("CKImSo_00", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_00
        {
            get { return (string)GetValue(CKImSo_00Property); }
            set { SetValue(CKImSo_00Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu01Property =
            DependencyProperty.Register("CKMenu01", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu01
        {
            get { return (KeyValue)GetValue(CKMenu01Property); }
            set { SetValue(CKMenu01Property, value); }
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
            set { SetValue(CKBaCo_01Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_01Property =
            DependencyProperty.Register("CKImSo_01", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_01
        {
            get { return (string)GetValue(CKImSo_01Property); }
            set { SetValue(CKImSo_01Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu02Property =
            DependencyProperty.Register("CKMenu02", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu02
        {
            get { return (KeyValue)GetValue(CKMenu02Property); }
            set { SetValue(CKMenu02Property, value); }
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
            set { SetValue(CKBaCo_02Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_02Property =
            DependencyProperty.Register("CKImSo_02", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_02
        {
            get { return (string)GetValue(CKImSo_02Property); }
            set { SetValue(CKImSo_02Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu03Property =
            DependencyProperty.Register("CKMenu03", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu03
        {
            get { return (KeyValue)GetValue(CKMenu03Property); }
            set { SetValue(CKMenu03Property, value); }
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
            set { SetValue(CKBaCo_03Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_03Property =
            DependencyProperty.Register("CKImSo_03", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_03
        {
            get { return (string)GetValue(CKImSo_03Property); }
            set { SetValue(CKImSo_03Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu04Property =
            DependencyProperty.Register("CKMenu04", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu04
        {
            get { return (KeyValue)GetValue(CKMenu04Property); }
            set { SetValue(CKMenu04Property, value); }
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
            set { SetValue(CKBaCo_04Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_04Property =
            DependencyProperty.Register("CKImSo_04", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_04
        {
            get { return (string)GetValue(CKImSo_04Property); }
            set { SetValue(CKImSo_04Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu10Property =
            DependencyProperty.Register("CKMenu10", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu10
        {
            get { return (KeyValue)GetValue(CKMenu10Property); }
            set { SetValue(CKMenu10Property, value); }
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
            set { SetValue(CKBaCo_10Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_10Property =
            DependencyProperty.Register("CKImSo_10", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_10
        {
            get { return (string)GetValue(CKImSo_10Property); }
            set { SetValue(CKImSo_10Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu11Property =
            DependencyProperty.Register("CKMenu11", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu11
        {
            get { return (KeyValue)GetValue(CKMenu11Property); }
            set { SetValue(CKMenu11Property, value); }
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
            set { SetValue(CKBaCo_11Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_11Property =
            DependencyProperty.Register("CKImSo_11", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_11
        {
            get { return (string)GetValue(CKImSo_11Property); }
            set { SetValue(CKImSo_11Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu12Property =
            DependencyProperty.Register("CKMenu12", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu12
        {
            get { return (KeyValue)GetValue(CKMenu12Property); }
            set { SetValue(CKMenu12Property, value); }
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
            set { SetValue(CKBaCo_12Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_12Property =
            DependencyProperty.Register("CKImSo_12", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_12
        {
            get { return (string)GetValue(CKImSo_12Property); }
            set { SetValue(CKImSo_12Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu13Property =
            DependencyProperty.Register("CKMenu13", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu13
        {
            get { return (KeyValue)GetValue(CKMenu13Property); }
            set { SetValue(CKMenu13Property, value); }
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
            set { SetValue(CKBaCo_13Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_13Property =
            DependencyProperty.Register("CKImSo_13", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_13
        {
            get { return (string)GetValue(CKImSo_13Property); }
            set { SetValue(CKImSo_13Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu14Property =
            DependencyProperty.Register("CKMenu14", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu14
        {
            get { return (KeyValue)GetValue(CKMenu14Property); }
            set { SetValue(CKMenu14Property, value); }
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
            set { SetValue(CKBaCo_14Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_14Property =
            DependencyProperty.Register("CKImSo_14", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_14
        {
            get { return (string)GetValue(CKImSo_14Property); }
            set { SetValue(CKImSo_14Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu20Property =
            DependencyProperty.Register("CKMenu20", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu20
        {
            get { return (KeyValue)GetValue(CKMenu20Property); }
            set { SetValue(CKMenu20Property, value); }
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
            set { SetValue(CKBaCo_20Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_20Property =
            DependencyProperty.Register("CKImSo_20", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_20
        {
            get { return (string)GetValue(CKImSo_20Property); }
            set { SetValue(CKImSo_20Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu21Property =
            DependencyProperty.Register("CKMenu21", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu21
        {
            get { return (KeyValue)GetValue(CKMenu21Property); }
            set { SetValue(CKMenu21Property, value); }
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
            set { SetValue(CKBaCo_21Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_21Property =
            DependencyProperty.Register("CKImSo_21", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_21
        {
            get { return (string)GetValue(CKImSo_21Property); }
            set { SetValue(CKImSo_21Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu22Property =
            DependencyProperty.Register("CKMenu22", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu22
        {
            get { return (KeyValue)GetValue(CKMenu22Property); }
            set { SetValue(CKMenu22Property, value); }
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
            set { SetValue(CKBaCo_22Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_22Property =
            DependencyProperty.Register("CKImSo_22", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_22
        {
            get { return (string)GetValue(CKImSo_22Property); }
            set { SetValue(CKImSo_22Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu23Property =
            DependencyProperty.Register("CKMenu23", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu23
        {
            get { return (KeyValue)GetValue(CKMenu23Property); }
            set { SetValue(CKMenu23Property, value); }
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
            set { SetValue(CKBaCo_23Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_23Property =
            DependencyProperty.Register("CKImSo_23", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_23
        {
            get { return (string)GetValue(CKImSo_23Property); }
            set { SetValue(CKImSo_23Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu24Property =
            DependencyProperty.Register("CKMenu24", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu24
        {
            get { return (KeyValue)GetValue(CKMenu24Property); }
            set { SetValue(CKMenu24Property, value); }
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
            set { SetValue(CKBaCo_24Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_24Property =
            DependencyProperty.Register("CKImSo_24", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_24
        {
            get { return (string)GetValue(CKImSo_24Property); }
            set { SetValue(CKImSo_24Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu30Property =
            DependencyProperty.Register("CKMenu30", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu30
        {
            get { return (KeyValue)GetValue(CKMenu30Property); }
            set { SetValue(CKMenu30Property, value); }
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
            set { SetValue(CKBaCo_30Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_30Property =
            DependencyProperty.Register("CKImSo_30", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_30
        {
            get { return (string)GetValue(CKImSo_30Property); }
            set { SetValue(CKImSo_30Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu31Property =
            DependencyProperty.Register("CKMenu31", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu31
        {
            get { return (KeyValue)GetValue(CKMenu31Property); }
            set { SetValue(CKMenu31Property, value); }
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
            set { SetValue(CKBaCo_31Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_31Property =
            DependencyProperty.Register("CKImSo_31", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_31
        {
            get { return (string)GetValue(CKImSo_31Property); }
            set { SetValue(CKImSo_31Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu32Property =
            DependencyProperty.Register("CKMenu32", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu32
        {
            get { return (KeyValue)GetValue(CKMenu32Property); }
            set { SetValue(CKMenu32Property, value); }
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
            set { SetValue(CKBaCo_32Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_32Property =
            DependencyProperty.Register("CKImSo_32", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_32
        {
            get { return (string)GetValue(CKImSo_32Property); }
            set { SetValue(CKImSo_32Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu33Property =
            DependencyProperty.Register("CKMenu33", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu33
        {
            get { return (KeyValue)GetValue(CKMenu33Property); }
            set { SetValue(CKMenu33Property, value); }
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
            set { SetValue(CKBaCo_33Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_33Property =
            DependencyProperty.Register("CKImSo_33", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_33
        {
            get { return (string)GetValue(CKImSo_33Property); }
            set { SetValue(CKImSo_33Property, "/Resources/CommuniKateImages/" + value + ".png"); }
        }

        public static readonly DependencyProperty CKMenu34Property =
            DependencyProperty.Register("CKMenu34", typeof(KeyValue), typeof(Key), new PropertyMetadata(default(KeyValue)));

        public KeyValue CKMenu34
        {
            get { return (KeyValue)GetValue(CKMenu34Property); }
            set { SetValue(CKMenu34Property, value); }
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
            set { SetValue(CKBaCo_34Property, dec2hex(value)); }
        }

        public static readonly DependencyProperty CKImSo_34Property =
            DependencyProperty.Register("CKImSo_34", typeof(string), typeof(Key), new PropertyMetadata(default(string)));

        public string CKImSo_34
        {
            get { return (string)GetValue(CKImSo_34Property); }
            set { SetValue(CKImSo_34Property, "/Resources/CommuniKateImages/" + value + ".png"); }
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
            DependencyProperty.Register("CKKeCo_00", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_00Changed));

        private static void CKKeCo_00Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_00 = value;
            }
        }

        public string CKKeCo_00
        {
            get { return (string)GetValue(CKKeCo_00Property); }
            set { SetValue(CKKeCo_00Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_01Property =
            DependencyProperty.Register("CKKeCo_01", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_01Changed));

        private static void CKKeCo_01Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_01 = value;
            }
        }

        public string CKKeCo_01
        {
            get { return (string)GetValue(CKKeCo_01Property); }
            set { SetValue(CKKeCo_01Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_02Property =
            DependencyProperty.Register("CKKeCo_02", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_02Changed));

        private static void CKKeCo_02Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_02 = value;
            }
        }

        public string CKKeCo_02
        {
            get { return (string)GetValue(CKKeCo_02Property); }
            set { SetValue(CKKeCo_02Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_03Property =
            DependencyProperty.Register("CKKeCo_03", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_03Changed));

        private static void CKKeCo_03Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_03 = value;
            }
        }

        public string CKKeCo_03
        {
            get { return (string)GetValue(CKKeCo_03Property); }
            set { SetValue(CKKeCo_03Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_04Property =
            DependencyProperty.Register("CKKeCo_04", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_04Changed));

        private static void CKKeCo_04Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_04 = value;
            }
        }

        public string CKKeCo_04
        {
            get { return (string)GetValue(CKKeCo_04Property); }
            set { SetValue(CKKeCo_04Property, value); }
        }


        public static readonly DependencyProperty CKKeCo_10Property =
            DependencyProperty.Register("CKKeCo_10", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_10Changed));

        private static void CKKeCo_10Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_10 = value;
            }
        }

        public string CKKeCo_10
        {
            get { return (string)GetValue(CKKeCo_10Property); }
            set { SetValue(CKKeCo_10Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_11Property =
            DependencyProperty.Register("CKKeCo_11", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_11Changed));

        private static void CKKeCo_11Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_11 = value;
            }
        }

        public string CKKeCo_11
        {
            get { return (string)GetValue(CKKeCo_11Property); }
            set { SetValue(CKKeCo_11Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_12Property =
            DependencyProperty.Register("CKKeCo_12", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_12Changed));

        private static void CKKeCo_12Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_12 = value;
            }
        }

        public string CKKeCo_12
        {
            get { return (string)GetValue(CKKeCo_12Property); }
            set { SetValue(CKKeCo_12Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_13Property =
            DependencyProperty.Register("CKKeCo_13", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_13Changed));

        private static void CKKeCo_13Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_13 = value;
            }
        }

        public string CKKeCo_13
        {
            get { return (string)GetValue(CKKeCo_13Property); }
            set { SetValue(CKKeCo_13Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_14Property =
            DependencyProperty.Register("CKKeCo_14", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_14Changed));

        private static void CKKeCo_14Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_14 = value;
            }
        }

        public string CKKeCo_14
        {
            get { return (string)GetValue(CKKeCo_14Property); }
            set { SetValue(CKKeCo_14Property, value); }
        }


        public static readonly DependencyProperty CKKeCo_20Property =
            DependencyProperty.Register("CKKeCo_20", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_20Changed));

        private static void CKKeCo_20Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_20 = value;
            }
        }

        public string CKKeCo_20
        {
            get { return (string)GetValue(CKKeCo_20Property); }
            set { SetValue(CKKeCo_20Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_21Property =
            DependencyProperty.Register("CKKeCo_21", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_21Changed));

        private static void CKKeCo_21Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_21 = value;
            }
        }

        public string CKKeCo_21
        {
            get { return (string)GetValue(CKKeCo_21Property); }
            set { SetValue(CKKeCo_21Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_22Property =
            DependencyProperty.Register("CKKeCo_22", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_22Changed));

        private static void CKKeCo_22Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_22 = value;
            }
        }

        public string CKKeCo_22
        {
            get { return (string)GetValue(CKKeCo_22Property); }
            set { SetValue(CKKeCo_22Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_23Property =
            DependencyProperty.Register("CKKeCo_23", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_23Changed));

        private static void CKKeCo_23Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_23 = value;
            }
        }

        public string CKKeCo_23
        {
            get { return (string)GetValue(CKKeCo_23Property); }
            set { SetValue(CKKeCo_23Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_24Property =
            DependencyProperty.Register("CKKeCo_24", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_24Changed));

        private static void CKKeCo_24Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_24 = value;
            }
        }

        public string CKKeCo_24
        {
            get { return (string)GetValue(CKKeCo_24Property); }
            set { SetValue(CKKeCo_24Property, value); }
        }


        public static readonly DependencyProperty CKKeCo_30Property =
            DependencyProperty.Register("CKKeCo_30", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_30Changed));

        private static void CKKeCo_30Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_30 = value;
            }
        }

        public string CKKeCo_30
        {
            get { return (string)GetValue(CKKeCo_30Property); }
            set { SetValue(CKKeCo_30Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_31Property =
            DependencyProperty.Register("CKKeCo_31", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_31Changed));

        private static void CKKeCo_31Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_31 = value;
            }
        }

        public string CKKeCo_31
        {
            get { return (string)GetValue(CKKeCo_31Property); }
            set { SetValue(CKKeCo_31Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_32Property =
            DependencyProperty.Register("CKKeCo_32", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_32Changed));

        private static void CKKeCo_32Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_32 = value;
            }
        }

        public string CKKeCo_32
        {
            get { return (string)GetValue(CKKeCo_32Property); }
            set { SetValue(CKKeCo_32Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_33Property =
            DependencyProperty.Register("CKKeCo_33", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_33Changed));

        private static void CKKeCo_33Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_33 = value;
            }
        }

        public string CKKeCo_33
        {
            get { return (string)GetValue(CKKeCo_33Property); }
            set { SetValue(CKKeCo_33Property, value); }
        }

        public static readonly DependencyProperty CKKeCo_34Property =
            DependencyProperty.Register("CKKeCo_34", typeof(string), typeof(Key), new PropertyMetadata(default(string), CKKeCo_34Changed));

        private static void CKKeCo_34Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var key = dependencyObject as CK20Page;

            if (key != null)
            {
                var value = dependencyPropertyChangedEventArgs.NewValue as string;
                if (value != null)
                    Settings.Default.CommuniKateKeyboardCurrentContext_34 = value;
            }
        }

        public string CKKeCo_34
        {
            get { return (string)GetValue(CKKeCo_34Property); }
            set { SetValue(CKKeCo_34Property, value); }
        }
    }
}
