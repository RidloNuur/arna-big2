using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using Cysharp.Threading.Tasks;

namespace Arna.Runtime
{
    public class LoadingHelper : MonoBehaviour
    {
        public GameObject loadingPanel;
        public Image loadingBar;
        public TextMeshProUGUI loadingMessage;
        public bool trackSceneLoading;

        public bool ProgressToMessage { get; set; }

        private string _message;
        private LoadingProcess _process;
        private CanvasGroup _cg;
        private Coroutine _fading;

        private void Awake()
        {
            _cg = loadingPanel.GetComponent<CanvasGroup>();
            if(!_cg)
                _cg = loadingPanel.AddComponent<CanvasGroup>();
            SetVisible(false);
        }

        private void Start()
        {
            if(trackSceneLoading)
            {
                RuntimeManager.onSceneStartLoad += (sname) => Fade(true);
                RuntimeManager.onSceneLoading += OnLoadScene;
                RuntimeManager.onSceneEndLoad += (sname) => Fade(false);
            }
        }

        #region SCENE LOADING
        private void OnLoadScene(Scene scene, AsyncOperation async)
        {
            string key = "Loading scene: " + scene.name;
            if (LoadingManager.StartLoading(key, async))
            {
                Track(LoadingManager.GetLoading(key));
            }
        }
        #endregion

        public void Track(LoadingProcess process)
        {
            if(process == null)
            {
                SetVisible(false);
                return;
            }

            SetMessage(process.Message);
            UpdateProgress(0);
            SetVisible(true);
            _process = process;
            _process.onProgressNormalized += UpdateProgress;
            _process.onFinished += () =>
            {
                UpdateProgress(1f);
                ProgressToMessage = false;
                SetVisible(false);
            };
        }

        public void SetMessage(string message)
        {
            _message = message;
            loadingMessage.text = _message;
        }

        private void UpdateProgress(float p)
        {
            loadingBar.fillAmount = p;
            if (ProgressToMessage)
                loadingMessage.text = _message + " (" + _process.NormalizedProgress.ToString("P") + ")";
        }

        public void SetVisible(bool value)
        {
            if (_cg.interactable != value)
            {
                _cg.interactable = value;
                //if(_fading != null)
                //    StopCoroutine(_fading);
                Fade(value);
            }
        }

        private async void Fade(bool visible)
        {
            _cg.blocksRaycasts = visible;
            _cg.interactable = visible;
            float a = 0;
            float from = _cg.alpha;
            float to = visible ? 1f : 0f;
            while(a < 1f)
            {
                a += Time.deltaTime / .2f;
                _cg.alpha = Mathf.Lerp(from, to, a);
                await UniTask.WaitForEndOfFrame();
            }
        }
    }
}
