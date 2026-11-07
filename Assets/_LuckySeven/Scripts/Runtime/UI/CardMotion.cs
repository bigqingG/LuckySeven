using System;
using UnityEngine;
using UnityEngine.EventSystems;
namespace LuckySeven.UI
{
    public sealed class CardMotion:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler,IBeginDragHandler,IDragHandler,IEndDragHandler,ICanvasRaycastFilter
    {
        [SerializeField] RectTransform visual;
        RectTransform rect,parent;UnityEngine.UI.Button button;Vector2 home,grabOffset;
        bool hover,staged,dragging;int identity=-1,sibling,suppressClickFrame=-1;float deal,exposedWidth=100;Action toggle;PokerCardFace face;
        public void SetVisual(RectTransform face){visual=face;}
        public void Bind(Action action){toggle=action;rect=(RectTransform)transform;parent=(RectTransform)rect.parent;button=GetComponent<UnityEngine.UI.Button>();face=visual!=null?visual.GetComponent<PokerCardFace>():null;}
        public void Present(Vector2 position,bool picked,int id,float exposed=100)
        {
            if(rect==null)rect=(RectTransform)transform;
            EndHover();home=position;staged=picked;exposedWidth=exposed;
            if(identity!=id){identity=id;deal=1;}
            if(button!=null&&!button.IsInteractable())dragging=false;
        }
        public void Click(){if(!dragging&&Time.frameCount>suppressClickFrame&&button.IsInteractable())toggle?.Invoke();}
        public void OnPointerEnter(PointerEventData e)
        {
            if(button==null||dragging||hover)return;
            hover=true;if(face!=null)face.SetHover(true);sibling=rect.GetSiblingIndex();rect.SetAsLastSibling();button.targetGraphic.CrossFadeColor(Color.white,.1f,true,true);
        }
        void EndHover(){if(face!=null)face.SetHover(false);if(hover&&rect!=null){rect.SetSiblingIndex(sibling);if(button!=null)button.targetGraphic.CrossFadeColor(button.IsInteractable()?button.colors.normalColor:button.colors.disabledColor,.1f,true,true);}hover=false;}
        public void OnPointerExit(PointerEventData e){if(!dragging)EndHover();}
        public bool IsRaycastLocationValid(Vector2 screenPoint,Camera eventCamera)
        {
            if(rect==null||staged||dragging)return true;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,screenPoint,eventCamera,out var point);
            // The stationary lower strip keeps neighbouring cards reachable even while a face is raised in front.
            // The lifted portion is clickable too, so moving onto the face does not lose hover.
            return point.y>0 ? hover : point.x>=0&&point.x<=exposedWidth;
        }
        public void OnBeginDrag(PointerEventData e)
        {
            if(e.button!=PointerEventData.InputButton.Left||!button.IsInteractable())return;
            EndHover();dragging=true;e.eligibleForClick=false;sibling=rect.GetSiblingIndex();rect.SetAsLastSibling();
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out var local);grabOffset=rect.anchoredPosition-local;
        }
        public void OnDrag(PointerEventData e){if(dragging&&RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out var local))rect.anchoredPosition=local+grabOffset;}
        public void OnEndDrag(PointerEventData e)
        {
            if(!dragging)return;dragging=false;hover=false;suppressClickFrame=Time.frameCount+1;e.eligibleForClick=false;rect.SetSiblingIndex(sibling);
            if(!button.IsInteractable())return;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parent,e.position,e.pressEventCamera,out var local);
            bool inTable=local.x>=180&&local.x<=1100&&local.y<=-350&&local.y>=-484;
            bool inHand=local.x>=40&&local.x<=1240&&local.y<=-540&&local.y>=-685;
            if((!staged&&inTable)||(staged&&inHand))toggle?.Invoke();
        }
        void OnDisable(){EndHover();dragging=false;}
        void OnApplicationFocus(bool focused){if(!focused){EndHover();if(dragging){dragging=false;rect.SetSiblingIndex(sibling);}}}
        void Update()
        {
            if(rect==null)return;
            float speed=1-Mathf.Exp(-20*Time.unscaledDeltaTime);
            if(!dragging){deal=Mathf.Max(0,deal-Time.unscaledDeltaTime*3.5f);rect.anchoredPosition=Vector2.Lerp(rect.anchoredPosition,home-Vector2.up*deal*25,speed);}
            if(visual!=null){bool lifted=hover&&!staged;visual.anchoredPosition=Vector2.Lerp(visual.anchoredPosition,lifted?new Vector2(-7,35):Vector2.zero,speed);visual.localScale=Vector3.Lerp(visual.localScale,Vector3.one*(lifted?1.14f:1),speed);}
        }
    }
}
