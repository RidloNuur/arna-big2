using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Arna.Runtime;

namespace PlayingCard
{
    public class CardUI : MonoBehaviour
    {
        public Image outline;
        public Image preview;

        private CapsaPlayer _player;

        public Card Card { get; private set; }
        public bool IsRevealed { get; private set; }
        public bool IsSelected { get; private set; }

        private void Start()
        {
            _player = GetComponentInParent<CapsaPlayer>();
        }

        public void SetInteractable(bool value)
        {
            var button = GetComponent<Button>();
            button.onClick.RemoveAllListeners();
            if (value)
                button.onClick.AddListener(OnCardClicked);
        }

        public void Initialize(Card cd)
        {
            Card = cd;
            IsSelected = false;
            SetOutline(false);
            SetReveal(false);
        }

        public void OnCardClicked()
        {
            //Debug.Log(string.Format("Card clicked, type: {0}, index: {1}", Card.cardSuit, Card.cardIndex));
            IsSelected = !IsSelected;
            if (IsSelected)
                _player.AddSelected(this);
            else
                _player.RemoveSelected(this);
            SetOutline(IsSelected);
        }

        public void SetOutline(bool value)
        {
            outline.enabled = value;
        }

        public void SetReveal(bool value)
        {
            IsRevealed = value;
            preview.sprite = IsRevealed ? Card.Preview : Card.BackPreview;
            RuntimeManager.PlaySound(AudioManager.CARD_FLIP);
        }
    }
}