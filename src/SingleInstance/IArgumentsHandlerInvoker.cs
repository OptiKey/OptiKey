using System;

namespace WindowsRecipes.TaskbarSingleInstance
{
    /// <summary>
    /// Implement this interface to provide a custom arguments handler invoker.
    /// </summary>
    /// <remarks>
    /// A default <see cref="ThreadPoolInvoker"/> is provided out-of-the-box, for invoking handler on thread-pool threads.
    /// In addition, custom invokers for WPF &amp; WinForms are available.
    /// 
    /// Implement this interface to provide custom handler invocation.
    /// </remarks>
    public interface IArgumentsHandlerInvoker
    {
        /// <summary>
        /// The method called to invoke the handler.
        /// </summary>
        /// <param name="handlerToInvoke">The handler to invoke.</param>
        /// <param name="args">The command line arguments to deliver.</param>
        void Invoke(Action<string[]> handlerToInvoke, string[] args);
    }
}
