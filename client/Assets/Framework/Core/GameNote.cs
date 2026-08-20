using System;
using System.Collections.Generic;
using System.Linq;
using Sanmon.Note;
using UnityEngine;
using Logger = Sanmon.Helper.Logger;

namespace Sanmon.Core
{
    /// <summary>
    /// 游戏记录器，与存档交互，由System修改
    /// </summary>
    public class GameNote: MonoBehaviour
    {
        private readonly Dictionary<Type, NoteBase> _notes = new Dictionary<Type, NoteBase>();
        
        public IReadOnlyCollection<NoteBase> Notes => _notes.Values;
        
        internal bool IsInit { get; private set; }
        
        internal void Init()
        {
            IsInit = true;
        }

        internal void Destroy()
        {
            IsInit = false;
        }

        public T Get<T>() where T : NoteBase
        {
            var type = typeof(T);
            if (_notes.TryGetValue(type, out var note)) return (T)note;
            
            var @new = (T)Activator.CreateInstance(type);
            _notes.Add(type, @new);
            
            Logger.LogInfo($"create note '{type.FullName}'", "note");
            
            return @new;
        }
    }
}