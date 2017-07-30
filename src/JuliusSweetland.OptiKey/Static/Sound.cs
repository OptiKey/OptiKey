using System;
using System.Text;
using JuliusSweetland.OptiKey.Native;

namespace JuliusSweetland.OptiKey.Static
{
    public static class Sound
    {
        public static int GetSoundLength(string fileName)
        {
            StringBuilder lengthBuf = new StringBuilder(32);

            PInvoke.mciSendString(string.Format("open \"{0}\" type waveaudio alias wave", fileName), null, 0, IntPtr.Zero);
            PInvoke.mciSendString("status wave length", lengthBuf, lengthBuf.Capacity, IntPtr.Zero);
            PInvoke.mciSendString("close wave", null, 0, IntPtr.Zero);

            int length = 0;
            int.TryParse(lengthBuf.ToString(), out length);

            return length;
        }
    }
}
