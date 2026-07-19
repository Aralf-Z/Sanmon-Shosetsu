using Sanmon.Entities;
using UnityEditor;
using UnityEngine;


namespace GameScripts
{
    public class CharRegister: MonoBehaviour
    {
        public Entity self;
        
        public CmResource res;
        
        public CmAttribute attri;
        
        private void Awake()
        {
            self = new Entity();
            
            attri = self.AddComponent<CmAttribute>();
            res = self.AddComponent<CmResource>();
            self.AddComponent<CmWorldModel>().SetModel(gameObject);
            
            var hp = attri.AddValue("hp", 100);
            attri.AddValue("atk", 10);
            res.Add("hp", hp, hp.Value);
            
            Game.Entity.Register(self);

            if (name == "Player")
            {
                self.AddEffect(new EfInputMove());
            }
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
            
            GUILayout.Label($"hp: {charRegister.attri["hp"].Value}/{charRegister.res["hp"]}");
            GUILayout.Label($"atk: {charRegister.attri["atk"].Value}");
        }
    }
}
