// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace JuliusSweetland.OptiKey.Models
{
    public class XmlKeyStates
    {

        [XmlElement(ElementName = "KeyUp")]
        public List<string> KeysUp
        { get; set; }

        [XmlElement(ElementName = "KeyDown")]
        public List<string> KeysDown
        { get; set; }

        [XmlElement(ElementName = "KeyLockedDown")]
        public List<string> KeysLockedDown
        { get; set; }

        public List<Tuple<string, KeyDownStates>> GetKeyOverrides()
        {
            // Combine all requests for key state overrides
            var overrideKeyStates = new List<Tuple<string, KeyDownStates>>();
            KeysUp?.ForEach(keyString =>
            {
                overrideKeyStates.Add(Tuple.Create(keyString, KeyDownStates.Up));
            });
            KeysDown?.ForEach(keyString =>
            {
                overrideKeyStates.Add(Tuple.Create(keyString, KeyDownStates.Down));
            });
            KeysLockedDown?.ForEach(keyString =>
            {
                overrideKeyStates.Add(Tuple.Create(keyString, KeyDownStates.LockedDown));
            });
            return overrideKeyStates;
        }
    }
}
