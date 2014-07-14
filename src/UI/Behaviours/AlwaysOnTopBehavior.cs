using System.Windows;
using System.Windows.Interactivity;

namespace JuliusSweetland.ETTA.UI.Behaviours
{
    /*
       Usage:
       <i:Interaction.Behaviors>
         <behaviors:AlwaysOnTopBehavior/>
       </i:Interaction.Behaviors>
     */
    public class AlwaysOnTopBehavior : Behavior<Window>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LostFocus += (s, e) => AssociatedObject.Topmost = true;
        }
    }
}
