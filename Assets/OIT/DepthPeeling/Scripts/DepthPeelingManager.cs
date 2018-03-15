using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[ExecuteInEditMode]
[RequireComponent(typeof(Camera))]
public class DepthPeelingManager : MonoBehaviour
{
    public enum TransparentMode { ODT = 0, DepthPeeling }

    #region Public params
    public Camera m_transparentCamera = null;
    public Shader initializationShader = null;
    public Shader depthPeelingShader = null;
    public Shader blendShader = null;
    public Shader depthCopyShader = null;
    public TransparentMode transparentMode = TransparentMode.ODT;
    [Range(2, 8)]
    public int layers = 4;
    #endregion

    #region Private params
    private Camera m_camera = null;
    private RenderTexture top = null;
    private RenderTexture second = null;
    private Material m_material = null;
    #endregion

    // Use this for initialization
    void Start()
    {
        m_camera = GetComponent<Camera>();
        m_camera.depthTextureMode = DepthTextureMode.Depth;
        m_transparentCamera.cullingMask = 1 << LayerMask.NameToLayer("Transparent");
        m_transparentCamera.clearFlags = CameraClearFlags.Nothing;
        m_material = new Material(depthCopyShader);
    }

    void OnPreRender()
    {
        if (transparentMode == TransparentMode.ODT)
        {
            // The main camera render everything as normal
            m_camera.cullingMask = -1;
        }
        else
        {
            // The main camera just render opaque object
            m_camera.cullingMask = ~(1 << LayerMask.NameToLayer("Transparent"));
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (transparentMode == TransparentMode.ODT)
        {
            Graphics.Blit(src, dst);
        }
        else
        {
            m_material.shader = depthCopyShader;
            CommandBuffer command = new CommandBuffer();
            top = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            second = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.Depth, RenderTextureReadWrite.Linear);
            RenderTexture[] colorTexs = new RenderTexture[layers];

            // First render Top-layer transparent objects
            colorTexs[0] = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            Graphics.Blit(null, top, m_material);
            command.SetRenderTarget(colorTexs[0]);
            command.ClearRenderTarget(false, true, new Color(1.0f, 1.0f, 1.0f, 0.0f));
            Graphics.ExecuteCommandBuffer(command);
            m_transparentCamera.SetTargetBuffers(colorTexs[0].colorBuffer, top.depthBuffer);
            m_transparentCamera.RenderWithShader(initializationShader, null);

            // Render next layer transparent objects in sequence
            for (int count = 1; count < layers; count++)
            {
                colorTexs[count] = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
                Graphics.Blit(null, second, m_material);
                command.Clear();
                command.SetRenderTarget(colorTexs[count]);
                command.ClearRenderTarget(false, true, new Color(1.0f, 1.0f, 1.0f, 0.0f));
                Graphics.ExecuteCommandBuffer(command);
                Shader.SetGlobalTexture("_PrevDepthTex", top);
                m_transparentCamera.SetTargetBuffers(colorTexs[count].colorBuffer, second.depthBuffer);
                m_transparentCamera.RenderWithShader(depthPeelingShader, null);
                RenderTexture temp = top;
                top = second;
                second = temp;
            }

            // Blend all the layers
            m_material.shader = blendShader;
            for (int count = layers - 1; count >= 0; count--)
            {
                Graphics.Blit(colorTexs[count], src, m_material);
            }

            Graphics.Blit(src, dst);

            RenderTexture.ReleaseTemporary(top);
            RenderTexture.ReleaseTemporary(second);
            for (int count = 0; count < layers; count++)
            {
                RenderTexture.ReleaseTemporary(colorTexs[count]);
            }
        }
    }
}
