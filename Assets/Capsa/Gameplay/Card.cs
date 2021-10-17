using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace PlayingCard
{
    public enum PairType
    {
        Invalid = 0,
        Single = 1,
        Pair = 2,
        ToaK = 3,
        Straight = 4,
        Flush = 5,
        FullHouse = 6,
        FoaK = 7,
        StraighFlush = 8
    }
    public struct Pair
    {
        public static bool FirstTurn;

        public PairType type;

        public int Length => _cards.Length;
        public Card[] Cards => _cards;

        private int _suit;
        private int _index;
        private Card[] _cards;

        public Pair(Card[] cards)
        {
            _cards = cards;
            _suit = 0;
            _index = 0;
            type = PairType.Invalid;
            DefinePair();
        }

        private void DefinePair()
        {
            if (_cards.Length < 1 || _cards.Length > 5)
                return;

            if (FirstTurn && !ValidFirstTurn)
                return;

            OrderByIndex();
            switch (_cards.Length)
            {
                case 1:
                    _suit = (int)_cards[0].cardSuit;
                    _index = _cards[0].cardIndex;
                    type = PairType.Single;
                    break;
                case 2:
                    if (_cards[0].cardIndex == _cards[1].cardIndex)
                    {
                        _suit = (int)_cards[1].cardSuit;
                        _index = _cards[1].cardIndex;
                        type = PairType.Pair;
                    }
                    break;
                case 3:
                    var cIndex = _cards[0].cardIndex;
                    if (_cards.Any(e => e.cardIndex != cIndex))
                        break;

                    _suit = (int)_cards[2].cardSuit;
                    _index = _cards[2].cardIndex;
                    type = PairType.ToaK;
                    break;
                case 5:
                    if (CheckStraightFlush())
                        break;
                    else if (CheckFoaK())
                        break;
                    else if (CheckFullHouse())
                        break;
                    else if (CheckFlush())
                        break;
                    else
                        CheckStraight();
                    break;
                default:
                    break;
            }

            Debug.Log("Pair type: " + type);
        }

        private bool CheckStraight()
        {
            if (FirstTurn && !ValidFirstTurn)
                return false;

            OrderByIndex();
            for (int i = 0; i < _cards.Length - 1; i++)
            {
                if (_cards[i + 1].cardIndex - _cards[i].cardIndex > 1)
                    return false;
            }

            _suit = (int)_cards[_cards.Length - 1].cardSuit;
            _index = _cards[_cards.Length - 1].cardIndex;
            type = PairType.Straight;
            return true;
        }

        private bool CheckFlush()
        {
            if (FirstTurn && !ValidFirstTurn)
                return false;

            OrderBySuit();
            var cSuit = _cards[0].cardSuit;
            if (_cards.Any(e => e.cardSuit != cSuit))
                return false;

            _suit = (int)_cards[_cards.Length - 1].cardSuit;
            _index = _cards[_cards.Length - 1].cardIndex;
            type = PairType.Flush;
            return true;
        }

        private bool CheckFullHouse()
        {
            if (FirstTurn && !ValidFirstTurn)
                return false;

            OrderByIndex();
            int changeCount = 0;
            int indexCount = 0;
            var indexSort = _cards.OrderBy(e => e.cardIndex);
            var cIndex = _cards[0].cardIndex;
            foreach (var e in _cards)
            {
                if (cIndex == e.cardIndex)
                {
                    indexCount++;
                    if (indexCount == 3)
                    {
                        _suit = (int)e.cardSuit;
                        _index = e.cardIndex;
                    }
                }
                else
                {
                    cIndex = e.cardIndex;
                    indexCount = 1;
                    changeCount++;
                }
            }

            if(changeCount < 2)
            {
                type = PairType.FullHouse;
                return true;
            }
            else
                return false;
        }

        private bool CheckFoaK()
        {
            if (FirstTurn && !ValidFirstTurn)
                return false;

            OrderByIndex();
            int equalCount = 0;
            var indexSort = _cards.OrderBy(e => e.cardIndex);
            var cIndex = _cards[0].cardIndex;
            foreach(var e in indexSort)
            {
                if (e.cardIndex == cIndex)
                {
                    equalCount++; 
                    if (equalCount == 4)
                    {
                        _suit = (int)e.cardSuit;
                        _index = e.cardIndex;
                        type = PairType.FoaK;
                        return true;
                    }
                }
                else 
                {
                    cIndex = e.cardIndex;
                    equalCount = 1;
                }
            }
            return false;
        }

        private bool CheckStraightFlush()
        {
            if (FirstTurn && !ValidFirstTurn)
                return false;

            if (CheckStraight() && CheckFlush())
            {
                _suit = (int)_cards[_cards.Length - 1].cardSuit;
                _index = _cards[_cards.Length - 1].cardIndex;
                type = PairType.StraighFlush;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool ValidFirstTurn => _cards.Any(e => e.cardSuit == 0 && e.cardIndex == 0);

        public static bool operator >(Pair a, Pair b)
        {
            Debug.Log("Compare type, a: " + a.type + ", b: " + b.type
                + "\r\nCompare suit, a: " + a._suit + ", b: " + b._suit
                + "\r\nCompare index, a: " + a._index + ", b: " + b._index);
            if (a.type == PairType.Invalid)
                return false;
            if (b.type == PairType.Invalid)
                return true;

            if ((int)b.type > 3 || a.type == b.type)
            {
                if (a.type > b.type)
                    return true;
                else if (a.type == b.type && a.type == PairType.Flush)
                {
                    if (a._suit > b._suit)
                        return true;
                    else if (a._suit == b._suit)
                        return a._index > b._index;
                    else
                        return false;
                }
                else if (a.type < b.type)
                    return false;

                if (a._index > b._index)
                    return true;
                else if (a._index == b._index)
                    return a._suit > b._suit;
                else
                    return false;
            }
            return false;
        }

        public static bool operator <(Pair a, Pair b)
        {
            if (a.type == PairType.Invalid)
                return true;

            if (b.type == PairType.Invalid)
                return false;

            if ((int)b.type > 3 || a.type == b.type)
            {
                if (a.type < b.type)
                    return true;
                else if (a.type == b.type && a.type == PairType.Flush)
                {
                    if (a._suit < b._suit)
                        return true;
                    else if (a._suit == b._suit)
                        return a._index < b._index;
                    else
                        return false;
                }
                else if (a.type > b.type)
                    return false;

                if (a._index < b._index)
                    return true;
                else if (a._index == b._index)
                    return a._suit < b._suit;
                else
                    return false;
            }
            return false;
        }

        private void OrderByIndex()
        {
            _cards = _cards.OrderBy(e => e.cardIndex).ThenBy(e => e.cardSuit).ToArray();
        }

        private void OrderBySuit()
        {
            _cards = _cards.OrderBy(e => e.cardSuit).ThenBy(e => e.cardIndex).ToArray();
        }
    }

    public enum CardType
    {
        Diamond = 0,
        Club = 1,
        Heart = 2,
        Spade = 3
    }

    public struct Card
    {
        [Range(0, 3)]
        public CardType cardSuit;

        [Range(0, 13)]
        public int cardIndex;

        public Card(int type, int index)
        {
            cardSuit = (CardType)type;
            cardIndex = index;
        }

        public Sprite Preview => CapsaManager.instance.capsaProfile.GetSprite(cardSuit, cardIndex);
        public Sprite BackPreview => CapsaManager.instance.capsaProfile.backCard;
    }

    public static class Deck
    {
        public static Card[] Cards { get; private set; }

        private static readonly Random rng = new Random();
        private const int DECK_SIZE = 52;

        public static void Initialize()
        {
            if(Cards == null)
            {
                Cards = new Card[DECK_SIZE];
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < 13; j++)
                    {
                        Cards[13 * i + j] = new Card(i, j);
                    }
                }
            }

            Shuffle();
        }

        public static void Shuffle()
        {
            Cards = Cards.OrderBy(a => rng.Next()).ToArray();
        }
    }
}