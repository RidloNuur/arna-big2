using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Arna.Runtime;
using Cysharp.Threading.Tasks;
using TMPro;

namespace PlayingCard
{
    public class CapsaManager : MonoBehaviour
    {
        public static CapsaManager instance;

        [Header("Profiles")]
        public CapsaProfile capsaProfile;
        public CharacterProfile characterProfile;

        [Header("Players")]
        public CapsaPlayerController playerController;
        public CapsaPlayer[] players;

        [Header("EndGame UI")]
        public Transform endPanel;
        public TextMeshProUGUI endPlayerText;
        public Button endReplayButton;
        public Button endLeaveButton;

        [Header("Pause UI")]
        public Transform pausePanel;
        public Button pauseAudioButton;
        public Button pauseResumeButton;
        public Button pauseLeaveButton;
        private SpriteSwapToggle pauseAudioToggle;

        public const string PLAYER_SELECTED_CHARACTER = "Player Sected Character";
        private const string LOBBY_SCENE = "Lobby";
        private int _currentPlayerIndex;
        private int _lastPlayedPlayerIndex;
        private List<CapsaAI> _bots;

        public Pair CurrentHighestPair { get; private set; }

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            _bots = new List<CapsaAI>();

            #region BUTTON INIT
            endReplayButton.onClick.AddListener(InitializeGame);
            endLeaveButton.onClick.AddListener(LeaveGame);

            pauseAudioToggle = pauseAudioButton.GetComponentInChildren<SpriteSwapToggle>();
            pauseAudioToggle.SetToggle(!AudioManager.IsMuted);

            pauseAudioButton.onClick.AddListener(() =>
            {
                RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
                pauseAudioToggle.SetToggle(RuntimeManager.ToggleAudio());
            });
            pauseResumeButton.onClick.AddListener(() =>
            {
                GameState.ChangeState(GameState.State.GAMEPLAY);
                RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
                pausePanel.ScaleZero(.2f);
            });
            pauseLeaveButton.onClick.AddListener(LeaveGame);
            #endregion

            InitializeGame();
        }

        private void Update()
        {
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                if (GameState.CurrentState == GameState.State.PAUSED
                    || GameState.CurrentState == GameState.State.GAMEEND)
                    return;

                GameState.ChangeState(GameState.State.PAUSED);
                RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
                pausePanel.ScaleOne(.2f);
            }
        }

        public async void InitializeGame()
        {
            RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
            GameState.ChangeState(GameState.State.GAMEPLAY);
            endPanel.ScaleZero(.2f);
            _bots.Clear();
            Deck.Initialize();
            await UniTask.Yield();

            var hands = new List<UniTask>();
            for (int i = 0; i < 4; i++)
            {
                Card[] cards = new Card[13];
                for (int j = 0; j < 13; j++)
                {
                    cards[j] = Deck.Cards[i * 13 + j];
                }

                int chId = i == 0 ? PlayerPrefs.GetInt(PLAYER_SELECTED_CHARACTER) : Random.Range(0, 6);
                hands.Add(players[i].Initialize(i, cards, chId));
                if (i != 0)
                    _bots.Add(new CapsaAI(players[i]));
            }

            await UniTask.WhenAll(hands);
            playerController.Initialize(players[0]);
            CurrentHighestPair = new Pair();
            Pair.FirstTurn = true;

            await UniTask.Yield();
            for(int i = 0; i < 4; i++)
            {
                players[i].SetTurnState(players[i].Cards.Any(e => e.cardSuit == 0 && e.cardIndex == 0)
                    ? CapsaPlayer.TurnState.Play
                    : CapsaPlayer.TurnState.Wait);
                if (players[i].CurrentTurnState == CapsaPlayer.TurnState.Play)
                    _currentPlayerIndex = i;
            }
        }

        public void SetHighestPair(Pair pair)
        {
            _lastPlayedPlayerIndex = _currentPlayerIndex;
            CurrentHighestPair = pair;
        }

        public async void NextPlayer()
        {
            players[_currentPlayerIndex].SetTurnState(CapsaPlayer.TurnState.Wait);
            _currentPlayerIndex++;
            if (_currentPlayerIndex > 3)
                _currentPlayerIndex = 0;

            if(_lastPlayedPlayerIndex == _currentPlayerIndex)
            {
                CurrentHighestPair = new Pair();
                var discarding = new List<UniTask>();
                foreach(var e in players)
                {
                    discarding.Add(e.DiscardCard());
                }
                await UniTask.WhenAll(discarding);
            }

            players[_currentPlayerIndex].SetTurnState(CapsaPlayer.TurnState.Play);
        }

        public void EndGame(int playerId)
        {
            for(int i = 0; i < players.Length; i++)
            {
                players[i].SetTurnState(CapsaPlayer.TurnState.Stop);
                players[i].SetCharacterState(i == playerId ? CharacterState.Winning : CharacterState.Losing);
            }

            endPlayerText.text = string.Format("Player#{0} WIN!!", playerId + 1);
            endPanel.ScaleOne(.2f);
            GameState.ChangeState(GameState.State.GAMEEND);
        }

        private void LeaveGame()
        {
            endPanel.ScaleZero(.2f);
            pausePanel.ScaleZero(.2f);
            RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
            RuntimeManager.LoadScene(LOBBY_SCENE, LoadSceneMode.Single);
        }
    }
}