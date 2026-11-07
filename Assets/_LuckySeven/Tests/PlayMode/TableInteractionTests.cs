using System.Collections;
using LuckySeven.Game;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace LuckySeven.Tests
{
    public sealed class TableInteractionTests
    {
        static void Click(string name) => GameObject.Find(name).GetComponent<UnityEngine.UI.Button>().onClick.Invoke();
        static void Drag(GameObject card, Vector2 destination)
        {
            var rect=(RectTransform)card.transform;
            var parent=(RectTransform)rect.parent;
            var e=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
            e.position=RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(new Vector3(20,-20)));
            ExecuteEvents.Execute(card,e,ExecuteEvents.beginDragHandler);
            e.position=RectTransformUtility.WorldToScreenPoint(null,parent.TransformPoint(destination));
            ExecuteEvents.Execute(card,e,ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(card,e,ExecuteEvents.endDragHandler);
        }
        [UnityTest, Timeout(60000)] public IEnumerator StageReturnAndDragRejectInvalidDrops()
        {
            yield return SceneManager.LoadSceneAsync("Main");yield return null;
            var flow=Object.FindFirstObjectByType<GameFlowController>();
            yield return new WaitUntil(()=>flow.Ready);Click("Play");flow.Session.Start(7);flow.Render();Click("Lucky0");Click("Draw");
            var s=flow.Session;var card=GameObject.Find("Card0");int count=s.Hand.Count,id=s.Hand[0].Id;
            Click("Card0");yield return new WaitForSeconds(.35f);
            Assert.True(s.IsSelected(id));Assert.That(((RectTransform)card.transform).anchoredPosition.y,Is.GreaterThan(-400));
            Assert.AreEqual(count,s.Hand.Count,"Staging must not consume cards.");
            Click("Card0");yield return new WaitForSeconds(.35f);
            Assert.False(s.IsSelected(id));Assert.That(((RectTransform)card.transform).anchoredPosition.y,Is.LessThan(-530));
            Drag(card,new Vector2(30,-200));yield return new WaitForSeconds(.35f);
            Assert.False(s.IsSelected(id),"Drop outside the table must not play.");
            Drag(card,new Vector2(640,-410));yield return new WaitForSeconds(.35f);
            Assert.True(s.IsSelected(id),"Drop inside table must stage exactly once.");Assert.AreEqual(1,s.SelectedCount);
            Drag(card,new Vector2(640,-610));yield return new WaitForSeconds(.35f);
            Assert.False(s.IsSelected(id),"Dragging back to the hand must return the card.");
            Click("Card0");Click("Card1");Click("Primary");yield return null;
            Assert.AreEqual(LuckySeven.Core.SessionState.AttackChoice,s.State);
            Click("Card0");Click("Card1");Assert.AreEqual(1,s.SelectedCount,"Attack selection replaces previous card.");
            Click("Primary");yield return new WaitForSeconds(1.6f);
            Assert.AreEqual(LuckySeven.Core.SessionState.SupplyChoice,s.State);
            card=GameObject.Find("Card0");Drag(card,new Vector2(640,-410));
            Assert.AreEqual(0,s.SelectedCount,"Supply phase must reject card input.");
        }
    }
}
