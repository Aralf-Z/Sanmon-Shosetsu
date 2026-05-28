using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Sanmon.Core;
using Sanmon.Module;
using UnityEngine;
using UnityEngine.UI;


namespace GameScripts
{
    public class DebugUI : UIWindow, IGetNote
    {
        public Text infoText;

        protected override void OnCreate()
        {
            
        }

        protected override void OnOpen()
        {
            
        }

        protected override void OnHide()
        {
            
        }

        protected override void OnClose()
        {
            
        }

        public override EmUIOrder Order => EmUIOrder.Tip;
    }
 
}