// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Properties;

namespace JuliusSweetland.OptiKey.Enums
{
    public enum DataStreamProcessingLevels
    {
        None,
        Low,
        Medium,
        High
    }

    public static partial class EnumExtensions
    {
        public static string ToDescription(this DataStreamProcessingLevels pointSource)
        {
            switch (pointSource)
            {
                case DataStreamProcessingLevels.High: return Resources.HIGH;
                case DataStreamProcessingLevels.Medium: return Resources.MEDIUM;
                case DataStreamProcessingLevels.Low: return Resources.LOW;
                case DataStreamProcessingLevels.None: return Resources.NONE;
            }

            return pointSource.ToString();
        }
    }
}
