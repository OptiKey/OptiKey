// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using System.Collections.Generic;

namespace JuliusSweetland.OptiKey.Models
{
    public class KeyCommand
    {
        public KeyCommand() { }
        public KeyCommand(KeyCommands name, KeyValue keyValue)
        {
            this.Name = name;
            this.KeyValue = keyValue;
        }

        public KeyCommands Name { get; set; }
        public string Value { get; set; }
        public KeyValue KeyValue { get; set; }
        public List<KeyCommand> LoopCommands { get; set; }
        public DynamicPlugin Plugin { get; set; }

    }
}
