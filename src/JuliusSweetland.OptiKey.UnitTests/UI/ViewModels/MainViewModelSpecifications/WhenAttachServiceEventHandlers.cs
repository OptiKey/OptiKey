// Copyright (c) 2019 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Properties;
using JuliusSweetland.OptiKey.Services;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace JuliusSweetland.OptiKey.UnitTests.UI.ViewModels.MainViewModelSpecifications
{
    public class WhenAttachServiceEventHandlers : MainViewModelTestBase
    {
        protected Mock<INotifyErrors> NotifyErrorsMock { get; set; }
        protected NotificationEventArgs NotificationEvent { get; private set; }

        protected override void Arrange()
        {
            base.Arrange();

            NotifyErrorsMock = new Mock<INotifyErrors>();
            ErrorNotifyingServices.Add(NotifyErrorsMock.Object);

            MainViewModel.ToastNotification += (s, e) => NotificationEvent = e;
        }

        protected override void Act()
        {
            MainViewModel.AttachErrorNotifyingServiceHandlers();
            MainViewModel.AttachInputServiceEventHandlers();
        }

        [Test]
        public void ThenAnErrorEventHandlerShouldBeAttached()
        {
            var ex = new Exception("dummy");
            NotifyErrorsMock.Raise(e => e.Error += null, this, ex);

            InputService.Verify(s => s.RequestSuspend(), Times.Once());
            AudioService.Verify(s => s.PlaySound(Settings.Default.ErrorSoundFile, Settings.Default.ErrorSoundVolume), Times.Once());

            Assert.IsNotNull(NotificationEvent);
            Assert.AreEqual(Resources.CRASH_TITLE, NotificationEvent.Title);
            Assert.AreEqual(ex.Message, NotificationEvent.Content);
            Assert.AreEqual(NotificationTypes.Error, NotificationEvent.NotificationType);
        }

        [Test]
        public void ThenPointsPerSecondEventHandlerShouldBeAttachedToInputService()
        {
            InputService.Raise(s => s.PointsPerSecond += null, this, 123);
            Assert.AreEqual(123, MainViewModel.PointsPerSecond);
        }

        [Test]
        public void ThenPointToKeyValueMapShouldBeSetOnInputService()
        {
            InputService.VerifySet(s => s.PointToKeyValueMap = It.IsAny<Dictionary<Rect, KeyValue>>(), Times.Once());
        }

        [Test]
        public void ThenSelectionModeShouldBeSetOnInputService()
        {
            InputService.VerifySet(s => s.SelectionMode = MainViewModel.SelectionMode, Times.AtLeastOnce());
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenMouseMagneticCursorKeyDownAndSleepKeyUp : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            var keyDownStates = new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>();
            keyDownStates[KeyValues.MouseMagneticCursorKey] = new NotifyingProxy<KeyDownStates>(KeyDownStates.Down);
            keyDownStates[KeyValues.SleepKey] = new NotifyingProxy<KeyDownStates>(KeyDownStates.Up);
            KeyStateService.Setup(s => s.KeyDownStates).Returns(keyDownStates);
        }

        [Test]
        public void ThenA_CurrentPositionEventHandlerShouldBeAttachedToInputService()
        {
            var point = new Point(27, 10);
            var keyValue = new KeyValue();
            InputService.Raise(s => s.CurrentPosition += null, this, new Tuple<Point, KeyValue>(point, keyValue));

            Assert.AreEqual(point, MainViewModel.CurrentPositionPoint);
            Assert.AreEqual(keyValue, MainViewModel.CurrentPositionKey);

            MouseOutputService.Verify(s => s.MoveTo(point), Times.Once());
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenMouseMagneticCursorKeyUp : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            var keyDownStates = new NotifyingConcurrentDictionary<KeyValue, KeyDownStates>();
            keyDownStates[KeyValues.MouseMagneticCursorKey] = new NotifyingProxy<KeyDownStates>(KeyDownStates.Up);
            KeyStateService.Setup(s => s.KeyDownStates).Returns(keyDownStates);
        }

        [Test]
        public void ThenA_CurrentPositionEventHandlerShouldBeAttachedToInputService()
        {
            var point = new Point(27, 10);
            var keyValue = new KeyValue();
            InputService.Raise(s => s.CurrentPosition += null, this, new Tuple<Point, KeyValue>(point, keyValue));

            Assert.AreEqual(point, MainViewModel.CurrentPositionPoint);
            Assert.AreEqual(keyValue, MainViewModel.CurrentPositionKey);

            MouseOutputService.Verify(s => s.MoveTo(point), Times.Never());
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenEmptyProgress : WhenAttachServiceEventHandlers
    {
        protected NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get; set; }

        protected override void Arrange()
        {
            base.Arrange();

            KeySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
            KeySelectionProgress[new KeyValue()] = new NotifyingProxy<double>(123);
            KeyStateService.Setup(s => s.KeySelectionProgress)
                .Returns(KeySelectionProgress);
        }

        [Test]
        public void ThenSelectionProgressShouldBeReset()
        {

            InputService.Raise(s => s.SelectionProgress += null, this, new Tuple<PointAndKeyValue, double>(null, 0));

            Assert.IsNull(MainViewModel.PointSelectionProgress);

            KeySelectionProgress.Keys
                .ToList()
                .ForEach(k => Assert.AreEqual(default(double), KeySelectionProgress[k].Value));
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenProgressNotEmptyAndSelectionModeKey : WhenAttachServiceEventHandlers
    {
        protected NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get; set; }
        protected KeyValue KeyValueToAssert { get; set; }

        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.Key;

            KeyValueToAssert = new KeyValue();
            KeySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
            KeySelectionProgress[KeyValueToAssert] = new NotifyingProxy<double>(123);
            KeyStateService.Setup(s => s.KeySelectionProgress)
                .Returns(KeySelectionProgress);
        }

        [Test]
        public void ThenKeySelectionProgressOnKeyStateServiceShouldBeSet()
        {
            InputService.Raise(s => s.SelectionProgress += null, this, new Tuple<PointAndKeyValue, double>(
                new PointAndKeyValue(new Point(), KeyValueToAssert), 14));

            Assert.AreEqual(14, KeySelectionProgress[KeyValueToAssert].Value);
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenProgressNotEmptyAndSelectionModePoint : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.Point;
        }

        [Test]
        public void ThenPointSelectionProgressShouldBeSet()
        {
            var pointAndKeyValue = new PointAndKeyValue(new Point(), new KeyValue());

            InputService.Raise(s => s.SelectionProgress += null, this, new Tuple<PointAndKeyValue, double>(
                pointAndKeyValue, 83));

            Assert.IsNotNull(MainViewModel.PointSelectionProgress);
            Assert.AreEqual(pointAndKeyValue.Point, MainViewModel.PointSelectionProgress.Item1);
            Assert.AreEqual(83, MainViewModel.PointSelectionProgress.Item2);
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenSelectionModesKeyAndIsNotCapturingMultiKeySelection : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.Key;

            CapturingStateManager.Setup(s => s.CapturingMultiKeySelection)
                .Returns(false);
        }

        [Test]
        public void ThenSelectionProgressShouldBeProcessed()
        {
            var pointAndKeyValue = new PointAndKeyValue(new Point(), new KeyValue());

            InputService.Raise(s => s.Selection += null, this, pointAndKeyValue);

            Assert.IsNull(MainViewModel.SelectionResultPoints);

            AudioService.Verify(s => s.PlaySound(Settings.Default.KeySelectionSoundFile, Settings.Default.KeySelectionSoundVolume));

            Assert.IsTrue(IsKeySelectionEventHandlerCalled);
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenSelectionModesPoint : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.Point;
        }

        [Test]
        public void ThenSelectionProgressShouldBeProcessed()
        {
            var pointAndKeyValue = new PointAndKeyValue(new Point(), new KeyValue());

            InputService.Raise(s => s.Selection += null, this, pointAndKeyValue);

            Assert.IsNull(MainViewModel.SelectionResultPoints);

            Assert.IsTrue(IsPointSelectionEventHandlerCalled);
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenSelectionModesKey : WhenAttachServiceEventHandlers
    {
        [Test]
        public void GivenSingleKeyIsStringThenSelectionResultEventHandlerShouldBeAttachedToInputService()
        {
            var points = new List<Point>();
            var selection = new Tuple<List<Point>, KeyValue, List<string>>(
                points, new KeyValue("SingleKeyValueIsString"), new List<string>());

            InputService.Raise(s => s.SelectionResult += null, this, selection);

            Assert.AreEqual(points, MainViewModel.SelectionResultPoints);

            KeyboardOutputService.Verify(s => s.ProcessSingleKeyText("SingleKeyValueIsString"), Times.Once());
        }

        [Test]
        public void GivenSingleKeyIsFunctionKeySelectionResultEventHandlerShouldBeAttachedToInputService()
        {
            var points = new List<Point>();
            var selection = new Tuple<List<Point>, KeyValue, List<string>>(
                points, new KeyValue(FunctionKeys.Suggestion1), new List<string>());

            InputService.Raise(s => s.SelectionResult += null, this, selection);

            Assert.AreEqual(points, MainViewModel.SelectionResultPoints);

            KeyStateService.Verify(s => s.ProgressKeyDownState(It.IsAny<KeyValue>()), Times.Once());

            //TODO: validate 101 function key cases of MainViewModel.HandleFunctionKeySelectionResult

            KeyboardOutputService.Verify(s => s.ProcessFunctionKey(It.IsAny<FunctionKeys>()), Times.Once());
        }

        [Test]
        public void GivenMultiKeySelectionThenSelectionResultEventHandlerShouldBeAttachedToInputService()
        {
            var points = new List<Point>();
            var multiKeySelection = new List<string> { "test-multi" };
            var selection = new Tuple<List<Point>, KeyValue, List<string>>(
                points, null, multiKeySelection);

            InputService.Raise(s => s.SelectionResult += null, this, selection);

            Assert.AreEqual(points, MainViewModel.SelectionResultPoints);

            KeyboardOutputService.Verify(s => s.ProcessMultiKeyTextAndSuggestions(multiKeySelection), Times.Once());
        }
    }
}
