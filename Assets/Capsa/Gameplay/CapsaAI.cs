using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace PlayingCard
{
    public class CapsaAI
    {
        public CapsaAI(CapsaPlayer player)
        {
            _cm = CapsaManager.instance;
            _player = player;
            _player.onTurnStateChanged += OnStateChanged;
            _player.SetPlayerName(string.Format("{0} (Bot {1})", _player.Character.name, _player.PlayerId));
        }

        private CapsaManager _cm;
        private CapsaPlayer _player;

        private List<CardUI> _pairCandidate = new List<CardUI>();
        private Pair _pair;

        public void OnStateChanged(CapsaPlayer.TurnState state)
        {
            switch (state)
            {
                case CapsaPlayer.TurnState.Wait:
                    HandleWait();
                    break;
                case CapsaPlayer.TurnState.Play:
                    HandlePlay();
                    break;
                case CapsaPlayer.TurnState.Stop:
                    HandleStop();
                    break;
            }
        }

        private void HandleWait()
        {

        }

        private async void HandlePlay()
        {
            _pairCandidate.Clear();
            _pair = new Pair();
            await UniTask.Delay(1000);
            if (_cm.CurrentHighestPair.type == PairType.Invalid)
            {
                Debug.Log("No highest pair yet.");
                if((_player.Hand.Count >= 5 && FindFiveCombination())
                    || FindToaK() || FindPair() || FindSingle(SingleNextPlayerWinning()))
                    PlayCard();
                else
                    _player.PassTurn();
            }
            else
            {
                Debug.Log("Highest pair, type: " + _cm.CurrentHighestPair.type);
                int count = _cm.CurrentHighestPair.Length;
                if (count == 1)
                    FindSingle(SingleNextPlayerWinning());
                else if (count == 2)
                    FindPair();
                else if (count == 3)
                    FindToaK();
                else
                    FindFiveCombination();

                if (_pair.type != PairType.Invalid)
                    PlayCard();
                else
                    _player.PassTurn();
            }
        }

        private void HandleStop()
        {
            _player.RevealAll();
        }

        private void PlayCard()
        {
            Debug.Log("AI submitting selections, length: " + _pairCandidate.Count);
            foreach (var e in _pairCandidate)
                Debug.Log("suit: " + e.Card.cardSuit + ", index: " + e.Card.cardIndex);
            _player.SetSelections(_pairCandidate);
            _player.PlayCard();
        }

        #region FINDING PAIR
        private bool FindFiveCombination()
        {
            if (_player.Hand.Count < 5)
                return false;
            return FindStraightFlush() || FindFoaK() || FindFullHouse() || FindFlush() || FindStraight();
        }

        private bool FindSingle(bool reversed)
        {
            CardUI[] indexSort;
            if(reversed)
                indexSort = _player.Hand.OrderByDescending(e => e.Card.cardIndex).OrderByDescending(e => e.Card.cardSuit).ToArray();
            else
                indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToArray();

            for (int i = 0; i < indexSort.Length; i++)
            {
                _pairCandidate.Clear();
                _pairCandidate.Add(indexSort[i]);

                _pair = _pairCandidate.ToPair();
                if (_pair > _cm.CurrentHighestPair && (!SingleCritical(indexSort[i]) || reversed))
                {
                    if (i < (indexSort.Length - 1) && indexSort[i].Card.cardIndex == indexSort[i + 1].Card.cardIndex)
                        return Random.value > .5f;
                    else
                        return true;
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindPair()
        {
            if (_player.Hand.Count < 2)
                return false;

            var indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToArray();
            var cIndex = indexSort[0].Card.cardIndex;

            _pairCandidate.Clear();
            for (int i = 0; i < indexSort.Length; i++)
            {
                if (cIndex == indexSort[i].Card.cardIndex)
                {
                    _pairCandidate.Add(indexSort[i]);

                    if (_pairCandidate.Count < 2)
                        continue;

                    if (_pairCandidate.Count > 2)
                        _pairCandidate.RemoveAt(0);

                    _pair = _pairCandidate.ToPair();
                    if (_pair.type == PairType.Pair && _pair > _cm.CurrentHighestPair)
                        return true;
                }
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(indexSort[i]);
                    cIndex = indexSort[i].Card.cardIndex;
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindToaK()
        {
            if (_player.Hand.Count < 3)
                return false;

            var indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToArray();
            var cIndex = indexSort[0].Card.cardIndex;

            _pairCandidate.Clear();
            for (int i = 0; i < indexSort.Length; i++)
            {
                if (cIndex == indexSort[i].Card.cardIndex)
                {
                    _pairCandidate.Add(indexSort[i]);

                    if (_pairCandidate.Count < 3)
                        continue;

                    if (_pairCandidate.Count > 3)
                        _pairCandidate.RemoveAt(0);

                    _pair = _pairCandidate.ToPair();
                    if (_pair.type == PairType.ToaK && _pair > _cm.CurrentHighestPair)
                        return true;
                }
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(indexSort[i]);
                    cIndex = indexSort[i].Card.cardIndex;
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindStraight()
        {
            if (_player.Hand.Count < 5 || _cm.CurrentHighestPair.type > PairType.Straight)
                return false;

            var indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToArray();
            _pairCandidate.Clear();
            _pairCandidate.Add(indexSort[0]);
            foreach (var e in indexSort)
            {
                int count = _pairCandidate.Count;
                if (e.Card.cardIndex == _pairCandidate[count - 1].Card.cardIndex)
                    continue;
                else if (e.Card.cardIndex - _pairCandidate[count - 1].Card.cardIndex == 1)
                {
                    _pairCandidate.Add(e);
                    if (_pairCandidate.Count < 5)
                        continue;

                    if (_pairCandidate.Count > 5)
                        _pairCandidate.RemoveAt(0);

                    _pair = _pairCandidate.ToPair();
                    if (_pair.type == PairType.Straight && _pair > _cm.CurrentHighestPair)
                        return true;
                }
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(e);
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindFlush()
        {
            if (_player.Hand.Count < 5 || _cm.CurrentHighestPair.type > PairType.Flush)
                return false;

            var suitSort = _player.Hand.OrderBy(e => e.Card.cardSuit).ThenBy(e => e.Card.cardIndex).ToArray();
            var cSuit = suitSort[0].Card.cardSuit;
            _pairCandidate.Clear();
            foreach (var e in suitSort)
            {
                if(e.Card.cardSuit == cSuit)
                {
                    _pairCandidate.Add(e);

                    if (_pairCandidate.Count < 5)
                        continue;

                    if (_pairCandidate.Count > 5)
                        _pairCandidate.RemoveAt(0);

                    _pair = _pairCandidate.ToPair();
                    if (_pair.type == PairType.Flush && _pair > _cm.CurrentHighestPair)
                        return true;
                }
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(e);
                    cSuit = e.Card.cardSuit;
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindFullHouse()
        {
            if (_player.Hand.Count < 5 || _cm.CurrentHighestPair.type > PairType.FullHouse || AnyPair(-1) == null)
                return false;

            var indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToArray();
            var cIndex = indexSort[0].Card.cardIndex;

            _pairCandidate.Clear();
            for(int i = 0; i < indexSort.Length; i++)
            {
                if (cIndex == indexSort[i].Card.cardIndex)
                {
                    _pairCandidate.Add(indexSort[i]);

                    if (_pairCandidate.Count < 3)
                        continue;

                    if (_pairCandidate.Count > 3)
                        _pairCandidate.RemoveAt(0);

                    var cards = new List<CardUI>();
                    cards.AddRange(_pairCandidate);
                    var pair = AnyPair(cIndex);
                    if (pair == null)
                        return false;

                    cards.AddRange(pair);
                    _pair = cards.ToPair();
                    if (_pair.type == PairType.FullHouse && _pair > _cm.CurrentHighestPair)
                        return true;
                }
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(indexSort[i]);
                    cIndex = indexSort[i].Card.cardIndex;
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindFoaK()
        {
            if (_player.Hand.Count < 5 || _cm.CurrentHighestPair.type > PairType.FoaK)
                return false;

            var indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToArray();
            var cIndex = indexSort[0].Card.cardIndex;

            _pairCandidate.Clear();
            foreach(var e in indexSort)
            { 
                if (e.Card.cardIndex == cIndex)
                {
                    _pairCandidate.Add(e);

                    if (_pairCandidate.Count < 4)
                        continue;

                    _pairCandidate.Add(indexSort.First(e => e.Card.cardIndex != cIndex));
                    _pair = _pairCandidate.ToPair();
                    if (_pair > _cm.CurrentHighestPair)
                        return true;
                }
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(e);
                    cIndex = e.Card.cardIndex;
                }
            }
            _pair = new Pair();
            return false;
        }

        private bool FindStraightFlush()
        {
            if (_player.Hand.Count < 5)
                return false;

            var cards = _player.Hand.OrderBy(e => e.Card.cardSuit).ThenBy(e => e.Card.cardIndex).ToArray();
            var cSuit = cards[0].Card.cardSuit;

            _pairCandidate.Clear();
            foreach(var e in cards)
            {
                if(e.Card.cardSuit == cSuit)
                {
                    _pairCandidate.Add(e);

                    if (_pairCandidate.Count < 5)
                        continue;

                    if (_pairCandidate.Count > 5)
                        _pairCandidate.RemoveAt(0);

                    _pair = _pairCandidate.ToPair();
                    if(_pair.type == PairType.StraighFlush && _pair > _cm.CurrentHighestPair)
                        return true;
                }    
                else
                {
                    _pairCandidate.Clear();
                    _pairCandidate.Add(e);
                    cSuit = e.Card.cardSuit;
                }
            }
            _pair = new Pair();
            return false;
        }
        #endregion

        private List<CardUI> AnyPair(int exclude)
        {
            var indexSort = _player.Hand.OrderBy(e => e.Card.cardIndex);
            var pair = new List<CardUI>();
            foreach(var e in indexSort)
            {
                if (e.Card.cardIndex == exclude)
                    continue;

                if (pair.Count == 0)
                {
                    pair.Add(e);
                }
                else if(pair[0].Card.cardSuit == e.Card.cardSuit)
                {
                    pair.Add(e);
                    return pair;
                }
                else
                {
                    pair.Clear();
                    pair.Add(e);
                }
            }
            return null;
        }

        private bool SingleCritical(CardUI card)
        {
            if (!(_player.Hand.Count == 1 || _player.Hand.Count > 2 || card.Card.cardIndex < 13))
                return Random.value > .2f;

            return false;
        }

        private bool SingleNextPlayerWinning()
        {
            int nextPlayerId = _player.PlayerId == 3 ? 0 : _player.PlayerId + 1;
            int nextPlayerHandCount = _cm.players[nextPlayerId].Hand.Count;
            Debug.Log("Player#" + nextPlayerId + " hand count: " + nextPlayerHandCount);
            return nextPlayerHandCount == 1;
        }
    }
}