using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEditorInternal;
using UnityEngine;

namespace FinTOKMAK.SkillSystem
{
    [CustomEditor(typeof(SkillInfo))]
    public class SkillInfoEditor : Editor
    {
        #region Private Field

        private SkillInfo _info;

        private SkillEventNameConfig _config
        {
            set
            {
                if (_info.eventNameConfig == value)
                    return;
                
                _info.eventNameConfig = value;

                if (value != null)
                {
                    // reset event names and index
                    eventNames = value.eventNames.ToArray();
                    triggerEventIndex = 0;
                    prepareEventIndex = 0;
                    for (int i = 0; i < cancelEventIndex.Count; i++)
                    {
                        cancelEventIndex[i] = 0;
                    }
                }
            }
        }

        /// <summary>
        /// All the names of skill events
        /// </summary>
        private string[] eventNames;

        private int triggerEventIndex;
        private int prepareEventIndex;
        private List<int> cancelEventIndex;
        private ReorderableList cancelList;

        #endregion
        
        #region SerializedProperties

        private SerializedProperty _id;

        private SerializedProperty _skillName;
        private SerializedProperty _skillDescription;
        private SerializedProperty _skillIcon;

        private SerializedProperty _cd;
        private SerializedProperty _maxCumulateCount;

        private SerializedProperty _prerequisite;

        #endregion
        
        private void OnEnable()
        {
            _info = (SkillInfo)serializedObject.targetObject;
            
            _id = serializedObject.FindProperty("id");
            _skillName = serializedObject.FindProperty("skillName");
            _skillDescription = serializedObject.FindProperty("description");
            _skillIcon = serializedObject.FindProperty("skillIcon");

            _cd = serializedObject.FindProperty("cd");
            _maxCumulateCount = serializedObject.FindProperty("maxCumulateCount");

            _prerequisite = serializedObject.FindProperty("needActiveSkillID");
            
            // initialize eventNames
            if (_info != null && _info.eventNameConfig != null && _info.eventNameConfig.eventNames != null)
            {
                eventNames = _info.eventNameConfig.eventNames.ToArray();
            }
            else
            {
                eventNames = new string[0];
            }
            
            // initialize index
            cancelEventIndex = new List<int>();
            if (_info.eventNameConfig != null && _info.eventNameConfig.eventNames.Count != 0)
            {
                if (_info.eventNameConfig.eventNames.Contains(_info.triggerEventName))
                {
                    triggerEventIndex = _info.eventNameConfig.eventNames.IndexOf(_info.triggerEventName);
                }

                if (_info.eventNameConfig.eventNames.Contains(_info.prepareEventName))
                {
                    prepareEventIndex = _info.eventNameConfig.eventNames.IndexOf(_info.prepareEventName);
                }

                if (_info.cancelEventName != null)
                {
                    for (int i = 0; i < _info.cancelEventName.Count; i++)
                    {
                        if (_info.eventNameConfig.eventNames.Contains(_info.cancelEventName[i]))
                        {
                            cancelEventIndex.Add(_info.eventNameConfig.eventNames.IndexOf(_info.cancelEventName[i]));
                        }
                    }
                }
            }

            cancelList = new ReorderableList(cancelEventIndex, typeof(int));
            cancelList.drawHeaderCallback = DrawCancelListHeader;
            cancelList.drawElementCallback = DrawCancelListItems;
            cancelList.onAddCallback = AddCancelListItem;
            cancelList.onRemoveCallback = RemoveCancelListItem;
            cancelList.onReorderCallback = ReorderCancelListItem;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            serializedObject.Update();

            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("Indexing 索引", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(_id);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("User Interface 用户交互", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(_skillName);
                EditorGUILayout.PropertyField(_skillDescription);
                EditorGUILayout.PropertyField(_skillIcon);
            }
            EditorGUILayout.EndVertical();
            
            GUILayout.Space(10);

            _config = (SkillEventNameConfig) EditorGUILayout.ObjectField("Skill Event Config", _info.eventNameConfig,
                typeof(SkillEventNameConfig), false);
            bool eventNameConfigMissing = _info.eventNameConfig == null;
            if (eventNameConfigMissing)
            {
                EditorGUILayout.HelpBox("Event name config missing!", MessageType.Error);
            }

            if (!eventNameConfigMissing)
            {
                EditorGUILayout.BeginVertical("Box");
                {
                    GUILayout.Label("Skill Events 触发事件", EditorStyles.boldLabel);
                 
                    // trigger type
                    _info.triggerType = (TriggerType) EditorGUILayout.EnumPopup("Trigger Type", _info.triggerType);

                    // trigger event
                    triggerEventIndex = EditorGUILayout.Popup("Trigger Event", triggerEventIndex, eventNames);
                    _info.triggerEventName = eventNames[triggerEventIndex];

                    if (_info.triggerType == TriggerType.Prepared)
                    {
                        // prepare event
                        prepareEventIndex = EditorGUILayout.Popup("Prepare Event", prepareEventIndex, eventNames);
                        _info.prepareEventName = eventNames[prepareEventIndex];
                    
                        // cancel event
                        cancelList.DoLayoutList();
                    }
                }
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("CD 技能冷却", EditorStyles.boldLabel);
                
                EditorGUILayout.PropertyField(_cd);
                EditorGUILayout.PropertyField(_maxCumulateCount);
            }
            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical("Box");
            {
                GUILayout.Label("Prerequisite 先导要求", EditorStyles.boldLabel);

                EditorGUILayout.PropertyField(_prerequisite);
            }
            EditorGUILayout.EndVertical();
            
            serializedObject.ApplyModifiedProperties();
        }

        #region Private Field

        /// <summary>
        /// The callback to draw the cancel list header
        /// </summary>
        private void DrawCancelListHeader(Rect rect)
        {
            EditorGUI.LabelField(rect, "Cancel Events");
        }

        /// <summary>
        /// The callback to draw the cancel list element
        /// </summary>
        /// <param name="rect"></param>
        /// <param name="index"></param>
        /// <param name="isActive"></param>
        /// <param name="isFocused"></param>
        private void DrawCancelListItems(Rect rect, int index, bool isActive, bool isFocused)
        {
            rect.y += 2;
            int eventNameIndex = cancelEventIndex[index];
            cancelEventIndex[index] = EditorGUI.Popup(rect, eventNameIndex, eventNames);
            _info.cancelEventName[index] = eventNames[eventNameIndex];
        }

        /// <summary>
        /// The call back when new event add to the cancel list
        /// </summary>
        /// <param name="reorderableList"></param>
        private void AddCancelListItem(ReorderableList reorderableList)
        {
            cancelEventIndex.Add(0);
            _info.cancelEventName = new List<string>();
            foreach (int i in cancelEventIndex)
            {
                _info.cancelEventName.Add(eventNames[i]);
            }
        }

        /// <summary>
        /// The callback when element is removed from the cancel list
        /// </summary>
        /// <param name="reorderableList"></param>
        private void RemoveCancelListItem(ReorderableList reorderableList)
        {
            cancelEventIndex.RemoveAt(reorderableList.index);
            _info.cancelEventName = new List<string>();
            foreach (int i in cancelEventIndex)
            {
                _info.cancelEventName.Add(eventNames[i]);
            }
        }

        /// <summary>
        /// The callback when cancel list is reordered
        /// </summary>
        /// <param name="reorderableList"></param>
        private void ReorderCancelListItem(ReorderableList reorderableList)
        {
            _info.cancelEventName = new List<string>();
            foreach (int i in cancelEventIndex)
            {
                _info.cancelEventName.Add(eventNames[i]);
            }
        }

        #endregion
    }
}