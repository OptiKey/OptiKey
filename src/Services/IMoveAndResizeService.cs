namespace JuliusSweetland.ETTA.Services
{
    public interface IMoveAndResizeService : INotifyErrors
    {
        void ExpandToBottom(double pixels);
        void ExpandToBottomAndLeft(double pixels);
        void ExpandToBottomAndRight(double pixels);
        void ExpandToLeft(double pixels);
        void ExpandToRight(double pixels);
        void ExpandToTop(double pixels);
        void ExpandToTopAndLeft(double pixels);
        void ExpandToTopAndRight(double pixels);
        void Maximise();
        void MoveLeft(double pixels);
        void MoveLeftToBoundary();
        void MoveRight(double pixels);
        void MoveRightToBoundary();
        void MoveUp(double pixels);
        void MoveUpToBoundary();
        void MoveDown(double pixels);
        void MoveDownToBoundary();
        void Restore();
        void ShrinkFromBottom(double pixels);
        void ShrinkFromBottomAndLeft(double pixels);
        void ShrinkFromBottomAndRight(double pixels);
        void ShrinkFromLeft(double pixels);
        void ShrinkFromRight(double pixels);
        void ShrinkFromTop(double pixels);
        void ShrinkFromTopAndLeft(double pixels);
        void ShrinkFromTopAndRight(double pixels);
    }
}
