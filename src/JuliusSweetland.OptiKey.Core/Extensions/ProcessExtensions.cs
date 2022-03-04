// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Diagnostics;
using System.Windows;
using log4net;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class ProcessExtensions
    {
        public static void CloseOnApplicationExit(this Process proc, ILog log, string description)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                Application.Current.Exit += (o, args) =>
                {
                    if (proc == null) return;

                    if (proc.HasExited)
                    {
                        log.InfoFormat("{0} has already been closed.", description);
                    }
                    else
                    {
                        try
                        {
                            proc.CloseMainWindow();
                            log.InfoFormat("{0} has been closed.", description);
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("Error closing {0} on OptiKey shutdown", description), ex);
                        }
                    }
                };
            });
        }
    }
}
