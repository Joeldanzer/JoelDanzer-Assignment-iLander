using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;

public class LightBufferHandler : MonoBehaviour
{
    private struct PointLightDataBuffer
    {
        public Vector4 m_color;
        public Vector3 m_position;
        public float   m_radius;
    }
    private struct SpotlightDataBuffer
    {
        public Vector4 m_color;
        public Vector3 m_positon;
        public float m_radius;
        public Vector3 m_direction;
        public float m_angle;
    }
    

    [SerializeField] private Shader m_shader;
    [SerializeField] private Material m_material;
    [SerializeField] private SpotlightData m_spotlight;
    [SerializeField] private RenderTexture m_shadowTexture;
    [SerializeField] private PointlightData m_pointLight;
    [SerializeField] private ShadowMapBuffer m_shadowMapBuffer; 

    private SpotlightDataBuffer  m_spotligthData;
    private PointLightDataBuffer m_pointLightData;
    private ComputeBuffer        m_structuredSpotlightBuffer;
    private ComputeBuffer        m_structuredPointlightBuffer;

    private const float m_asepctRatio = 16.0f / 9.0f;
    
    void Start()
    {
        m_material.shader = m_shader;

        m_spotligthData  = new SpotlightDataBuffer();
        m_pointLightData = new PointLightDataBuffer();

        m_structuredPointlightBuffer = new ComputeBuffer(3, sizeof(float) *  8, ComputeBufferType.Default);
        m_structuredSpotlightBuffer  = new ComputeBuffer(5, sizeof(float) * 12, ComputeBufferType.Default);
    }

    private void OnDestroy()
    {
        // Need to release compute buffers after the application has ended
        m_structuredSpotlightBuffer.Release();
        m_structuredPointlightBuffer.Release();
    }

    void Update()
    {
        m_material.SetTexture("ShadowTexture", m_shadowMapBuffer.ShadowMapTexture);

        { // Spotlight Buffer begin
            // Set the structured buffer values
            m_spotligthData.m_color     = new Vector4(
            m_spotlight.m_color.x, m_spotlight.m_color.y, m_spotlight.m_color.z, m_spotlight.m_intensity);
            m_spotligthData.m_direction = m_spotlight.transform.right;

            // Coordinates in the shaders are different from world
            // so I did some hard coded calculations to make them lineup and it works very well
            // x & y values also have an offset of 0.5f in world view.
            Vector3 newPosition = m_spotlight.transform.position;
            newPosition.x = newPosition.x / (10.0f * m_asepctRatio) + 0.5f;
            newPosition.y = newPosition.y / 10.0f + 0.5f;

            m_spotligthData.m_positon   = newPosition;
            m_spotligthData.m_radius    = m_spotlight.m_radius;
            m_spotligthData.m_angle     = m_spotlight.m_angle;

            List<SpotlightDataBuffer> list = new List<SpotlightDataBuffer>{ m_spotligthData };
            m_structuredSpotlightBuffer.SetData<SpotlightDataBuffer>(list);
            m_material.SetBuffer("Spotlights", m_structuredSpotlightBuffer);
            m_material.SetInt("SpotlightCount", list.Count);
        } // Spotlight Buffer end

        { // Pointlight Buffer begin
            Vector3 newPosition = m_pointLight.transform.position;
            newPosition.x = newPosition.x / (10.0f * m_asepctRatio) + 0.5f;
            newPosition.y = newPosition.y / 10.0f + 0.5f;

            m_pointLightData.m_color    = new Vector4(m_pointLight.m_color.x, m_pointLight.m_color.y, m_pointLight.m_color.z, m_pointLight.m_intensity);
            m_pointLightData.m_position = newPosition;
            m_pointLightData.m_radius   = m_pointLight.m_radius;

            List<PointLightDataBuffer> list = new List<PointLightDataBuffer> { m_pointLightData };
            m_structuredPointlightBuffer.SetData<PointLightDataBuffer>(list);

            m_material.SetBuffer("Pointlights", m_structuredPointlightBuffer);
            m_material.SetInt("PointlightCount", list.Count);

        } // Pointlight Buffer end 
    }
}
