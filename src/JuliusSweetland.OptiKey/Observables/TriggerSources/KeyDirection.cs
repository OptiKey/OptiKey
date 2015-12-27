namespace JuliusSweetland.OptiKey.Observables.TriggerSources
{
    public sealed class KeyDirection
    {
        public static readonly KeyDirection KeyDown = new KeyDirection(true);
        public static readonly KeyDirection KeyUp = new KeyDirection(false);

        private KeyDirection(bool isKeyDown)
        {
            IsKeyDown = isKeyDown;
        }

        public bool IsKeyDown { get;}
    }
}