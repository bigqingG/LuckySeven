using LuckySeven.Localization;
using System.Linq;
using LuckySeven.Core;
using LuckySeven.Game;
using LuckySeven.Presentation;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace LuckySeven.UI
{
    public sealed partial class BattleHUDView:MonoBehaviour
    {
        [SerializeField] GameObject menuPanel,battlePanel,resultPanel,helpPanel;
        [SerializeField] TextMeshProUGUI playerStatus,enemyStatus,enemyName,stage,emblem,targets,expression,hint,piles,resultTitle,resultBody,soundLabel,primaryLabel,drawLabel;
        [SerializeField] Button play,help,closeHelp,sound,clear,draw,exchange,primary,attack,next,replay;
        [SerializeField] Button[] cards,lucky;
        [SerializeField] TextMeshProUGUI[] values,effects,cardNames,luckyLabels;
        [SerializeField] Image playerBar,enemyBar;
        [SerializeField] TextMeshProUGUI feedback;
        [SerializeField] RectTransform tableZone;
        [SerializeField] TextMeshProUGUI tableHint;
        [SerializeField] UnityEngine.UI.Button language;
        [SerializeField] PokerCardFace[] faces;
        [SerializeField] TextMeshProUGUI[] oppositeValues;
        CanvasGroup inputGroup;
        public void SetInputReady(bool ready){if(inputGroup==null){inputGroup=GetComponent<CanvasGroup>();if(inputGroup==null)inputGroup=gameObject.AddComponent<CanvasGroup>();}inputGroup.interactable=ready;}
        public void ApplyLocaleFont(){foreach(var text in GetComponentsInChildren<TextMeshProUGUI>(true))text.font=L10n.Font;}

        public TextMeshProUGUI Feedback=>feedback;
        public RectTransform EnemyVisual=>pixelEnemy!=null?pixelEnemy.rectTransform:emblem.rectTransform;
        public void Bind(GameFlowController flow){
            // CJK ascenders need room in the existing 32px formula band.
            if(pixelSkin){expression.fontSize=22;expression.fontSizeMax=22;}
            language.onClick.AddListener(flow.ToggleLanguage);
            play.onClick.AddListener(flow.StartGame);replay.onClick.AddListener(flow.StartGame);next.onClick.AddListener(flow.NextBattle);
            clear.onClick.AddListener(flow.Clear);draw.onClick.AddListener(flow.Draw);exchange.onClick.AddListener(flow.Exchange);
            primary.onClick.AddListener(flow.Primary);attack.onClick.AddListener(flow.Attack);sound.onClick.AddListener(flow.ToggleSound);
            help.onClick.AddListener(()=>helpPanel.SetActive(true));closeHelp.onClick.AddListener(()=>helpPanel.SetActive(false));
            for(int i=0;i<cards.Length;i++){
                int n=i;var motion=cards[i].GetComponent<CardMotion>();
                motion.Bind(()=>flow.Card(n));cards[i].onClick.AddListener(motion.Click);
            }
            for(int i=0;i<3;i++){int n=i;lucky[i].onClick.AddListener(()=>flow.ChooseLucky(n));}
        }
        public void SetSound(bool muted){soundLabel.text=L10n.T(muted?"sound.off":"sound.on");}
        public void Render(BattleSession s,string symbol){
            language.GetComponentInChildren<TextMeshProUGUI>().text=L10n.T("language");
            bool menu=s.State==SessionState.MainMenu;
            bool result=s.State==SessionState.BetweenBattles||s.State==SessionState.Victory||s.State==SessionState.Defeat;
            menuPanel.SetActive(menu);battlePanel.SetActive(!menu&&!result);resultPanel.SetActive(result);
            if(menu)return;
            stage.text=L10n.T("stage",s.Stage+1,s.StageCount,s.Turn);
            playerStatus.text=L10n.T("player.status",s.PlayerHp,s.MaxHp,s.Block);
            enemyName.text=L10n.T("enemy."+s.Stage);emblem.text=symbol;
            bool broken=s.State==SessionState.AttackChoice;
            enemyStatus.text=L10n.T("enemy.status",s.EnemyHp,s.Enemy.MaxHp,broken?L10n.T("armor.broken"):L10n.T("armor",s.Enemy.Armor),s.Enemy.Attack);
            targets.text=s.LuckyNumber==0?"7 / ?":$"7 / {s.LuckyNumber}";
            SetBar(playerBar,(float)s.PlayerHp/s.MaxHp);SetBar(enemyBar,(float)s.EnemyHp/s.Enemy.MaxHp);
            piles.text=L10n.T("piles",s.Hand.Count,BattleSession.HandLimit,s.Deck.Count,s.Discard.Count,s.BonusDraw);
            string formula=string.Join(" + ",s.Hand.Where(c=>s.IsSelected(c.Id)).Select(c=>c.Value));
            expression.text=s.SelectedCount==0?L10n.T("sum.empty"):formula+" = "+s.SelectedSum;
            expression.color=s.CanBreak?BattleHUDColors.Gold:Color.white;
            switch(s.State){
                case SessionState.ChooseLucky:hint.text=L10n.T("hint.choose");break;
                case SessionState.SupplyChoice:hint.text=L10n.T("hint.supply");break;
                case SessionState.ExchangeSelection:hint.text=L10n.T("hint.exchange",s.SelectedCount);break;
                case SessionState.TrimHand:hint.text=L10n.T("hint.trim",s.Hand.Count-9,s.SelectedCount);break;
                case SessionState.PlayerInput:hint.text=L10n.T("hint.play");break;
                case SessionState.AttackChoice:hint.text=L10n.T("hint.attack");break;
                default:hint.text=L10n.T("hint.resolve");break;
            }
            if(s.State==SessionState.AttackChoice)
                expression.text=L10n.T("combo.pending",s.PendingComboBonus);
            if(s.CanBreak) {
                hint.text=L10n.T("combo.ready",s.SelectedCount,s.SelectedComboBonus);
                expression.text=formula+" = "+s.SelectedSum+"   /   "+L10n.T("combo.short",s.SelectedComboBonus);
            }
            if(s.TryPreviewAttack(out var preview)) {
                string damage=L10n.T(preview.Breached?"preview.break":"preview.normal",preview.Damage,preview.Value,preview.Effect==CardEffect.Strike?2:0,preview.Breached?preview.ComboBonus:s.Enemy.Armor);
                hint.text=damage;
                if(preview.Breached) expression.text=L10n.T("combo.locked",s.PendingComboBonus);
            }
            expression.gameObject.SetActive(s.State!=SessionState.ChooseLucky&&s.State!=SessionState.Resolving&&s.State!=SessionState.EnemyAct);
            for(int i=0;i<3;i++){lucky[i].gameObject.SetActive(s.State==SessionState.ChooseLucky);luckyLabels[i].text=s.Candidates[i].ToString();}
            draw.gameObject.SetActive(pixelSkin||s.State==SessionState.SupplyChoice);exchange.gameObject.SetActive(pixelSkin||s.State==SessionState.SupplyChoice);
            draw.interactable=s.State==SessionState.SupplyChoice;exchange.interactable=s.State==SessionState.SupplyChoice&&s.Hand.Count>=2;
            drawLabel.text=L10n.T("draw",BattleSession.DrawCount+s.BonusDraw);
            exchange.GetComponentInChildren<TextMeshProUGUI>().text=L10n.T("exchange",BattleSession.ExchangeDrawCount+s.BonusDraw);
            bool choosing=s.State==SessionState.PlayerInput||broken||s.State==SessionState.ExchangeSelection||s.State==SessionState.TrimHand;
            clear.gameObject.SetActive(choosing);primary.gameObject.SetActive(choosing);attack.gameObject.SetActive(s.State==SessionState.PlayerInput);
            clear.interactable=s.SelectedCount>0||s.State==SessionState.ExchangeSelection;
            primaryLabel.text=L10n.T(broken?"attack.confirm":s.State==SessionState.ExchangeSelection?"exchange.confirm":s.State==SessionState.TrimHand?"discard.confirm":"break");
            primary.interactable=broken?s.CanAttack:s.State==SessionState.ExchangeSelection?s.SelectedCount==2:s.State==SessionState.TrimHand?s.SelectedCount==s.Hand.Count-9:s.CanBreak;
            attack.interactable=s.CanAttack;
            int handCount=s.Hand.Count-s.SelectedCount, handIndex=0, tableIndex=0;
            float handSpacing=pixelSkin?Mathf.Min(82,676f/Mathf.Max(1,handCount-1)):Mathf.Min(58,1000f/Mathf.Max(1,handCount));
            float handStart=(1280-(Mathf.Max(0,handCount-1)*handSpacing+100))/2;
            float tableSpacing=Mathf.Min(112,900f/Mathf.Max(1,s.SelectedCount));
            float tableStart=(1280-(Mathf.Max(0,s.SelectedCount-1)*tableSpacing+104))/2;
            if(tableZone!=null)tableZone.gameObject.SetActive(s.State!=SessionState.ChooseLucky);
            if(tableHint!=null){tableHint.gameObject.SetActive(s.SelectedCount==0);tableHint.text=L10n.T(choosing?"table.empty":s.State==SessionState.SupplyChoice?"table.supply":"hint.resolve");}

            for(int i=0;i<cards.Length;i++){
                bool active=i<s.Hand.Count;cards[i].gameObject.SetActive(active);if(!active)continue;
                var c=s.Hand[i];bool picked=s.IsSelected(c.Id);var r=(RectTransform)cards[i].transform;float width=picked?104:100;r.sizeDelta=new Vector2(width,130);
                cards[i].interactable=choosing;cards[i].GetComponent<Image>().color=Color.clear;faces[i].Present(c.Value,c.Id,picked);
                var destination=picked?new Vector2(tableStart+tableIndex++*tableSpacing,-352):new Vector2(handStart+handIndex++*handSpacing,pixelSkin?-566:-550);
                cards[i].GetComponent<CardMotion>().Present(destination,picked,c.Id,picked?width:handIndex==handCount?width:handSpacing);
                cards[i].transform.SetAsLastSibling();
                values[i].text=c.Value.ToString();oppositeValues[i].text=values[i].text;values[i].color=oppositeValues[i].color=faces[i].Ink;cardNames[i].text=L10n.Effect(c.Effect);
                effects[i].text=L10n.T("ability."+c.Effect.ToString().ToLowerInvariant());
                faces[i].rectTransform.sizeDelta=new Vector2(width,130);
                if(pixelSkin){faces[i].SetEffect(c.Effect);cardNames[i].color=PixelGraphic.Paper;effects[i].color=PixelGraphic.Ink;}
            }
            // Keep table cards above the hand during travel; hand order stays left-to-right.
            for(int i=0;i<s.Hand.Count;i++)if(s.IsSelected(s.Hand[i].Id))cards[i].transform.SetAsLastSibling();
            if(pixelSkin)RenderPixel(s);
            if(!result)return;
            bool between=s.State==SessionState.BetweenBattles,won=s.State==SessionState.Victory;
            resultTitle.text=L10n.T(between?"result.between":won?"result.win":"result.lose");
            resultBody.text=between?L10n.T("result.between.body",L10n.T("enemy."+s.Stage),s.LastHealing)
                :L10n.T("result.stats",L10n.T(won?"result.win.desc":"result.lose.desc"),s.Attacks,s.LuckyHits,s.SecondaryHits,s.Seed);
            next.gameObject.SetActive(between);replay.gameObject.SetActive(!between);
        }
        static void SetBar(Image bar,float value){bar.rectTransform.anchorMax=new Vector2(Mathf.Clamp01(value),1);}
        public void BuildLayout(TMP_FontAsset font){
            var root=(RectTransform)transform;
            Panel("Backdrop",root,new Color32(15,25,34,255),0,0,1280,720);
            Label("Brand",root,"L / 7     THE LUCKY DECK",font,18,40,20,800,35,BattleHUDColors.Teal,TextAlignmentOptions.Left);
            sound=Button("Sound",root,"SOUND ON",font,1080,20,160,36);soundLabel=sound.GetComponentInChildren<TextMeshProUGUI>();
            Label("Footer",root,"LUCKY SEVEN / CARD PROTOTYPE 02      •      ADD. BREAK. STRIKE.      •      ABILITIES TRIGGER ONLY ON ATTACK",font,13,40,687,1200,22,new Color32(139,159,169,255));
            menuPanel=Panel("MainMenuPanel",root,Color.clear,0,70,1280,600).gameObject;var menu=menuPanel.transform;
            Label("Index",menu,"BUILD YOUR HAND. CHANGE YOUR ODDS.",font,18,100,30,1080,40,BattleHUDColors.Teal);
            Label("Seven",menu,"7",font,180,510,60,260,210,BattleHUDColors.Gold);
            Label("Title",menu,"LUCKY SEVEN",font,66,100,265,1080,85,Color.white);
            Label("Tagline",menu,"Two lucky numbers. One perfect opening.",font,24,100,360,1080,50,Color.white);
            play=Button("Play",menu,"DEAL ME IN",font,420,440,440,60,true);
            help=Button("HowToPlay",menu,"HOW TO PLAY",font,510,520,260,45);
            battlePanel=Panel("BattleHUD",root,Color.clear,0,0,1280,680).gameObject;var battle=battlePanel.transform;
            stage=Label("Stage",battle,"",font,16,40,72,1200,28,BattleHUDColors.Teal,TextAlignmentOptions.Left);
            Panel("PlayerPlate",battle,new Color32(24,39,49,255),40,112,360,170);
            Label("PlayerName",battle,"THE CALIBRATOR",font,23,64,130,310,35,Color.white,TextAlignmentOptions.Left);
            playerStatus=Label("PlayerStatus",battle,"",font,16,64,176,312,34,BattleHUDColors.Teal,TextAlignmentOptions.Left);
            playerBar=Bar("PlayerHP",battle,64,222,312,BattleHUDColors.Teal);
            Label("YourPlan",battle,"SAVE A CARD FOR THE FINISHING BLOW",font,13,64,242,312,24,Color.white);
            Label("LuckyCaption",battle,"LUCKY TARGETS",font,16,440,116,350,30,BattleHUDColors.Teal);
            targets=Label("LuckyTargets",battle,"7 / ?",font,78,425,150,390,95,BattleHUDColors.Gold);
            Label("TargetRule",battle,"ADD CARD VALUES TO EITHER TARGET",font,13,425,246,390,24,Color.white);
            Panel("EnemyPlate",battle,new Color32(39,35,45,255),840,112,400,170);
            emblem=Label("EnemyEmblem",battle,"7",font,56,858,128,95,90,BattleHUDColors.Coral);
            enemyName=Label("EnemyName",battle,"",font,21,958,125,265,50,Color.white);
            enemyStatus=Label("EnemyStatus",battle,"",font,15,952,181,274,58,BattleHUDColors.Coral);
            enemyBar=Bar("EnemyHP",battle,864,257,352,BattleHUDColors.Coral);
            feedback=Label("AttackFeedback",battle,"",font,19,60,290,1160,30,BattleHUDColors.Gold);
            expression=Label("Sum",battle,"",font,32,60,325,1160,45,Color.white);
            hint=Label("Hint",battle,"",font,17,40,376,1200,40,BattleHUDColors.Teal);
            lucky=new Button[3];luckyLabels=new TextMeshProUGUI[3];
            for(int i=0;i<3;i++){lucky[i]=Button("Lucky"+i,battle,"?",font,460+i*125,320,110,55,true);luckyLabels[i]=lucky[i].GetComponentInChildren<TextMeshProUGUI>();luckyLabels[i].fontSize=30;}
            cards=new Button[14];values=new TextMeshProUGUI[14];effects=new TextMeshProUGUI[14];cardNames=new TextMeshProUGUI[14];
            for(int i=0;i<cards.Length;i++){
                cards[i]=Button("Card"+i,battle,"",font,40+i*85,432,110,148,true);cards[i].gameObject.AddComponent<CardMotion>();
                var r=cards[i].transform;var dark=new Color32(22,32,40,255);
                cardNames[i]=Label("CardName",r,"STRIKE",font,14,4,8,102,23,dark);
                values[i]=Label("Value",r,"7",font,58,4,32,102,75,dark);
                effects[i]=Label("Ability",r,"ATK +2",font,14,4,113,102,28,dark);
            }
            piles=Label("Piles",battle,"",font,14,40,592,1200,25,new Color32(175,192,202,255));
            draw=Button("Draw",battle,"DRAW 2",font,350,630,230,44,true);drawLabel=draw.GetComponentInChildren<TextMeshProUGUI>();
            exchange=Button("Exchange",battle,"RETURN 2 / DRAW 3",font,610,630,320,44);
            clear=Button("Clear",battle,"CLEAR / BACK",font,180,630,200,44);
            attack=Button("NormalAttack",battle,"NORMAL ATTACK",font,410,630,270,44);
            primary=Button("Primary",battle,"BREAK ARMOR",font,710,630,360,44,true);primaryLabel=primary.GetComponentInChildren<TextMeshProUGUI>();
            resultPanel=Panel("ResultPanel",root,new Color32(12,20,28,255),160,125,960,480).gameObject;var result=resultPanel.transform;
            resultTitle=Label("ResultTitle",result,"",font,39,40,65,880,65,BattleHUDColors.Gold);
            resultBody=Label("ResultBody",result,"",font,22,60,155,840,180,Color.white);
            next=Button("NextBattle",result,"NEXT ENCOUNTER",font,280,365,400,60,true);
            replay=Button("Replay",result,"DEAL AGAIN",font,280,365,400,60,true);
            helpPanel=Panel("HowToPlayPanel",root,new Color32(18,30,39,255),140,95,1000,540).gameObject;var rules=helpPanel.transform;
            Label("HelpTitle",rules,"ADD. BREAK. STRIKE.",font,34,40,25,920,55,BattleHUDColors.Gold);
            Label("HelpBody",rules,"1. Start with 5 cards. Choose a second lucky number each encounter.\n2. Each turn: draw 2 OR return 2 to the deck bottom and draw 3.\n3. Select cards whose VALUES ADD to 7 or your second lucky number.\n4. BREAK ARMOR spends those cards. Keep at least one card!\n5. Select one remaining card, then ATTACK for its value + ability.\n6. STRIKE: +2 damage. WARD: block 3. ECHO: +1 draw next turn.\n7. No lucky sum? Select one card for a NORMAL ATTACK through armor.\n8. Surviving enemies counterattack; armor returns next turn.\n9. Unspent cards stay. Above 9 cards, choose excess cards to discard.\n\nDefeat three enemies. Heal 6 between encounters. No time limit.",font,21,40,95,920,345,Color.white,TextAlignmentOptions.Left);
            closeHelp=Button("CloseHelp",rules,"GOT IT",font,350,465,300,50,true);
            ConfigureTableLayout();
            battlePanel.SetActive(false);resultPanel.SetActive(false);helpPanel.SetActive(false);
        }
        public void ConfigureTableLayout()
        {
            var battle=battlePanel.transform;
            var font=expression.font;
            if(tableZone==null){
                tableZone=Panel("TableDropZone",battle,new Color32(24,43,52,255),180,350,920,134);
                tableZone.SetAsFirstSibling();
                tableHint=Label("TableHint",tableZone,"",font,19,20,25,880,80,new Color32(140,174,180,255));
            }
            Move(expression.rectTransform,180,313,920,34);expression.fontSize=26;
            Move(hint.rectTransform,40,529,1200,22);hint.fontSize=14;
            Move(piles.rectTransform,590,72,650,28);piles.fontSize=12;
            Move((RectTransform)clear.transform,180,490,200,36);
            Move((RectTransform)attack.transform,410,490,270,36);
            Move((RectTransform)primary.transform,710,490,360,36);
            Move((RectTransform)draw.transform,350,490,230,36);
            Move((RectTransform)exchange.transform,610,490,320,36);
            for(int i=0;i<3;i++)Move((RectTransform)lucky[i].transform,460+i*125,380,110,55);
            for(int i=0;i<cards.Length;i++){
                var outline=cards[i].GetComponent<UnityEngine.UI.Outline>();
                if(outline==null)outline=cards[i].gameObject.AddComponent<UnityEngine.UI.Outline>();
                outline.effectColor=new Color32(37,58,63,255);outline.effectDistance=new Vector2(1.5f,-1.5f);
                Move((RectTransform)cards[i].transform,40+i*74,550,120,130);
                Move(values[i].rectTransform,8,3,104,58);values[i].fontSize=42;values[i].alignment=TextAlignmentOptions.TopLeft;
                Move(cardNames[i].rectTransform,8,59,104,23);cardNames[i].fontSize=13;cardNames[i].alignment=TextAlignmentOptions.Left;
                Move(effects[i].rectTransform,8,98,104,25);effects[i].fontSize=12;effects[i].alignment=TextAlignmentOptions.Left;
            }
            var helpBody=helpPanel.transform.Find("HelpBody").GetComponent<TextMeshProUGUI>();
            helpBody.text=helpBody.text.Replace("3. Select cards whose VALUES ADD to 7 or your second lucky number.","3. Click / drag cards to the table. Click them again to return to hand.")
                .Replace("4. BREAK ARMOR spends those cards. Keep at least one card!","4. Add to 7 or your lucky number, then BREAK. Keep one attack card!");
        }
        public void ConfigurePokerCards(TMP_FontAsset font)
        {
            if(language==null)language=Button("Language",transform,"语言：中文",font,875,20,190,36);
            faces=new PokerCardFace[cards.Length];oppositeValues=new TextMeshProUGUI[cards.Length];
            for(int i=0;i<cards.Length;i++){
                var card=cards[i];var visual=card.transform.Find("PokerFace") as RectTransform;
                if(visual==null)visual=Rect("PokerFace",card.transform,0,0,100,130);
                if(visual.GetComponent<CanvasRenderer>()==null)visual.gameObject.AddComponent<CanvasRenderer>();
                faces[i]=visual.GetComponent<PokerCardFace>()??visual.gameObject.AddComponent<PokerCardFace>();
                faces[i].raycastTarget=true;card.GetComponent<Image>().color=Color.clear;card.targetGraphic=faces[i];
                var outline=card.GetComponent<UnityEngine.UI.Outline>();if(outline!=null)outline.enabled=false;
                foreach(var text in new[]{values[i],cardNames[i],effects[i]})text.rectTransform.SetParent(visual,false);
                Move(values[i].rectTransform,4,2,25,30);values[i].fontSize=24;values[i].alignment=TextAlignmentOptions.TopLeft;
                Move(cardNames[i].rectTransform,20,91,64,18);cardNames[i].fontSize=12;cardNames[i].alignment=TextAlignmentOptions.Center;
                Move(effects[i].rectTransform,22,109,60,17);effects[i].fontSize=10;effects[i].alignment=TextAlignmentOptions.Center;
                var opposite=visual.Find("OppositeValue");
                oppositeValues[i]=opposite!=null?opposite.GetComponent<TextMeshProUGUI>():Label("OppositeValue",visual,"7",font,24,0,0,25,30,Color.black);
                var corner=oppositeValues[i].rectTransform;corner.anchorMin=corner.anchorMax=new Vector2(1,0);corner.pivot=new Vector2(0,1);corner.anchoredPosition=new Vector2(-4,2);corner.sizeDelta=new Vector2(25,30);corner.localEulerAngles=new Vector3(0,0,180);oppositeValues[i].alignment=TextAlignmentOptions.TopLeft;
                card.GetComponent<CardMotion>().SetVisual(visual);
                foreach(var text in visual.GetComponentsInChildren<TextMeshProUGUI>(true))text.font=font;
                card.GetComponentInChildren<TextMeshProUGUI>().raycastTarget=false;
            }
            foreach(var text in GetComponentsInChildren<TextMeshProUGUI>(true))text.font=font;
            helpPanel.transform.Find("HelpBody").GetComponent<TextMeshProUGUI>().fontSize=20;
        }
        static void Move(RectTransform r,float x,float y,float w,float h){r.anchoredPosition=new Vector2(x,-y);r.sizeDelta=new Vector2(w,h);}

        private static RectTransform Rect(string name, Transform parent, float x, float y, float width, float height)
        {
            var rect = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();
            rect.SetParent(parent, false); rect.anchorMin = rect.anchorMax = new Vector2(0, 1); rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(x, -y); rect.sizeDelta = new Vector2(width, height); return rect;
        }
        private static RectTransform Panel(string name, Transform parent, Color color, float x, float y, float width, float height)
        {
            var rect = Rect(name, parent, x, y, width, height);
            var image = rect.gameObject.AddComponent<UnityEngine.UI.Image>(); image.color = color;
            image.raycastTarget = name == "ResultPanel" || name == "HowToPlayPanel";
            return rect;
        }
        private static TextMeshProUGUI Label(string name, Transform parent, string text, TMP_FontAsset font, int size, float x, float y, float w, float h, Color color, TextAlignmentOptions alignment = TextAlignmentOptions.Center)
        {
            var label = Rect(name, parent, x, y, w, h).gameObject.AddComponent<TextMeshProUGUI>();
            label.font = font; label.text = text; label.fontSize = size; label.color = color; label.alignment = alignment;
            label.raycastTarget = false; label.textWrappingMode = TextWrappingModes.Normal; return label;
        }
        private static UnityEngine.UI.Button Button(string name, Transform parent, string text, TMP_FontAsset font, float x, float y, float w, float h, bool accent = false)
        {
            var rect = Panel(name, parent, accent ? BattleHUDColors.Gold : new Color32(39, 58, 71, 255), x, y, w, h);
            var image = rect.GetComponent<UnityEngine.UI.Image>(); image.raycastTarget = true;
            var button = rect.gameObject.AddComponent<UnityEngine.UI.Button>(); button.targetGraphic = image;
            var colors = button.colors; colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f); colors.disabledColor = new Color(.42f, .45f, .48f, .75f); button.colors = colors;
            var navigation = button.navigation; navigation.mode = UnityEngine.UI.Navigation.Mode.None; button.navigation = navigation;
            Label("Label", rect, text, font, 17, 4, 0, w - 8, h, accent ? new Color32(22, 32, 40, 255) : Color.white);
            return button;
        }
        private static UnityEngine.UI.Image Bar(string name, Transform parent, float x, float y, float width, Color color)
        {
            var track = Panel(name + "Track", parent, new Color32(56, 68, 76, 255), x, y, width, 8);
            var fill = Panel(name + "Fill", track, color, 0, 0, width, 8);
            fill.anchorMin = Vector2.zero; fill.anchorMax = Vector2.one; fill.offsetMin = fill.offsetMax = Vector2.zero;
            return fill.GetComponent<UnityEngine.UI.Image>();
        }
    }
}
