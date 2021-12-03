// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

namespace JuliusSweetland.OptiKey.Models
{
    public interface IXmlKey
    {
        int Row { get; }
        int Col { get; }
        int Width { get; }
        int Height { get; }
        string Label { get; }
        string Symbol { get; }
    }
}
