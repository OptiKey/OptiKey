// Copyright (c) 2022 OPTIKEY LTD (UK company number 11854839) - All Rights Reserved
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

            Assert.That(NotificationEvent, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(NotificationEvent.Title, Is.EqualTo(Resources.CRASH_TITLE));
                Assert.That(NotificationEvent.Content, Is.EqualTo(ex.Message));
                Assert.That(NotificationEvent.NotificationType, Is.EqualTo(NotificationTypes.Error));
            });            
        }

        [Test]
        public void ThenPointsPerSecondEventHandlerShouldBeAttachedToInputService()
        {
            InputService.Raise(s => s.PointsPerSecond += null, this, 123);
            Assert.That(MainViewModel.PointsPerSecond, Is.EqualTo(123));
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

            Assert.Multiple(() =>
            {
                Assert.That(MainViewModel.CurrentPositionPoint, Is.EqualTo(point));
                Assert.That(MainViewModel.CurrentPositionKey, Is.EqualTo(keyValue));
            });            

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

            Assert.Multiple(() =>
            {
                Assert.That(MainViewModel.CurrentPositionPoint, Is.EqualTo(point));
                Assert.That(MainViewModel.CurrentPositionKey, Is.EqualTo(keyValue));
            });            

            MouseOutputService.Verify(s => s.MoveTo(point), Times.Never());
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenEmptyPointProgress : WhenAttachServiceEventHandlers
    {
        protected NotifyingConcurrentDictionary<KeyValue, double> KeySelectionProgress { get; set; }

        protected override void Arrange()
        {
            base.Arrange();
        }

        [Test]
        public void ThenSelectionProgressShouldBeReset()
        {
            InputService.Raise(s => s.SelectionProgress += null, this, new Tuple<TriggerTypes, PointAndKeyValue, double>(TriggerTypes.Point, null, 0));
            Assert.That(MainViewModel.PointSelectionProgress, Is.Null);
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenEmptyKeyProgress : WhenAttachServiceEventHandlers
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
            InputService.Raise(s => s.SelectionProgress += null, this, new Tuple<TriggerTypes, PointAndKeyValue, double>(TriggerTypes.Key, null, 0));

            KeySelectionProgress.Keys
                .ToList()
                .ForEach(k => Assert.That(KeySelectionProgress[k].Value, Is.EqualTo(default(double))));
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

            MainViewModel.SelectionMode = SelectionModes.Keys;

            KeyValueToAssert = new KeyValue();
            KeySelectionProgress = new NotifyingConcurrentDictionary<KeyValue, double>();
            KeySelectionProgress[KeyValueToAssert] = new NotifyingProxy<double>(123);
            KeyStateService.Setup(s => s.KeySelectionProgress)
                .Returns(KeySelectionProgress);
        }

        [Test]
        public void ThenKeySelectionProgressOnKeyStateServiceShouldBeSet()
        {
            InputService.Raise(s => s.SelectionProgress += null, this, new Tuple<TriggerTypes, PointAndKeyValue, double>(
                TriggerTypes.Key, new PointAndKeyValue(new Point(), KeyValueToAssert), 14));

            Assert.That(KeySelectionProgress[KeyValueToAssert].Value, Is.EqualTo(14));
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenProgressNotEmptyAndSelectionModePoint : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.SinglePoint;
        }

        [Test]
        public void ThenPointSelectionProgressShouldBeSet()
        {
            var pointAndKeyValue = new PointAndKeyValue(new Point(), new KeyValue());

            InputService.Raise(s => s.SelectionProgress += null, this, 
                new Tuple<TriggerTypes, PointAndKeyValue, double>(
                    TriggerTypes.Point, pointAndKeyValue, 83));

            Assert.That(MainViewModel.PointSelectionProgress, Is.Not.Null);

            Assert.Multiple(() =>
            {
                Assert.That(MainViewModel.PointSelectionProgress.Item1, Is.EqualTo(pointAndKeyValue.Point));
                Assert.That(MainViewModel.PointSelectionProgress.Item2, Is.EqualTo(83));
            });            
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenSelectionModesKeyAndIsNotCapturingMultiKeySelection : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.Keys;

            CapturingStateManager.Setup(s => s.CapturingMultiKeySelection)
                .Returns(false);
        }

        [Test]
        public void ThenSelectionProgressShouldBeProcessed()
        {
            var pointAndKeyValue = new PointAndKeyValue(new Point(), new KeyValue());

            InputService.Raise(s => s.Selection += null, this, new Tuple<TriggerTypes, PointAndKeyValue>(TriggerTypes.Key, pointAndKeyValue));

            Assert.That(MainViewModel.SelectionResultPoints, Is.Null);

            AudioService.Verify(s => s.PlaySound(Settings.Default.KeySelectionSoundFile, Settings.Default.KeySelectionSoundVolume));

            Assert.That(IsKeySelectionEventHandlerCalled, Is.True);
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenSelectionModesPoint : WhenAttachServiceEventHandlers
    {
        protected override void Arrange()
        {
            base.Arrange();

            MainViewModel.SelectionMode = SelectionModes.SinglePoint;
        }

        [Test]
        public void ThenSelectionProgressShouldBeProcessed()
        {
            var pointAndKeyValue = new PointAndKeyValue(new Point(), new KeyValue());

            InputService.Raise(s => s.Selection += null, this, new Tuple<TriggerTypes, PointAndKeyValue>(
                TriggerTypes.Point, pointAndKeyValue));

            Assert.Multiple(() =>
            {
                Assert.That(MainViewModel.SelectionResultPoints, Is.Null);
                Assert.That(IsPointSelectionEventHandlerCalled, Is.True);
            });            
        }
    }

    [TestFixture]
    public class WhenAttachServiceEventHandlersGivenSelectionModesKey : WhenAttachServiceEventHandlers
    {
        [Test]
        public void GivenSingleKeyIsStringThenSelectionResultEventHandlerShouldBeAttachedToInputService()
        {
            var points = new List<Point>();
            var selection = new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                TriggerTypes.Key, points, new KeyValue("SingleKeyValueIsString"), new List<string>());

            InputService.Raise(s => s.SelectionResult += null, this, selection);

            Assert.That(MainViewModel.SelectionResultPoints, Is.EqualTo(points));

            KeyboardOutputService.Verify(s => s.ProcessSingleKeyText("SingleKeyValueIsString"), Times.Once());
        }

        [Test]
        public void GivenSingleKeyIsFunctionKeySelectionResultEventHandlerShouldBeAttachedToInputService()
        {
            var points = new List<Point>();
            var selection = new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                TriggerTypes.Key, points, new KeyValue(FunctionKeys.Suggestion1), new List<string>());

            InputService.Raise(s => s.SelectionResult += null, this, selection);

            Assert.That(MainViewModel.SelectionResultPoints, Is.EqualTo(points));

            KeyStateService.Verify(s => s.ProgressKeyDownState(It.IsAny<KeyValue>()), Times.Once());

            //TODO: validate 101 function key cases of MainViewModel.HandleFunctionKeySelectionResult

            KeyboardOutputService.Verify(s => s.ProcessFunctionKey(It.IsAny<FunctionKeys>()), Times.Once());
        }

        [Test]
        public void GivenMultiKeySelectionThenSelectionResultEventHandlerShouldBeAttachedToInputService()
        {
            var points = new List<Point>();
            var multiKeySelection = new List<string> { "test-multi" };
            var selection = new Tuple<TriggerTypes, List<Point>, KeyValue, List<string>>(
                TriggerTypes.Key, points, null, multiKeySelection);

            InputService.Raise(s => s.SelectionResult += null, this, selection);

            Assert.That(MainViewModel.SelectionResultPoints, Is.EqualTo(points));

            KeyboardOutputService.Verify(s => s.ProcessMultiKeyTextAndSuggestions(multiKeySelection), Times.Once());
        }
    }
}
