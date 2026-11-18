using System;
using System.Globalization;
using UnityEngine.UIElements;

namespace GameConsole.UI
{
    public sealed partial class ToolkitOptionsPanel
    {
        private void BindWheelScrollSensitivity(VisualElement root, ToolkitWindowSettings settings)
        {
            var field = ToolkitView.Require<Slider>(root, "wheelScrollSensitivity");

            field.lowValue = 0f;
            field.highValue = Config.MAX_WHEEL_SCROLL_SENSITIVITY;
            _text.Track(field.labelElement);
            Action refresh = () =>
            {
                var value = settings.WheelScrollSensitivity;

                field.SetValueWithoutNotify(value);
                _text.SetText(field.labelElement, ToolkitLocalization.Format("ui.Options.wheelScrollSensitivity.value", ("value", value.ToString("0.00", CultureInfo.InvariantCulture))));
            };

            refresh();
            EventCallback<ChangeEvent<float>> change = evt =>
            {
                settings.WheelScrollSensitivity = evt.newValue;
                field.SetValueWithoutNotify(settings.WheelScrollSensitivity);
            };

            field.RegisterValueChangedCallback(change);
            _unbind.Add(() => field.UnregisterValueChangedCallback(change));
            _refresh.Add(refresh);
        }
    }
}
