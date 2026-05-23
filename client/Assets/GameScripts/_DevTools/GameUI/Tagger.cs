using System;
using UnityEngine;
using UnityEngine.UI;

namespace GameConsole.GameUI
{
    public class Tagger: MonoBehaviour
    {
        [SerializeField] private Button taggerButton;
        [SerializeField] private Color onSelectedColor = Color.cyan;
        [SerializeField] private Color onDeselectedColor = Color.gray;
        
        public string TagName { get; private set; }

        public void Init(Action<Tagger> onClick)
        {
            taggerButton.onClick.AddListener(() =>
            {
                onClick?.Invoke(this);
            });
            Deselect();
        }

        public void SetTagName(string tagName)
        {
            TagName = tagName;
            taggerButton.GetComponentInChildren<Text>().text = TagName;
        }
        
        public void Select()
        {
            taggerButton.targetGraphic.color = onSelectedColor;
        }
        
        public void Deselect()
        {
            taggerButton.targetGraphic.color = onDeselectedColor;
        }
    }
}