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
        private readonly Dictionary<Type, NoteBase> mNotes = new Dictionary<Type, NoteBase>();
        
        public IReadOnlyCollection<NoteBase> Notes => mNotes.Values;
        
        internal bool IsInited { get; private set; }
        
        internal void Init()
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var count = 0;

            foreach (var assembly in assemblies)
            {
                foreach (var type in assembly.GetTypes().Where(t => !t.IsAbstract && typeof(NoteBase).IsAssignableFrom(t)))
                {
                    var note = (NoteBase)Activator.CreateInstance(type);
                    note.Init();
                    mNotes.Add(type, note);
                    count++;
                    Logger.LogInfo($"create note '{type.FullName}'", "note");
                }
            }
            
            Logger.LogInfo($"notes loaded '{count}'.", "note");
            
            IsInited = true;
        }

        internal void Destroy()
        {
            IsInited = false;
        }

        public T Get<T>() where T : NoteBase => mNotes[typeof(T)] as T;
    }
}