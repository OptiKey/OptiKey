using Microsoft.Reactive.Testing;
using NUnit.Framework;
using TestStack.BDDfy;

namespace JuliusSweetland.OptiKey.UnitTests.Observables.TriggerSources
{
    [TestFixture]
    public class KeyboardKeyDownUpSourceTests : ReactiveTest
    {
        [Test]
        public void KeyDown_with_PointSource_yields_TriggerSignal()
        {
            new KeyboardKeyDownUpNominalScenario()
                .Given(s => s.Given_a_Running_KeyboardKeyDownUpSource())
                .When(s => s.When_PointSource_event())
                .When(s => s.When_a_triggerkey_down())
                .Then(s => s.Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown())
                .BDDfy();

            new KeyboardKeyDownUpNominalScenario()
                .Given(s => s.Given_a_Running_KeyboardKeyDownUpSource())
                .When(s => s.When_a_triggerkey_down())
                .When(s => s.When_PointSource_event())
                .Then(s => s.Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown())
                .BDDfy();
        }

        [Test]
        public void KeyDown_KeyUp_with_PointSource_yields_two_TriggerSignals()
        {
            new KeyboardKeyDownUpNominalScenario()
                .Given(s => s.Given_a_Running_KeyboardKeyDownUpSource())
                .When(s => s.When_PointSource_event())
                .When(s => s.When_a_triggerkey_down())
                .When(s => s.When_a_triggerkey_up())
                .Then(s => s.Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown_then_KeyUp())
                .BDDfy();

            new KeyboardKeyDownUpNominalScenario()
                .Given(s => s.Given_a_Running_KeyboardKeyDownUpSource())
                .When(s => s.When_a_triggerkey_down())
                .When(s => s.When_PointSource_event())
                .When(s => s.When_a_triggerkey_up())
                .Then(s => s.Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown_then_KeyUp())
                .BDDfy();
        }

        [Test]
        public void Paused_does_not_yield_values()
        {
            new KeyboardKeyDownUpNominalScenario()
                .Given(s => s.Given_a_Paused_KeyboardKeyDownUpSource())
                .When(s => s.When_PointSource_event())
                .When(s => s.When_a_triggerkey_down())
                .Then(s => s.Then_Sequence_yeilds_no_values())
                .BDDfy();
        }

        [Test]
        public void Pausing_mutes_sequence()
        {
            new KeyboardKeyDownUpNominalScenario()
                .Given(s => s.Given_a_Running_KeyboardKeyDownUpSource())
                .When(s => s.When_PointSource_event())
                .When(s => s.When_a_triggerkey_down())
                .When(s => s.When_Paused())
                .When(s => s.When_a_triggerkey_up())
                .Then(s => s.Then_Sequence_yields_TriggerSignal_with_PointSource_location_and_KeyDown())
                .BDDfy();
        }
    }
}
