using System.Text;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace UnityEngine.XR.ARFoundation.Samples
{
    /// <summary>
    /// This component displays a picture-in-picture view of the environment depth texture, the human depth texture, or
    /// the human stencil texture.
    /// </summary>
    public class Mei_DepthImage : MonoBehaviour
    {
        /// <summary>
        /// Name of the max distance property in the shader.
        /// </summary>
        const string k_MaxDistanceName = "_MaxDistance";

        /// <summary>
        /// Name of the display rotation matrix in the shader.
        /// </summary>
        const string k_DisplayRotationPerFrameName = "_DisplayRotationPerFrame";

        /// <summary>
        /// The default texture aspect ratio.
        /// </summary>
        const float k_DefaultTextureAspectRadio = 1.0f;

        /// <summary>
        /// ID of the max distance  property in the shader.
        /// </summary>
        static readonly int k_MaxDistanceId = Shader.PropertyToID(k_MaxDistanceName);

        /// <summary>
        /// ID of the display rotation matrix in the shader.
        /// </summary>
        static readonly int k_DisplayRotationPerFrameId = Shader.PropertyToID(k_DisplayRotationPerFrameName);

        /// <summary>
        /// The current screen orientation remembered so that we are only updating the raw image layout when it changes.
        /// </summary>
        ScreenOrientation m_CurrentScreenOrientation;

        /// <summary>
        /// The current texture aspect ratio remembered so that we can resize the raw image layout when it changes.
        /// </summary>
        float m_TextureAspectRatio = k_DefaultTextureAspectRadio;

        /// <summary>
        /// The display rotation matrix for the shader.
        /// </summary>
        Matrix4x4 m_DisplayRotationMatrix = Matrix4x4.identity;

// #if UNITY_ANDROID
//         /// <summary>
//         /// A matrix to flip the Y coordinate for the Android platform.
//         /// </summary>
//         Matrix4x4 k_AndroidFlipYMatrix = Matrix4x4.identity;
// #endif // UNITY_ANDROID

        /// <summary>
        /// Get or set the <c>AROcclusionManager</c>.
        /// </summary>
        public AROcclusionManager occlusionManager
        {
            get => m_OcclusionManager;
            set => m_OcclusionManager = value;
        }

        [SerializeField]
        [Tooltip("The AROcclusionManager which will produce depth textures.")]
        AROcclusionManager m_OcclusionManager;

        /// <summary>
        /// Get or set the <c>ARCameraManager</c>.
        /// </summary>
        public ARCameraManager cameraManager
        {
            get => m_CameraManager;
            set => m_CameraManager = value;
        }

        [SerializeField]
        [Tooltip("The ARCameraManager which will produce camera frame events.")]
        ARCameraManager m_CameraManager;

        /// <summary>
        /// The UI RawImage used to display the image on screen.
        /// </summary>
        public RawImage rawImage
        {
            get => m_RawImage;
            set => m_RawImage = value;
        }

        [SerializeField]
        RawImage m_RawImage;

        /// <summary>
        /// The depth material for rendering depth textures.
        /// </summary>
        public Material depthMaterial
        {
            get => m_DepthMaterial;
            set => m_DepthMaterial = value;
        }

        [SerializeField]
        Material m_DepthMaterial;

        /// <summary>
        /// The max distance value for the shader when showing an environment depth texture.
        /// </summary>
        public float maxEnvironmentDistance
        {
            get => m_MaxEnvironmentDistance;
            set => m_MaxEnvironmentDistance = value;
        }

        [SerializeField]
        float m_MaxEnvironmentDistance = 8.0f;


//         void Awake()
//         {
// #if UNITY_ANDROID
//             k_AndroidFlipYMatrix[1,1] = -1.0f;
//             k_AndroidFlipYMatrix[2,1] = 1.0f;
// #endif // UNITY_ANDROID
//         }

        void OnEnable()
        {
            // Subscribe to the camera frame received event, and initialize the display rotation matrix.
            Debug.Assert(m_CameraManager != null, "no camera manager");
            m_CameraManager.frameReceived += OnCameraFrameEventReceived;
            m_DisplayRotationMatrix = Matrix4x4.identity;

            // When enabled, get the current screen orientation, and update the raw image UI.
            m_CurrentScreenOrientation = Screen.orientation;
            UpdateRawImage();
        }

        void OnDisable()
        {
            // Unsubscribe to the camera frame received event, and initialize the display rotation matrix.
            Debug.Assert(m_CameraManager != null, "no camera manager");
            m_CameraManager.frameReceived -= OnCameraFrameEventReceived;
            m_DisplayRotationMatrix = Matrix4x4.identity;
        }

        void Update()
        {
            // Get all of the occlusion textures.
            Texture2D displayTexture = m_OcclusionManager.environmentDepthTexture;

            // Assign the texture to display to the raw image.
            m_RawImage.texture = displayTexture;

            // Get the aspect ratio for the current texture.
            float textureAspectRatio = (float)displayTexture.width / (float)displayTexture.height;

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

        /// <summary>
        /// When the camera frame event is raised, capture the display rotation matrix.
        /// </summary>
        /// <param name="cameraFrameEventArgs">The arguments when a camera frame event is raised.</param>
        void OnCameraFrameEventReceived(ARCameraFrameEventArgs cameraFrameEventArgs)
        {
            Debug.Assert(m_RawImage != null, "no raw image");
            if (m_RawImage.material != null)
            {
                // Copy the display rotation matrix from the camera.
                Matrix4x4 cameraMatrix = cameraFrameEventArgs.displayMatrix ?? Matrix4x4.identity;

                Vector2 affineBasisX = new Vector2(1.0f, 0.0f);
                Vector2 affineBasisY = new Vector2(0.0f, 1.0f);
                Vector2 affineTranslation = new Vector2(0.0f, 0.0f);
#if UNITY_IOS
                affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[1, 0]);
                affineBasisY = new Vector2(cameraMatrix[0, 1], cameraMatrix[1, 1]);
                affineTranslation = new Vector2(cameraMatrix[2, 0], cameraMatrix[2, 1]);
#endif // UNITY_IOS
// #if UNITY_ANDROID
//                 affineBasisX = new Vector2(cameraMatrix[0, 0], cameraMatrix[0, 1]);
//                 affineBasisY = new Vector2(cameraMatrix[1, 0], cameraMatrix[1, 1]);
//                 affineTranslation = new Vector2(cameraMatrix[0, 2], cameraMatrix[1, 2]);
// #endif // UNITY_ANDROID

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

// #if UNITY_ANDROID
//                 m_DisplayRotationMatrix = k_AndroidFlipYMatrix * m_DisplayRotationMatrix;
// #endif // UNITY_ANDROID

                // Set the matrix to the raw image material.
                m_RawImage.material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
            }
        }

        /// <summary>
        /// Update the raw image with the current configurations.
        /// </summary>
        void UpdateRawImage()
        {
            Debug.Assert(m_RawImage != null, "no raw image");

            // Determine the raw imge rectSize preserving the texture aspect ratio, matching the screen orientation,
            // and keeping a minimum dimension size.
            float minDimension = 1080.0f;
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

            // Determine the raw image material and maxDistance material parameter based on the display mode.
            float maxDistance;
            Material material = m_DepthMaterial;
            maxDistance = m_MaxEnvironmentDistance;

            // Update the raw image dimensions and the raw image material parameters.
            m_RawImage.rectTransform.sizeDelta = rectSize;
            material.SetFloat(k_MaxDistanceId, maxDistance);
            material.SetMatrix(k_DisplayRotationPerFrameId, m_DisplayRotationMatrix);
            m_RawImage.material = material;
        }
    }
}
