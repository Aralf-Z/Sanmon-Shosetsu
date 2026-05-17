namespace Sanmon.Core
{
    public interface IGetNote
    {
        
    }

    public static class IGetNoteExtension
    {
        public static GameNote Note(this IGetNote getNote)
        {
            return GameApplication.Instance.gameNote;
        }
    }
}