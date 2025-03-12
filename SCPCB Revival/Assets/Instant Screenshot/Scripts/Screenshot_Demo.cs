using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SaadKhawaja.InstantScreenshot;

public class Screenshot_Demo : MonoBehaviour
{
    public void TakeScreenshotExample()
    {
        InstantScreenshot.TakeScreenshot(Application.dataPath, "Test.png", isTransparent: true);

        //Take screenshot with more options
        //InstantScreenshot.TakeScreenshot(Application.dataPath, "Test.png", Camera.main, 1920, 1080, 1, false);

        //Take Screenshot at a 3x resolution
        //InstantScreenshot.TakeScreenshot(Application.dataPath, "Test.png", Camera.main, 1920, 1080, 3, false);


    }
}
