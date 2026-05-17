using Sanmon.Core;
using UnityEngine;

namespace Sanmon.Syztem
{
    public abstract class SystemBase: 
        IGetModule
        , IGetNote
    {
        protected internal abstract void Init();
    }
}