using System;
using System.Collections.Generic;
using System.Linq;
namespace LuckySeven.Core
{
    public enum SessionState { MainMenu, ChooseLucky, SupplyChoice, ExchangeSelection, TrimHand, PlayerInput, AttackChoice, Resolving, EnemyAct, BetweenBattles, Victory, Defeat }
    public enum CardEffect { Strike, Ward, Echo }
    public sealed class NumberCard
    {
        public int Id { get; } public int Value { get; } public CardEffect Effect { get; }
        public NumberCard(int id, int value, CardEffect effect) { Id=id; Value=value; Effect=effect; }
    }
    public sealed class EnemyDefinition
    {
        public string Name { get; } public int MaxHp { get; } public int Attack { get; } public int Armor { get; }
        public EnemyDefinition(string name,int hp,int attack,int armor) { Name=name; MaxHp=hp; Attack=attack; Armor=armor; }
    }
    public readonly struct AttackResult
    {
        public readonly int Value, Damage, Block, NextDraw, ComboBonus; public readonly bool Breached; public readonly CardEffect Effect;
        public AttackResult(NumberCard card,bool breached,int armor,int comboBonus=0)
        { Value=card.Value; Effect=card.Effect; Breached=breached; Block=card.Effect==CardEffect.Ward?3:0; NextDraw=card.Effect==CardEffect.Echo?1:0;
          ComboBonus=breached?comboBonus:0;
          Damage=Math.Max(1,card.Value+(card.Effect==CardEffect.Strike?2:0)+ComboBonus-(breached?0:armor)); }
    }
    public sealed class BattleSession
    {
        public const int HandLimit=9, DrawCount=2, ExchangeDrawCount=3;
        readonly EnemyDefinition[] enemies; readonly int healing;
        readonly List<NumberCard> hand=new List<NumberCard>(), deck=new List<NumberCard>(), discard=new List<NumberCard>();
        readonly HashSet<int> selected=new HashSet<int>(); readonly int[] candidates=new int[3]; Random random;
        public SessionState State { get; private set; }=SessionState.MainMenu;
        public IReadOnlyList<NumberCard> Hand=>hand; public IReadOnlyList<NumberCard> Deck=>deck; public IReadOnlyList<NumberCard> Discard=>discard;
        public IReadOnlyList<int> Candidates=>candidates;
        public int MaxHp { get; } public int PlayerHp { get; private set; } public int EnemyHp { get; private set; }
        public int Stage { get; private set; } public int StageCount=>enemies.Length; public EnemyDefinition Enemy=>enemies[Stage];
        public int Turn { get; private set; } public int LuckyNumber { get; private set; } public int Block { get; private set; }
        public int BonusDraw { get; private set; } public int Seed { get; private set; } public int Attacks { get; private set; }
        public int PendingComboBonus { get; private set; }
        public int SelectedComboBonus=>CanBreak?ComboBonusFor(SelectedCount):0;
        static int ComboBonusFor(int count)=>count>=3?4:count==2?2:0;
        public int LuckyHits { get; private set; } public int SecondaryHits { get; private set; } public int LastHealing { get; private set; }
        public int SelectedCount=>selected.Count; public int SelectedSum=>hand.Where(c=>selected.Contains(c.Id)).Sum(c=>c.Value);
        public bool IsSelected(int id)=>selected.Contains(id);
        public bool CanBreak=>State==SessionState.PlayerInput && selected.Count>0 && selected.Count<hand.Count && (SelectedSum==7 || SelectedSum==LuckyNumber);
        public bool CanAttack=>(State==SessionState.PlayerInput || State==SessionState.AttackChoice) && selected.Count==1;
        public BattleSession(EnemyDefinition[] definitions,int hp=30,int heal=6)
        { if(definitions==null || definitions.Length==0) throw new ArgumentException("Enemies required"); enemies=definitions; MaxHp=hp; healing=heal; }
        public void Start(int seed)
        {
            random=new Random(seed); Seed=seed; Stage=0; PlayerHp=MaxHp; Attacks=LuckyHits=SecondaryHits=LastHealing=0; BonusDraw=Block=0;
            hand.Clear(); deck.Clear(); discard.Clear(); selected.Clear();
            for(int copy=0;copy<3;copy++) for(int value=1;value<=9;value++) deck.Add(new NumberCard(copy*9+value,value,(CardEffect)((value+copy)%3)));
            Shuffle(deck);
            // Opening lesson: 2 + 5 breaks seven, leaving a strong attack. All five cards belong to the real deck.
            foreach(int value in new[]{2,5,9,1,3}) { int at=deck.FindIndex(c=>c.Value==value); hand.Add(deck[at]); deck.RemoveAt(at); }
            BeginEncounter();
        }
        void Shuffle(List<NumberCard> cards) { for(int i=cards.Count-1;i>0;i--) { int j=random.Next(i+1); var c=cards[i];cards[i]=cards[j];cards[j]=c; } }
        void BeginEncounter()
        {
            EnemyHp=Enemy.MaxHp; Turn=1; LuckyNumber=0; Block=PendingComboBonus=0; selected.Clear();
            var values=Enumerable.Range(1,10).Where(n=>n!=7).ToList();
            for(int i=0;i<3;i++) { int j=random.Next(values.Count); candidates[i]=values[j];values.RemoveAt(j); }
            State=SessionState.ChooseLucky;
        }
        public bool ChooseLucky(int index)
        { if(State!=SessionState.ChooseLucky || index<0 || index>=3) return false; LuckyNumber=candidates[index];State=SessionState.SupplyChoice;return true; }
        void Draw(int amount)
        {
            for(int i=0;i<amount;i++) {
                if(deck.Count==0) { deck.AddRange(discard);discard.Clear();Shuffle(deck); }
                if(deck.Count==0) break;
                hand.Add(deck[0]);deck.RemoveAt(0);
            }
        }
        void SupplyFinished() { selected.Clear();State=hand.Count>HandLimit?SessionState.TrimHand:SessionState.PlayerInput; }
        public bool DrawSupply()
        { if(State!=SessionState.SupplyChoice) return false;Draw(DrawCount+BonusDraw);BonusDraw=0;SupplyFinished();return true; }
        public bool BeginExchange()
        { if(State!=SessionState.SupplyChoice || hand.Count<2) return false;selected.Clear();State=SessionState.ExchangeSelection;return true; }
        public bool CancelExchange()
        { if(State!=SessionState.ExchangeSelection) return false;selected.Clear();State=SessionState.SupplyChoice;return true; }
        public bool CompleteExchange()
        {
            if(State!=SessionState.ExchangeSelection || selected.Count!=2) return false;
            var returned=hand.Where(c=>selected.Contains(c.Id)).ToList();hand.RemoveAll(c=>selected.Contains(c.Id));
            Draw(ExchangeDrawCount+BonusDraw);BonusDraw=0;deck.AddRange(returned);SupplyFinished();return true;
        }
        public bool ToggleCard(int index)
        {
            if(index<0 || index>=hand.Count) return false;
            if(State!=SessionState.PlayerInput && State!=SessionState.AttackChoice && State!=SessionState.ExchangeSelection && State!=SessionState.TrimHand) return false;
            int id=hand[index].Id;if(selected.Remove(id)) return true;
            if(State==SessionState.AttackChoice) selected.Clear();
            if(State==SessionState.ExchangeSelection && selected.Count>=2) return false;
            selected.Add(id);return true;
        }
        public void ClearSelection() { selected.Clear(); }
        public bool Trim()
        {
            if(State!=SessionState.TrimHand || selected.Count!=hand.Count-HandLimit) return false;
            SpendSelected();State=SessionState.PlayerInput;return true;
        }
        void SpendSelected() { discard.AddRange(hand.Where(c=>selected.Contains(c.Id)));hand.RemoveAll(c=>selected.Contains(c.Id));selected.Clear(); }
        public bool BreakArmor()
        {
            if(!CanBreak) return false;
            PendingComboBonus=SelectedComboBonus;
            if(SelectedSum==LuckyNumber) SecondaryHits++;
            SpendSelected();LuckyHits++;State=SessionState.AttackChoice;return true;
        }
        public bool TryPreviewAttack(out AttackResult result)
        {
            result=default;if(!CanAttack) return false;
            var card=hand.First(c=>selected.Contains(c.Id));result=new AttackResult(card,State==SessionState.AttackChoice,Enemy.Armor,PendingComboBonus);
            return true;
        }
        public bool BeginAttack(out AttackResult result)
        {
            if(!TryPreviewAttack(out result)) return false;
            PendingComboBonus=0;
            EnemyHp=Math.Max(0,EnemyHp-result.Damage);Block=result.Block;BonusDraw+=result.NextDraw;SpendSelected();Attacks++;State=SessionState.Resolving;return true;
        }
        public void FinishPlayerAttack()
        {
            if(State!=SessionState.Resolving) return;
            if(EnemyHp>0) { State=SessionState.EnemyAct;return; }
            Block=0;
            if(Stage==enemies.Length-1) State=SessionState.Victory;
            else { LastHealing=Math.Min(healing,MaxHp-PlayerHp);PlayerHp+=LastHealing;State=SessionState.BetweenBattles; }
        }
        public void FinishEnemyAttack()
        {
            if(State!=SessionState.EnemyAct) return;
            PlayerHp=Math.Max(0,PlayerHp-Math.Max(0,Enemy.Attack-Block));Block=PendingComboBonus=0;selected.Clear();
            if(PlayerHp==0) {State=SessionState.Defeat;return;}
            Turn++;State=SessionState.SupplyChoice;
        }
        public bool NextBattle() { if(State!=SessionState.BetweenBattles) return false;Stage++;BeginEncounter();return true; }
        public int[] FindLuckyCombo()
        {
            // Used by validation and optional hints; reserve at least one card for the attack.
            for(int mask=1;mask<(1<<hand.Count)-1;mask++) {
                int sum=0;var indices=new List<int>();
                for(int i=0;i<hand.Count;i++) if((mask&(1<<i))!=0) {sum+=hand[i].Value;indices.Add(i);}
                if(sum==7 || sum==LuckyNumber) return indices.ToArray();
            }
            return Array.Empty<int>();
        }
    }
}
