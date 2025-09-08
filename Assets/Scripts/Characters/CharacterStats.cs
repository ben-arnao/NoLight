using UnityEngine;

namespace RogueLike2D.Characters
{
    [System.Serializable]
    public class Resistances
    {
        [Range(0, 1f)] public float Physical;
        [Range(0, 1f)] public float Magic;
        [Range(0, 1f)] public float Poison;
        [Range(0, 1f)] public float Bleed;
        [Range(0, 1f)] public float Stun;
    }

    [System.Serializable]
    public class CharacterStats
    {
        public int MaxHP = 100;
        public int CurrentHP = 100;
        public int Attack = 10;
        public int Defense = 5;
        public int Speed = 10;
        public Resistances Resist = new Resistances();

        public CharacterStats Clone()
        {
            return new CharacterStats
            {
                MaxHP = MaxHP,
                CurrentHP = CurrentHP,
                Attack = Attack,
                Defense = Defense,
                Speed = Speed,
                Resist = new Resistances
                {
                    Physical = Resist.Physical,
                    Magic = Resist.Magic,
                    Poison = Resist.Poison,
                    Bleed = Resist.Bleed,
                    Stun = Resist.Stun
                }
            };
        }
    }
}
