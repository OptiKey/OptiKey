using System;
using System.Threading;

namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// A general-purpose <see cref="IArgumentsHandlerInvoker"/> which invokes argument handlers on Thread-Pool threads.
    /// </summary>
    public sealed class ThreadPoolInvoker : IArgumentsHandlerInvoker
    {
        #region IHandlerInvoker Members

        /// <summary>
        /// Invokes the given handler on a thread pool therad.
        /// </summary>
        /// <param name="handlerToInvoke">The handler to invoke.</param>
        /// <param name="args">The command line arguments to deliver.</param>
        public void Invoke(Action<string[]> handlerToInvoke, string[] args)
        {
            ThreadPool.QueueUserWorkItem(obj => handlerToInvoke((string[])obj), args);
        }

        #endregion
    }
}
