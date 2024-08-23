using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenshotCapture : MonoBehaviour
{
    // This object is set to the Sentis GameObject in the Editor.
    [SerializeField] private ClassifyHandwriting classifier;
    
    // Set file names
    private const string FileName = "screenshot-img.png";
    private const string CroppedFileName = "cropped-screenshot-img.png";
    
    private TextureProcessor textureProcessor;

    void Start()
    {
        textureProcessor = new TextureProcessor();
    }

    /*
     * On button press, function called
     */
    public void TakeScreenshot()
    {
        StartCoroutine(CaptureAndProcessScreenshot());
    }

    /*
     * Capture and Processing combined into one function.
     * As we want a screenshot, we must disable all UI objects when taking it.
     * This also serves as a user feedback mechanism.
     */
    private IEnumerator CaptureAndProcessScreenshot()
    {
        
        GameObject.Find("Canvas").GetComponent<Canvas>().enabled = false; // Disable Canvas (all UI)

        yield return new WaitForEndOfFrame();

        Texture2D screenshotTexture = ScreenCapture.CaptureScreenshotAsTexture();

        GameObject.Find("Canvas").GetComponent<Canvas>().enabled = true; // Enable all UI

        textureProcessor.SaveImage(screenshotTexture, FileName); // Save image for manual inspection
        Debug.Log("Screenshot taken and saved");
        yield return ProcessScreenshot(screenshotTexture);

        Destroy(screenshotTexture);

        Debug.Log("Processed screenshot");
    }

    /*
     * Processes (crop) screenshot and sends to classifier.
     * Saves cropped image for inspection.
     */
    private IEnumerator ProcessScreenshot(Texture2D screenshotTexture)
    {
        Texture2D croppedTexture = textureProcessor.CropTexture(screenshotTexture, 980, 980);

        textureProcessor.SaveImage(croppedTexture, CroppedFileName);

        yield return new WaitForEndOfFrame();

        classifier.ExecuteModel(croppedTexture);

        Destroy(croppedTexture );
    }
}


