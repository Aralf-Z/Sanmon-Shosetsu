using System;
using System.Collections;
using UnityEngine;

namespace Sanmon.Core
{
    /// <summary>
    /// 游戏入口，初始化gameApplication
    /// </summary>
    public class GameEntry : MonoBehaviour
    {
        public GameApplication gameApplication;

        private void Start()
        {
            DontDestroyOnLoad(gameObject);
            DontDestroyOnLoad(gameApplication);
            
            StartCoroutine(StartGame());
        }
        
        private IEnumerator StartGame()
        {
            yield return null;
            gameApplication.StartGame();
        }
    }
}