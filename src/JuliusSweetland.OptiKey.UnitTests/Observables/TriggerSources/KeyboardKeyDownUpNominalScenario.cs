using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using JuliusSweetland.OptiKey.Enums;
using JuliusSweetland.OptiKey.Models;
using JuliusSweetland.OptiKey.Observables.PointSources;
using JuliusSweetland.OptiKey.Observables.TriggerSources;
using Microsoft.Reactive.Testing;
using Moq;
using NUnit.Framework;

namespace JuliusSweetland.OptiKey.UnitTests.Observables.TriggerSources
{
    public class KeyboardKeyDownUpNominalScenario
    {
        #region Fields

        private readonly TestScheduler testScheduler;

        private readonly Keys triggerKey;
        private readonly PointAndKeyValue? pushedPoint;

        private readonly IList<Recorded<Notification<KeyDirection>>> keyPressEvents =
            new List<Recorded<Notification<KeyDirection>>>();

        private readonly IList<Recorded<Notification<PointAndKeyValue?>>> pointEvents =
            new List<Recorded<Notification<PointAndKeyValue?>>>();

        private readonly Lazy<IList<Recorded<Notification<TriggerSignal>>>> messages;
        private readonly Lazy<KeyboardKeyDownUpSource> sut;
        private RunningStates state;
        private long clock;

        #endregion

        public KeyboardKeyDownUpNominalScenario()
        {
            testScheduler = new TestScheduler();
            sut = new Lazy<KeyboardKeyDownUpSource>(CreateSystemUnderTest);
            messages = new Lazy<IList<Recorded<Notification<TriggerSignal>>>>(GetObservedData);

            triggerKey = Enums.Keys.Insert;
            pushedPoint = new PointAndKeyValue(new Point(1, 2),
                new KeyValue { FunctionKey = Enums.FunctionKeys.Escape });
        }

        public void Given_a_Running_KeyboardKeyDownUpSource()
        {
            state = Enums.RunningStates.Running;
        }
        public void Given_a_Paused_KeyboardKeyDownUpSource()
        {
            state = Enums.RunningStates.Paused;
        }

        public void When_a_triggerkey_down()
        {
            keyPressEvents.Add(ReactiveTest.OnNext(NextTick(), KeyDirection.KeyDown));
        }
        public void When_a_triggerkey_up()
        {
            keyPressEvents.Add(ReactiveTest.OnNext(NextTick(), KeyDirection.KeyUp));
        }
        public void When_PointSource_event()
        {
            pointEvents.Add(ReactiveTest.OnNext(NextTick(), pushedPoint));
        }
        public void When_Paused()
        {
            var dueTime = TimeSpan.FromTicks(NextTick());
            testScheduler.Schedule(dueTime, () => { sut.Value.State = Enums.RunningStates.Paused; });
        }

        public void Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown()
        {
            var expected = new Recorded<Notification<TriggerSignal>>[]
            {
                ReactiveTest.OnNext(200, new TriggerSignal(1, null, pushedPoint)),
            };
            CollectionAssert.AreEqual(expected, messages.Value);
        }
        public void Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown_then_KeyUp()
        {
            var expected = new Recorded<Notification<TriggerSignal>>[]
            {
                ReactiveTest.OnNext(200, new TriggerSignal(1, null, pushedPoint)),
                ReactiveTest.OnNext(300, new TriggerSignal(-1, null, pushedPoint)),
            };
            CollectionAssert.AreEqual(expected, messages.Value);
        }
        public void Then_Sequence_yeilds_no_values()
        {
            var expected = new Recorded<Notification<TriggerSignal>>[]
            {
            };
            CollectionAssert.AreEqual(expected, messages.Value);
        }

        #region Private Methods

        private IList<Recorded<Notification<TriggerSignal>>> GetObservedData()
        {
            var testableObserver = testScheduler.CreateObserver<TriggerSignal>();
            using (sut.Value.Sequence.Subscribe(testableObserver))
            {
                testScheduler.Start();
            }
            return testableObserver.Messages;
        }

        private KeyboardKeyDownUpSource CreateSystemUnderTest()
        {
            var pointSource = CreatePointSource();
            var keyboardHookListener = CreateKeyboardHookListener();

            return new KeyboardKeyDownUpSource(
                triggerKey,
                keyboardHookListener,
                pointSource)
            { State = state };

        }

        private IPointSource CreatePointSource()
        {
            var sequence = testScheduler.CreateHotObservable(this.pointEvents.ToArray());
            var pointSource = new Mock<IPointSource>();
            pointSource.Setup(ps => ps.Sequence).Returns(sequence.Timestamp(testScheduler));
            return pointSource.Object;
        }

        private IKeyboardHookListener CreateKeyboardHookListener()
        {
            var keyPresses = testScheduler.CreateHotObservable(this.keyPressEvents.ToArray());
            var keyboardHookListenerMock = new Mock<IKeyboardHookListener>();
            keyboardHookListenerMock.Setup(khl => khl.KeyMovements(It.IsAny<System.Windows.Forms.Keys>()))
                .Returns(Observable.Never<KeyDirection>());
            var expected = (System.Windows.Forms.Keys)triggerKey;
            keyboardHookListenerMock.Setup(khl => khl.KeyMovements(expected))
                .Returns(keyPresses);
            return keyboardHookListenerMock.Object;
        }

        private long NextTick()
        {
            return (clock += 100);
        }
        
        #endregion
    }
}