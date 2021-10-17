using System.Collections;
using UnityEngine.UI;
using UnityEngine;

public class CanvasRatioFitter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        var fitter = GetComponentInChildren<AspectRatioFitter>();
        float screenRatio = Screen.width / (float)Screen.height;
        GetComponent<CanvasScaler>().matchWidthOrHeight = screenRatio < fitter.aspectRatio ? 0f : 1f;
        Destroy(this);
    }
}
