using System;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;
using TextureSendReceive;


namespace UnityEngine.XR.ARFoundation.Samples
{
    public class Client : MonoBehaviour
    {
  //      const string k_MaxDistanceName = "_MaxDistance";
  //      const string k_DisplayDistanceName = "_DisplayDistance";
  //      const string k_DisplayRotationPerFrameName = "_DisplayRotationPerFrame";
  //      const string k_InverseMatrix = "_InverseMatrix";
  //      const float k_DefaultTextureAspectRadio = 1.0f;
  //      static readonly int k_MaxDistanceId = Shader.PropertyToID(k_MaxDistanceName);
  //      static readonly int k_DisplayDistanceId = Shader.PropertyToID(k_DisplayDistanceName);
  //      static readonly int k_DisplayRotationPerFrameId = Shader.PropertyToID(k_DisplayRotationPerFrameName);
  //      static readonly int k_InverseMatrixId = Shader.PropertyToID(k_InverseMatrix);
  //      ScreenOrientation m_CurrentScreenOrientation;
  //      float m_TextureAspectRatio = k_DefaultTextureAspectRadio;
  //      Matrix4x4 m_DisplayRotationMatrix = Matrix4x4.identity;
  //      Texture2D m_CameraTexture;
		//Material m_Material;

        TextureReceiver receiver;
        Texture2D targetTexture;

        public RawImage m_RawImage;
        //public AROcclusionManager m_OcclusionManager;
        //public ARCameraManager m_CameraManager;
        //public Scrollbar m_Scrollbar;
        //public Text m_BaseInfoText;
        //public Text m_DistanceInfoText;
        //public float m_DisplayDistance = 1.5f;
        //public float m_MaxEnvironmentDistance = 8.0f;
        //public Material m_EnvDepthMaterial;
        //public Material m_HumanDepthMaterial;

        void Start()
        {
            receiver = GetComponent<TextureReceiver>();
            receiver.SetTargetTexture(targetTexture);
        }


        void Update()
        {
            m_RawImage.texture = null;
            targetTexture = new Texture2D(1, 1);
            receiver.SetTargetTexture(targetTexture);
            m_RawImage.texture = targetTexture;

            //Get all of the occlusion textures.

   //         sender.SetSourceTexture(humanDepth);
			//m_Material = m_HumanDepthMaterial;

   //         //Assign the texture to display to the raw image.
   //         //m_RawImage.material.SetTexture("_MainTex", displayTexture);
   //         m_RawImage.texture = displayTexture;
   //         m_RawImage.material = m_Material;
   //         m_RawImage.material.SetTexture("_HumanTex", humanDepth);

   //         // Get the aspect ratio for the current texture.
   //         float textureAspectRatio = (float)envDepth.width / (float)envDepth.height;

   //         // If the raw image needs to be updated because of a device orientation change or because of a texture
   //         // aspect ratio difference, then update the raw image with the new values.
   //         if ((m_CurrentScreenOrientation != Screen.orientation)
   //             || !Mathf.Approximately(m_TextureAspectRatio, textureAspectRatio))
   //         {
   //             m_CurrentScreenOrientation = Screen.orientation;
   //             m_TextureAspectRatio = textureAspectRatio;
   //             UpdateRawImage();
   //         }
        }


        public void OnIPChanged(InputField inputField)
        {
            // Update the display mode from the dropdown value.
            receiver.IP = inputField.text;
        }

    }
}
