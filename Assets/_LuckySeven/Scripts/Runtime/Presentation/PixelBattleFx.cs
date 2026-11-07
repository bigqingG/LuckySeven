using LuckySeven.UI;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuckySeven.Presentation
{
    // Presentation only. BattleSession and GameFlowController own all timing and damage.
    public sealed class PixelBattleFx : MonoBehaviour
    {
        [SerializeField] Image enemy;
        [SerializeField] PixelGraphic slash;
        [SerializeField] RectTransform[] fragments;
        [SerializeField] TextMeshProUGUI damage;
        float hitTime,breakTime;
        public void Configure(Image target,Transform parent,TMP_FontAsset font)
        {
            enemy=target;
            slash=BattleHUDView.Pixel("PixelSlash",parent,PixelGraphic.Shape.Slash,PixelGraphic.Paper,867,125,146,105);
            fragments=new RectTransform[6];
            for(int i=0;i<6;i++)fragments[i]=BattleHUDView.Pixel("ArmorFragment"+i,parent,PixelGraphic.Shape.Fragment,PixelGraphic.Gold,975,210,10+(i%2)*4,10+(i%2)*4).rectTransform;
            var go=parent.Find("PixelDamage");
            var rect=go as RectTransform;
            if(rect==null){rect=new GameObject("PixelDamage",typeof(RectTransform)).GetComponent<RectTransform>();rect.SetParent(parent,false);}
            rect.anchorMin=rect.anchorMax=new Vector2(0,1);rect.pivot=new Vector2(0,1);rect.sizeDelta=new Vector2(160,60);
            damage=rect.GetComponent<TextMeshProUGUI>()??rect.gameObject.AddComponent<TextMeshProUGUI>();
            damage.font=font;damage.fontSize=38;damage.fontStyle=FontStyles.Bold;damage.color=PixelGraphic.Paper;damage.alignment=TextAlignmentOptions.Center;damage.raycastTarget=false;
            var shadow=rect.GetComponent<Shadow>()??rect.gameObject.AddComponent<Shadow>();shadow.effectColor=PixelGraphic.Red;shadow.effectDistance=new Vector2(3,-3);
            ResetVisuals();
        }
        public void Hit(int amount){hitTime=.65f;damage.text="-"+amount;damage.gameObject.SetActive(true);slash.gameObject.SetActive(true);}
        public void Break(){breakTime=.35f;foreach(var r in fragments)r.gameObject.SetActive(true);}
        public void ResetVisuals(){
            hitTime=breakTime=0;if(enemy!=null)enemy.color=Color.white;if(slash!=null)slash.gameObject.SetActive(false);
            if(damage!=null)damage.gameObject.SetActive(false);if(fragments!=null)foreach(var r in fragments)if(r!=null)r.gameObject.SetActive(false);
        }
        void OnDisable(){ResetVisuals();}
        void Update()
        {
            if(hitTime>0){
                hitTime=Mathf.Max(0,hitTime-Time.unscaledDeltaTime);
                enemy.color=hitTime>.5f?new Color(1.7f,1.7f,1.7f,1):Color.white;
                slash.gameObject.SetActive(hitTime>.42f);
                damage.rectTransform.anchoredPosition=new Vector2(890,-100+(.65f-hitTime)*42);
                damage.color=new Color(1,.87f,.67f,Mathf.Min(1,hitTime*5));
                if(hitTime==0)damage.gameObject.SetActive(false);
            }
            if(breakTime>0){
                breakTime=Mathf.Max(0,breakTime-Time.unscaledDeltaTime);float t=1-breakTime/.35f;
                for(int i=0;i<fragments.Length;i++){
                    float a=i*Mathf.PI/3;fragments[i].anchoredPosition=new Vector2(977+Mathf.Cos(a)*t*55,-214+Mathf.Sin(a)*t*42-t*t*25);
                    fragments[i].localEulerAngles=new Vector3(0,0,t*(i%2==0?100:-100));
                    if(breakTime==0)fragments[i].gameObject.SetActive(false);
                }
            }
        }
    }
}

