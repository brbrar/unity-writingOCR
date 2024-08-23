using System.IO;
using TMPro;
using UnityEditor.Rendering.Universal;
using UnityEngine;

public class TextureProcessor
{
    /*
     * Process the input texture by:
     * Flipping the image to match EMNIST dataset;
     * Convert to grayscale and apply binary threshold to make the image b/w;
     * Invert colors to match EMNIST dataset of white text on a black background.
     */
    public float[] ProcessTexture(Texture2D texture)
    {
        float[] processedTexture;

        // Get pixels from texture
        Color[] pixels = texture.GetPixels();
        // Convert pixels to float array
        float[] textureFloatArray = PixelsToFloatArray(pixels);

        int width = texture.width;
        int height = texture.height;
        float threshold = 0.5f;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Flip the image
                int invertedY = height - 1 - y;
                Color pixelColor = pixels[invertedY * width + x];

                // Convert to grayscale
                float grayscale = 0.299f * pixelColor.r + 0.587f * pixelColor.g + 0.114f * pixelColor.b;

                // Apply binary threshold
                textureFloatArray[y * width + x] = grayscale > threshold ? 1.0f : 0.0f;
            }
        }

        // Invert colours
        for (int i = 0; i < textureFloatArray.Length; i++)
        {
            textureFloatArray[i] = 1 - textureFloatArray[i];
        }

        // Convert back to texture and downscale to 28x28 
        Texture2D downscaledTexture = DownscaleTexture(FloatArrayToTexture(textureFloatArray, width, height), 28, 28);
        // Convert to float array to input into model.
        processedTexture = PixelsToFloatArray(downscaledTexture.GetPixels());
        SaveImage(downscaledTexture, "processed-img.png"); // Save downscaled texture for manual inspection

        return processedTexture;
    }

    /*
     * Convert the float array to Texture2D
     * Creates a new texture of required size;
     * Creates Color[] to set float array values on texture
     */
    public Texture2D FloatArrayToTexture(float[] inputFloat, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGB24, false);
        Color[] pixels = new Color[inputFloat.Length];

        for (int i = 0; i < inputFloat.Length; i++)
        {
            pixels[i] = new Color(inputFloat[i], inputFloat[i], inputFloat[i], 1.0f);
        }

        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }

    /*
     * Converts pixels to a float array.
     * 
     * May be better to change this to TextureToFloatArray via GetPixels().
     */
    private float[] PixelsToFloatArray(Color[] pixels)
    {
        float[] textureFloat = new float[pixels.Length];
        for (int i = 0; i < pixels.Length; i++)
        {
            textureFloat[i] = pixels[i].grayscale;
        }
        return textureFloat;
    }

    /*
     * Save processed image for manual inspection.
     * Encode to PNG and save in the persistent data path (Android/data/{app}
     */
    public void SaveImage(Texture2D texture, string fileName)
    {
        byte[] bytes = texture.EncodeToPNG();
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        File.WriteAllBytes(filePath, bytes);
        Debug.Log(fileName + " Image saved to: " + filePath);
    }

    private Texture2D DownscaleTexture(Texture2D originalTexture, int targetWidth, int targetHeight)
    {
        // Create temp RenderTexture to apply filters
        RenderTexture rt = RenderTexture.GetTemporary(targetWidth, targetHeight);
        // Use bilinear filter for smooth downsampling
        rt.filterMode = FilterMode.Bilinear;

        // Set current  RenderTexture to temp and copy original texture to the rt
        RenderTexture.active = rt;
        Graphics.Blit(originalTexture, rt);

        // Create new Texture2D to store downscaled texture
        Texture2D downscaledTexture = new Texture2D(targetWidth, targetHeight, TextureFormat.RGB24, false);
        // Read pixels from rt to new Texture2D
        downscaledTexture.ReadPixels(new Rect(0, 0, targetWidth, targetHeight), 0, 0);
        downscaledTexture.Apply();
        // Clean up temp RT
        RenderTexture.ReleaseTemporary(rt);
        RenderTexture.active = null;

        return downscaledTexture;
    }

    /*
	 * Crop texture given width and height
	 */
    public Texture2D CropTexture(Texture2D originalTexture, int width, int height)
    {
        // Get bottom left pixel to start cropping
        int x = (originalTexture.width - width) / 2;
        int y = (originalTexture.height - height) / 2;

        // Create new Texture2D object for cropped image
        Texture2D croppedTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

        // Copy pixels from original texture to new cropped texture
        Color[] pixels = originalTexture.GetPixels(x, y, width, height);
        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }
}