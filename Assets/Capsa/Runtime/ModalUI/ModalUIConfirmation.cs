using System;

public class ModalUIConfirmation : ModalUI
{
    public override void Init(string title, string msg
        , string yesText = null, string noText = null
        , Action yesClicked = null, Action noClicked = null)
    {
        titleText.text = title;
        messageText.text = msg;

        ResetState();
        yesButton.onClick.AddListener(() => yesClicked?.Invoke());
        noButton.onClick.AddListener(() => noClicked?.Invoke());
    }
}