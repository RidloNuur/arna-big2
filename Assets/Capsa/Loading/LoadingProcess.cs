using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

namespace Arna.Runtime
{
    public class LoadingProcess
    {
        #region PRIVATE VARIABLES
        private float _progress;
        private string _message;
        #endregion

        #region AUTO PROPERTIES
        public string Id { get; private set; }
        public string Error { get; private set; }

        public long Size { get; private set; }
        public float ElapsedTime { get; private set; }

        public bool IsRunning { get; private set; }
        public bool IsFinished { get; private set; }

        public AsyncOperation asyncOperation { get; private set; }
        #endregion

        #region PROPERTIES
        public float Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                onProgress?.Invoke(_progress);
                onProgressNormalized?.Invoke(NormalizedProgress);
                if (asyncOperation == null && NormalizedProgress >= 1)
                    Finish();
            }
        }

        public float NormalizedProgress
        {
            get
            {
                return Progress / Size;
            }
        }

        public string Message
        {
            get => _message;
            set
            {
                _message = value;
                onMessageChanged?.Invoke(_message);
            }
        }
        #endregion

        #region EVENTS
        public Action<float> onProgress;
        public Action<float> onProgressNormalized;
        public Action<string> onMessageChanged;
        public Action onFinished;
        #endregion

        #region PROCESS
        private async void Processing()
        {
#if UNITY_EDITOR
            string debugKey = "Loading: " + Id;
#endif
            IsRunning = true;
            if (asyncOperation == null)
            {
                Finish();
            }
            else
            {
                ElapsedTime = 0f;
                Progress = 0f;

                while (!asyncOperation.isDone)
                {
                    await UniTask.Delay(LoadingManager.RefreshRate);
                    Progress = asyncOperation.progress;
                    ElapsedTime += LoadingManager.RefreshRate;
                }
                Finish();
            }
        }

        public async void Finish()
        {
            if (!IsFinished)
            {
                IsRunning = false;
                IsFinished = true;
                Debug.Log("Finished loading id: " + Id + ", ETA: " + ElapsedTime);
                await UniTask.Yield();
                onFinished?.Invoke();
                await UniTask.Delay(200);
                LoadingManager.EndLoading(Id);
            }
        }
        #endregion

        #region CONSTRUCTOR
        public LoadingProcess(string key, long sz, string msg = null)
        {
            Id = key;
            Size = sz;
            Progress = 0f;
            Message = null;
        }

        public LoadingProcess(string key, AsyncOperation async, string msg = null)
        {
            Id = key;
            Size = long.MaxValue;
            Message = null;
            asyncOperation = async;
            Processing();
        }
        #endregion
    }
}