// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyCommand
    {
        public KeyCommand() { }
        public KeyCommand(KeyCommands name, string value)
        {
            this.Name = name;
            this.Value = value;
        }

        public KeyCommands Name { get; set; }
        public string Value { get; set; }
        public bool BackAction { get; set; }
        public string Method { get; set; }
        public List<DynamicArgument> Argument { get; set; }
        public List<KeyCommand> LoopCommands { get; set; }
    }
}
