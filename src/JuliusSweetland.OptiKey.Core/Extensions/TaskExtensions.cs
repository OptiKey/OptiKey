// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Threading.Tasks;
using log4net;

namespace JuliusSweetland.OptiKey.Extensions
{
    public static class TaskExtensions
    {
        private static readonly ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

#pragma warning disable RECS0165 // Asynchronous methods should return a Task instead of void
        public static async void FireAndForgetSafeAsync(this Task task, Action<Exception> handler = null)
#pragma warning restore RECS0165 // Asynchronous methods should return a Task instead of void
        {
            try
            {
                await task;
            }
            catch (Exception ex)
            {
                Log.Error("Exception caught by TaskExtensions.FireAndForgetSafeAsync", ex);
                handler?.Invoke(ex);
            }
        }
    }
}