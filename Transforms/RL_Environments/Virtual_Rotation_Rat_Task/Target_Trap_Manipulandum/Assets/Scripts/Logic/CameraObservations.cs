

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
        //if (screenResNotUpdated)
        //{
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
       //     screenResNotUpdated = false; // The screen res is allowed to be set only once
        //}
        
    }
    
    private void Start()
    {
        EventManager.Instance.onNewScreenResolution.AddListener(RecalculateScreenRes);

        EventManager.Instance.onNeedingNewObservation.AddListener(PrepareNewObservation);

        PrepareNewObservation();
    }

    private void PrepareNewObservation()
    {
        //Debug.Log("2. Received Observation Required Message");
        observationCamera.Render();
        CaptureScreenShot();
    }

    public void CaptureScreenShot()
    {
        //Debug.Log("3. Starting Capturing Screen shot");
        var rt = default(RenderTexture);

        while (rt == default(RenderTexture))
            rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 0);

        format = rt.graphicsFormat;

        ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);
        //Debug.Log("4. Requesting GPU Readback");
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
            //Debug.Log("5. Turning GPU readback into bytes array");
            byte[]  array = request.GetData<byte>().ToArray();

            byte[] pngBytes = ImageConversion.EncodeArrayToPNG(array, format, (uint)Screen.width, (uint)Screen.height);
            //Debug.Log("6. Sending Observation Ready Message");
            EventManager.Instance.onObservationReady.Invoke(pngBytes);
             
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

    }

}