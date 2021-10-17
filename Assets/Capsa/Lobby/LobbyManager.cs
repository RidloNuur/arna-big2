using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using PlayingCard;
using Arna.Runtime;

public class LobbyManager : MonoBehaviour
{
    public Transform[] panels;

    [Header("Buttons")]
    public Button selectorButton;
    public Button infoButton;
    public Button quitButton;
    public Button audioButton;
    public Button playButton;

    private int _currentPanel = -1;
    private List<int> _panelHistory = new List<int>();
    private SpriteSwapToggle audioToggle;

    private const float TWEEN_DURATION = .1f;

    private void Start()
    {
        GameState.ChangeState(GameState.State.LOBBY);
        foreach (var e in panels)
            e.ScaleZero(0);
        SetPanel(0);

        selectorButton.onClick.AddListener(Selector);
        playButton.onClick.AddListener(Play);
        infoButton.onClick.AddListener(ShowInfo);
        quitButton.onClick.AddListener(Quit);
        audioButton.onClick.AddListener(ToggleAudio);
        audioToggle = audioButton.GetComponent<SpriteSwapToggle>();
        audioToggle.SetToggle(!AudioManager.IsMuted);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            PreviousPanel();
    }

    public void SetPanel(int id)
    {
        RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
        if (_currentPanel == id)
            return;
        
        if(_currentPanel >= 0)
            panels[_currentPanel].ScaleZero(TWEEN_DURATION);

        panels[id].ScaleOne(TWEEN_DURATION);
        _currentPanel = id;
        _panelHistory.Add(id);
    }

    public void PreviousPanel()
    {
        RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
        if (_panelHistory.Count <= 1)
        {
            Quit();
            return;
        }

        int goIndex = _panelHistory.Count - 2;
        panels[_currentPanel].ScaleZero(TWEEN_DURATION);
        panels[goIndex].ScaleOne(TWEEN_DURATION);
        _currentPanel = goIndex;
        _panelHistory.RemoveAt(goIndex + 1);
    }

    private void ToggleAudio()
    {
        RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
        audioToggle.SetToggle(RuntimeManager.ToggleAudio());
    }

    private void ShowInfo()
    {
        SetPanel(1);
    }

    private void Selector()
    {
        SetPanel(2);
    }

    private void Play()
    {
        RuntimeManager.PlaySound(AudioManager.BUTTON_CLICK);
        RuntimeManager.LoadScene("Gameplay", LoadSceneMode.Single);
    }

    private void Quit()
    {
        RuntimeManager.ShowModal(0, "QUIT"
            , "Are you sure you want to close the game?"
            , yesClick: () => Application.Quit());
    }
}
