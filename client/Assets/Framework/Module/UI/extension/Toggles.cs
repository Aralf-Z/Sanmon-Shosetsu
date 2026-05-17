using UnityEngine.UI;

namespace Sanmon.Module
{
    public class Toggles
    {
        public Toggle CurToggle { get; private set; }

        public Toggles(Toggle defaultToggle,  params Toggle[] toggles)
        {
            CurToggle = defaultToggle;
            
            foreach (var toggle in toggles)
            {
                toggle.isOn = toggle == CurToggle;
            }
            
            foreach (var toggle in toggles)
            {
                toggle.onValueChanged.AddListener(isOn => OnToggleChange(toggle, isOn));
            }
        }

        private void OnToggleChange(Toggle toggle, bool isOn)
        {
            if (isOn)
            {
                CurToggle.isOn = false;
                CurToggle = toggle;
            }
        }
    }
}
