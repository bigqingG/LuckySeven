using LuckySeven.Localization;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using System.Collections;
using LuckySeven.Core;
using LuckySeven.Config;
using LuckySeven.UI;
using LuckySeven.Presentation;
using UnityEngine;
namespace LuckySeven.Game
{
    public sealed class GameFlowController:MonoBehaviour
    {
        [SerializeField] GameConfig config; [SerializeField] BattleHUDView view; [SerializeField] BattleFxPlayer effects;
        public BattleSession Session {get;private set;}
        public void Configure(GameConfig c,BattleHUDView v,BattleFxPlayer e){config=c;view=v;effects=e;}
        public bool Ready { get; private set; }
        IEnumerator Start(){
            view.SetInputReady(false);
            yield return L10n.Initialize();
            view.ApplyLocaleFont();Session=config.CreateSession();view.Bind(this);view.SetSound(effects.Muted);Render();
            LocalizationSettings.SelectedLocaleChanged+=LocaleChanged;
            Ready=true;view.SetInputReady(true);
#if UNITY_WEBGL && !UNITY_EDITOR
        Application.targetFrameRate=-1;
#else
        Application.targetFrameRate=60;
#endif
        }
        void LocaleChanged(Locale locale){StartCoroutine(RefreshLocale());}
        IEnumerator RefreshLocale(){
            Ready=false;view.SetInputReady(false);yield return L10n.LoadLocale();
            view.ApplyLocaleFont();view.SetSound(effects.Muted);effects.RefreshText();Render();L10n.SavePreference();
            Ready=true;view.SetInputReady(true);
        }
        public void ToggleLanguage(){if(!Ready)return;Ready=false;view.SetInputReady(false);LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(L10n.LocaleCode=="en"?"zh-Hans":"en");}
        void OnDestroy(){LocalizationSettings.SelectedLocaleChanged-=LocaleChanged;}
        public void Render(){view.Render(Session,config.Emblem(Session.Stage));WebVisibleState.Publish(Session);}
        public void StartGame(){StopAllCoroutines();effects.ResetFeedback();Session.Start(config.NewSeed());Render();}
        public void ChooseLucky(int i){if(Session.ChooseLucky(i)){effects.ResetFeedback();Render();}}
        public void Card(int i){if(Session.ToggleCard(i))Render();}
        public void Draw(){if(Session.DrawSupply()){effects.ResetFeedback();Render();}}
        public void Exchange(){if(Session.BeginExchange())Render();}
        public void Clear(){if(Session.State==SessionState.ExchangeSelection)Session.CancelExchange();else Session.ClearSelection();Render();}
        public void Primary(){
            bool changed=false;
            if(Session.State==SessionState.ExchangeSelection)changed=Session.CompleteExchange();
            else if(Session.State==SessionState.TrimHand)changed=Session.Trim();
            else if(Session.State==SessionState.PlayerInput){changed=Session.BreakArmor();if(changed)effects.Break(Session.LuckyNumber);}
            else if(Session.State==SessionState.AttackChoice){Attack();return;}
            if(changed)Render();
        }
        public void Attack(){if(Session.BeginAttack(out var result)){Render();StartCoroutine(Resolve(result));}}
        IEnumerator Resolve(AttackResult result){effects.Attack(result);yield return new WaitForSeconds(.75f);Session.FinishPlayerAttack();Render();
            if(Session.State==SessionState.EnemyAct){effects.Counter(Session.Enemy.Attack,Session.Block);yield return new WaitForSeconds(.65f);Session.FinishEnemyAttack();Render();}}
        public void NextBattle(){if(Session.NextBattle()){effects.ResetFeedback();Render();}}
        public void ToggleSound(){effects.ToggleMute();view.SetSound(effects.Muted);}
    }
}
