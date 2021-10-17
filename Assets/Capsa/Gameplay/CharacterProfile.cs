using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard
{
    [CreateAssetMenu(fileName = "New CharacterProfile", menuName = "Capsa/Character")]
    public class CharacterProfile : ScriptableObject
    {
        [SerializeField]
        private CharacterData[] characters;

        public CharacterData Get(int id) => characters[id];
        public int Count => characters.Length;
    }

    public enum CharacterState
    {
        Neutral = 0,
        Winning = 1,
        Losing = 2
    }

    [System.Serializable]
    public struct CharacterData
    {
        public string name;
        public Sprite[] avatars;

        public Sprite GetAvatar(CharacterState state)
        {
            return avatars[(int)state];
        }
    }

}