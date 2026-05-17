namespace Sanmon.Utility.UnityExtension
{
    public interface INavSet
    {
        void ResetFocus(bool focusFirst);
        void SetShow(bool show);
        void Focus(int index);
        void Unfocus(int index);
        bool MoveLeft();
        bool MoveRight();
        bool MoveUp();
        bool MoveDown();
        bool MoveIn(int index);
        bool MoveOut();
    }
}