using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using JuliusSweetland.OptiKey.Extensions;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using Prism.Commands;
using log4net;
using System.Reflection;

namespace JuliusSweetland.OptiKey.UI.ViewModels.Management
{
    public class GesturesViewModel : INotifyPropertyChanged
    {
        protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public GesturesViewModel() 
        {
            OpenFileCommand = new DelegateCommand(OpenFile);
            SaveFileCommand = new DelegateCommand(SaveFile);
            EnableAllCommand = new DelegateCommand(EnableAll);
            DisableAllCommand = new DelegateCommand(DisableAll);
            EnableCommand = new DelegateCommand(Enable);
            AddGestureCommand = new DelegateCommand(AddGesture);
            DeleteGestureCommand = new DelegateCommand(DeleteGesture);
            Load();
        }

        #region Properties

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged()
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
            if (xmlEyeGestures != null && GestureList != null && GestureList.Count > 1)
                GestureList = new ObservableCollection<EyeGesture>(GestureList.OrderBy(g => g.Name));
        }

        public DelegateCommand OpenFileCommand { get; private set; }
        public DelegateCommand SaveFileCommand { get; private set; }
        public DelegateCommand EnableAllCommand { get; private set; }
        public DelegateCommand DisableAllCommand { get; private set; }
        public DelegateCommand EnableCommand { get; private set; }
        public DelegateCommand AddGestureCommand { get; private set; }
        public DelegateCommand DeleteGestureCommand { get; private set; }

        public bool NotAllEnabled
        { get { return xmlEyeGestures != null && GestureList != null && GestureList.Where(g => !g.Enabled).Any(); } }
        public bool AnyEnabled
        { get { return xmlEyeGestures != null && GestureList != null && GestureList.Where(g => g.Enabled).Any(); } }
        private string enabledCountLabel;
        public string EnabledCountLabel
        {
            get { return enabledCountLabel + " Enabled"; }
            set { enabledCountLabel = xmlEyeGestures != null && GestureList != null && GestureList.Any()
                    ? GestureList.Where(g => g.Enabled).ToList().Count.ToString() + " of " + GestureList.Count.ToString() : "0"; }
        }

        private bool eyeGesturesEnabled = false;
        public bool EyeGesturesEnabled
        {
            get { return eyeGesturesEnabled; }
            set { eyeGesturesEnabled = value; OnPropertyChanged(); }
        }
        public string EyeGestureFile { get; set; }
        public string EyeGestureString;

        private XmlEyeGestures xmlEyeGestures;
        public XmlEyeGestures XmlEyeGestures
        {
            get { return xmlEyeGestures; }
            set { xmlEyeGestures = value; }
        }

        public ObservableCollection<EyeGesture> GestureList
        {
            get { return xmlEyeGestures.GestureList; }
            set { xmlEyeGestures.GestureList = value; }
        }

        private EyeGesture eyeGesture;
        public EyeGesture EyeGesture
        {
            get { return eyeGesture; }
            set { eyeGesture = value; Preview = null; }
        }

        private Canvas preview;
        public Canvas Preview
        {
            get { return preview; }
            set { RenderPreview(); }
        }

        #endregion

        #region Methods

        public void ApplyChanges()
        {
            EyeGestureString = xmlEyeGestures.WriteToString();
            if (EyeGestureString != Settings.Default.EyeGestureString)
            {
                Settings.Default.EyeGestureString = EyeGestureString;
                Settings.Default.EyeGesturesEnabled = EyeGesturesEnabled;
                Settings.Default.EyeGestureUpdated = true;
            }
            Settings.Default.EyeGestureFile = EyeGestureFile;
        }

        private void Load()
        {
            EyeGesturesEnabled = Settings.Default.EyeGesturesEnabled;
            EyeGestureFile = Settings.Default.EyeGestureFile;
            EyeGestureString = Settings.Default.EyeGestureString;
            xmlEyeGestures = XmlEyeGestures.ReadFromString(EyeGestureString) ?? new XmlEyeGestures();
            EyeGesture = GestureList != null && GestureList.Any() ? GestureList[0] : null;
        }

        private void OpenFile()
        {
            var fileDialog = new System.Windows.Forms.OpenFileDialog() { FileName = Path.GetFileName(EyeGestureFile) };
            var result = fileDialog.ShowDialog();
            string tempFilename;
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    tempFilename = fileDialog.FileName; // we will commit it if loading is okay                    
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    return;
            }
            try
            {
                xmlEyeGestures = XmlEyeGestures.ReadFromFile(tempFilename);                
            }
            catch (Exception e)
            {
                Log.Error($"Error reading from gesture file: {tempFilename} :");
                Log.Info(e.ToString());
                return;
            }
            EyeGestureFile = tempFilename;
            EyeGesture = GestureList != null && GestureList.Any() ? GestureList[0] : null;
        }

        private void SaveFile()
        {
            var fp = Path.GetDirectoryName(EyeGestureFile);
            var fn = Path.GetFileName(EyeGestureFile);
            var saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            saveFileDialog.InitialDirectory = string.IsNullOrEmpty(fp) ? @"C:\" : fp;
            saveFileDialog.FileName = string.IsNullOrEmpty(fn) ? "EyeGestures.xml" : fn;
            saveFileDialog.Title = "Save File";
            saveFileDialog.CheckFileExists = false;
            saveFileDialog.CheckPathExists = true;
            saveFileDialog.DefaultExt = "xml";
            saveFileDialog.Filter = "Xml files (*.xml)|*.xml|All files (*.*)|*.*";
            saveFileDialog.FilterIndex = 2;
            saveFileDialog.RestoreDirectory = true;
            var result = saveFileDialog.ShowDialog();
            switch (result)
            {
                case System.Windows.Forms.DialogResult.OK:
                    EyeGestureFile = saveFileDialog.FileName;
                    XmlEyeGestures.WriteToFile(EyeGestureFile);
                    break;
                case System.Windows.Forms.DialogResult.Cancel:
                default:
                    break;
            }
        }

        private void EnableAll()
        {
            EyeGesturesEnabled = true;
            foreach (var e in GestureList)
                e.Enabled = true;
            UpdateState();
        }

        private void DisableAll()
        {
            EyeGesturesEnabled = false;
            foreach (var e in GestureList)
                e.Enabled = false;
            UpdateState();
        }

        private void Enable()
        {
            EyeGesture.Enabled = !EyeGesture.Enabled;
            EyeGesturesEnabled = AnyEnabled;
            UpdateState();
        }

        private void UpdateState()
        {
            EnabledCountLabel = null;
            ApplyChanges();
            OnPropertyChanged();
        }

        private void AddGesture()
        {
            var eyeGesture = new EyeGesture() { Steps = new ObservableCollection<EyeGestureStep>() { new EyeGestureStep() } };

            if (GestureList != null)
                GestureList.Add(eyeGesture);
            else
                GestureList = new ObservableCollection<EyeGesture>() { eyeGesture };

            EyeGesture = eyeGesture;
        }

        private void DeleteGesture()
        {
            if (GestureList != null && GestureList.Contains(EyeGesture))
                GestureList.Remove(EyeGesture);
            EyeGesture = GestureList != null && GestureList.Any() ? GestureList[0] : null;
        }

        private void RenderPreview()
        {
            if (preview != null && preview.Children != null && preview.Children.Count > 0)
                preview.Children.Clear();

            if (eyeGesture != null && eyeGesture.Steps != null && eyeGesture.Steps.Any())
            {
                var scrWidth = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width / 8;
                var scrHeight = System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height / 8;
                var gestureShapes = new List<EyeGestureShape>();

                double xPos = 0.5 * scrWidth;
                double yPos = 0.5 * scrHeight;
                double screenLeft = 0.2 * scrWidth;
                double screenTop = 0.2 * scrHeight;
                double canvasWidth = 1.4 * scrWidth;
                double canvasHeight = 1.4 * scrHeight;
                double labelRadius = 18;

                foreach (var step in eyeGesture.Steps)
                {
                    var shape = new EyeGestureShape();

                    if (step.type == Enums.EyeGestureStepTypes.Fixation)
                    {
                        eyeGesture.FixationPoint = new Point(xPos, yPos);
                        shape.Left = xPos - step.Radius * scrHeight / 100;
                        shape.Top = yPos - step.Radius * scrHeight / 100;
                        shape.Width = Math.Max(step.Radius * scrHeight / 50, 5);
                        shape.Height = Math.Max(step.Radius * scrHeight / 50, 5);
                        shape.Radius = shape.Width;
                        shape.Background = step.DwellTime > 100 ? Brushes.LightCoral : Brushes.ForestGreen;
                        shape.X = shape.Width / 2 > labelRadius ? xPos : xPos - labelRadius;
                        shape.Y = shape.Width / 2 > labelRadius ? yPos : yPos - labelRadius;
                    }
                    else if (step.type == Enums.EyeGestureStepTypes.LookInDirection)
                    {
                        xPos += step.X * scrWidth / 100;
                        yPos += step.Y * scrHeight / 100;
                        shape.Left = step.X > 0 ? xPos : xPos - 2 * scrWidth;
                        shape.Top = step.Y > 0 ? yPos : yPos - 2 * scrHeight;
                        shape.Width = step.X != 0 ? 2 * scrWidth : 4 * scrWidth;
                        shape.Height = step.Y != 0 ? 2 * scrHeight : 4 * scrHeight;
                        shape.Radius = 0;
                        shape.Background = Brushes.ForestGreen;
                        shape.X = step.X != 0 ? xPos + Math.Sign(step.X) * labelRadius : xPos;
                        shape.Y = step.Y != 0 ? yPos + Math.Sign(step.Y) * labelRadius : yPos;
                    }
                    else if (step.type == Enums.EyeGestureStepTypes.LookAtArea)
                    {
                        xPos = xPos.Clamp(step.Left * scrWidth / 100 + labelRadius, step.Left * scrWidth / 100 + step.Width * scrWidth / 100 - labelRadius);
                        yPos = yPos.Clamp(step.Top * scrHeight / 100 + labelRadius, step.Top * scrHeight / 100 + step.Height * scrHeight / 100 - labelRadius);
                        shape.Left = step.Left * scrWidth / 100;
                        shape.Top = step.Top * scrHeight / 100;
                        shape.Width = step.Width * scrWidth / 100;
                        shape.Height = step.Height * scrHeight / 100;
                        shape.Radius = step.Round ? shape.Width + shape.Height : 0;
                        shape.Background = step.DwellTime > 100 ? Brushes.LightCoral : Brushes.ForestGreen;
                        shape.X = xPos;
                        shape.Y = yPos;
                    }
                    else if (step.type == Enums.EyeGestureStepTypes.ReturnToFixation)
                    {
                        shape.Width = step.Radius * scrHeight / 50;
                        shape.Height = step.Radius * scrHeight / 50;
                        shape.Radius = shape.Width;
                        shape.Background = step.DwellTime > 100 ? Brushes.LightCoral : Brushes.ForestGreen;
                        shape.X = xPos < eyeGesture.FixationPoint.X
                            ? eyeGesture.FixationPoint.X - labelRadius : eyeGesture.FixationPoint.X + labelRadius;
                        shape.Y = eyeGesture.FixationPoint.Y + labelRadius;
                        xPos = eyeGesture.FixationPoint.X;
                        yPos = eyeGesture.FixationPoint.Y;
                        shape.Left = xPos - step.Radius * scrHeight / 100;
                        shape.Top = yPos - step.Radius * scrHeight / 100;
                    }
                    gestureShapes.Add(shape);
                    screenLeft = Math.Max(screenLeft, labelRadius - shape.X);
                    screenTop = Math.Max(screenTop, labelRadius - shape.Y);
                    canvasWidth = Math.Max(canvasWidth, labelRadius + shape.X);
                    canvasHeight = Math.Max(canvasHeight, labelRadius + shape.Y);
                }

                var canvas = new Canvas()
                {
                    Width = canvasWidth,
                    Height = canvasHeight
                };
                canvas.Children.Add(new Border()
                {
                    Background = Brushes.Black,
                    BorderBrush = Brushes.White,
                    BorderThickness = new Thickness(2),
                    Width = scrWidth,
                    Height = scrHeight,
                    Margin = new Thickness(screenLeft, screenTop, 0, 0),
                });
                canvas.Children.Add(new System.Windows.Shapes.Line()
                {
                    X1 = screenLeft + .25 * scrWidth,
                    Y1 = screenTop + 1.15 * scrHeight,
                    X2 = screenLeft + .75 * scrWidth,
                    Y2 = screenTop + 1.15 * scrHeight,
                    SnapsToDevicePixels = true,
                    Stroke = Brushes.White,
                    StrokeThickness = 4
                });
                canvas.Children.Add(new System.Windows.Shapes.Line()
                {
                    X1 = screenLeft + .5 * scrWidth,
                    Y1 = screenTop + scrHeight,
                    X2 = screenLeft + .5 * scrWidth,
                    Y2 = screenTop + 1.15 * scrHeight,
                    SnapsToDevicePixels = true,
                    Stroke = Brushes.White,
                    StrokeThickness = 12
                });

                foreach (var shape in gestureShapes)
                {
                    canvas.Children.Add(new Border()
                    {
                        Background = shape.Background,
                        Opacity = 0.5,
                        CornerRadius = new CornerRadius(shape.Radius),
                        Margin = new Thickness(screenLeft + shape.Left, screenTop + shape.Top, 0, 0),
                        Width = shape.Width,
                        Height = shape.Height
                    });

                    if (eyeGesture.Steps.Count > 1)
                    {
                        for(int i = 1; i < eyeGesture.Steps.Count; i++)
                        {
                            canvas.Children.Add(new System.Windows.Shapes.Line()
                            {
                                X1 = gestureShapes[i - 1].X + screenLeft,
                                Y1 = gestureShapes[i - 1].Y + screenTop,
                                X2 = gestureShapes[i].X + screenLeft,
                                Y2 = gestureShapes[i].Y + screenTop,
                                SnapsToDevicePixels = true,
                                Stroke = Brushes.White,
                                StrokeThickness = 2
                            });
                        }
                    }
                }

                int b = 1;
                foreach (var shape in gestureShapes)
                {
                    var labelBorder = new Border()
                    {
                        Margin = new Thickness(screenLeft + shape.X - labelRadius, screenTop + shape.Y - labelRadius, 0, 0),
                        Width = 2 * labelRadius,
                        Height = 2 * labelRadius,
                        Background = Brushes.LightSlateGray,
                        BorderBrush = Brushes.White,
                        BorderThickness = new Thickness(2),
                        CornerRadius = new CornerRadius(9999)
                    };

                    labelBorder.Child = new Label()
                    {
                        Background = Brushes.Transparent,
                        Content = b.ToString(),
                        FontSize = labelRadius,
                        Foreground = Brushes.White,
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    canvas.Children.Add(labelBorder);
                    b++;
                }
                canvas.ClipToBounds = true;
                preview = canvas;
            }
            EnabledCountLabel = null;
            OnPropertyChanged();
        }

        #endregion

    }
}