﻿using JuliusSweetland.OptiKey.Enums;
using NUnit.Framework;
using System;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    [TestFixture]
    public class WhenRaiseToastNotificationGivenToastNotificationEventHandler : MainViewModelTestBase
    {
        protected string Title { get { return "dummy-title"; } }
        protected string Content { get { return "dummy-content"; } }
        protected NotificationTypes NotificationType { get { return NotificationTypes.Error; } }
        protected Action Callback { get { return new Action(() => { }); } }

        protected bool IsToastNotificationEventHandlerCalled { get; private set; }

        protected override void Arrange()
        {
            base.Arrange();
            MainViewModel.SubscribeToToastNotification((s, e) => IsToastNotificationEventHandlerCalled = true);
        }

        protected override void Act()
        {
            MainViewModel.RaiseToastNotification(Title, Content, NotificationType, () => { }, Callback);
        }

        [Test]
        public void ThenToastNotificationEventHandlerShouldBeTriggered()
        {
            Assert.IsTrue(IsToastNotificationEventHandlerCalled);
        }
    }
}
