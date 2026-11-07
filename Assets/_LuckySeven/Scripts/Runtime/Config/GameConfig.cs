using System;
using LuckySeven.Core;
using UnityEngine;
namespace LuckySeven.Config
{
    [CreateAssetMenu(menuName="Lucky Seven/Game Config")]
    public sealed class GameConfig:ScriptableObject
    {
        [Serializable] public sealed class EnemyEntry {
            public string displayName; public int hp,attack,armor; public string emblem;
            public EnemyEntry(string name,int hp,int attack,int armor,string emblem) {displayName=name;this.hp=hp;this.attack=attack;this.armor=armor;this.emblem=emblem;}
        }
        [SerializeField] int playerHp=30,betweenBattleHeal=6;
        [SerializeField] bool useFixedSeed;
        [SerializeField] int fixedSeed=7;
        [SerializeField] EnemyEntry[] enemies=Defaults();
        static EnemyEntry[] Defaults()=>new[]{new EnemyEntry("THE BROKEN DIE",18,2,3,": :"),new EnemyEntry("THE DEVIATION",24,3,4,"±"),new EnemyEntry("ROGUE SEVEN",30,4,5,"7")};
        public int NewSeed()=>useFixedSeed?fixedSeed:Environment.TickCount;
        public string Emblem(int stage)=>enemies[stage].emblem;
        public BattleSession CreateSession(){var defs=new EnemyDefinition[enemies.Length];for(int i=0;i<defs.Length;i++)defs[i]=new EnemyDefinition(enemies[i].displayName,enemies[i].hp,enemies[i].attack,enemies[i].armor);return new BattleSession(defs,playerHp,betweenBattleHeal);}
        public void ApplyCardDefaults(){playerHp=30;betweenBattleHeal=6;enemies=Defaults();}
    }
}
