using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using LuckySeven.Game;
using LuckySeven.Localization;
using LuckySeven.UI;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
namespace LuckySeven.Tests
{
    public sealed class LocalizationCardTests
    {
        static void Click(string name)=>GameObject.Find(name).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        static void ChineseVisible(){
            foreach(var t in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None)){
                Assert.False(Regex.IsMatch(t.text,"[A-Za-z]"),t.name+": "+t.text);
                foreach(char c in t.text.Distinct().Where(c=>!char.IsWhiteSpace(c)))Assert.True(t.font.HasCharacter(c),"Missing glyph "+c+" on "+t.name);
            }
        }
        [UnityTest, Timeout(60000)] public IEnumerator ChineseEnglishSwitchPreservesBattleAndHoverIsReadable()
        {
            PlayerPrefs.DeleteKey(L10n.PreferenceKey);
            yield return SceneManager.LoadSceneAsync("Main");
            var flow=Object.FindFirstObjectByType<GameFlowController>();yield return new WaitUntil(()=>flow.Ready);yield return new WaitForSeconds(.2f);
            Assert.AreEqual("zh-Hans",L10n.LocaleCode);ChineseVisible();ScreenCapture.CaptureScreenshot("Docs/Validation/ResourceComboV1/editor-menu.png");yield return null;
            Click("HowToPlay");yield return new WaitForSeconds(.1f);ChineseVisible();
            Click("Language");yield return new WaitForSeconds(.2f);yield return new WaitUntil(()=>flow.Ready);
            Assert.AreEqual("en",L10n.LocaleCode);ScreenCapture.CaptureScreenshot("Docs/Validation/ResourceComboV1/editor-english.png");yield return null;
            var body=GameObject.Find("HelpBody").GetComponent<TextMeshProUGUI>();Assert.True(body.text.Contains("Start with 5"),"Actual help: "+body.text+"; Table: "+L10n.T("help.body"));
            foreach(var text in Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsSortMode.None))Assert.False(Regex.IsMatch(text.text,"[\u4e00-\u9fff]"),"Untranslated English UI: "+text.text);
            Click("CloseHelp");Click("Play");Click("Lucky0");Click("Draw");Click("Card0");
            var s=flow.Session;var ids=s.Hand.Select(c=>c.Id).ToArray();int hp=s.PlayerHp;
            Click("Language");yield return new WaitForSeconds(.2f);yield return new WaitUntil(()=>flow.Ready);
            Assert.AreEqual("zh-Hans",L10n.LocaleCode);Assert.AreEqual(hp,s.PlayerHp);Assert.AreEqual(ids,s.Hand.Select(c=>c.Id).ToArray());Assert.AreEqual(1,s.SelectedCount);ChineseVisible();
            Click("Card0");yield return new WaitForSeconds(.3f);
            var card=GameObject.Find("Card0");var face=(RectTransform)card.transform.Find("PokerFace");
            ExecuteEvents.Execute(card,new PointerEventData(EventSystem.current),ExecuteEvents.pointerEnterHandler);yield return new WaitForSeconds(.3f);
            Assert.Greater(face.anchoredPosition.y,30);Assert.Greater(face.localScale.x,1.1f);Assert.AreEqual(card.transform.parent.childCount-1,card.transform.GetSiblingIndex());
            var renderer=face.GetComponent<CanvasRenderer>();Assert.NotNull(renderer,"Card stock must have a renderer.");
            var mesh=renderer.GetMesh();Assert.NotNull(mesh);Assert.Greater(mesh.vertexCount,100,"Poker card stock/pips must generate visible geometry.");
            ScreenCapture.CaptureScreenshot("Docs/Validation/ResourceComboV1/editor-hover.png");yield return null;
            var neighbour=RectTransformUtility.WorldToScreenPoint(null,card.transform.TransformPoint(new Vector3(90,-70)));
            Assert.False(card.GetComponent<CardMotion>().IsRaycastLocationValid(neighbour,null),"Raised face must not block the next card's hand strip.");
            ExecuteEvents.Execute(card,new PointerEventData(EventSystem.current),ExecuteEvents.pointerExitHandler);yield return new WaitForSeconds(.3f);Assert.Less(face.anchoredPosition.y,1);
            Assert.AreEqual("zh-Hans",PlayerPrefs.GetString(L10n.PreferenceKey));
        }
    }
}
