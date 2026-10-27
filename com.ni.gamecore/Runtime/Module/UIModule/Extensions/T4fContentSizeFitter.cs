using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
[RequireComponent(typeof(RectTransform))]
public class T4fContentSizeFitter : ContentSizeFitter
{

    [SerializeField]
    public RectTransform m_Parent;

    private RectTransform m_Rect;
    private RectTransform m_RectTransform
    {
        get
        {
            if (m_Rect == null)
                m_Rect = GetComponent<RectTransform>();
            return m_Rect;
        }
    }

    protected override void OnRectTransformDimensionsChange()
    {
        base.OnRectTransformDimensionsChange();
        if(m_Parent!= null && m_Parent.gameObject.activeInHierarchy)
        {
            LayoutRebuilder.MarkLayoutForRebuild(m_Parent);
        }
    }

    public override void SetLayoutVertical()
    {
        base.SetLayoutVertical();
        if (m_Parent != null)
        {
            m_Parent.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, LayoutUtility.GetPreferredSize(m_RectTransform, 1));
        }
    }
}
