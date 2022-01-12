using System;
using Windows.Kinect;
using UnityEngine;
using UnityEngine.UI;

namespace DefaultNamespace
{
    public class ColorBasedObjectDetector : MonoBehaviour
    {
        public MultiSourceManager multiSourceManager;
        public RawImage rawImage;
        
        private void Start()
        {
            this.rawImage.texture = multiSourceManager.GetColorTexture();

        }
    }
}