namespace JuliusSweetland.OptiKey.Services
{
    public interface IWindowManipulationService : INotifyErrors
    {
        void ArrangeWindowsHorizontally();
        void ArrangeWindowsMaximised();
        void ArrangeWindowsVertically();
        void ExpandToBottom(double pixels);
        void ExpandToBottomAndLeft(double pixels);
        void ExpandToBottomAndRight(double pixels);
        void ExpandToLeft(double pixels);
        void ExpandToRight(double pixels);
        void ExpandToTop(double pixels);
        void ExpandToTopAndLeft(double pixels);
        void ExpandToTopAndRight(double pixels);
        void Maximise();
        void MinimiseToBottomAndLeftBoundaries();
        void MinimiseToBottomAndRightBoundaries();
        void MinimiseToMiddleOfBottomBoundary();
        void MinimiseToMiddleOfLeftBoundary();
        void MinimiseToMiddleOfRightBoundary();
        void MinimiseToMiddleOfTopBoundary();
        void MinimiseToTopAndLeftBoundaries();
        void MinimiseToTopAndRightBoundaries();
        void MoveToBottom(double pixels);
        void MoveToBottomAndLeft(double pixels);
        void MoveToBottomAndLeftBoundaries();
        void MoveToBottomAndRight(double pixels);
        void MoveToBottomAndRightBoundaries();
        void MoveToBottomBoundary();
        void MoveToLeft(double pixels);
        void MoveToLeftBoundary();
        void MoveToRight(double pixels);
        void MoveToRightBoundary();
        void MoveToTop(double pixels);
        void MoveToTopAndLeft(double pixels);
        void MoveToTopAndLeftBoundaries();
        void MoveToTopAndRight(double pixels);
        void MoveToTopAndRightBoundaries();
        void MoveToTopBoundary();
        void RestoreFromMaximised();
        void RestoreFromMinimised();
        void ShrinkFromBottom(double pixels);
        void ShrinkFromBottomAndLeft(double pixels);
        void ShrinkFromBottomAndRight(double pixels);
        void ShrinkFromLeft(double pixels);
        void ShrinkFromRight(double pixels);
        void ShrinkFromTop(double pixels);
        void ShrinkFromTopAndLeft(double pixels);
        void ShrinkFromTopAndRight(double pixels);
        void IncreaseOpacity();
        void DecreaseOpacity();
    }
}
