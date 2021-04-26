using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    public class Mei_Master : MonoBehaviour
    {
        const string k_MaxDistanceName = "_MaxDistance";
        const string k_DisplayDistanceName = "_DisplayDistance";
        const string k_DisplayRotationPerFrameName = "_DisplayRotationPerFrame";
        const string k_InverseMatrix = "_InverseMatrix";
        const float k_DefaultTextureAspectRadio = 1.0f;
        static readonly int k_MaxDistanceId = Shader.PropertyToID(k_MaxDistanceName);
        static readonly int k_DisplayDistanceId = Shader.PropertyToID(k_DisplayDistanceName);
        static readonly int k_DisplayRotationPerFrameId = Shader.PropertyToID(k_DisplayRotationPerFrameName);
        static readonly int k_InverseMatrixId = Shader.PropertyToID(k_InverseMatrix);
        ScreenOrientation m_CurrentScreenOrientation;
        float m_TextureAspectRatio = k_DefaultTextureAspectRadio;
        Matrix4x4 m_DisplayRotationMatrix = Matrix4x4.identity;
        Texture2D m_CameraTexture;
		Material m_Material;

        //int textureWidth, textureHeight;

        public RawImage m_RawImage;
        public AROcclusionManager m_OcclusionManager;
        public ARCameraManager m_CameraManager;
        public Scrollbar m_Scrollbar;
        public Text m_BaseInfoText;
        public Text m_DistanceInfoText;
        public float m_DisplayDistance = 1.5f;
        public float m_MaxEnvironmentDistance = 8.0f;
        public Material m_EnvDepthMaterial;
        public Material m_HumanDepthMaterial;

        void Start()
        {
            m_Scrollbar.value = m_DisplayDistance / m_MaxEnvironmentDistance;
            m_DistanceInfoText.text = $"Display Distance: {m_DisplayDistance.ToString("F2")} meter";
        }

        void OnEnable()
        {
            if (m_CameraManager != null)
            {
                m_CameraManager.frameReceived += OnCameraFrameReceived;
            }
        }

        void OnDisable()
        {
            if (m_CameraManager != null)
            {
                m_CameraManager.frameReceived -= OnCameraFrameReceived;
            }
        }

        void Update()
        {
			m_RawImage.texture = null;
            //Get all of the occlusion textures.
			Texture2D envDepth = m_OcclusionManager.environmentDepthTexture;
            Texture2D humanDepth = m_OcclusionManager.humanDepthTexture;
            Texture2D displayTexture;

            var descriptor = m_OcclusionManager.descriptor;

            if (descriptor == null || descriptor.environmentDepthImageSupported == Supported.Unsupported)
            { 
                displayTexture = humanDepth;
                m_Material = m_HumanDepthMaterial;
                m_BaseInfoText.text = "Display: Human Depth";
			}
            else
            {
                displayTexture = envDepth;
                m_Material = m_EnvDepthMaterial;
                m_BaseInfoText.text = "Display: Enviroment Depth";
			}

            m_DistanceInfoText.text = $"Display Distance: {m_DisplayDistance.ToString("F2")} meter";

            //Assign the texture to display to the raw image.
            //m_RawImage.material.SetTexture("_MainTex", displayTexture);
            m_RawImage.texture = displayTexture;
            m_RawImage.material = m_Material;
            m_RawImage.material.SetTexture("_HumanTex", humanDepth);

            // Get the aspect ratio for the current texture.
            float textureAspectRatio = (float)envDepth.width / (float)envDepth.height;

            // If the raw image needs to be updated because of a device orientation change or because of a texture
            // aspect ratio difference, then update the raw image with the new values.
            if ((m_CurrentScreenOrientation != Screen.orientation)
                || !Mathf.Approximately(m_TextureAspectRatio, textureAspectRatio))
            {
                m_CurrentScreenOrientation = Screen.orientation;
                m_TextureAspectRatio = textureAspectRatio;
                UpdateRawImage();
            }
        }

        unsafe void OnCameraFrameReceived(ARCameraFrameEventArgs eventArgs)
        {
            // Attempt to get the latest camera image. If this method succeeds,
            // it acquires a native resource that must be disposed (see below).
            XRCpuImage image;
            if (!m_CameraManager.TryAcquireLatestCpuImage(out image))
            {
                return;
            }

            var format = TextureFormat.RGBA32;

            if (m_CameraTexture == null || m_CameraTexture.width != image.width || m_CameraTexture.height != image.height)
            {
                m_CameraTexture = new Texture2D(image.width, image.height, format, false);
            }

            // Convert the image to format, flipping the image across the Y axis.
            // We can also get a sub rectangle, but we'll get the full image here.
            var conversionParams = new XRCpuImage.ConversionParams(image, format, XRCpuImage.Transformation.MirrorY);
            // Texture2D allows us write directly to the raw texture data
            // This allows us to do the conversion in-place without making any copies.
            var rawTextureData = m_CameraTexture.GetRawTextureData<byte>();
            try
            {
                image.Convert(conversionParams, new IntPtr(rawTextureData.GetUnsafePtr()), rawTextureData.Length);
            }
            finally
            {
                // We must dispose of the XRCameraImage after we're finished
                // with it to avoid leaking native resources.
                image.Dispose();
            }

            // Apply the updated texture data to our texture
            m_CameraTexture.Apply();

            // Set the RawImage's texture so we can visualize it.
            m_RawImage.material.SetTexture("_CameraTex", m_CameraTexture);

            ///////////////////////////////////////

			Matrix4x4 cameraMatrix = eventArgs.displayMatrix ?? Matrix4x4.identity;

			Vector2 affineBasisX = new Vector2(1.0f, 0.0f);
			Vector2 affineBasisY = new Vector2(0.0f, 1.0f);
			Vector2 affineTranslation = new Vector2(0.0f, 0.0f);

			affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[1, 0]);
			affineBasisY = new Vector2(cameraMatrix[0, 1], cameraMatrix[1, 1]);
			affineTranslation = new Vector2(cameraMatrix[2, 0], cameraMatrix[2, 1]);

			// The camera display matrix includes scaling and offsets to fit the aspect ratio of the device. In most
			// cases, the camera display matrix should be used directly without modification when applying depth to
			// the scene because that will line up the depth image with the camera image. However, for this demo,
			// we want to show the full depth image as a picture-in-picture, so we remove these scaling and offset
			// factors while preserving the orientation.
			affineBasisX = affineBasisX.normalized;
			affineBasisY = affineBasisY.normalized;
			m_DisplayRotationMatrix = Matrix4x4.identity;
			m_DisplayRotationMatrix[0,0] = affineBasisX.x;
			m_DisplayRotationMatrix[0,1] = affineBasisY.x;
			m_DisplayRotationMatrix[1,0] = affineBasisX.y;
			m_DisplayRotationMatrix[1,1] = affineBasisY.y;
			m_DisplayRotationMatrix[2,0] = Mathf.Round(affineTranslation.x);
			m_DisplayRotationMatrix[2,1] = Mathf.Round(affineTranslation.y);

			// Set the matrix to the raw image material.
			m_RawImage.material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
			m_RawImage.material.SetMatrix(k_InverseMatrixId, m_DisplayRotationMatrix.inverse);
        }

        void UpdateRawImage()
        {

            // Determine the raw imge rectSize preserving the texture aspect ratio, matching the screen orientation,
            // and keeping a minimum dimension size.
            //float minDimension = 1500.0f;
            float minDimension = Math.Min(Screen.width, Screen.height);
            float maxDimension = Mathf.Round(minDimension * m_TextureAspectRatio);
            Vector2 rectSize;
            switch (m_CurrentScreenOrientation)
            {
                case ScreenOrientation.LandscapeRight:
                case ScreenOrientation.LandscapeLeft:
                    rectSize = new Vector2(maxDimension, minDimension);
                    break;
                case ScreenOrientation.PortraitUpsideDown:
                case ScreenOrientation.Portrait:
                default:
                    rectSize = new Vector2(minDimension, maxDimension);
                    break;
            }

            m_RawImage.rectTransform.sizeDelta = rectSize;

            // Determine the raw image material and maxDistance material parameter based on the display mode.
            Material material = m_RawImage.material;

            // Update the raw image dimensions and the raw image material parameters.
            material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
			material.SetFloat(k_DisplayDistanceId, m_DisplayDistance);
			material.SetFloat(k_MaxDistanceId, m_MaxEnvironmentDistance);

            m_RawImage.material = material;
        }

        public void OnDropdownValueChanged(Scrollbar scrollbar)
        {
            // Update the display mode from the dropdown value.
            m_DisplayDistance = (float)scrollbar.value * 8;

            // Update the raw image following the mode change.
            UpdateRawImage();
        }

    }
}
