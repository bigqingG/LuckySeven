using System.Collections;
using LuckySeven.Core;
using LuckySeven.Game;
using LuckySeven.Localization;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LuckySeven.Tests
{
    public sealed class ResourceComboPresentationTests
    {
        static TextMeshProUGUI Text(string name)=>GameObject.Find(name).GetComponent<TextMeshProUGUI>();
        [UnityTest,Timeout(60000)] public IEnumerator ComboAndDamagePreviewFollowSelectionAndLanguage()
        {
            yield return SceneManager.LoadSceneAsync("Main");
            var flow=Object.FindFirstObjectByType<GameFlowController>();yield return new WaitUntil(()=>flow.Ready);
            flow.StartGame();flow.Session.Start(7);flow.Render();flow.ChooseLucky(0);
            Assert.AreEqual(L10n.T("draw",2),Text("Draw/Label").text);
            flow.Draw();Assert.AreEqual(7,flow.Session.Hand.Count);
            flow.Card(0);flow.Card(1);Assert.AreEqual(L10n.T("combo.ready",2,2),Text("Hint").text);
            Assert.True(Text("Sum").text.Contains(L10n.T("combo.short",2)));
            flow.Primary();Assert.AreEqual(L10n.T("combo.pending",2),Text("Sum").text);
            flow.Card(0);var s=flow.Session;Assert.True(s.TryPreviewAttack(out var preview));
            Assert.AreEqual(L10n.T("preview.break",preview.Damage,preview.Value,preview.Effect==CardEffect.Strike?2:0,2),Text("Hint").text);
            for(int i=0;i<2;i++){
                flow.ToggleLanguage();yield return new WaitUntil(()=>flow.Ready);yield return null;
                Assert.AreEqual(2,s.PendingComboBonus);Assert.AreEqual(1,s.SelectedCount);
                Assert.AreEqual(L10n.T("combo.locked",2),Text("Sum").text);
                Assert.AreEqual(L10n.T("preview.break",preview.Damage,preview.Value,preview.Effect==CardEffect.Strike?2:0,2),Text("Hint").text);
                foreach(string name in new[]{"Sum","Hint"}){
                    var label=Text(name);label.ForceMeshUpdate();
                    Assert.LessOrEqual(label.preferredWidth,label.rectTransform.rect.width+1,name+" must fit horizontally");
                    Assert.LessOrEqual(label.preferredHeight,label.rectTransform.rect.height+1,name+" must fit vertically");
                }
            }
            flow.Clear();Assert.AreEqual(2,s.PendingComboBonus);Assert.AreEqual(L10n.T("combo.pending",2),Text("Sum").text);
            flow.Card(0);int hp=s.EnemyHp;flow.Primary();Assert.AreEqual(Mathf.Max(0,hp-preview.Damage),s.EnemyHp);
            yield return new WaitForSeconds(1.6f);Assert.AreEqual(0,s.PendingComboBonus);
        }
    }
}
