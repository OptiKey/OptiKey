using System;
using System.Reactive.Linq;
using JuliusSweetland.ETTA.Models;
using Microsoft.Practices.Prism.Mvvm;

namespace JuliusSweetland.ETTA.Services
{
    public class OutputService : BindableBase, IOutputService
    {
        #region Private Member Vars

        private readonly IKeyboardStateManager keyboardStateManager;

        #endregion

        #region Properties

        private string text;
        public string Text
        {
            get { return text; }
            private set { SetProperty(ref text, value); }
        }

        #endregion

        #region Ctor

        public OutputService(IKeyboardStateManager keyboardStateManager)
        {
            this.keyboardStateManager = keyboardStateManager;

            //TESTING START
            Text = "This is some test output. I will make it arbitrarily long so we can see what is going on.";

            Observable.Interval(TimeSpan.FromMilliseconds(500))
                .ObserveOnDispatcher()
                .Subscribe(l => Text = Text + " " + l);
            //TESTING END
        }

        #endregion

        #region Methods

        public void ClearText()
        {
            Text = null;
        }

        #endregion
    }
}