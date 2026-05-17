using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Sanmon.Editor
{
    public class WindowDefine
    {

        /// <summary>
        /// 停靠窗口类型集合
        /// </summary>
        public static readonly Type[] DockedWindowTypes =
        {
            typeof(PanelWindow),
            typeof(BuildWindow),
        };
    }
}
