using System.Collections;
using System.Linq;
using LuckySeven.Core;
using LuckySeven.Game;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace LuckySeven.Tests
{
    public sealed class PixelPresentationTests
    {
        [UnityTest,Timeout(60000)] public IEnumerator PixelSkinShowsArtAndSeparatesBreakFromDamage()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            var flow=Object.FindFirstObjectByType<GameFlowController>();yield return new WaitUntil(()=>flow.Ready);
            flow.StartGame();flow.Session.Start(7);flow.Render();flow.ChooseLucky(0);flow.Draw();yield return new WaitForSeconds(.3f);
            var enemy=GameObject.Find("PixelEnemy").GetComponent<Image>();Assert.NotNull(enemy.sprite);Assert.AreEqual("Die",enemy.sprite.name);
            Assert.NotNull(GameObject.Find("PixelHero").GetComponent<Image>().sprite);
            foreach(string name in new[]{"LuckyCoinA","SupplyPlate","LegendPlate","PixelArmor"}){
                var renderer=GameObject.Find(name).GetComponent<CanvasRenderer>();
                Assert.NotNull(renderer,name+" needs a CanvasRenderer");
                Assert.Greater(renderer.GetMesh().vertexCount,0,name+" must produce visible geometry");
            }
            Assert.False(GameObject.Find("Draw").GetComponent<Button>().interactable,"Supply must stay visible but disabled after drawing.");
            Assert.False(GameObject.Find("Exchange").GetComponent<Button>().interactable);
            ScreenCapture.CaptureScreenshot("Docs/Validation/ResourceComboV1/editor-hand.png");yield return null;
            var s=flow.Session;int hp=s.EnemyHp,block=s.Block,bonus=s.BonusDraw;
            foreach(int index in s.FindLuckyCombo())flow.Card(index);
            Assert.True(s.CanBreak);flow.Primary();
            Assert.AreEqual(SessionState.AttackChoice,s.State);Assert.AreEqual(hp,s.EnemyHp);
            Assert.AreEqual(block,s.Block);Assert.AreEqual(bonus,s.BonusDraw);
            Assert.NotNull(GameObject.Find("ArmorFragment0"));Assert.IsNull(GameObject.Find("PixelDamage"));
            ScreenCapture.CaptureScreenshot("Docs/Validation/ResourceComboV1/editor-break.png");yield return new WaitForSeconds(.45f);
            Assert.IsNull(GameObject.Find("ArmorFragment0"),"Fragments must finish without changing the attack window.");
            int attackIndex=s.Hand.ToList().FindIndex(c=>c.Effect==CardEffect.Strike);Assert.GreaterOrEqual(attackIndex,0);
            int amount=s.Hand[attackIndex].Value+2+s.PendingComboBonus;flow.Card(attackIndex);flow.Primary();yield return new WaitForSeconds(.12f);
            Assert.AreEqual(Mathf.Max(0,hp-amount),s.EnemyHp);
            Assert.IsNull(GameObject.Find("PixelArmor"),"Broken armor must not visually restore during the same attack.");
            Assert.AreEqual(LuckySeven.Localization.L10n.T("hint.resolve"),GameObject.Find("TableHint").GetComponent<TextMeshProUGUI>().text);
            Assert.AreEqual("-"+amount,GameObject.Find("PixelDamage").GetComponent<TextMeshProUGUI>().text);
            Assert.NotNull(GameObject.Find("PixelSlash"));ScreenCapture.CaptureScreenshot("Docs/Validation/ResourceComboV1/editor-hit.png");
            yield return new WaitForSeconds(1.5f);
            Assert.IsNull(GameObject.Find("PixelDamage"));Assert.IsNull(GameObject.Find("PixelSlash"));
            Assert.AreEqual(SessionState.SupplyChoice,s.State);Assert.NotNull(GameObject.Find("PixelArmor"));
            flow.StartGame();yield return null;Assert.IsNull(GameObject.Find("PixelDamage"));Assert.AreEqual(Vector3.one,enemy.transform.localScale);
        }
    }
}
