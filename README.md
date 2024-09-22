# unity-writingOCR
Template code for simple Unity app to showcase Unity Sentis for OCR. Includes the options to draw/write on-screen or take a picture of handwriting.
This formed half of my 3rd year project, which was an Android app to enhance handwritten letter formation learning by implementing AR for teaching and
**ML to check handwritten letters**. Thus, this project is set up for models trained on the EMNIST dataset, including all the processing for inputs.


## Installation
.unitypackage not yet uploaded, ignore below.

The repo includes a very barebones .unitypackage file. This consists of only the scripts and Scene assets.

1. Download the .unitypackage file.
2. Import the file in the Assets section of a chosen Unity Project.
3. Import chosen ML model. **Models are not provided in this repo, nor in the .unitypackage**. Read below for more guidance regarding this.
4. Run in PlayMode or build to an Android mobile phone. Running in PlayMode only allows usage of the on-screen drawing option.

ARCore was used in the original project, and has been kept in this project to utilise the device camera. So, Player Settings should be updated to conform (see Google's guide on setting up ARCore in Unity for guidance).


## Use (and Unity Sentis)
[Unity Sentis](https://docs.unity3d.com/Packages/com.unity.sentis@1.6/manual/index.html) is used to run models in real-time and offline. Models are required to be in the ONNX format. 
### Scripts
- `ClassifyHandwriting.cs`: the main script showcasing usage of Sentis; It covers how a model is imported and run, and tensor input/output utilisation. The tensor shape of the model needs to be mirrored in the script for the model to take input via Sentis.
- `TextureProcessor.cs`: covers all the processing required before inputting to the model. This includes all requirements for EMNIST: $28\times28$ pixel image and grayscale with normalised pixel values (white character on black backgroud). To achieve this, the user input is cropped, converted to grayscale, normalised via binary threshold, colour inverted, flipped and downscaled. Images of the texture are saved during this process for manual inspection if necessary.

### Models
- The ONNX model should be assigned to the `Sentis` GameObject as the 'Model Asset'. This object can be found in the Hierarchy in both scenes.
- Models used should be trained on the EMNIST dataset if `TextureProcessor.cs` is to be used. Models are not provided in this repo, but I suggest using the [MNIST-12 model for digits provided by ONNX](https://github.com/onnx/models/tree/main/validated/vision/classification/mnist) for testing. Models may have different input tensor shapes, so the shape should be modified to match this in `ClassifyHandwriting.cs`. For example:
    - For the MNIST-12 model with input (1, 1, 28, 28), it is  `shape = new TensorShape(1, 1, 28, 28);`
    - For a model that takes input (unk_25, 28, 28, 1), the appropriate shape is `shape = new TensorShape(1, 28, 28, 1);`.
    
- As outputs can also be different, this should be accounted for. Look for comments highlighting this in the script.


Refer to the Sentis documentation (https://docs.unity3d.com/Packages/com.unity.sentis@1.6/manual/index.html) for additional help.


## Loading times & Accuracy
Upon first startup on an Android device, a slight lag may be observed when running the model. After the first time, this is not an issue as the model is now in the memory.

For the on-screen drawing method, accuracy is completely dependent on the model's accuracy (I think). However, the picture method has several limitations which can affect accuracy (see below).

### Limitations
The on-screen writing method inherits all of the limitations of the chosen implemented model.

For handwritten letter checking via picture:
- The letter must take up a lot of the space in the input image for highest accuracy. This means the picture must be taken close to the letter on the paper. Writing large characters with a thick marker pen mitigates this issue.
- Optimal lighting is required as this affects the processing procedures. Poor lighting -> darker image -> negatively affects conversion to grayscale.
- Loss in focus of the letter can affect quality of conversion.

## TODO:
- Implement a gallery screen to view images saved during input processing.
- Upload app screenshots to this repo.



