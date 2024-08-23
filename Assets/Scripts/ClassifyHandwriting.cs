using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Sentis;
using Unity.Sentis.Layers;
using System.Linq;
using UnityEngine.UI;
using System.IO;
using Unity.VisualScripting;
using System;


public class ClassifyHandwriting : MonoBehaviour
{
    //[SerializeField] private Texture2D inputTexture;
    //private TensorFloat inputTensor1;

    // Serialize field to link ML model
    [SerializeField] private ModelAsset modelAsset;
    // Store output probabilities
    [SerializeField] private float[] results;

    // Model object to load model
    private Model runtimeModel;
    // Worker to execture model inference
    private IWorker worker;
    // Tensor shape to store input data
    private TensorShape shape;
    // Store input
    private TensorFloat inputTensor;
    int highestResIdx; // Store index of highest result
    private float highestResult; // Store highest result
    List<float> temp;

    // UI objects to display results
    GameObject outputText;
    GameObject outputText2;
    GameObject outputText3;

    // Instance to process textures
    private TextureProcessor textureProcessor;

    private void Start()
    {
        // Load model
        runtimeModel = ModelLoader.Load(modelAsset);
        // UI elements to display results
        outputText = GameObject.Find("Results");
        outputText2 = GameObject.Find("Results2");
        outputText3 = GameObject.Find("Results3");

        // Softmax layer at the end of the model from documentation
        // Not needed if the model already has a softmax layer
        /*
        string softmaxOutputName = "Softmax_Output";
        runtimeModel.AddLayer(new Softmax(softmaxOutputName, runtimeModel.outputs[0]));
        runtimeModel.outputs[0] = softmaxOutputName; // modifies output to be softmax
        */

        // Create worker of type GPU to run model
        worker = WorkerFactory.CreateWorker(BackendType.GPUCompute, runtimeModel);
        textureProcessor = new TextureProcessor();

        //ExecuteModel();
    }

    public void ExecuteModel(Texture2D inputTexture)
    {
        // Dispose of old input tensor if exists
        if (inputTensor != null)
        {
            inputTensor?.Dispose();
        }

        if(inputTexture == null)
        {
            Debug.Log("inputTexture is null.");
            return;
        }
        
        // Process texture to get grayscale
        float[] processedTexture = textureProcessor.ProcessTexture(inputTexture);

        // MODEL INPUT
        // Set input Tensor shape and create input tensor with shape and grayscale values
        // Edit according to model's input tensor
        shape = new TensorShape(1, 28, 28, 1);
        inputTensor = new TensorFloat(shape, processedTexture);

        

        // Execute model with input tensor
        worker.Execute(inputTensor);

        // Get output tensor, make it readable and convert to readable array
        TensorFloat outputTensor = worker.PeekOutput() as TensorFloat;
        outputTensor.MakeReadable();
        results = outputTensor.ToReadOnlyArray();

        // MODEL OUTPUT
        // Convert results to temp list and find index of max value
        temp = results.ToList();
        // Index of most likely value
        highestResIdx = temp.IndexOf(temp.Max());
        // Get probability of most likely value
        highestResult = temp.Max(); 

        // Convert index to character and display results
        char resultChar = IndexToChar(highestResIdx);
        outputText.GetComponent<Text>().text = $"{resultChar}";
        outputText2.GetComponent<Text>().text = $"({highestResult:P2})";

        // Dispose of tensors manually
        inputTensor.Dispose();
        outputTensor.Dispose();
    }
 
    /*
     * Convert index of output tensor results to char
     */
    private char IndexToChar(int index)
    {
        if (index < 10)
            return (char)('0' + index); // 0-9
        if (index < 36)
            return (char)('A' + index - 10); // A-Z
        return (char)('a' + index - 36); // a-z
    }

    // Dispose of tensor when model disabled
    private void OnDisable()
    {
        inputTensor?.Dispose();        
        worker?.Dispose();
    }

}