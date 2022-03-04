// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System.Threading.Tasks;
using System.Windows;

namespace JuliusSweetland.OptiKey.Services
{
    public interface ICalibrationService
    {
        Task<string> Calibrate(Window parentWindow);
        bool CanBeCompletedWithoutManualIntervention { get; }
    }
}
