using System;
using System.Linq;
using System.Runtime.InteropServices;
using LuckySeven.Core;
using UnityEngine;
namespace LuckySeven.Game
{
    // Read-only mirror of visible HUD information for browser acceptance tests.
    // No mutation endpoint and no hidden deck information.
    public static class WebVisibleState
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [Serializable] sealed class Snapshot {
            public string state,locale; public int hp,enemyHp,armor,counter,stage,turn,lucky,bonus,attacks,breaks,secondary,combo,selectedCombo,predicted;
            public int[] values,effects,candidates;public bool[] selected;
        }
        [DllImport("__Internal")] static extern void LuckySevenPublish(string json);
#endif
        public static void Publish(BattleSession s){
#if UNITY_WEBGL && !UNITY_EDITOR
            LuckySevenPublish(JsonUtility.ToJson(new Snapshot{state=s.State.ToString(),locale=Localization.L10n.LocaleCode,hp=s.PlayerHp,enemyHp=s.EnemyHp,armor=s.Enemy.Armor,counter=s.Enemy.Attack,stage=s.Stage,turn=s.Turn,lucky=s.LuckyNumber,bonus=s.BonusDraw,attacks=s.Attacks,breaks=s.LuckyHits,secondary=s.SecondaryHits,combo=s.PendingComboBonus,selectedCombo=s.SelectedComboBonus,predicted=s.TryPreviewAttack(out var preview)?preview.Damage:-1,values=s.Hand.Select(c=>c.Value).ToArray(),effects=s.Hand.Select(c=>(int)c.Effect).ToArray(),candidates=s.Candidates.ToArray(),selected=s.Hand.Select(c=>s.IsSelected(c.Id)).ToArray()}));
#endif
        }
    }
}
