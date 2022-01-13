# Kinect OpenCV Object Detection in Unity

## Description
This module provides support for detecting shapes with OpenCV in Unity using the Kinect's RGB sensor.

## Modules

### ColorBasedObjectDetector
Main class of the module. Simply binds the texture of the Kinect RGB sensor to the RawImage background that displays the data.

### ColorObject 
This class defines the object. It has the data to recognize certain colors and can be changed.

### MultiObjectTrackingBasedOnColorExample (**W.I.P**)
This class contains the main logic for transmuting the RGB Kinect stream to the detected object array. It first goes through rigorous texture filtering to remove noise. Then it goes through a color filter and tries to find objects with that certain color with a minimal density. 

### MultiSourceManager
This manages the kinect input data stream. With all forms of data covered.

## Dependencies
OpenCV For Unity - [Unity Assetstore](https://assetstore.unity.com/packages/tools/integration/opencv-for-unity-21088)
