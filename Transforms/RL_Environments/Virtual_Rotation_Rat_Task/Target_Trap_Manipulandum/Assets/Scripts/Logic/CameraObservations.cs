

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
    private bool screenResNotUpdated = true;


    // Event onNewScreenResolution that triggers this is thrown by the RatController
    void RecalculateScreenRes(int width, int height)
    {
        if (screenResNotUpdated)
        {
            Screen.SetResolution(width, height, FullScreenMode.Windowed);
            screenResNotUpdated = false; // The screen res is allowed to be set only once
        }
        
    }
    
    IEnumerator Start()
    {
        EventManager.Instance.onNewScreenResolution.AddListener(RecalculateScreenRes);

        yield return new WaitForSeconds(1);
        while (screenResNotUpdated) // While the screen res is not set then the game does not return aby pixel observations
        {
            yield return new WaitForSeconds(0.001f);
        }

        while (true) // Now that the screen res is set the game will capture every frame and return it as an observation if asked
        {
            yield return new WaitForSeconds(0.001f);
            //yield return new WaitForEndOfFrame();

            var rt = default(RenderTexture);

            while(rt == default(RenderTexture))
                rt = RenderTexture.GetTemporary(Screen.width, Screen.height, 32);

            format = rt.graphicsFormat;
            
            ScreenCapture.CaptureScreenshotIntoRenderTexture(rt);

            AsyncGPUReadback.Request(rt, 0, TextureFormat.RGBA32, OnCompleteReadback);

            RenderTexture.ReleaseTemporary(rt);
        }
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