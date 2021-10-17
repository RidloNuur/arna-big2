using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Arna.Runtime;

namespace PlayingCard
{
    public class CapsaPlayerController : MonoBehaviour
    {
        [Header("UI Ref")]
        public Button playButton;
        public Button passButton;
        public Button sortButton;
        public CanvasGroup actionGroup;

        private Color _playColor;
        private Color _passColor;
        private CapsaPlayer _player;

        private void Start()
        {
            actionGroup.interactable = false;

            playButton.onClick.AddListener(HandlePlay);
            passButton.onClick.AddListener(HandlePass);
            sortButton.onClick.AddListener(HandleSort);

            _playColor = playButton.image.color;
            _passColor = passButton.image.color;
            SetPlayButton(false);
            SetPassButton(false);

            CapsaManager.instance.playerController = this;
        }

        public void Initialize(CapsaPlayer pl)
        {
            _player = pl;
            _player.onPairChanged += HandlePairChange;
            _player.onTurnStateChanged += HandleTurnStateChange;

            _player.RevealAll();
            _player.SetCardInteraction(true);
            _player.SetPlayerName(_player.Character.name);
            actionGroup.interactable = true;
        }

        private void HandlePairChange(Pair pair)
        {
            Debug.Log("Pair changed, type: " + pair.type);
            RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
            SetPlayButton(_player.CanPlay);
        }

        private void HandleTurnStateChange(CapsaPlayer.TurnState state)
        {
            switch (state)
            {
                case CapsaPlayer.TurnState.Wait:
                    SetPlayButton(false);
                    SetPassButton(false);
                    break;
                case CapsaPlayer.TurnState.Play:
                    SetPlayButton(_player.CanPlay);
                    SetPassButton(true);
                    break;
                case CapsaPlayer.TurnState.Stop:
                    SetPlayButton(false);
                    SetPassButton(false);
                    actionGroup.interactable = false;
                    break;
            }
        }

        private void HandlePlay()
        {
            RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
            _player.PlayCard();
        }

        private void HandlePass()
        {
            RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
            _player.PassTurn();
        }

        private void HandleSort()
        {
            RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
            _player.ToggleSort();
        }

        private void SetPlayButton(bool value)
        {
            playButton.image.color = value ? _playColor : Color.gray;
            playButton.interactable = value;
        }

        private void SetPassButton(bool value)
        {
            passButton.image.color = value ? _passColor : Color.gray;
            passButton.interactable = value;
        }
    }
}
