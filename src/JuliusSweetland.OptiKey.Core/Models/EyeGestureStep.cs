// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using JuliusSweetland.OptiKey.Extensions;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class EyeGestureStep : INotifyPropertyChanged
    {
        [XmlIgnore] public Enums.EyeGestureStepTypes type { get; set; }
        public string Type
        {
            get { return type.ToString(); }
            set 
            {
                if (System.Enum.TryParse(value, out Enums.EyeGestureStepTypes result))
                    type = result;
                else
                    type = Enums.EyeGestureStepTypes.LookAtArea;
                OnPropertyChanged();
            }
        }

        private double timeLimit = 500;
        public double TimeLimit
        {
            get { return timeLimit; }
            set { timeLimit = value.Clamp(dwellTime + 50, 10000);
                OnPropertyChanged(); }
        }

        private double dwellTime = 0;
        public double DwellTime
        {
            get { return dwellTime; }
            set
            {
                dwellTime = type == Enums.EyeGestureStepTypes.Fixation
                    ? value.Clamp(400, 9000) : value.Clamp(0, 9000);
                TimeLimit = TimeLimit.Clamp(dwellTime + 50, TimeLimit);
                OnPropertyChanged();
            }
        }

        private double radius = 1;
        public double Radius
        {
            get { return radius; }
            set {
                radius = value.Clamp(0, 10);
                OnPropertyChanged();
            }
        }
        private double x;
        public double X
        {
            get { return x; }
            set { x = value.Clamp(-120, 120);
                OnPropertyChanged();
            }
        }
        private double y = 20;
        public double Y
        {
            get { return y; }
            set { y = value.Clamp(-120, 120);
                OnPropertyChanged();
            }
        }
        private double left = 35;
        public double Left
        {
            get { return left; }
            set { left = value.Clamp(-20, 115);
                OnPropertyChanged();
            }
        }
        private double top = 35;
        public double Top
        {
            get { return top; }
            set {
                top = value.Clamp(-20, 115);
                OnPropertyChanged();
            }
        }
        private double width = 30;
        public double Width
        {
            get { return width; }
            set
            {
                width = value.Clamp(5, 140);
                OnPropertyChanged();
            }
        }
        private double height = 30;
        public double Height
        {
            get { return height; }
            set { height = value.Clamp(5, 140);
                OnPropertyChanged();
            }
        }
        private bool round;
        public bool Round
        {
            get { return round; }
            set { round = value;
                OnPropertyChanged();
            }
        }

        [XmlIgnore] public EyeGestureShape EyeGestureShape { get; set; }
        [XmlIgnore] public DateTimeOffset DwellStart { get; set; } = DateTimeOffset.Now;

        private ObservableCollection<XmlKeyCommand> commands;
        [XmlElement("Command")] public ObservableCollection<XmlKeyCommand> Commands 
        { get { return commands; }
            set { commands = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}