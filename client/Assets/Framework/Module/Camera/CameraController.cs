using Alchemy.Inspector;
using UnityEngine;
using Unity.Cinemachine;

namespace Framework.Module
{
    internal class CameraController: MonoBehaviour
    {
        [LabelText("主相机")] public Camera main;
        [LabelText("UI")] public Camera ui;
        
        [LabelText("cm1")] public CinemachineCamera normalCamera;
        
        
    }
}