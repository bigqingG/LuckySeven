using System.Collections;
using LuckySeven.Core;
using LuckySeven.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace LuckySeven.Tests
{
 public sealed class FlowTests {
  static void Click(string name){var go=GameObject.Find(name);Assert.NotNull(go,name);var b=go.GetComponent<Button>();Assert.True(b.interactable,name);b.onClick.Invoke();}
  [UnityTest, Timeout(60000)] public IEnumerator CardUiCompleteRunAndReplay(){
   yield return SceneManager.LoadSceneAsync("Main");yield return null;
   var flow=Object.FindFirstObjectByType<GameFlowController>();Assert.NotNull(flow);
   yield return new WaitUntil(()=>flow.Ready);
   Click("HowToPlay");Click("CloseHelp");Click("Play");
   var s=flow.Session;s.Start(7);flow.Render();
   for(int step=0;step<100;step++){
    switch(s.State){
     case SessionState.ChooseLucky:Click("Lucky0");break;
     case SessionState.SupplyChoice:Click("Draw");break;
     case SessionState.TrimHand:int count=s.Hand.Count-9;for(int i=0;i<count;i++)Click("Card"+i);Click("Primary");break;
     case SessionState.PlayerInput:var combo=s.FindLuckyCombo();foreach(int i in combo)Click("Card"+i);if(combo.Length>0){Click("Primary");break;}goto case SessionState.AttackChoice;
     case SessionState.AttackChoice:int best=0;for(int i=1;i<s.Hand.Count;i++)if(s.Hand[i].Value>s.Hand[best].Value)best=i;Click("Card"+best);Click(s.State==SessionState.AttackChoice?"Primary":"NormalAttack");yield return new WaitForSeconds(1.6f);break;
     case SessionState.BetweenBattles:Assert.IsNull(GameObject.Find("BattleHUD"));Click("NextBattle");break;
     case SessionState.Victory:Assert.Greater(s.LuckyHits,0);Click("Replay");Assert.AreEqual(SessionState.ChooseLucky,s.State);Assert.AreEqual(5,s.Hand.Count);Click("Sound");yield break;
     case SessionState.Defeat:Assert.Fail("Baseline strategy lost seed 7");break;
     default:Assert.Fail("Unexpected state "+s.State);break;
    }
    yield return null;
   }
   Assert.Fail("Run did not finish");
  }
 }
}
