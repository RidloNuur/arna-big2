using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using TMPro;

namespace PlayingCard
{
    public class CharacterSelector : MonoBehaviour
    {
        public CharacterProfile profile;
        public Image[] avatars;

        public Button nextButton;
        public Button previousButton;

        public float near;
        public float far;

        private int _currentIndex;
        private int[] _indexes;

        private Vector3 _nearScale;

        private void Start()
        {
            _currentIndex = PlayerPrefs.GetInt(CapsaManager.PLAYER_SELECTED_CHARACTER);
            _indexes = GetIndexes(true);
            _nearScale = Vector3.one * .8f;
            for (int i = 0; i < 4; i++)
            {
                avatars[i].GetComponentInChildren<TextMeshProUGUI>().text = profile.Get(_indexes[i]).name;
                avatars[i].sprite = profile.Get(_indexes[i]).GetAvatar(CharacterState.Neutral);
            }
            Debug.Log("Load character id: " + _currentIndex);

            nextButton.onClick.AddListener(MoveRight);
            previousButton.onClick.AddListener(MoveLeft);
        }

        private async void MoveRight()
        {
            _indexes = GetIndexes(true);
            _currentIndex--;
            if (_currentIndex < 0)
                _currentIndex = profile.Count - 1;

            Debug.Log("Set character id: " + _currentIndex);
            PlayerPrefs.SetInt(CapsaManager.PLAYER_SELECTED_CHARACTER, _currentIndex);
            SetButton(false);

            PrepareLastIndex(true);
            ((RectTransform)avatars[0].transform).DOAnchorPosX(0, .2f);
            ((RectTransform)avatars[1].transform).DOAnchorPosX(near, .2f);
            ((RectTransform)avatars[2].transform).DOAnchorPosX(far, .2f);
            ((RectTransform)avatars[3].transform).DOAnchorPosX(-near, .2f);

            avatars[0].transform.DOScale(Vector3.one, .2f);
            avatars[1].transform.DOScale(_nearScale, .2f);
            avatars[2].transform.DOScale(Vector3.zero, .2f);
            avatars[3].transform.DOScale(_nearScale, .2f);

            avatars[2].DOColor(Color.clear, .2f);
            avatars[3].DOColor(Color.white, .2f);

            await UniTask.Delay(200);
            Image[] temp = new Image[4] { avatars[3], avatars[0], avatars[1], avatars[2] };
            for (int i = 0; i < 4; i++)
                avatars[i] = temp[i];
            SetButton(true);
        }

        private async void MoveLeft()
        {
            _indexes = GetIndexes(false);
            _currentIndex++;
            if (_currentIndex >= profile.Count)
                _currentIndex = 0;

            Debug.Log("Set character id: " + _currentIndex);
            PlayerPrefs.SetInt(CapsaManager.PLAYER_SELECTED_CHARACTER, _currentIndex);
            SetButton(false);

            PrepareLastIndex(false);
            ((RectTransform)avatars[0].transform).DOAnchorPosX(-far, .2f);
            ((RectTransform)avatars[1].transform).DOAnchorPosX(-near, .2f);
            ((RectTransform)avatars[2].transform).DOAnchorPosX(0, .2f);
            ((RectTransform)avatars[3].transform).DOAnchorPosX(near, .2f);

            avatars[0].transform.DOScale(Vector3.zero, .2f);
            avatars[1].transform.DOScale(_nearScale, .2f);
            avatars[2].transform.DOScale(Vector3.one, .2f);
            avatars[3].transform.DOScale(_nearScale, .2f);

            avatars[0].DOColor(Color.clear, .2f);
            avatars[3].DOColor(Color.white, .2f);

            await UniTask.Delay(200);
            Image[] temp = new Image[4] { avatars[1], avatars[2], avatars[3], avatars[0] };
            for (int i = 0; i < 4; i++)
                avatars[i] = temp[i];
            SetButton(true);
        }

        private void PrepareLastIndex(bool moveRight)
        {
            avatars[3].sprite = profile.Get(_indexes[3]).GetAvatar(CharacterState.Neutral);
            avatars[3].GetComponentInChildren<TextMeshProUGUI>().text = profile.Get(_indexes[3]).name;
            avatars[3].transform.localPosition = new Vector3(moveRight ? -far : far, 0f, 0f);
        }

        private int[] GetIndexes(bool moveRight)
        {
            var ids = new int[4];
            for(int i = 0; i < 3; i++)
            {
                CheckIndex(_currentIndex + (i - 1), out ids[i]);
                Debug.Log("Index " + i + ": " + ids[i]);
            }
            CheckIndex(moveRight ? ids[0] - 1 : ids[2] + 1, out ids[3]);
            Debug.Log("Index " + 3 + ": " + ids[3]);

            return ids;
        }

        private void CheckIndex(int index, out int id)
        {
            if (index < 0)
                id = profile.Count - 1;
            else if (index >= profile.Count)
                id = index - profile.Count;
            else
                id = index;
        }

        private void SetButton(bool value)
        {
            nextButton.interactable = value;
            previousButton.interactable = value;
        }
    }
}