using Sanmon.Utility.Inspector;
using UnityEngine;

namespace Sanmon.Note
{
    [Inspectable]
    public abstract class NoteBase
    {
        protected internal abstract void Init();
    }
}