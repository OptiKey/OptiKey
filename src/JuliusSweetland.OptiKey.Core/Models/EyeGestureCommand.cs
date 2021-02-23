// Copyright (c) 2020 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class EyeGestureCommand
    {
        [XmlIgnore] public Enums.KeyCommands CommandName;
        public string Name
        {
            get { return CommandName.ToString(); }
            set
            {
                if (Enums.KeyCommands.TryParse(value, out Enums.KeyCommands key))
                    CommandName = key;
            }
        }

        public string Value { get; set; }
        public bool BackAction { get; set; }
        public string Method { get; set; }
        public List<DynamicArgument> Argument { get; set; }
    }
}