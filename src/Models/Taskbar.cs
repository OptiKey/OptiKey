using System;
using System.Drawing;
using System.Runtime.InteropServices;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Native;
using JuliusSweetland.OptiKey.Native.Enums;
using JuliusSweetland.OptiKey.Native.Static;
using JuliusSweetland.OptiKey.Native.Structs;

namespace JuliusSweetland.OptiKey.Models
{
    public sealed class Taskbar
    {
        private const string ClassName = "Shell_TrayWnd";

        public Taskbar()
        {
            IntPtr taskbarHandle = PInvoke.FindWindow(Taskbar.ClassName, null);

            var data = new APPBARDATA
            {
                cbSize = (uint) Marshal.SizeOf(typeof (APPBARDATA)), 
                hWnd = taskbarHandle
            };

            IntPtr result = PInvoke.SHAppBarMessage(AppBarMessages.GetTaskbarPos, ref data);
            
            if (result == IntPtr.Zero)
                throw new InvalidOperationException();

            this.Position = (TaskbarPosition) data.uEdge;
            this.Bounds = Rectangle.FromLTRB(data.rc.Left, data.rc.Top, data.rc.Right, data.rc.Bottom);

            data.cbSize = (uint) Marshal.SizeOf(typeof (APPBARDATA));
            result = PInvoke.SHAppBarMessage(AppBarMessages.GetState, ref data);
            int state = result.ToInt32();
            this.AlwaysOnTop = (state & AppBarState.AlwaysOnTop) == AppBarState.AlwaysOnTop;
            this.AutoHide = (state & AppBarState.Autohide) == AppBarState.Autohide;
        }

        public TaskbarPosition Position { get; private set; }
        public Point Location { get { return this.Bounds.Location; } }
        public Rectangle Bounds { get; private set; }
        public Size Size { get { return this.Bounds.Size; } }
        /// <summary>
        /// Always returns false under Windows 7
        /// </summary>
        public bool AlwaysOnTop { get; private set; }
        public bool AutoHide { get; private set; }
    }
}
