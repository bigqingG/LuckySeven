using LuckySeven.Core;
using UnityEngine;
using UnityEngine.UI;
namespace LuckySeven.UI
{
    [RequireComponent(typeof(CanvasRenderer))]
    public sealed class PokerCardFace : MaskableGraphic
    {
        int value=7,suit;bool staged,hover;CardEffect effect;
        public Color Ink=>suit==1||suit==2?PixelGraphic.Red:PixelGraphic.Ink;
        public void Present(int number,int identity,bool selected)
        {int next=(identity-1)%4;if(value==number&&suit==next&&staged==selected)return;value=number;suit=next;staged=selected;SetVerticesDirty();}
        public void SetEffect(CardEffect next){if(effect==next)return;effect=next;SetVerticesDirty();}
        public void SetHover(bool value){if(hover==value)return;hover=value;SetVerticesDirty();}
        protected override void OnPopulateMesh(VertexHelper v)
        {
            v.Clear();float w=rectTransform.rect.width,h=rectTransform.rect.height;
            PixelGraphic.Step(v,2,-3,w,h,7,new Color32(8,12,24,150)*color);
            PixelGraphic.Step(v,0,0,w,h,7,(hover||staged?PixelGraphic.Gold:PixelGraphic.Ink)*color);
            PixelGraphic.Step(v,2,-2,w-4,h-4,6,PixelGraphic.Red*color);
            PixelGraphic.Step(v,4,-4,w-8,h-8,5,PixelGraphic.Gold*color);
            PixelGraphic.Step(v,6,-6,w-12,h-12,4,PixelGraphic.Paper*color);
            PixelGraphic.Mask(v,PixelGraphic.Suits[suit],29,-9,1.4f,Ink*color);
            PixelGraphic.Digit(v,value,w/2-17.5f,-32,7,Ink*color);
            PixelGraphic.Mask(v,PixelGraphic.Suits[suit],w-25,-74,1.5f,Ink*color);
            var badge=effect==CardEffect.Strike?PixelGraphic.Red:effect==CardEffect.Ward?PixelGraphic.Slate:PixelGraphic.Teal;
            PixelGraphic.Step(v,10,-96,w-20,18,3,badge*color);
            var icon=effect==CardEffect.Strike?PixelGraphic.Sword:effect==CardEffect.Ward?PixelGraphic.Shield:PixelGraphic.Echo;
            PixelGraphic.Mask(v,icon,13,-101,1,PixelGraphic.Paper*color);
            if(hover||staged){
                PixelGraphic.Quad(v,6,-2,w-12,2,PixelGraphic.Paper*color);
                PixelGraphic.Quad(v,0,-8,3,15,PixelGraphic.Gold*color);PixelGraphic.Quad(v,w-3,-h+23,3,15,PixelGraphic.Gold*color);
            }
        }
    }
}
