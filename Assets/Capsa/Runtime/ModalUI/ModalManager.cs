using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Arna.Runtime;

public class ModalManager : MonoBehaviour
{
    [SerializeField]
    private ModalUI[] modals;

    private void Start()
    {
        RuntimeManager.SetModalManager(this);
    }

    public ModalUI GetModal(int id)
    {
        return modals[id];
    }
}