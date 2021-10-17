using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using PlayingCard;

public static class ExtensionMethods
{
    public static void ScaleZero(this Transform transform, float duration)
    {
        transform.DOScale(Vector3.zero, duration).SetEase(Ease.OutQuad);
    }

    public static void ScaleOne(this Transform transform, float duration)
    {
        transform.DOScale(Vector3.one, duration).SetEase(Ease.InQuad);
    }

    public static async UniTask SetParentZero(this Transform transform, Transform parent)
    {
        transform.SetParent(parent);
        transform.DOLocalMove(Vector3.zero, .2f).SetEase(Ease.OutQuad);
        transform.ScaleZero(.2f);
        await UniTask.Delay(200);
        transform.gameObject.SetActive(false);
    }

    public static async UniTask SetParentOne(this Transform transform, Transform parent)
    {
        transform.gameObject.SetActive(true);
        transform.SetParent(parent);
        transform.DOLocalMove(Vector3.zero, .2f).SetEase(Ease.InQuad);
        transform.ScaleOne(.2f);
        await UniTask.Delay(200);
    }

    public static void Nudge(this Button button, Vector3 position, Vector3 rotation, Vector3 scale)
    {
        button.transform.DOPunchPosition(position, .2f, 2, .2f);
        button.transform.DOPunchRotation(rotation, .2f, 2, .2f);
        button.transform.DOPunchScale(scale, .2f, 2, .2f);
    }

    public static Pair ToPair(this List<CardUI> cards)
    {
        return new Pair(cards.Select(e => e.Card).ToArray());
    }
}