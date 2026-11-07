using System;
using System.Linq;
using LuckySeven.Core;
using NUnit.Framework;
namespace LuckySeven.Tests
{
    public sealed class RulesTests
    {
        static BattleSession New(int seed=7,int hp=30,int enemyHp=18,int attack=2){var s=new BattleSession(new[]{new EnemyDefinition("A",enemyHp,attack,3),new EnemyDefinition("B",24,3,4),new EnemyDefinition("C",30,4,5)},hp);s.Start(seed);return s;}
        static void Supply(BattleSession s){s.ChooseLucky(0);s.DrawSupply();}
        static void Conserve(BattleSession s){var all=s.Hand.Concat(s.Deck).Concat(s.Discard).ToArray();Assert.That(all.Length,Is.EqualTo(27));Assert.That(all.Select(c=>c.Id).Distinct().Count(),Is.EqualTo(27));}
        [Test] public void OpeningAndCandidates(){var s=New();Assert.That(s.Hand.Select(c=>c.Value),Is.EqualTo(new[]{2,5,9,1,3}));Assert.That(s.Candidates.Distinct().Count(),Is.EqualTo(3));Assert.That(s.Candidates.All(n=>n>=1&&n<=10&&n!=7));Conserve(s);}
        [Test] public void SupplyCannotRepeat(){var s=New();Supply(s);Assert.That(s.Hand.Count,Is.EqualTo(7));Assert.False(s.DrawSupply());Assert.False(s.ChooseLucky(1));Conserve(s);}
        [Test] public void BreakSpendsSumButDoesNoDamage(){var s=New();Supply(s);s.ToggleCard(0);s.ToggleCard(1);Assert.That(s.SelectedSum,Is.EqualTo(7));Assert.True(s.BreakArmor());Assert.That(s.EnemyHp,Is.EqualTo(18));Assert.That(s.Hand.Count,Is.EqualTo(5));Assert.That(s.State,Is.EqualTo(SessionState.AttackChoice));Assert.False(s.BreakArmor());Conserve(s);}
        [Test] public void AllCardsCannotBreak(){var s=New();Supply(s);for(int i=0;i<s.Hand.Count;i++)s.ToggleCard(i);Assert.False(s.CanBreak);Assert.False(s.BreakArmor());}
        [Test] public void SecondaryTargetActuallyBreaks(){BattleSession s=null;for(int seed=0;seed<100;seed++){s=New(seed);int at=Array.IndexOf(s.Candidates.ToArray(),5);if(at>=0){s.ChooseLucky(at);break;}}Assert.That(s.LuckyNumber,Is.EqualTo(5));s.DrawSupply();s.ToggleCard(1);Assert.True(s.BreakArmor());Assert.That(s.SecondaryHits,Is.EqualTo(1));}
        [Test] public void ExchangeReturnsAfterDrawing(){var s=New();s.ChooseLucky(0);var ids=s.Hand.Take(2).Select(c=>c.Id).ToArray();Assert.True(s.BeginExchange());s.ToggleCard(0);s.ToggleCard(1);Assert.False(s.ToggleCard(2));Assert.True(s.CompleteExchange());Assert.That(s.Hand.Count,Is.EqualTo(6));Assert.False(s.Hand.Any(c=>ids.Contains(c.Id)));Assert.That(s.Deck.Skip(s.Deck.Count-2).Select(c=>c.Id),Is.EqualTo(ids));Conserve(s);}
        [Test] public void ExchangeCancelPreservesHand(){var s=New();s.ChooseLucky(0);s.BeginExchange();s.ToggleCard(0);Assert.True(s.CancelExchange());Assert.That(s.Hand.Count,Is.EqualTo(5));Assert.That(s.SelectedCount,Is.Zero);Conserve(s);}
        [TestCase(CardEffect.Strike,11,0,0)] [TestCase(CardEffect.Ward,9,3,0)] [TestCase(CardEffect.Echo,9,0,1)]
        public void Effects(CardEffect effect,int damage,int block,int draw){var r=new AttackResult(new NumberCard(1,9,effect),true,5);Assert.That(r.Damage,Is.EqualTo(damage));Assert.That(r.Block,Is.EqualTo(block));Assert.That(r.NextDraw,Is.EqualTo(draw));}
        [Test] public void ArmorReducesNormalAndMinimumIsOne(){Assert.That(new AttackResult(new NumberCard(1,9,CardEffect.Ward),false,5).Damage,Is.EqualTo(4));Assert.That(new AttackResult(new NumberCard(2,1,CardEffect.Ward),false,5).Damage,Is.EqualTo(1));}
        [Test] public void AttackLocksAndEnemyStillCountersAfterBreak(){var s=New(enemyHp:100);Supply(s);s.ToggleCard(0);s.ToggleCard(1);s.BreakArmor();s.ToggleCard(0);Assert.True(s.BeginAttack(out var r));Assert.True(r.Breached);Assert.False(s.BeginAttack(out _));Assert.False(s.ToggleCard(0));s.FinishPlayerAttack();Assert.That(s.State,Is.EqualTo(SessionState.EnemyAct));s.FinishEnemyAttack();Assert.That(s.PlayerHp,Is.EqualTo(30-Math.Max(0,2-r.Block)));Assert.That(s.State,Is.EqualTo(SessionState.SupplyChoice));Conserve(s);}
        [Test] public void OverflowRequiresExactDiscard(){var s=New(enemyHp:100);Supply(s);s.ToggleCard(0);s.BeginAttack(out _);s.FinishPlayerAttack();s.FinishEnemyAttack();s.DrawSupply();while(s.State==SessionState.PlayerInput){s.ToggleCard(0);s.BeginAttack(out _);s.FinishPlayerAttack();s.FinishEnemyAttack();s.DrawSupply();}Assert.That(s.State,Is.EqualTo(SessionState.TrimHand));Assert.False(s.Trim());int excess=s.Hand.Count-9;for(int i=0;i<excess;i++)s.ToggleCard(i);Assert.True(s.Trim());Assert.That(s.Hand.Count,Is.EqualTo(9));Conserve(s);}
        [Test] public void ResetClearsRun(){var s=New();Supply(s);s.ToggleCard(0);s.BeginAttack(out _);s.Start(7);Assert.That(s.PlayerHp,Is.EqualTo(30));Assert.That(s.Attacks,Is.Zero);Assert.That(s.BonusDraw,Is.Zero);Assert.That(s.SelectedCount,Is.Zero);Assert.That(s.State,Is.EqualTo(SessionState.ChooseLucky));Conserve(s);}
        [Test] public void DefeatAndNoPostDefeatInput(){var s=New(hp:1,enemyHp:100,attack:100);Supply(s);s.ToggleCard(0);s.BeginAttack(out _);s.FinishPlayerAttack();s.FinishEnemyAttack();Assert.That(s.State,Is.EqualTo(SessionState.Defeat));Assert.False(s.DrawSupply());Assert.False(s.BeginAttack(out _));}
        [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)] [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)]
        public void CompleteRunsConserveDeckAndNeverDeadlock(int seed){var s=New(seed);for(int step=0;step<400;step++){Conserve(s);switch(s.State){
            case SessionState.ChooseLucky:s.ChooseLucky(0);break;
            case SessionState.SupplyChoice:s.DrawSupply();break;
            case SessionState.TrimHand:for(int i=0;i<s.Hand.Count-9;i++)s.ToggleCard(i);Assert.True(s.Trim());break;
            case SessionState.PlayerInput:var combo=s.FindLuckyCombo();foreach(var i in combo)s.ToggleCard(i);if(combo.Length>0){Assert.True(s.BreakArmor());break;}goto case SessionState.AttackChoice;
            case SessionState.AttackChoice:int best=0;for(int i=1;i<s.Hand.Count;i++)if(s.Hand[i].Value>s.Hand[best].Value)best=i;s.ToggleCard(best);Assert.True(s.BeginAttack(out _));break;
            case SessionState.Resolving:s.FinishPlayerAttack();break;
            case SessionState.EnemyAct:s.FinishEnemyAttack();break;
            case SessionState.BetweenBattles:Assert.True(s.NextBattle());break;
            case SessionState.Victory:case SessionState.Defeat:Assert.That(s.Attacks,Is.GreaterThan(2));return;
            default:Assert.Fail("Stuck in "+s.State);break;
        }}Assert.Fail("Run failed to terminate");}
    }
}
