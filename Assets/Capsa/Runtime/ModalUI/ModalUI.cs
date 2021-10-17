using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public abstract class ModalUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button yesButton;
    public Button noButton;

    private const string YES_DEFAULT_TEXT = "YES";
    private const string NO_DEFAULT_TEXT = "NO";
    private const float TWEEN_DURATION = .1f;

    private void Start()
    {
        if (transform.localScale.magnitude > 0)
            transform.DOScale(Vector3.zero, 0);
    }

    protected void ResetState()
    {
        transform.ScaleOne(TWEEN_DURATION);
        yesButton.GetComponentInChildren<TextMeshProUGUI>().text = YES_DEFAULT_TEXT;
        yesButton.onClick.RemoveAllListeners();
        yesButton.onClick.AddListener(() => transform.ScaleZero(TWEEN_DURATION));

        noButton.GetComponentInChildren<TextMeshProUGUI>().text = NO_DEFAULT_TEXT;
        noButton.onClick.RemoveAllListeners();
        noButton.onClick.AddListener(() => transform.ScaleZero(TWEEN_DURATION));
    }

    public abstract void Init(string title, string msg
        , string yesText = null, string noText = null
        , Action yesClicked = null, Action noClicked = null);
}