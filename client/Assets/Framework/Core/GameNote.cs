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
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(NoteBase).IsAssignableFrom(t)))
                {
                    var note = (NoteBase)Activator.CreateInstance(type);
                    note.Init();
                    _notes.Add(type, note);
                    
                    Logger.LogInfo($"create note '{type.FullName}'", "note");
                }
            }
            
            Logger.LogInfo($"notes loaded, total: {_notes.Count}.", "note");
            
            IsInit = true;
        }

        internal void Destroy()
        {
            IsInit = false;
        }

        public T Get<T>() where T : NoteBase => _notes[typeof(T)] as T;
    }
}