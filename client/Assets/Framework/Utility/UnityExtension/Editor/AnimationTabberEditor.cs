using System.Collections.Generic;
using Sanmon.Utility.UnityExtension;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Sanmon.Utility.UnityExtension.Editor
{
    [CustomEditor(typeof(AnimationTabber))]
    public class AnimationTabberEditor : UnityEditor.Editor
    {
        private AnimationTabber mAs;
        private SerializedObject mAsSo;

        private string mNewName;
        private SerializedProperty mTriggers;

        private void OnEnable()
        {
            mAs = (AnimationTabber)target;
            mAsSo = new SerializedObject(mAs);
            mTriggers = mAsSo.FindProperty("triggers");
        }

        public override void OnInspectorGUI()
        {
            var changed = false;
            var animator = mAs.GetComponent<Animator>();

            //triggers Name
            using (var v = new EditorGUILayout.VerticalScope())
            {
                //rename and delete
                for (var i = 0; i < mTriggers.arraySize; i++)
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        var trigger = mTriggers.GetArrayElementAtIndex(i).stringValue;
                        var newTrigger = EditorGUILayout.DelayedTextField("name: ", trigger);

                        if (newTrigger != trigger)
                        {
                            if (IsTriggerValid(newTrigger))
                            {
                                mTriggers.GetArrayElementAtIndex(i).stringValue = newTrigger;
                                if (animator && animator.runtimeAnimatorController)
                                {
                                    var controller = (AnimatorController)animator.runtimeAnimatorController;
                                    RenameTriggerableTransition(trigger, newTrigger, controller);
                                }

                                changed = true;
                            }
                            else
                            {
                                EditorUtility.DisplayDialog("错误提示", "状态为空，或已存在", "确定");
                            }
                        }

                        if (GUILayout.Button("X", Styles.NormalButtonStyle, GUILayout.Width(50)))
                        {
                            mTriggers.DeleteArrayElementAtIndex(i);
                            if (animator && animator.runtimeAnimatorController)
                            {
                                var controller = (AnimatorController)animator.runtimeAnimatorController;
                                DeleteTriggerableTransition(trigger, controller);
                            }

                            changed = true;
                        }
                    }
                }

                //add new
                using (new EditorGUILayout.HorizontalScope())
                {
                    mNewName = EditorGUILayout.TextField("name: ", mNewName);

                    if (GUILayout.Button("add new", Styles.NormalButtonStyle, GUILayout.Width(100)))
                    {
                        if (IsTriggerValid(mNewName))
                        {
                            if (animator && animator.runtimeAnimatorController)
                            {
                                var controller = (AnimatorController)animator.runtimeAnimatorController;
                                AddTriggerableTransition(mNewName, controller);
                            }

                            mTriggers.InsertArrayElementAtIndex(mTriggers.arraySize);
                            mTriggers.GetArrayElementAtIndex(mTriggers.arraySize - 1).stringValue = mNewName;
                            changed = true;
                        }
                        else
                        {
                            EditorUtility.DisplayDialog("错误提示", "状态为空，或已存在", "确定");
                        }
                    }
                }
            }

            //generate Animator or AnimatorController
            if (!animator || !animator.runtimeAnimatorController)
            {
                var button = animator ? "Generate AnimatorController" : "Generate Animator";

                if (GUILayout.Button(button, Styles.NormalButtonStyle, GUILayout.Width(200)))
                {
                    var triggers = new List<string>();

                    for (var i = 0; i < mTriggers.arraySize; i++)
                    {
                        var element = mTriggers.GetArrayElementAtIndex(i).stringValue;
                        triggers.Add(element);
                    }

                    if (!animator)
                    {
                        animator = mAs.gameObject.AddComponent<Animator>();
                    }

                    var controller = animator.runtimeAnimatorController
                        ? (AnimatorController)animator.runtimeAnimatorController
                        : GenerateSelectableAnimatorController(triggers);

                    AnimatorController.SetAnimatorController(animator, controller);

                    changed = true;
                }
            }

            if (changed)
            {
                mAsSo.ApplyModifiedProperties();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        private bool IsTriggerValid(string triggerName)
        {
            if (string.IsNullOrEmpty(triggerName)) return false;

            for (var i = 0; i < mTriggers.arraySize; i++)
            {
                if (triggerName == mTriggers.GetArrayElementAtIndex(i).stringValue)
                {
                    return false;
                }
            }

            return true;
        }

        private AnimatorController GenerateSelectableAnimatorController(IEnumerable<string> triggers)
        {
            if (target == null)
                return null;

            var path = GetSaveControllerPath();
            if (string.IsNullOrEmpty(path))
                return null;

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            foreach (var trigger in triggers)
            {
                AddTriggerableTransition(trigger, controller);
            }

            AssetDatabase.ImportAsset(path);

            return controller;
        }

        private string GetSaveControllerPath()
        {
            var defaultName = ((AnimationTabber)target).gameObject.name;
            var message = $"Create a new animator for the game object 'defaultName':";
            return EditorUtility.SaveFilePanelInProject("New Animation Controller", defaultName, "controller", message);
        }

        private static void AddTriggerableTransition(string name, AnimatorController controller)
        {
            var clip = AnimatorController.AllocateAnimatorClip(name);
            AnimationUtility.SetAnimationClipSettings(clip, new AnimationClipSettings() { loopTime = false });
            AssetDatabase.AddObjectToAsset(clip, controller);
            var state = controller.AddMotion(clip);
            controller.AddParameter(name, AnimatorControllerParameterType.Trigger);
            var stateMachine = controller.layers[0].stateMachine;
            var transition = stateMachine.AddAnyStateTransition(state);
            transition.AddCondition(AnimatorConditionMode.If, 0, name);
            transition.duration = 0;

            EditorUtility.SetDirty(controller);
        }

        private static void RenameTriggerableTransition(string oldName, string newName, AnimatorController controller)
        {
            var parameters = controller.parameters;

            for (var i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                if (p.name == oldName && p.type == AnimatorControllerParameterType.Trigger)
                {
                    p.name = newName;
                    parameters[i] = p;
                    controller.parameters = parameters;
                    break;
                }
            }

            var sm = controller.layers[0].stateMachine;
            foreach (var childAnimatorState in sm.states)
            {
                if (childAnimatorState.state.name == oldName)
                {
                    childAnimatorState.state.name = newName;
                    childAnimatorState.state.motion.name = newName;
                    ((AnimationClip)childAnimatorState.state.motion).name = newName;

                    foreach (var transition in sm.anyStateTransitions)
                    {
                        foreach (var condition in transition.conditions)
                        {
                            if (condition.parameter == oldName)
                            {
                                transition.AddCondition(AnimatorConditionMode.If, 0, newName);
                                transition.RemoveCondition(condition);
                                break;
                            }
                        }
                    }

                    break;
                }
            }

            EditorUtility.SetDirty(controller);
        }

        private static void DeleteTriggerableTransition(string name, AnimatorController controller)
        {
            var sm = controller.layers[0].stateMachine;
            foreach (var childAnimatorState in sm.states)
            {
                if (childAnimatorState.state.motion.name == name)
                {
                    var clip = ((AnimationClip)childAnimatorState.state.motion);
                    AssetDatabase.RemoveObjectFromAsset(clip);
                    sm.RemoveState(childAnimatorState.state);
                    break;
                }
            }

            foreach (var t in controller.parameters)
            {
                if (t.name == name && t.type == AnimatorControllerParameterType.Trigger)
                {
                    controller.RemoveParameter(t);
                    break;
                }
            }

            EditorUtility.SetDirty(controller);
        }

        private static class Styles
        {
            public static readonly GUIStyle NormalButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 22,
                padding = new RectOffset(10, 10, 4, 4),
                normal = new GUIStyleState()
                {
                    textColor = new Color(.9f, .9f, .9f)
                }
            };

            public static readonly GUIStyle HintButtonStyle = new GUIStyle(GUI.skin.button)
            {
                fontSize = 12,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter,
                fixedHeight = 22,
                padding = new RectOffset(10, 10, 4, 4),
                normal = new GUIStyleState()
                {
                    textColor = new Color(1f, 0.85f, 0.4f)
                },
                hover = new GUIStyleState()
                {
                    textColor = new Color(1f, 0.85f, 0.4f)
                },
            };
        }
    }
}