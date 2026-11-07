using LuckySeven.Core;
using LuckySeven.Localization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LuckySeven.UI
{
    public sealed partial class BattleHUDView
    {
        [SerializeField] bool pixelSkin;
        [SerializeField] Image pixelEnemy;
        [SerializeField] Sprite[] enemyPortraits;
        [SerializeField] TextMeshProUGUI firstTarget,secondTarget,playerHpText,enemyHpText,legendTitle,legendBody,legendFoot;
        [SerializeField] PixelGraphic armorIcon;
        bool pixelArmorBroken;
        public Image PixelEnemy=>pixelEnemy;
        public PixelGraphic ArmorIcon=>armorIcon;
        public Transform BattleRoot=>battlePanel.transform;

        void RenderPixel(BattleSession s)
        {
            pixelEnemy.sprite=enemyPortraits[Mathf.Clamp(s.Stage,0,enemyPortraits.Length-1)];
            playerStatus.text=L10n.T("pixel.player",s.PlayerHp,s.MaxHp,s.Block);
            if(s.State==SessionState.AttackChoice)pixelArmorBroken=true;
            else if(s.State!=SessionState.Resolving&&s.State!=SessionState.EnemyAct)pixelArmorBroken=false;
            bool broken=pixelArmorBroken;
            enemyStatus.text=L10n.T("pixel.enemy",s.EnemyHp,s.Enemy.MaxHp,broken?L10n.T("armor.broken"):L10n.T("armor",s.Enemy.Armor),s.Enemy.Attack);
            armorIcon.gameObject.SetActive(!broken);
            firstTarget.text="7";secondTarget.text=s.LuckyNumber==0?"?":s.LuckyNumber.ToString();
            playerHpText.text=s.PlayerHp+"/"+s.MaxHp;enemyHpText.text=s.EnemyHp+"/"+s.Enemy.MaxHp;
            piles.text=L10n.T("pixel.piles",s.Hand.Count,BattleSession.HandLimit,s.Deck.Count,s.Discard.Count,s.BonusDraw);
            legendTitle.text=L10n.T("pixel.legend");
            legendBody.text=L10n.T("effect.strike")+"  "+L10n.T("ability.strike")+"\n"+L10n.T("effect.ward")+"  "+L10n.T("ability.ward")+"\n"+L10n.T("effect.echo")+"  "+L10n.T("ability.echo");
            legendFoot.text=L10n.T("pixel.attackonly");
            // Every visible control still derives its availability from the same rules.
            drawLabel.text=L10n.T("draw",BattleSession.DrawCount+s.BonusDraw);
            exchange.GetComponentInChildren<TextMeshProUGUI>().text=L10n.T("pixel.exchange",BattleSession.ExchangeDrawCount+s.BonusDraw);
        }

        public void ConfigurePixelLayout(Sprite[] portraits)
        {
            pixelSkin=true;enemyPortraits=new[]{portraits[1],portraits[2],portraits[3]};
            var font=playerStatus.font;var battle=battlePanel.transform;
            foreach(var t in GetComponentsInChildren<TextMeshProUGUI>(true)){
                t.color=PixelGraphic.Paper;t.enableAutoSizing=true;t.fontSizeMax=t.fontSize;t.fontSizeMin=Mathf.Min(12,t.fontSize);
            }
            var backdrop=transform.Find("Backdrop").GetComponent<Image>();backdrop.color=Color.clear;
            Decoration(backdrop.rectTransform,PixelGraphic.Shape.Stone,PixelGraphic.Navy);
            SkinRect((RectTransform)battle.Find("PlayerPlate"),PixelGraphic.Slate,16,82,400,170);
            SkinRect((RectTransform)battle.Find("EnemyPlate"),PixelGraphic.Slate,864,82,400,170);
            Pixel("StagePlate",battle,PixelGraphic.Shape.Frame,PixelGraphic.Navy,434,18,412,46).transform.SetAsFirstSibling();
            Place(stage,450,23,380,36,21);
            Place(transform.Find("Brand").GetComponent<TextMeshProUGUI>(),28,12,350,55,34);
            transform.Find("Brand").GetComponent<TextMeshProUGUI>().color=PixelGraphic.Gold;
            Place(transform.Find("Footer").GetComponent<TextMeshProUGUI>(),24,699,1232,18,11);
            transform.Find("Footer").GetComponent<TextMeshProUGUI>().color=new Color32(169,174,189,255);
            Place(battle.Find("PlayerName").GetComponent<TextMeshProUGUI>(),174,94,222,33,23);
            Place(playerStatus,174,140,222,65,17);playerStatus.alignment=TextAlignmentOptions.Left;
            Place(enemyName,1008,94,238,33,22);
            Place(enemyStatus,1008,134,238,78,16);enemyStatus.alignment=TextAlignmentOptions.Left;
            var hero=Portrait("PixelHero",battle,portraits[0],20,94,152,152);
            pixelEnemy=Portrait("PixelEnemy",battle,portraits[1],860,103,155,150);
            emblem.enabled=false;
            battle.Find("YourPlan").gameObject.SetActive(false);
            Place(battle.Find("LuckyCaption").GetComponent<TextMeshProUGUI>(),442,86,396,36,22);
            Place(battle.Find("TargetRule").GetComponent<TextMeshProUGUI>(),436,226,408,24,14);
            targets.enabled=false;
            var a=Pixel("LuckyCoinA",battle,PixelGraphic.Shape.Coin,PixelGraphic.Gold,520,127,88,88);
            var b=Pixel("LuckyCoinB",battle,PixelGraphic.Shape.Coin,PixelGraphic.Gold,672,127,88,88);
            firstTarget=GetLabel("PixelFirstTarget",a.transform,font,8,8,72,72,58);firstTarget.color=PixelGraphic.Ink;
            secondTarget=GetLabel("PixelSecondTarget",b.transform,font,8,8,72,72,58);secondTarget.color=PixelGraphic.Ink;
            var separator=GetLabel("TargetSeparator",battle,font,620,145,40,44,24);separator.text="/";
            ConfigureHealth(playerBar,174,221,222);ConfigureHealth(enemyBar,1008,221,238);
            playerHpText=GetLabel("PlayerHpNumber",battle,font,174,220,222,24,15);
            enemyHpText=GetLabel("EnemyHpNumber",battle,font,1008,220,238,24,15);
            armorIcon=Pixel("PixelArmor",battle,PixelGraphic.Shape.Shield,PixelGraphic.Gold,970,205,28,28);
            Place(feedback,120,256,1040,25,17);
            Place(hint,100,282,1080,26,16);
            Place(expression,180,312,920,32,25);
            SkinRect(tableZone,PixelGraphic.Navy,180,350,920,134);
            Place(tableHint,26,35,868,65,18);
            tableHint.color=new Color32(170,181,203,255);
            Pixel("SupplyPlate",battle,PixelGraphic.Shape.Frame,PixelGraphic.Navy,16,548,204,148).transform.SetAsFirstSibling();
            Pixel("LegendPlate",battle,PixelGraphic.Shape.Frame,PixelGraphic.Navy,1040,548,224,148).transform.SetAsFirstSibling();
            Place(piles,28,557,180,56,13);
            Move((RectTransform)draw.transform,28,615,180,31);
            Move((RectTransform)exchange.transform,28,654,180,31);
            legendTitle=GetLabel("PixelLegendTitle",battle,font,1052,558,200,24,18);
            legendBody=GetLabel("PixelLegendBody",battle,font,1055,586,194,69,15);
            legendFoot=GetLabel("PixelLegendFoot",battle,font,1052,665,200,20,12);
            legendTitle.text="攻击附效";legendBody.text="强攻 伤害+2\n守护 格挡+3\n回响 加抽+1";legendFoot.text="附效仅在攻击时触发";
            for(int i=0;i<cards.Length;i++){
                Move(values[i].rectTransform,8,3,22,28);values[i].fontSize=23;values[i].fontSizeMax=23;
                Move(cardNames[i].rectTransform,23,96,65,18);cardNames[i].fontSize=12;cardNames[i].fontSizeMax=12;cardNames[i].fontSizeMin=9;
                Move(effects[i].rectTransform,10,114,80,15);effects[i].fontSize=11;effects[i].fontSizeMax=11;effects[i].fontSizeMin=9;
                oppositeValues[i].enabled=false;
                var empty=cards[i].transform.Find("Label");if(empty!=null)empty.gameObject.SetActive(false);
                var colors=cards[i].colors;colors.disabledColor=new Color(.78f,.78f,.78f,1);colors.highlightedColor=Color.white;colors.selectedColor=Color.white;cards[i].colors=colors;
            }
            foreach(var button in GetComponentsInChildren<Button>(true)){
                if(button.GetComponent<CardMotion>()!=null)continue;
                bool accent=button==play||button==primary||button==next||button==replay;
                var graphic=Decoration((RectTransform)button.transform,PixelGraphic.Shape.Frame,accent?PixelGraphic.Gold:PixelGraphic.Slate);
                var old=button.GetComponent<Image>();if(old!=null)old.color=Color.clear;
                button.targetGraphic=graphic;
                var colors=button.colors;colors.normalColor=Color.white;colors.highlightedColor=new Color(1.15f,1.15f,1.15f);colors.selectedColor=Color.white;colors.disabledColor=new Color(.55f,.58f,.64f,1);button.colors=colors;
                var label=button.GetComponentInChildren<TextMeshProUGUI>(true);
                label.color=accent?PixelGraphic.Ink:PixelGraphic.Paper;
                label.rectTransform.anchorMin=Vector2.zero;label.rectTransform.anchorMax=Vector2.one;label.rectTransform.offsetMin=new Vector2(10,3);label.rectTransform.offsetMax=new Vector2(-10,-3);
                label.enableAutoSizing=true;label.fontSizeMax=18;label.fontSizeMin=11;
            }
            SkinRect((RectTransform)resultPanel.transform,PixelGraphic.Navy,160,125,960,480);
            SkinRect((RectTransform)helpPanel.transform,PixelGraphic.Navy,140,95,1000,540);
            var menu=menuPanel.transform;
            Portrait("MenuMage",menu,portraits[0],120,110,260,260);
            Portrait("MenuDie",menu,portraits[1],910,110,260,260);
            Place(menu.Find("Title").GetComponent<TextMeshProUGUI>(),380,275,520,80,62);
            menu.Find("Title").GetComponent<TextMeshProUGUI>().color=PixelGraphic.Gold;
            Place(menu.Find("Seven").GetComponent<TextMeshProUGUI>(),520,80,240,180,150);
            var menuCoin=Pixel("MenuCoin",menu,PixelGraphic.Shape.Coin,PixelGraphic.Red,540,100,200,170);
            menuCoin.transform.SetAsFirstSibling();
        }
        static void ConfigureHealth(Image bar,float x,float y,float width){
            var track=(RectTransform)bar.transform.parent;Move(track,x,y,width,20);track.GetComponent<Image>().color=PixelGraphic.Ink;
            bar.color=new Color32(183,60,61,255);bar.rectTransform.offsetMin=new Vector2(2,2);bar.rectTransform.offsetMax=new Vector2(-2,-2);
        }
        static void Place(TextMeshProUGUI label,float x,float y,float w,float h,int size){
            Move(label.rectTransform,x,y,w,h);label.fontSize=size;label.fontSizeMax=size;label.fontSizeMin=Mathf.Min(size,12);label.enableAutoSizing=true;
        }
        static Image Portrait(string name,Transform parent,Sprite sprite,float x,float y,float w,float h){
            var existing=parent.Find(name) as RectTransform;var r=existing??Rect(name,parent,x,y,w,h);Move(r,x,y,w,h);
            var image=r.GetComponent<Image>()??r.gameObject.AddComponent<Image>();image.sprite=sprite;image.preserveAspect=true;image.raycastTarget=false;return image;
        }
        static TextMeshProUGUI GetLabel(string name,Transform parent,TMP_FontAsset font,float x,float y,float w,float h,int size){
            var old=parent.Find(name);var text=old!=null?old.GetComponent<TextMeshProUGUI>():Label(name,parent,"",font,size,x,y,w,h,PixelGraphic.Paper);
            Place(text,x,y,w,h,size);return text;
        }
        public static PixelGraphic Pixel(string name,Transform parent,PixelGraphic.Shape shape,Color fill,float x,float y,float w,float h){
            var r=parent.Find(name) as RectTransform;if(r==null)r=Rect(name,parent,x,y,w,h);Move(r,x,y,w,h);
            if(r.GetComponent<CanvasRenderer>()==null)r.gameObject.AddComponent<CanvasRenderer>();
            var g=r.GetComponent<PixelGraphic>()??r.gameObject.AddComponent<PixelGraphic>();g.shape=shape;g.fill=fill;g.raycastTarget=false;g.SetAllDirty();return g;
        }
        static PixelGraphic Decoration(RectTransform host,PixelGraphic.Shape shape,Color fill){
            var graphic=Pixel("PixelDecoration",host,shape,fill,0,0,host.rect.width,host.rect.height);
            var r=graphic.rectTransform;r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;r.SetAsFirstSibling();return graphic;
        }
        static void SkinRect(RectTransform host,Color fill,float x,float y,float w,float h){
            Move(host,x,y,w,h);var image=host.GetComponent<Image>();if(image!=null)image.color=Color.clear;Decoration(host,PixelGraphic.Shape.Frame,fill);
        }
    }
}

