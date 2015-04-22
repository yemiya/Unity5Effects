﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

public class MosaicField : MonoBehaviour
{
    public float m_block_size = 15.0f;
    public Shader m_mosaic_shader;
    Material m_mat_mosaic;
    Dictionary<Camera, CommandBuffer> m_cameras = new Dictionary<Camera, CommandBuffer>();


#if UNITY_EDITOR
    void Reset()
    {
        m_mosaic_shader = AssetDatabase.LoadAssetAtPath("Assets/Mosaic/Scripts/Mosaic.shader", typeof(Shader)) as Shader;
    }
#endif // UNITY_EDITOR

    void OnDisable()
    {
        foreach (var cam in m_cameras)
        {
            if (cam.Key)
            {
                cam.Key.RemoveCommandBuffer(CameraEvent.BeforeImageEffects, cam.Value);
            }
        }
        m_cameras.Clear();
    }

    void Update()
    {
    }

    void OnWillRenderObject()
    {
        var act = gameObject.activeInHierarchy && enabled;
        if (!act)
        {
            OnDisable();
            return;
        }

        if(m_mat_mosaic==null)
        {
            m_mat_mosaic = new Material(m_mosaic_shader);
        }
        m_mat_mosaic.SetVector("_BlockSize", new Vector4(m_block_size, m_block_size, m_block_size, m_block_size));

        var cam = Camera.current;
        if (!cam || m_cameras.ContainsKey(cam)) return;

        CommandBuffer buf = new CommandBuffer();
        buf.name = "Mosaic";
        m_cameras[cam] = buf;

        int screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        buf.GetTemporaryRT(screenCopyID, -1, -1, 0, FilterMode.Point);
        buf.Blit(BuiltinRenderTextureType.CurrentActive, screenCopyID);

        buf.SetRenderTarget(BuiltinRenderTextureType.CameraTarget);
        buf.DrawRenderer(GetComponent<MeshRenderer>(), m_mat_mosaic);

        buf.ReleaseTemporaryRT(screenCopyID);

        cam.AddCommandBuffer(CameraEvent.BeforeImageEffects, buf);
    }
}