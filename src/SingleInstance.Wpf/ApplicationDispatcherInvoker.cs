using System;
using System.Windows;
using System.Windows.Threading;

namespace WindowsRecipes.TaskbarSingleInstance.Wpf
{
    /// <summary>
    /// A custom <see cref="IArgumentsHandlerInvoker"/> for WPF applications, which invokes argument handlers on the application dispatcher thread.
    /// </summary>
    public class ApplicationDispatcherInvoker : IArgumentsHandlerInvoker
    {
        #region IArgumentsHandlerInvoker Members

        /// <summary>
        /// Invokes the given handler on the WPF Application dispatcher thread.
        /// </summary>
        /// <param name="handlerToInvoke">The handler to invoke.</param>
        /// <param name="args">The command line arguments to deliver.</param>
        public void Invoke(Action<string[]> handlerToInvoke, string[] args)
        {
            if (Application.Current == null)
                return;

            DispatcherOperationCallback callBack = argsAsObj =>
                {
                    handlerToInvoke((string[])argsAsObj);
                    return null;
                };
            Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Normal, callBack, args);
        }

        #endregion
    }
}
