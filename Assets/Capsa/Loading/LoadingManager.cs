using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace Arna.Runtime
{
    public static class LoadingManager
    {
        public static int RefreshRate = 100;

        private static Dictionary<string, LoadingProcess> _loadings;

        [RuntimeInitializeOnLoadMethod]
        private static void Prepare()
        {
            if (_loadings == null)
                _loadings = new Dictionary<string, LoadingProcess>();

            Debug.Log("Initialized Loading Manager");
        }

        #region STARTING
        public static bool StartLoading(string key, long size, string msg = null)
        {
            if (_loadings.ContainsKey(key))
            {
                Debug.LogWarning("There is a process with key: " + key);
                return false;
            }

            var process = new LoadingProcess(key, size);
            _loadings.Add(key, process);
            return true;
        }

        public static bool StartLoading(string key, AsyncOperation async, string msg = null)
        {
            if (_loadings.ContainsKey(key))
            {
                Debug.LogWarning("There is a process with key: " + key);
                return false;
            }

            var process = new LoadingProcess(key, async);
            _loadings.Add(key, process);
            return true;
        }
        #endregion

        public static bool EndLoading(string key)
        {
            if (_loadings.ContainsKey(key))
            {
                //_loadings[key].Dispose();
                _loadings.Remove(key);
                return true;
            }
            return false;
        }

        public static LoadingProcess GetLoading(string key)
        {
            if (_loadings.ContainsKey(key))
                return _loadings[key];

            return null;
        }
    }
}