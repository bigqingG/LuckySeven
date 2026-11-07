using System;
using System.Linq;
using LuckySeven.Core;
using NUnit.Framework;

namespace LuckySeven.Tests
{
    public sealed class ResourceComboTests
    {
        static BattleSession New(int seed=7,int enemyHp=100)
        {
            var s=new BattleSession(new[]{new EnemyDefinition("A",enemyHp,2,3),new EnemyDefinition("B",24,3,4)});
            s.Start(seed);s.ChooseLucky(0);s.DrawSupply();return s;
        }
        static void NextTurn(BattleSession s){s.FinishPlayerAttack();s.FinishEnemyAttack();}
        static BattleSession StagedCombo(int count,bool secondary)
        {
            for(int seed=0;seed<1000;seed++){
                var s=New(seed);int target=secondary?s.LuckyNumber:7;
                for(int mask=1;mask<(1<<s.Hand.Count)-1;mask++){
                    var indices=Enumerable.Range(0,s.Hand.Count).Where(i=>(mask&(1<<i))!=0).ToArray();
                    if(indices.Length!=count||indices.Sum(i=>s.Hand[i].Value)!=target)continue;
                    foreach(int i in indices)s.ToggleCard(i);return s;
                }
            }
            throw new Exception("No test fixture combo found");
        }
        [TestCase(1,0,false)][TestCase(2,2,false)][TestCase(3,4,false)][TestCase(4,4,false)]
        [TestCase(1,0,true)][TestCase(2,2,true)][TestCase(3,4,true)][TestCase(4,4,true)]
        public void ComboRewardIsLockedAndConsumedOnce(int count,int bonus,bool secondary)
        {
            var s=StagedCombo(count,secondary);int hp=s.EnemyHp;
            Assert.AreEqual(bonus,s.SelectedComboBonus);Assert.True(s.BreakArmor());
            Assert.AreEqual(hp,s.EnemyHp);Assert.AreEqual(0,s.Block);Assert.AreEqual(0,s.BonusDraw);
            Assert.AreEqual(bonus,s.PendingComboBonus);Assert.False(s.TryPreviewAttack(out _));
            s.ToggleCard(0);Assert.True(s.TryPreviewAttack(out var first));Assert.AreEqual(bonus,first.ComboBonus);
            s.ClearSelection();Assert.AreEqual(bonus,s.PendingComboBonus);Assert.False(s.BeginAttack(out _));
            s.ToggleCard(0);s.ToggleCard(1);Assert.AreEqual(1,s.SelectedCount);
            Assert.True(s.TryPreviewAttack(out var preview));Assert.AreEqual(bonus,s.PendingComboBonus);
            Assert.AreEqual(hp,s.EnemyHp,"Preview must not mutate HP");
            Assert.True(s.BeginAttack(out var actual));Assert.AreEqual(preview.Damage,actual.Damage);
            Assert.AreEqual(actual.Value+(actual.Effect==CardEffect.Strike?2:0)+bonus,actual.Damage);
            Assert.AreEqual(hp-actual.Damage,s.EnemyHp);Assert.AreEqual(0,s.PendingComboBonus);
            Assert.False(s.BeginAttack(out _));NextTurn(s);s.DrawSupply();
            if(s.State==SessionState.TrimHand){for(int i=0;i<s.Hand.Count-9;i++)s.ToggleCard(i);s.Trim();}
            s.ToggleCard(0);Assert.True(s.TryPreviewAttack(out var normal));Assert.False(normal.Breached);
            Assert.AreEqual(0,normal.ComboBonus);Assert.AreEqual(Math.Max(1,normal.Value+(normal.Effect==CardEffect.Strike?2:0)-3),normal.Damage);
        }
        [TestCase(CardEffect.Strike,15,0,0)][TestCase(CardEffect.Ward,13,3,0)][TestCase(CardEffect.Echo,13,0,1)]
        public void BonusAddsDamageWithoutChangingAbilities(CardEffect effect,int damage,int block,int draw)
        {
            var r=new AttackResult(new NumberCard(1,9,effect),true,5,4);
            Assert.AreEqual(damage,r.Damage);Assert.AreEqual(block,r.Block);Assert.AreEqual(draw,r.NextDraw);
            Assert.AreEqual(0,new AttackResult(new NumberCard(1,9,effect),false,5,4).ComboBonus);
        }
        [TestCase(false)][TestCase(true)]
        public void EchoAddsOneToNewSupplyCounts(bool exchange)
        {
            var s=New();int at=s.Hand.ToList().FindIndex(c=>c.Effect==CardEffect.Echo);
            Assert.GreaterOrEqual(at,0);s.ToggleCard(at);s.BeginAttack(out _);NextTurn(s);
            int before=s.Hand.Count;Assert.AreEqual(1,s.BonusDraw);
            if(exchange){s.BeginExchange();s.ToggleCard(0);s.ToggleCard(1);Assert.True(s.CompleteExchange());}
            else Assert.True(s.DrawSupply());
            Assert.AreEqual(before+(exchange?2:3),s.Hand.Count);Assert.AreEqual(0,s.BonusDraw);
        }
        [Test] public void RestartAndNextBattleHaveNoComboReward()
        {
            var s=StagedCombo(3,false);s.BreakArmor();Assert.AreEqual(4,s.PendingComboBonus);s.Start(7);
            Assert.AreEqual(0,s.PendingComboBonus);
            s=New(enemyHp:1);s.ToggleCard(0);s.ToggleCard(1);s.BreakArmor();s.ToggleCard(0);s.BeginAttack(out _);s.FinishPlayerAttack();
            Assert.AreEqual(SessionState.BetweenBattles,s.State);Assert.True(s.NextBattle());Assert.AreEqual(0,s.PendingComboBonus);
        }
    }
}
