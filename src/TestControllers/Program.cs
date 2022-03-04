using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using JuliusSweetland.OptiKey.Models;
using SharpDX.XInput;
using JuliusSweetland.OptiKey.Models.Gamepads;
using static JuliusSweetland.OptiKey.Models.Gamepads.DirectInputListener;
using static JuliusSweetland.OptiKey.Models.Gamepads.XInputListener;

namespace TestControllers
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("");
            ListDirectInputControllers();
            ListXInputControllers();

            Console.WriteLine("Press the switch you want to use to see what appears.");
            Console.WriteLine("If it shows up as both DirectInput and XInput, prefer XInput.");            

            XInputListener xinputListener = XInputListener.Instance;
            DirectInputListener directInputListener = DirectInputListener.Instance;

            xinputListener.ButtonUp += XInputCallback;
            xinputListener.ButtonDown += XInputCallback;

            directInputListener.ButtonUp += DirectInputCallback;
            directInputListener.ButtonDown += DirectInputCallback;

            Console.ReadLine();
        }

        private static void DirectInputCallback(object sender, DirectInputButtonEventArgs e)
        {
            string shortGuid = e.controller.ToString().Substring(0, 8);
            Console.WriteLine($"DirectInput:\t{shortGuid} \t button:{e.button} \t {e.eventType}");
        }

        private static void XInputCallback(object sender, XInputListener.XInputButtonEventArgs e)
        {
            Console.WriteLine($"XInput:     \tUserIndex{e.userIndex} \t button:{e.button} \t {e.eventType}");
        }

        static void ListXInputControllers()
        {
            Console.WriteLine("Getting list of XInput controllers");

            foreach (UserIndex idx in Enum.GetValues(typeof(UserIndex)))
            {
                if (idx != UserIndex.Any)
                {                    
                    if (XInputListener.IsDeviceAvailable(idx))
                    {
                        Console.WriteLine($"UserIndex{idx}  -  connected ");
                    }
                }
            }
            Console.WriteLine("");
        }

        static void ListDirectInputControllers()
        {
            var controllers = DirectInputListener.GetConnectedControllers();

            Console.WriteLine("Getting list of DirectInput controllers");
            foreach (var controller in controllers)
            {
                Console.WriteLine($"{controller.Key}  -  {controller.Value} ");
            }
            Console.WriteLine("");
        }        
    }
}
