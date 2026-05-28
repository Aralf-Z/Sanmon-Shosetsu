using Sanmon.Entities;
using UnityEditor;
using UnityEngine;


namespace GameScripts
{
    public class CharRegister: MonoBehaviour
    {
        public Entity self;
        
        public Resource res;
        
        public Attribute attri;
        
        private void Awake()
        {
            self = new Entity();
            
            attri = self.AddComponent<Attribute>();
            res = self.AddComponent<Resource>();
            self.AddComponent<WorldModel>().SetModel(gameObject);
            
            var hp = attri.AddValue("hp", 100);
            attri.AddValue("atk", 10);
            res.Add("hp", hp, hp.Value);
            
            Game.Entity.Register(self);
        }
    }

    [CustomEditor(typeof(CharRegister))]
    public class CharRegisterEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            var charRegister = (CharRegister)target;
            
            if(!EditorApplication.isPlaying) return;
            
            GUILayout.Label($"hp: {charRegister.attri["hp"]}/{charRegister.res["hp"]}");
            GUILayout.Label($"atk: {charRegister.attri["atk"]}");
        }
    }
}
