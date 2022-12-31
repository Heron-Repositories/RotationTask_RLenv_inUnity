

using System;
using System.Collections;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Rendering;

public class CameraObservations : MonoBehaviour
{
    private GraphicsFormat format;
    public Camera observationCamera;


    // Event onNewScreenResolution that triggers this is thrown by the RatController
    void RecalculateScreenRes(int width, int height)
    {
        Screen.SetResolution(width, height, FullScreenMode.Windowed);
    }
    
    private void Start()
    {
        EventManager.Instance.onNewScreenResolution.AddListener(RecalculateScreenRes);

        EventManager.Instance.onNeedingNewObservation.AddListener(PrepareNewObservation);

        PrepareNewObservation();
    }

    private void PrepareNewObservation()
    {
        observationCamera.Render();
        CaptureScreenShot();
    }

    public void CaptureScreenShot()
    {
        var rt = default(RenderTexture);

        while (rt == default(RenderTexture))
            rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

        format = rt.graphicsFormat;

        ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);

        AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, OnCompleteReadback);

        RenderTexture.ReleaseTemporary(rt);
    }


    void OnCompleteReadback(AsyncGPUReadbackRequest request)
    {
        if (request.hasError)
        {
            Debug.Log("GPU readback error detected.");
            return;
        }

        try
        {
            byte[]  array = request.GetData<byte>().ToArray();

            byte[] pngBytes = ImageConversion.EncodeArrayToPNG(array, format, (uint)Screen.width, (uint)Screen.height);

            EventManager.Instance.onObservationReady.Invoke(pngBytes);
             
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

}