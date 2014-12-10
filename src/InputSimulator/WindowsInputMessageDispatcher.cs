using System;
using System.Runtime.InteropServices;
using System.Text;
using JuliusSweetland.ETTA.Native;
using JuliusSweetland.ETTA.Native.Structs;
using JuliusSweetland.ETTA.Properties;
using log4net;

namespace JuliusSweetland.ETTA.InputSimulator
{
    /// <summary>
    /// Implements the <see cref="IInputMessageDispatcher"/> by calling <see cref="Native.NativeMethods.SendInput"/>.
    /// </summary>
    internal class WindowsInputMessageDispatcher : IInputMessageDispatcher
    {
        private readonly static ILog Log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Dispatches the specified list of <see cref="INPUT"/> messages in their specified order by issuing a single called to <see cref="Native.NativeMethods.SendInput"/>.
        /// </summary>
        /// <param name="inputs">The list of <see cref="INPUT"/> messages to be dispatched.</param>
        /// <exception cref="ArgumentException">If the <paramref name="inputs"/> array is empty.</exception>
        /// <exception cref="ArgumentNullException">If the <paramref name="inputs"/> array is null.</exception>
        /// <exception cref="Exception">If the any of the commands in the <paramref name="inputs"/> array could not be sent successfully.</exception>
        public void DispatchInput(INPUT[] inputs)
        {
            if (inputs == null) throw new ArgumentNullException("inputs");
            if (inputs.Length == 0) throw new ArgumentException("The input array was empty", "inputs");

            if (Settings.Default.SendInputsIndividually)
            {
                foreach (var input in inputs)
                {
                    SendInput(new [] { input });
                }
            }
            else
            {
                SendInput(inputs);
            }
        }

        private static void SendInput(INPUT[] inputs)
        {
            var nInputs = (UInt32) inputs.Length;

            var pInputsSb = new StringBuilder();
            for (var index = 0; index < inputs.Length; index++)
            {
                pInputsSb.AppendLine(string.Format("[{0}]:\n{1}", index, inputs[index]));
            }
            
            var cbSize = Marshal.SizeOf(typeof (INPUT));
            
            Log.Debug(string.Format("Calling native method SendInput with params:" +
                                    "\nnInputs:{0}" +
                                    "\npInputs{1}" +
                                    "\ncbSize:{2}", 
                                    nInputs, pInputsSb, cbSize));
            
            var returnValue = NativeMethods.SendInput(nInputs, inputs, cbSize);

            if (returnValue != inputs.Length)
            {
                var ex = new Exception("Some simulated input commands were not sent successfully. " +
                                       "The most common reason for this happening are the security features of Windows including User Interface Privacy Isolation (UIPI). " +
                                       "Your application can only send commands to applications of the same or lower elevation. " +
                                       "Similarly certain commands are restricted to Accessibility/UIAutomation applications. " +
                                       "Refer to the project home page and the code samples for more information.");

                Log.Error(string.Format(
                    "SendInput returned {0}, but we were expecting a return value of {1}(the size of the INPUT array)", 
                    returnValue, inputs.Length));

                throw ex;
            }

            //NativeMethods.keybd_event(0x41, 0, 0, 0); // send keypress
            System.Threading.Thread.Sleep(100);
        }
    }
}