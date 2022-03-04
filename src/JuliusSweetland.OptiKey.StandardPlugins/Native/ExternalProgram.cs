// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using System;
using System.Diagnostics;

/**
 * This is a plugin that runs a command in windows. It can be anything that you would run using command line prompt.
 * 
 * It has only one method
 * 
 * 1) RUN
 *    . command: the command that will be executed by the plugin
 *    . parameters: parameters to be passed along with the command
 * 
 * Please refer to OptiKey wiki for more information on registering and developing extensions.
 */

namespace JuliusSweetland.OptiKey.StandardPlugins
{
    public class ExternalProgram
    {
        // Simply run it.
        public void RUN(string command, string parameters = null)
        {
            if (String.IsNullOrEmpty(parameters))
                Process.Start(command);
            else
                Process.Start(command, parameters);
        }
    }
}
