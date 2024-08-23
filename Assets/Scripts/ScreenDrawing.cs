using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ScreenDrawing : MonoBehaviour
{
    [SerializeField] private RawImage displayImage; // UI element where drawing is displayed
    [SerializeField] private ClassifyHandwriting classifier; // Ref classifier
    [SerializeField] private Button clearButton; // Button to clear drawing
    [SerializeField] private Button finishedButton; // Button to finish drawing and run model
    [SerializeField] private Transform fingerTipMarkerTransform;

    private Texture2D drawingTexture; // Texture to drawn on
    private const int PencilSize = 20; // Adjustable pencil size/thickness
    private const int TextureSize = 980;
    private Vector2? lastPosition = null; // Store last position of cursor
    private Color pencilColor = Color.black;
    private TextureProcessor textureProcessor;

    private void Start()
    {
        drawingTexture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGB24, false);
        displayImage.texture = drawingTexture; // Set drawing texture to display image
        ClearTexture(); // Clear texture initially 

        // Listeners for button press
        clearButton.onClick.AddListener(ClearTexture);
        finishedButton.onClick.AddListener(SendDrawing);

        textureProcessor = new TextureProcessor();
    }

    private void Update()
    {
        // Check if mouse button pressed (to draw)
        if (Input.GetMouseButton(0))
        {
            // Get position relative to texture
            Vector2 position = GetDrawPosition();
            // if there is a position, draw a line from last pos to current pos
            if (lastPosition != null)
            {
                DrawLine(lastPosition.Value, position);
            }
            // Update last pos to current
            lastPosition = position;
        }
        else
        {
            lastPosition = null; // Reset last pos
        }
    }

    private Vector2 GetDrawPosition()
    {
        // Calculate and return the position on the texture where the drawing should be drawn
        Vector2 position = Vector2.zero;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(displayImage.rectTransform, Input.mousePosition, null, out position);
        // Get center as origin
        position += new Vector2(displayImage.rectTransform.rect.width / 2, displayImage.rectTransform.rect.height / 2); // Adjust for center
        position *= (TextureSize / displayImage.rectTransform.rect.width); // store position scaled to texture size
        return position;
    }

    // Not needed now
    /*private void Draw(Vector2 position)
    {
        if (lastPosition.HasValue)
        {
            DrawLine(lastPosition.Value, position);
        }
        lastPosition = position;
        drawingTexture.Apply();
    }*/

    /*
     * Draw line between two points on texture
     */
    private void DrawLine(Vector2 start, Vector2 end)
    {
        int distance = (int)Vector2.Distance(start, end);
        Vector2 direction = (end - start).normalized;
        for (int i = 0; i <= distance; i += PencilSize / 2)
        {
            Vector2 point = start + direction * i;
            DrawPoint(point);
        }
    }

    /*
     * Draw point/circle
     */
    private void DrawPoint(Vector2 point)
    {
        for (int y = -PencilSize; y <= PencilSize; y++)
        {
            for (int x = -PencilSize; x <= PencilSize; x++)
            {
                if (x * x + y * y <= PencilSize * PencilSize)
                {
                    // Keep drawing within size limits of texture and set
                    int pixelX = Mathf.Clamp(Mathf.FloorToInt(point.x) + x, 0, TextureSize - 1);
                    int pixelY = Mathf.Clamp(Mathf.FloorToInt(point.y) + y, 0, TextureSize - 1);
                    drawingTexture.SetPixel(pixelX, pixelY, pencilColor);
                }
            }
        }
        drawingTexture.Apply();
    }

    /*
     * Clear texture 
     * Sets the texture to white
     */
    public void ClearTexture()
    {
        Color[] clearColors = new Color[TextureSize * TextureSize];
        for (int i = 0; i < clearColors.Length; i++)
        {
            clearColors[i] = Color.white;
        }
        drawingTexture.SetPixels(clearColors);
        drawingTexture.Apply();
    }

    /*
     * Send texture to classifier to be input in model.
     */
    private void SendDrawing()
    {
        Texture2D texture = drawingTexture;
        textureProcessor.SaveImage(texture, "drawn.png");
        classifier.ExecuteModel(texture);
    }

}

