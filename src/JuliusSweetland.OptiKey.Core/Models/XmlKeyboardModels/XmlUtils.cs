// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved

using System;
using System.Reflection;
using System.Xml;
using log4net;

namespace JuliusSweetland.OptiKey.Models
{
    public static class XmlUtils
    {
        private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool ConvertToBoolean(string value)
        {
            if (value.Trim().ToLower().Equals("true"))
                return true;
            else if (value.Trim().ToLower().Equals("false"))
                return false;
            else
            {
                bool bVal = false;
                try
                {
                    bVal = XmlConvert.ToBoolean(value);
                }
                catch (System.Exception)
                {
                    Log.ErrorFormat("Cannot convert xml string '{0}' to boolean", value);
                }
                return bVal;
            }
        }

        public static int ConvertToInt(string value)
        {
            int iVal = default(int);
            try
            {
                iVal = XmlConvert.ToInt32(value);
            }
            catch (System.Exception)
            {
                Log.ErrorFormat("Cannot convert xml string '{0}' to int", value);
            }
            return iVal;
        }
    }
}