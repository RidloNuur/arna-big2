using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

namespace PlayingCard
{
    public class CapsaPlayer : MonoBehaviour
    {
        public enum TurnState
        {
            Wait = 0,
            Play = 1,
            Stop = 2,
        }

        [Header("Identity UI")]
        public TextMeshProUGUI playerNameText;
        public Image playerAvatar;
        public Image playerAvatarFrame;
        public Image playerTurnLineIndicator;

        private Color _frameTurnColor;
        private Color _frameWaitColor;

        public CharacterData Character { get; private set; }
        public int PlayerId { get; private set; }
        public string PlayerName { get; private set; }

        [Header("Card Layout")]
        public int layoutThreshold;
        public float layoutMinSpacing;
        public float layoutMaxSpacing;
        public Transform handCardRoot;
        public Transform playedCardRoot;
        public Transform discardedCardRoot;

        public Action<Pair> onPairChanged;
        public Action<TurnState> onTurnStateChanged;
        public Action<CharacterState> onCharacterStateChanged;

        public int Score { get; private set; }
        public bool CanPlay => CurrentTurnState == TurnState.Play && _currentPair > _cm.CurrentHighestPair;
        public TurnState CurrentTurnState { get; private set; }
        public CharacterState CurrentCharacterState { get; private set; }
        public List<Card> Cards { get; private set; }
        public List<CardUI> Hand => _handCard;

        private HorizontalOrVerticalLayoutGroup _handLayout;
        private List<CardUI> _cardUIs;

        private List<CardUI> _selectionStack;
        private List<CardUI> _handCard;
        private List<CardUI> _playedCard;
        private List<CardUI> _discardedCard;

        private bool _suitSort;
        private bool _sorting;

        private CapsaManager _cm;
        private Pair _currentPair;

        private void Start()
        {
            _frameTurnColor = new Color(.65f, .6f, .55f);
            _frameWaitColor = new Color(.2f, .2f, .2f);

            _handLayout = handCardRoot.GetComponentInChildren<HorizontalLayoutGroup>();

            Cards = new List<Card>();
            _cardUIs = new List<CardUI>();
            _cardUIs.AddRange(GetComponentsInChildren<CardUI>(true));

            _selectionStack = new List<CardUI>();
            _handCard = new List<CardUI>();
            _playedCard = new List<CardUI>();
            _discardedCard = new List<CardUI>();

            _cm = CapsaManager.instance;
        }

        #region INIT
        public async UniTask Initialize(int id, Card[] crds, int charId)
        {
            PlayerId = id;
            SetCharacter(charId);
            ResetState();
            await DiscardAll();

            Cards.Clear();
            Cards.AddRange(crds);
            var hand = new List<UniTask>();
            for (int i = 0; i < 13; i++)
            {
                hand.Add(_cardUIs[i].transform.SetParentOne(_handLayout.transform)
                    .ContinueWith(() => CheckHandLayout()));
                _cardUIs[i].Initialize(Cards[i]);
                _handCard.Add(_cardUIs[i]);
                await UniTask.Delay(50);
            }
            await UniTask.WhenAll(hand);
        }

        public void SetCharacter(int id)
        {
            Debug.Log("Set character id: " + id);
            Character = _cm.characterProfile.Get(id);
            SetPlayerName(Character.name);
            playerAvatar.sprite = Character.GetAvatar(CharacterState.Neutral);
        }

        public void SetPlayerName(string pName)
        {
            PlayerName = pName;
            playerNameText.text = PlayerName;
        }
        #endregion

        #region STATES
        private void ResetState()
        {
            onPairChanged = null;
            onTurnStateChanged = null;
            onCharacterStateChanged = null;

            onTurnStateChanged += CheckTurnState;
            onCharacterStateChanged += CheckAvatar;

            _selectionStack.Clear();
            _handCard.Clear();
            _playedCard.Clear();
            _discardedCard.Clear();
            _handCard.AddRange(_cardUIs);

            SetTurnState(TurnState.Stop);
            SetCharacterState(CharacterState.Neutral);
        }

        public void SetTurnState(TurnState state)
        {
            if (CurrentTurnState == state)
                Debug.LogWarning(PlayerName + " transitioning to the same turn state: " + state);
            CurrentTurnState = state;
            onTurnStateChanged?.Invoke(state);
        }

        public void SetCharacterState(CharacterState state)
        {
            if (CurrentCharacterState == state)
                Debug.LogWarning(PlayerName + " transitioning to the same character state: " + state);
            CurrentCharacterState = state;
            onCharacterStateChanged?.Invoke(state);
        }
        #endregion

        #region ACTIONS
        public async void PlayCard()
        {
            if (_currentPair < _cm.CurrentHighestPair)
            {
                Debug.LogError("Pair value invalid.");
                return;
            }

            if (Pair.FirstTurn)
                Pair.FirstTurn = false;

            await DiscardCard();
            Debug.Log(string.Format("Player#{0} played: {1}", PlayerId, _currentPair.type));

            var played = new List<UniTask>();
            foreach (var e in _selectionStack)
            {
                _playedCard.Add(e);
                _handCard.Remove(e);
                played.Add(e.transform.SetParentOne(playedCardRoot)
                    .ContinueWith(() =>
                    {
                        e.SetReveal(true);
                        CheckHandLayout();
                        LayoutRebuilder.MarkLayoutForRebuild((RectTransform)playedCardRoot.transform);
                    }));
                e.SetOutline(false);

                await UniTask.Delay(20);
            }

            _selectionStack.Clear();
            _cm.SetHighestPair(_currentPair);
            _currentPair = new Pair();
            await UniTask.WhenAll(played);

            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)playedCardRoot.transform);
            CheckHandLayout();
            if (_handCard.Count == 0)
                _cm.EndGame(PlayerId);
            else
                _cm.NextPlayer();
        }

        public async UniTask DiscardCard()
        {
            var discard = new List<UniTask>();
            foreach(var e in _playedCard)
            {
                _discardedCard.Add(e);
                discard.Add(e.transform.SetParentZero(discardedCardRoot)
                    .ContinueWith(() => CheckHandLayout()));
                await UniTask.Delay(20);
            }
            _playedCard.Clear();
            await UniTask.WhenAll(discard);
            CheckHandLayout();
        }

        public async UniTask DiscardAll()
        {
            _playedCard.AddRange(_handCard);
            _handCard.Clear();
            await DiscardCard();
        }

        public async void RevealAll()
        {
            foreach (var e in _handCard)
            {
                e.SetReveal(true);
                await UniTask.Delay(50);
            }
        }

        public void PassTurn()
        {
            Debug.Log("Player #" + PlayerId + " skipped turn.");
            _cm.NextPlayer();
        }
        #endregion

        #region OPERATION
        public void AddSelected(CardUI card)
        {
            _selectionStack.Add(card);
            _currentPair = _selectionStack.ToPair();
            onPairChanged?.Invoke(_currentPair);
        }

        public void RemoveSelected(CardUI card)
        {
            _selectionStack.Remove(card);
            _currentPair = _selectionStack.ToPair();
            onPairChanged?.Invoke(_currentPair);
        }

        public void SetSelections(List<CardUI> cards)
        {
            _selectionStack.Clear();
            _selectionStack.AddRange(cards);
            _currentPair = _selectionStack.ToPair();
        }

        public async void ToggleSort()
        {
            if (_sorting)
                return;

            _sorting = true;
            _suitSort = !_suitSort;
            if (_suitSort)
                _handCard = _handCard.OrderBy(e => e.Card.cardSuit).ThenBy(e => e.Card.cardIndex).ToList();
            else
                _handCard = _handCard.OrderBy(e => e.Card.cardIndex).ThenBy(e => e.Card.cardSuit).ToList();

            for(int i = 0; i < _handCard.Count; i++)
            {
                _handCard[i].transform.SetSiblingIndex(i);
                await UniTask.Yield();
                LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_handLayout.transform);
                await UniTask.Delay(20);
            }
            _sorting = false;
        }

        public void SetCardInteraction(bool value)
        {
            foreach (var e in _cardUIs)
                e.SetInteractable(value);
        }
        #endregion

        #region RESPONSE
        private async void CheckHandLayout()
        {
            float spacing = Mathf.Lerp(layoutMinSpacing, layoutMaxSpacing
                    , (13f - _handCard.Count) / (13f - layoutThreshold));
            _handLayout.spacing = spacing;

            await UniTask.Yield();
            LayoutRebuilder.MarkLayoutForRebuild((RectTransform)_handLayout.transform);
        }

        private void CheckTurnState(TurnState state)
        {
            var hasTurn = state == TurnState.Play;
            playerTurnLineIndicator.DOColor(hasTurn ? _frameTurnColor : _frameWaitColor, .2f);
            playerAvatarFrame.transform.DOScale(hasTurn ? Vector3.one * 1.5f : Vector3.one, .2f);
            playerAvatarFrame.DOColor(hasTurn ? _frameTurnColor : _frameWaitColor, .2f);
            if (hasTurn)
                Debug.Log("Player #" + PlayerId + "'s turn.");
        }

        private void CheckAvatar(CharacterState state)
        {
            playerAvatar.sprite = Character.GetAvatar(state);
        }
        #endregion
    }
}