/****************************************************************************
 * Copyright (c) 2022 liangxiegame UNDER Paid MIT LICENSE  see licenses.txt
 ****************************************************************************/

using System;
using UnityEditor;
using UnityEngine;

namespace QFramework.Pro
{
    [CustomNodeGraphEditorAttribute(typeof(ClassGraph))]
    public class ClassGraphEditor : IMGUIGraphEditor
    {
        public override void OnLanguageChanged()
        {
            if (window.graph)
            {
                window.titleContent.text = window.graph.Name;
                window.Repaint();
            }
        }


        public override IMGUIGraphConnectionStroke GetNoodleStroke(IMGUIGraphNodePort output, IMGUIGraphNodePort input)
        {
            return IMGUIGraphConnectionStroke.Full;
        }


        public override IMGUIGraphConnectionPath GetNoodlePath(IMGUIGraphNodePort output, IMGUIGraphNodePort input)
        {
            return IMGUIGraphConnectionPath.Angled;
        }

        public override Color GetTypeColor(Type type)
        {
            return Color.white;
        }

        public override Color GetPortColor(IMGUIGraphNodePort port)
        {
            return Color.white;
        }


        private Texture2D mGridTexture;

        public override Texture2D GetGridTexture()
        {
            if (mGridTexture == null)
                mGridTexture =
                    IMGUIGraphResources.GenerateGridTexture(new Color32(32, 43, 60, 255), new Color32(32, 43, 60, 255));
            return mGridTexture;
        }


        public override void AddContextMenuItems(GenericMenu menu)
        {
            menu.AddItem(new GUIContent(ClassGraphLocale.Reload), false, () => { target.As<ClassGraph>().Parse(); });

            menu.AddItem(new GUIContent(ClassGraphLocale.ShowGraphInspector), false,
                () => { Selection.activeObject = target; });

            menu.AddItem(new GUIContent(ClassGraphLocale.Maximize), window.maximized,
                () => { window.maximized = !window.maximized; });

            menu.AddItem(new GUIContent(ClassGraphLocale.MoveToOriginPoint), false,
                () => { window.panOffset = Vector2.zero; });

            // base.AddContextMenuItems(menu);
        }
    }
}