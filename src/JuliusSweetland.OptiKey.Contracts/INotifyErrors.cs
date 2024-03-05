// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;

namespace JuliusSweetland.OptiKey.Contracts
{
    public interface INotifyErrors
    {
        event EventHandler<Exception> Error;
    }
}
