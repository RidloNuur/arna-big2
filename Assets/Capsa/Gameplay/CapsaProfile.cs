using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PlayingCard
{
    [CreateAssetMenu(fileName = "New CapsaProfile", menuName = "Capsa/Profile")]
    public class CapsaProfile : ScriptableObject
    {
        public string title;

        [Header("Card Preview")]
        public Sprite[] spades;
        public Sprite[] hearts;
        public Sprite[] clubs;
        public Sprite[] diamonds;
        public Sprite backCard;

        public Sprite GetSprite(CardType type, int index)
        {
            return type switch
            {
                CardType.Diamond => diamonds[index],
                CardType.Club => clubs[index],
                CardType.Heart => hearts[index],
                CardType.Spade => spades[index],
                _ => backCard
            };
        }
    }
}