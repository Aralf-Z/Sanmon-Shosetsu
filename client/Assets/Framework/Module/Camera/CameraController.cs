using Alchemy.Inspector;
using UnityEngine;

namespace Framework.Module
{
    internal class CameraController: MonoBehaviour
    {
        [LabelText("主相机")] public Camera main;
        [LabelText("UI")] public Camera ui;
    }
}