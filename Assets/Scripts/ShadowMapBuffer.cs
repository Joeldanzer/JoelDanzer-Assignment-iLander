using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.Collections;
using UnityEngine;

public class ShadowMapBuffer : MonoBehaviour
{

    private struct CircleDataBuffer
    {
        public Vector3 m_position;
        public float   m_radius;
    }

    private struct BoxDataBuffer
    {
        public Vector4 m_line1;
        public Vector4 m_line2;
        public Vector4 m_line3;
        public Vector4 m_line4;
    }
    private struct SpotlightDataBuffer
    {
        public Vector3 m_positon;
        public float   m_radius;
        public Vector3 m_direction;
        public float   m_angle;
    }

    [SerializeField] private Shader m_shader;
    [SerializeField] private Material m_material;
    [SerializeField] private RenderTexture m_buffer;
    [SerializeField] private SpotlightData m_spotlight;
    [SerializeField] private ObjectHandler m_objectHandler;
    [SerializeField] private CircleCollider2D m_shadowCircle;

    private Texture2D m_shadowMapTexture;
    public  Texture2D ShadowMapTexture
    {
        get { return m_shadowMapTexture; }
    }
    
    private ComputeBuffer m_structuredBoxBuffer;
    private ComputeBuffer m_structuredSpotlightBuffer;
    private ComputeBuffer m_structuredCircletBuffer;
    private SpotlightDataBuffer m_spotligthData;

    private List<CircleCollider2D> m_circleObjects;
    private List<BoxCollider2D>    m_boxObjects;

    private const int   m_shadowLayer = 3;
    private const float m_aspectRatio = 16.0f / 9.0f;
    void Start()
    {
        m_circleObjects = new List<CircleCollider2D>();
        m_boxObjects    = new List<BoxCollider2D>();

        // Fetch all objects that will render shadows
        for (int i = 0; i < m_objectHandler.GetObjects.Length; i++)
        {
            if (m_objectHandler.GetObjects[i].gameObject.layer == m_shadowLayer)
            {
                BoxCollider2D hasBox = m_objectHandler.GetObjects[i].GetComponent<BoxCollider2D>();
                if(hasBox)
                    m_boxObjects.Add(hasBox);

                CircleCollider2D hasCircle = m_objectHandler.GetObjects[i].GetComponent<CircleCollider2D>();
                if (hasCircle)
                    m_circleObjects.Add(hasCircle);
                
            }
        }

        m_material.shader = m_shader;

        m_spotligthData = new SpotlightDataBuffer();

        // this will crash if there are no box or circle objects in the scene but doesn't really matter to much since each scene has atleast one
        m_structuredBoxBuffer       = new ComputeBuffer(m_boxObjects.Count    * 4, sizeof(float) * 16, ComputeBufferType.Append);
        m_structuredCircletBuffer   = new ComputeBuffer(m_circleObjects.Count * 5, sizeof(float) *  4, ComputeBufferType.Append);
        m_structuredSpotlightBuffer = new ComputeBuffer(3,                         sizeof(float) *  8, ComputeBufferType.Default);
        m_shadowMapTexture          = new Texture2D(1024, 1024, TextureFormat.ARGB32, false);
    }

    private void OnDestroy()
    {
        // Release buffers
        m_structuredBoxBuffer.Release();
        m_structuredCircletBuffer.Release();
        m_structuredSpotlightBuffer.Release();
    }

    void Update()
    {
        // Not sure if this is the intended way of using Unity shaders but wanted to
        // try and make something cool with it and worked out quite well(it's probably terribly optimized)

        { // Spotlight Buffer begin         
            m_spotligthData.m_direction = m_spotlight.transform.right;

            Vector3 newPosition = m_spotlight.transform.position;
            newPosition.x = newPosition.x / (10.0f * m_aspectRatio) + 0.5f; // See explanation in LightBufferHandler
            newPosition.y = newPosition.y /  10.0f + 0.5f;

            m_spotligthData.m_positon = newPosition;
            m_spotligthData.m_radius  = m_spotlight.m_radius;
            m_spotligthData.m_angle   = m_spotlight.m_angle;

            List<SpotlightDataBuffer> list = new List<SpotlightDataBuffer> { m_spotligthData };
            m_structuredSpotlightBuffer.SetData<SpotlightDataBuffer>(list);
            m_material.SetBuffer("Spotlights", m_structuredSpotlightBuffer);
        } // Spotlight Buffer end

        { // Pointlight Buffer begin
            
            // Circle colliders
            List<CircleDataBuffer> circleList = new List<CircleDataBuffer>();
            circleList.Capacity = m_circleObjects.Count;

            for (int i = 0; i < m_circleObjects.Count; i++)
            {
                CircleDataBuffer circle = new CircleDataBuffer();

                Vector3 newPosition = m_circleObjects[i].transform.position;
                newPosition.x = newPosition.x / (10.0f * (16.0f / 9.0f)) + 0.5f;
                newPosition.y = newPosition.y /  10.0f + 0.5f;

                circle.m_position = newPosition;
                circle.m_radius   = m_circleObjects[i].radius/10.0f;

                circleList.Add(circle); 
            }
            
            m_structuredCircletBuffer.SetData<CircleDataBuffer>(circleList);
            m_material.SetBuffer("CircleObjects",  m_structuredCircletBuffer);
            m_material.SetInt("CircleObjectCount", circleList.Count);
            // Circle colliders

            // Box Colliders
            List<BoxDataBuffer> boxLists = new List<BoxDataBuffer>();
            for (int i = 0; i < m_boxObjects.Count; i++)
            {
                BoxDataBuffer box = new BoxDataBuffer();

                //box.m_lines = new Vector4[4];
                box.m_line1.x = m_boxObjects[i].bounds.max.x / (10.0f * m_aspectRatio) + 0.5f; //Would look much better if this was a Return function instead
                box.m_line1.y = m_boxObjects[i].bounds.max.y /  10.0f + 0.5f;
                box.m_line1.z = m_boxObjects[i].bounds.min.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line1.w = m_boxObjects[i].bounds.max.y / 10.0f + 0.5f;

                box.m_line2.x = m_boxObjects[i].bounds.min.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line2.y = m_boxObjects[i].bounds.max.y / 10.0f + 0.5f;
                box.m_line2.z = m_boxObjects[i].bounds.min.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line2.w = m_boxObjects[i].bounds.min.y / 10.0f + 0.5f;

                box.m_line3.x = m_boxObjects[i].bounds.min.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line3.y = m_boxObjects[i].bounds.min.y / 10.0f + 0.5f;
                box.m_line3.z = m_boxObjects[i].bounds.max.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line3.w = m_boxObjects[i].bounds.min.y / 10.0f + 0.5f;

                box.m_line4.x = m_boxObjects[i].bounds.max.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line4.y = m_boxObjects[i].bounds.min.y / 10.0f + 0.5f;
                box.m_line4.z = m_boxObjects[i].bounds.max.x / (10.0f * m_aspectRatio) + 0.5f;
                box.m_line4.w = m_boxObjects[i].bounds.max.y / 10.0f + 0.5f;

                boxLists.Add(box);
            }

            m_structuredBoxBuffer.SetData<BoxDataBuffer>(boxLists);
            m_material.SetBuffer("BoxObjects", m_structuredBoxBuffer);
            m_material.SetInt("BoxObjectCount", boxLists.Count);
            //Box Colliders
            
        } // Pointlight Buffer end 

        // Render the effect on to the RenderTexture & then copy the pixels
        // the from rendertexture to the Texture2D to be used for rendering spotlights
        Graphics.Blit(null, m_buffer, m_material, -1);
        RenderTexture.active = m_buffer;
        m_shadowMapTexture.ReadPixels(new Rect(0, 0, 1024, 1024), 0, 0, false);
        m_shadowMapTexture.Apply();
    }
}
