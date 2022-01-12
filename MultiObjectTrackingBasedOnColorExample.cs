#if !(PLATFORM_LUMIN && !UNITY_EDITOR)

using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Windows.Kinect;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;
using OpenCVForUnity.UnityUtils.Helper;
using OpenCVForUnity.UtilsModule;
using UnityEngine.UI;
using Color = System.Drawing.Color;
using Point = OpenCVForUnity.CoreModule.Point;
using Rect = OpenCVForUnity.CoreModule.Rect;
using Size = OpenCVForUnity.CoreModule.Size;

namespace OpenCVForUnityExample
{
    /// <summary>
    /// Multi Object Tracking Based on Color Example
    /// Referring to https://www.youtube.com/watch?v=hQ-bpfdWQh8.
    /// </summary>
    [RequireComponent (typeof(WebCamTextureToMatHelper))]
    public class MultiObjectTrackingBasedOnColorExample : MonoBehaviour
    {
        /// <summary>
        /// The texture.
        /// </summary>
        Texture2D _texture;

        private KinectSensor _sensor;
        private ColorFrameReader _reader;
        private byte[] _data;
        private Mat _rgbaMat;
        
        public RawImage RawImage;
        public Canvas Canvas;
        
    
        /// <summary>
        /// max number of objects to be detected in frame
        /// </summary>
        const int MAX_NUM_OBJECTS = 50;
        
        /// <summary>
        /// minimum and maximum object area
        /// </summary>
        const int MIN_OBJECT_AREA = 80 * 80;

        /// <summary>
        /// The rgb mat.
        /// </summary>
        Mat rgbMat;

        /// <summary>
        /// The threshold mat.
        /// </summary>
        Mat thresholdMat;

        /// <summary>
        /// The hsv mat.
        /// </summary>
        Mat hsvMat;

        ColorObject blue = new ColorObject ("blue");
        ColorObject yellow = new ColorObject ("yellow");
        ColorObject red = new ColorObject ("red");
        ColorObject green = new ColorObject ("green");

        // Use this for initialization
        void Start ()
        {
            
            _sensor = KinectSensor.GetDefault();

            if (_sensor != null)
            {
                _reader = _sensor.ColorFrameSource.OpenReader();
                FrameDescription frameDesc = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Rgba);
                _texture = new Texture2D(frameDesc.Width, frameDesc.Height, TextureFormat.RGBA32, false);
                _data = new byte[frameDesc.BytesPerPixel * frameDesc.LengthInPixels];
                
                if (!_sensor.IsOpen)
                {
                    _sensor.Open();
                }
                
                _rgbaMat = new Mat(_texture.height, _texture.width, CvType.CV_8UC4);

                Debug.Log ("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

                rgbMat = new Mat (_rgbaMat.rows (), _rgbaMat.cols (), CvType.CV_8UC3);
                thresholdMat = new Mat ();
                hsvMat = new Mat ();
            }
        }

        // Update is called once per frame
        void Update ()
        {
            if (KinectSensor.GetDefault().IsOpen)
            {
                
                if (_reader != null)
                {
                    ColorFrame frame = _reader.AcquireLatestFrame();

                    if (frame != null)
                    {
                        frame.CopyConvertedFrameDataToArray(_data, ColorImageFormat.Rgba);

                        frame.Dispose();
                        frame = null;
                    }
                }
                else
                {
                    return;
                }
                
                // Starts object detection (based on HSV color barriers
                MatUtils.copyToMat(_data, _rgbaMat);

                rgbMat = _rgbaMat;
                                        
                // Filter for blue and track object
                Imgproc.cvtColor (rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                Core.inRange (hsvMat, blue.getHSVmin (), blue.getHSVmax (), thresholdMat);
                MorphOps (thresholdMat);
                TrackFilteredObject (blue, thresholdMat, hsvMat, rgbMat);
                // Filter for yellow and track object
                Imgproc.cvtColor (rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                Core.inRange (hsvMat, yellow.getHSVmin (), yellow.getHSVmax (), thresholdMat);
                MorphOps (thresholdMat);
                TrackFilteredObject (yellow, thresholdMat, hsvMat, rgbMat);
                // Filter for reds and track object
                Imgproc.cvtColor (rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                Core.inRange (hsvMat, red.getHSVmin (), red.getHSVmax (), thresholdMat);
                MorphOps (thresholdMat);
                TrackFilteredObject (red, thresholdMat, hsvMat, rgbMat);
                // Filter for greens and track object
                Imgproc.cvtColor (rgbMat, hsvMat, Imgproc.COLOR_RGB2HSV);
                Core.inRange (hsvMat, green.getHSVmin (), green.getHSVmax (), thresholdMat);
                MorphOps (thresholdMat);
                TrackFilteredObject (green, thresholdMat, hsvMat, rgbMat);

                Imgproc.cvtColor(rgbMat, _rgbaMat, Imgproc.COLOR_RGB2RGBA);

                Utils.fastMatToTexture2D (rgbMat, _texture);
                RawImage.material.mainTexture = _texture;
            }
        }


        /// <summary>
        /// Draws the object.
        /// </summary>
        /// <param name="theColorObjects">The color objects.</param>
        /// <param name="frame">Frame.</param>
        /// <param name="temp">Temp.</param>
        /// <param name="contours">Contours.</param>
        /// <param name="hierarchy">Hierarchy.</param>
        private void DrawObject (List<ColorObject> theColorObjects, Mat frame, Mat temp, List<MatOfPoint> contours, Mat hierarchy)
        {
            for (int i = 0; i < theColorObjects.Count; i++) {
                Imgproc.drawContours (frame, contours, i, theColorObjects [i].getColor (), 3, 8, hierarchy, int.MaxValue, new Point ());
                Imgproc.circle (frame, new Point (theColorObjects [i].getXPos (), theColorObjects [i].getYPos ()), 5, theColorObjects [i].getColor ());
                Imgproc.putText (frame, theColorObjects [i].getXPos () + " , " + theColorObjects [i].getYPos (), new Point (theColorObjects [i].getXPos (), theColorObjects [i].getYPos () + 20), 1, 1, theColorObjects [i].getColor (), 2);
                Imgproc.putText (frame, theColorObjects [i].getType (), new Point (theColorObjects [i].getXPos (), theColorObjects [i].getYPos () - 20), 1, 2, theColorObjects [i].getColor (), 2);
                if (theColorObjects[i].getType() == "yellow")
                {
                    CreateRectangles(contours);
                }
            }
        }

        /// <summary>
        /// Creates gameobjects on screen with borders drawing from the 
        /// </summary>
        /// <param name="contours"></param>
        private void CreateRectangles(List<MatOfPoint> contours)
        {
            for (int i=0; i < contours.Count; i++)
            {
                var contour2f = new MatOfPoint2f();
                contours[i].convertTo(contour2f, CvType.CV_32S);
                
                var approxPolyDP = new MatOfPoint2f();
                Imgproc.approxPolyDP(contour2f, approxPolyDP, 3, true);
                var boundRect = Imgproc.boundingRect(approxPolyDP);

                var gameObject = new GameObject();
                gameObject.transform.SetParent(this.Canvas.transform);
                gameObject.transform.position = new Vector2(boundRect.x, boundRect.y);

                var drawableRectangle = gameObject.AddComponent<RectTransform>();
                drawableRectangle.position = new Vector3(boundRect.x, boundRect.y);
                drawableRectangle.sizeDelta = new Vector2(boundRect.width, boundRect.height);
                
                var image = gameObject.AddComponent<Image>();
                image.color = new UnityEngine.Color(1.0F, 0.0F, 0.0F);
                image.rectTransform.position = new Vector3(boundRect.x, boundRect.y);
                image.rectTransform.sizeDelta = new Vector2(boundRect.width, boundRect.height);
            }
        }

        /// <summary>
        /// Morphs the ops.
        /// </summary>
        /// <param name="thresh">Thresh.</param>
        private void MorphOps (Mat thresh)
        {
            //create structuring element that will be used to "dilate" and "erode" image.
            //the element chosen here is a 3px by 3px rectangle
            Mat erodeElement = Imgproc.getStructuringElement (Imgproc.MORPH_RECT, new Size (3, 3));
            //dilate with larger element so make sure object is nicely visible
            Mat dilateElement = Imgproc.getStructuringElement (Imgproc.MORPH_RECT, new Size (8, 8));

            Imgproc.erode (thresh, thresh, erodeElement);
            Imgproc.erode (thresh, thresh, erodeElement);

            Imgproc.dilate (thresh, thresh, dilateElement);
            Imgproc.dilate (thresh, thresh, dilateElement);
        }

        /// <summary>
        /// Tracks the filtered object.
        /// </summary>
        /// <param name="theColorObject">The color object.</param>
        /// <param name="threshold">Threshold.</param>
        /// <param name="HSV">HS.</param>
        /// <param name="cameraFeed">Camera feed.</param>
        private void TrackFilteredObject (ColorObject theColorObject, Mat threshold, Mat HSV, Mat cameraFeed)
        {

            List<ColorObject> colorObjects = new List<ColorObject> ();
            Mat temp = new Mat ();
            threshold.copyTo (temp);
            //these two vectors needed for output of findContours
            List<MatOfPoint> contours = new List<MatOfPoint> ();
            Mat hierarchy = new Mat ();
            //find contours of filtered image using openCV findContours function
            Imgproc.findContours (temp, contours, hierarchy, Imgproc.RETR_CCOMP, Imgproc.CHAIN_APPROX_SIMPLE);

            //use moments method to find our filtered object
            bool colorObjectFound = false;
            if (hierarchy.rows () > 0) {
                int numObjects = hierarchy.rows ();

                // Ff number of objects greater than MAX_NUM_OBJECTS we filter for noise
                if (numObjects < MAX_NUM_OBJECTS) {
                    for (int index = 0; index >= 0; index = (int)hierarchy.get (0, index) [0]) {

                        Moments moment = Imgproc.moments (contours [index]);
                        double area = moment.get_m00 ();

                        //if the area is less than 20 px by 20px then it is probably just noise
                        //if the area is the same as the 3/2 of the image size, probably just a bad filter
                        //we only want the object with the largest area so we safe a reference area each
                        //iteration and compare it to the area in the next iteration.
                        if (area > MIN_OBJECT_AREA) {

                            ColorObject colorObject = new ColorObject ();

                            colorObject.setXPos ((int)(moment.get_m10 () / area));
                            colorObject.setYPos ((int)(moment.get_m01 () / area));
                            colorObject.setType (theColorObject.getType ());
                            colorObject.setColor (theColorObject.getColor ());

                            colorObjects.Add (colorObject);

                            colorObjectFound = true;

                        } else {
                            colorObjectFound = false;
                        }
                    }
                    //let user know you found an object
                    if (colorObjectFound == true) {
                        //draw object location on screen
                        DrawObject (colorObjects, cameraFeed, temp, contours, hierarchy);
                    }

                } 
                else {
                    Imgproc.putText (cameraFeed, "TOO MUCH NOISE!", new Point (5, cameraFeed.rows () - 10), Imgproc.FONT_HERSHEY_SIMPLEX, 1.0, new Scalar (255, 255, 255, 255), 2, Imgproc.LINE_AA, false);
                }
            }
        }

        /// <summary>
        /// Raises the back button click event.
        /// </summary>
        public void OnBackButtonClick ()
        {
            SceneManager.LoadScene ("OpenCVForUnityExample");
        }
    }
}

#endif