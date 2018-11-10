using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using UnityEngine.UI;

/// <summary>
/// 将一张图片用作参考效果图
/// 目前的快捷键:
///     选中某张图片后:
///         "~"：快速将选中的图片作为参考图(背景)
///         "Shift+~":将选中图片作为参考图 （前景,50%透明度）
///     
///     创建参考图后:
///         "Ctrl+~":清除参考图        
/// 
///     在SceneView中:
///         "F1":将参考图切换到最底层/最上层
///         "Ctrl+F1":切换参考图是否可以被选中
///     
/// </summary>
public static class UseAsReference
{
    //static GameObject refImage;
    static GameObject RefImage
    {
        get { return GameObject.Find(RefImageName); }
    }

    const string RefImageName = "83E16BBB-DCCA-4DDF-A580-426B49180D97";
    const HideFlags hideFlags = HideFlags.HideAndDontSave; //创建出的效果图默认层
    const KeyCode alterKey = KeyCode.F1; //切换是否可选中的热键

    static bool IsSelectionTexture()
    {
        return Selection.activeObject != null && Selection.activeObject.GetType().IsSubclassOf(typeof(Texture));
    }

    [MenuItem("Assets/UI Utils/用作效果图(最上层) #`", true, 11)]
    static bool UseAsRefCheckTop()
    {
        return IsSelectionTexture();
    }

    [MenuItem("Assets/UI Utils/用作效果图(最上层) #`", false, 11)]
    private static void UseAsRefTop()
    {
        CreateAsRef(true);
    }

    [MenuItem("Assets/UI Utils/用作效果图(最底层) _`", true, 10)]
    static bool UseAsRefCheck()
    {
        return IsSelectionTexture();
    }

    [MenuItem("Assets/UI Utils/用作效果图(最底层) _`", false, 10)]
    private static void UseAsRef()
    {
        CreateAsRef(false);
    }

    [MenuItem("Assets/UI Utils/清除效果图 %`", true, 12)]
    static bool ClearRefCheck()
    {
        return RefImage != null;
    }

    [MenuItem("Assets/UI Utils/清除效果图 %`", false, 12)]
    private static void ClearRef()
    {
        SceneView.onSceneGUIDelegate -= OnSceneChange;
        while (RefImage != null)
        {
            GameObject.DestroyImmediate(RefImage);
        }
    }

    static void CreateAsRef(bool atTop)
    {
        Canvas canvas = UIUtils.GetOrCreateCanvasGameObject().GetComponent<Canvas>().rootCanvas;
        GameObject gameObject = CreateRefrence(canvas, atTop);
        gameObject.hideFlags = hideFlags;

        //使SceneView能接受当前的操作
        PrepareSceneView();
    }

    private static GameObject CreateRefrence(Canvas canvas, bool atTop)
    {
        GameObject old = GameObject.Find(RefImageName);
        if (old != null)
            GameObject.DestroyImmediate(old);

        GameObject refImage = new GameObject(RefImageName, typeof(RectTransform));
        refImage.transform.SetParent(canvas.transform, false);
        refImage.transform.localScale = Vector3.one;
        refImage.transform.localPosition = Vector3.zero;

        RawImage rawImage = refImage.AddComponent<RawImage>();
        rawImage.texture = Selection.activeObject as Texture;
        rawImage.SetNativeSize();

        if (atTop)
        {
            refImage.transform.SetSiblingIndex(refImage.transform.parent.childCount - 1);
            rawImage.color = new Color(1, 1, 1, 0.5f);
        }
        else
        {
            refImage.transform.SetSiblingIndex(0);
        }

        return refImage;
    }

    private static void PrepareSceneView()
    {
        SceneView.onSceneGUIDelegate -= OnSceneChange;
        SceneView.onSceneGUIDelegate += OnSceneChange;
    }

    static bool isCtrlDown = false;
    static void OnSceneChange(SceneView scene)
    {
        if (RefImage != null)
        {

            if (Event.current.keyCode == KeyCode.LeftControl)
            {
                if (Event.current.type == EventType.KeyDown)
                    isCtrlDown = true;
                else if (Event.current.type == EventType.KeyUp)
                    isCtrlDown = false;
            }

            if (Event.current.type == EventType.KeyDown)
            {
                //Debug.Log("KeyDown:" + Event.current.keyCode);
                if (Event.current.keyCode == alterKey && isCtrlDown)
                {
                    if (RefImage.hideFlags == HideFlags.None)
                    {
                        RefImage.hideFlags = hideFlags;
                        Selection.activeGameObject = null;
                    }
                    else
                        RefImage.hideFlags = HideFlags.None;
                }
                else if (Event.current.keyCode == alterKey)
                {
                    RawImage rawImage = RefImage.GetComponent<RawImage>();
                    int sibling = RefImage.transform.GetSiblingIndex();
                    if (sibling == 0)
                        sibling = RefImage.transform.parent.childCount - 1;
                    else
                        sibling = 0;
                    rawImage.transform.SetSiblingIndex(sibling);

                    Event.current.Use();
                }

                EditorApplication.RepaintHierarchyWindow();
            }
        }
    }
}
