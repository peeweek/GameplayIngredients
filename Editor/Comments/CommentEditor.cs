﻿using GameplayIngredients.Editor;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Net.Http.Headers;
using UnityEditor;
using UnityEngine;


namespace GameplayIngredients.Comments.Editor
{
    public class CommentEditor
    {
        SerializedObject serializedObject;
        SerializedProperty rootMessage;
        SerializedProperty title;
        SerializedProperty type;
        SerializedProperty state;
        SerializedProperty priority;
        string editMessagePath;

        bool editRoot => editMessagePath == rootMessage.propertyPath;

        public CommentEditor(SerializedObject serializedObject, SerializedProperty comment)
        {
            this.serializedObject = serializedObject;
            title = comment.FindPropertyRelative("title");
            type = comment.FindPropertyRelative("type");
            state = comment.FindPropertyRelative("state");
            priority = comment.FindPropertyRelative("priority");
            rootMessage = comment.FindPropertyRelative("message");
        }

        public bool DrawEditButton(bool edit)
        {
            return GUILayout.Toggle(edit, edit ? "Close" : "Edit", EditorStyles.miniButton, GUILayout.Width(64));
        }

        public void DrawComment()
        {
            using (new GUILayout.HorizontalScope())
            {
                TypeLabel((CommentType)type.intValue);
                StateLabel((CommentState)state.intValue);
                PriorityLabel((CommentPriority)priority.intValue);

                GUILayout.FlexibleSpace();
                EditorGUI.BeginChangeCheck();
                bool editRoot = DrawEditButton(this.editRoot);
                if(EditorGUI.EndChangeCheck())
                {
                    if (editRoot)
                        editMessagePath = rootMessage.propertyPath;
                    else
                        editMessagePath = string.Empty;
                }
            }

            GUILayout.Space(6);

            if(editRoot)
            {
                serializedObject.Update();
                EditorGUILayout.PropertyField(title);
                EditorGUILayout.PropertyField(type);
                EditorGUILayout.PropertyField(state);
                EditorGUILayout.PropertyField(priority);
                serializedObject.ApplyModifiedProperties();
            }
            else
            {
                GUILayout.Space(8);

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(CommentEditor.GetPriorityContent(" " + title.stringValue, (CommentPriority)(priority.intValue)), Styles.title);
                    GUILayout.FlexibleSpace();
                }
            }

            GUILayout.Space(6);
            DrawMessage(rootMessage, null, -1, 0);
        }

        void DrawMessage(SerializedProperty message, SerializedProperty parent, int indexInParent,  int depth)
        {
            bool delete = false;
            GUI.backgroundColor = new Color(1, 1, 1, Mathf.Clamp(depth * 0.05f, 0.05f, 0.5f));
            using(new EditorGUILayout.VerticalScope(Styles.message))
            {
                SerializedProperty body = message.FindPropertyRelative("body");
                SerializedProperty URL = message.FindPropertyRelative("URL");
                SerializedProperty from = message.FindPropertyRelative("from");
                SerializedProperty objects = message.FindPropertyRelative("targets");
                SerializedProperty replies = message.FindPropertyRelative("replies");

                if (editMessagePath == message.propertyPath)
                {
                    message.serializedObject.Update();
                    EditorGUILayout.PropertyField(body);
                    EditorGUILayout.PropertyField(URL);
                    EditorGUILayout.PropertyField(from);
                    EditorGUILayout.PropertyField(objects);
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (editMessagePath == message.propertyPath && GUILayout.Button("Apply", Styles.miniButton))
                        {
                            editMessagePath = string.Empty;
                        }
                        if (from.stringValue == CommentsWindow.user && GUILayout.Button("Delete", Styles.miniButton))
                        {
                            parent.serializedObject.Update();
                            parent.DeleteArrayElementAtIndex(indexInParent);
                            parent.serializedObject.ApplyModifiedProperties();
                            delete = true;
                        }
                    }
                    GUILayout.Space(2);
                    message.serializedObject.ApplyModifiedProperties();
                }
                else
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label($"<color={(from.stringValue == CommentsWindow.user ? "lime" : "white")}><b>{from.stringValue}</b></color>", Styles.from);
                        GUILayout.FlexibleSpace();

                        if (depth>0 && from.stringValue == CommentsWindow.user 
                            && GUILayout.Button(Styles.edit, Styles.miniButton, GUILayout.Width(32)))
                        {
                            editMessagePath = message.propertyPath;
                        }
                        if (GUILayout.Button(Styles.reply, Styles.miniButton, GUILayout.Width(32)))
                        {
                            replies.serializedObject.Update();
                            int index = replies.arraySize;
                            replies.InsertArrayElementAtIndex(index);
                            var reply = replies.GetArrayElementAtIndex(index);
                            editMessagePath = reply.propertyPath;
                            reply.FindPropertyRelative("body").stringValue = string.Empty;
                            reply.FindPropertyRelative("URL").stringValue = string.Empty;
                            reply.FindPropertyRelative("from").stringValue = CommentsWindow.user;
                            reply.FindPropertyRelative("replies").ClearArray();
                            replies.serializedObject.ApplyModifiedProperties();
                        }
                    }
                    EditorGUILayout.LabelField(body.stringValue, Styles.multiline);
                    if(!string.IsNullOrEmpty(URL.stringValue))
                    {
                        Color b = GUI.backgroundColor;
                        GUI.backgroundColor = new Color(0, 0, 0, 0.2f);
                        if(GUILayout.Button($"URL : <color=#44AAFFFF>{URL.stringValue}</color>", Styles.link))
                        {
                            Application.OpenURL(URL.stringValue);
                        }
                        GUI.backgroundColor = b;

                    }
                    GUILayout.Space(8);
                    
                }

                if (!delete && !editRoot && replies.arraySize > 0)
                {
                    for (int i = 0; i < replies.arraySize; i++)
                    {
                        DrawMessage(replies.GetArrayElementAtIndex(i), replies, i, depth+1);
                    }
                }

            }
            GUI.contentColor = Color.white;
        }

        void Separator()
        {
            Rect r = GUILayoutUtility.GetRect(0, 1, GUILayout.ExpandWidth(true));
            EditorGUI.DrawRect(r, Color.gray);
        }

        public static Color GetTypeColor(CommentType type)
        {
            float sat = 0.3f;
            float v = 1f;
            switch (type)
            {

                default:
                case CommentType.Info:
                    return Color.HSVToRGB(0.4f, 0, v);
                case CommentType.Bug:
                    return Color.HSVToRGB(0.05f, sat, v);
                case CommentType.Request:
                    return Color.HSVToRGB(0.15f, sat, v);
                case CommentType.ToDo:
                    return Color.HSVToRGB(0.25f, sat, v);
            }
        }

        public static Color GetStateColor(CommentState state)
        {
            float sat = 0.8f;
            float v = 1f;
            switch (state)
            {
                default:
                case CommentState.Open:
                    return Color.HSVToRGB(0.3f, sat, v);
                case CommentState.Blocked:
                    return Color.HSVToRGB(0.05f, sat, v);
                case CommentState.Resolved:
                    return Color.HSVToRGB(0.5f, sat, v);
                case CommentState.WontFix:
                    return Color.HSVToRGB(0.05f, sat, v);
                case CommentState.Closed:
                    return Color.HSVToRGB(0.7f, 0f, v);
            }
        }

        public static Color GetPriorityColor(CommentPriority priority)
        {
            float sat = 0.9f;
            float v = 1f;
            switch (priority)
            {
                default:
                case CommentPriority.High:
                    return Color.HSVToRGB(0.02f, sat, v);
                case CommentPriority.Medium:
                    return Color.HSVToRGB(0.15f, sat, v);
                case CommentPriority.Low:
                    return Color.HSVToRGB(0.3f, sat, v);
            }
        }

        public static void TypeLabel(CommentType value)
        {
            GUIUtils.ColoredLabel(value.ToString(), GetTypeColor(value));
        }

        public static void StateLabel(CommentState value)
        {
            GUIUtils.ColoredLabel(value.ToString(), GetStateColor(value));
        }

        public static void PriorityLabel(CommentPriority value)
        {
            GUIUtils.ColoredLabel(value.ToString(), GetPriorityColor(value));
        }

        public static GUIContent GetPriorityContent(string text, CommentPriority priority)
        {
            return new GUIContent(text, GetPriorityTexture(priority));
        }

        public static Texture GetPriorityTexture(CommentPriority priority)
        {
            switch (priority)
            {
                case CommentPriority.High:
                    return CheckResult.GetIcon(CheckResult.Result.Failed);
                case CommentPriority.Medium:
                    return CheckResult.GetIcon(CheckResult.Result.Warning);
                default:
                case CommentPriority.Low:
                    return CheckResult.GetIcon(CheckResult.Result.Notice);
            }
        }


        static class Styles
        {
            public static GUIStyle title;
            public static GUIStyle from;
            public static GUIStyle link;
            public static GUIStyle multiline;
            public static GUIStyle coloredLabel;
            public static GUIStyle message;
            public static GUIStyle miniButton;

            public static GUIContent reply;
            public static GUIContent edit;
            static Styles()
            {
                title = new GUIStyle(EditorStyles.boldLabel);
                title.fontSize = 16;

                from = new GUIStyle(EditorStyles.label);
                from.fontSize = 12;
                from.richText = true;

                link = new GUIStyle(EditorStyles.label);
                link.fontSize = 12;
                link.richText = true;
                link.margin = new RectOffset(0, 0, 4, 4);
                link.padding = new RectOffset(2, 2, 2, 2);
                SetWhiteBG(link);

                multiline = new GUIStyle(EditorStyles.label);
                multiline.wordWrap = true;
                multiline.richText = true;
                multiline.fontSize = 13;

                coloredLabel = new GUIStyle(EditorStyles.label);
                coloredLabel.fontSize = 12;
                coloredLabel.padding = new RectOffset(12, 12, 2, 2);

                message = new GUIStyle(EditorStyles.helpBox);
                message.margin = new RectOffset(16, 2, 2, 2);
                message.border = new RectOffset(4, 4, 4, 4);
                SetWhiteBG(message);

                miniButton = new GUIStyle(EditorStyles.miniButton);
                miniButton.fontSize = 10;
                miniButton.fixedHeight = 16;

                SetWhiteBG(miniButton);

                edit = new GUIContent(EditorGUIUtility.Load("Packages/net.peeweek.gameplay-ingredients/Icons/GUI/edit.png") as Texture);
                reply = new GUIContent(EditorGUIUtility.Load("Packages/net.peeweek.gameplay-ingredients/Icons/GUI/reply.png") as Texture);
            }

            static void SetWhiteBG(GUIStyle style)
            {
                SetBGTexture(style, Texture2D.whiteTexture);
            }
            static void SetBGTexture(GUIStyle style, Texture2D texture)
            {
                style.onActive.background = texture;
                style.active.background = texture;
                style.onFocused.background = texture;
                style.focused.background = texture;
                style.onHover.background = texture;
                style.hover.background = texture;
                style.onNormal.background = texture;
                style.normal.background = texture;
            }
        }
    }
}

