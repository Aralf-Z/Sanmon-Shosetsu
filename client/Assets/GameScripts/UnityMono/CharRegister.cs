using Sanmon.GameEntity;
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
}
