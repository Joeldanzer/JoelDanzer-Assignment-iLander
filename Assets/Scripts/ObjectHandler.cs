using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ObjectHandler : MonoBehaviour
{
    [SerializeField]
    GameObject         m_mainScene;
    ObjectProperties[] m_objects;

    [SerializeField] private BoxCollider2D m_objectBoundary;
    [SerializeField] private PauseMenu     m_pause;
    public ObjectProperties[] GetObjects
    {
        get{ return m_objects; }
    }
    void Awake()
    {
        // Fetch all the ObjectProperties in the scene
        m_objects = m_mainScene.GetComponentsInChildren<ObjectProperties>();
        // Instantiate all these objects
        for (int i = 0; i < m_objects.Length; i++)
        {
            //Debug.Log("INSTNATIATING");
            m_objects[i].Instantiate();

        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!m_pause.Paused)
        {
            for (int i = 0; i < m_objects.Length; i++)
            {
                ObjectProperties obj = m_objects[i];

                // Only update objects that are inside the cameras bounding box
                if (m_objectBoundary.OverlapPoint(new Vector2(obj.transform.position.x, obj.transform.position.y))){
                    obj.UpdateObject();

                    { // Simple force physics
                        
                        // Clamp so the object cannot reach more than it's max velocity
                        obj.Velocity = ClampVector2(obj.Velocity, -obj.m_maxVelocity, obj.m_maxVelocity);
                        float frictionDir = obj.Velocity.x > 0.0f ? 1.0f : -1.0f;

                        // Multiply together the objects weight with the worlds friciton & gravity
                        obj.Velocity           -= new Vector2(obj.Weight * DefinedValues.Friction * frictionDir, obj.Weight * DefinedValues.Gravity) * Time.deltaTime;            
                        obj.transform.position += new Vector3(obj.Velocity.x, obj.Velocity.y) * Time.deltaTime;

                    } // Force physics end
                }
            }
        }
    }

    Vector2 ClampVector2(Vector2 c, Vector2 min, Vector2 max)
    {
        c.x = Mathf.Clamp(c.x, min.x, max.x);
        c.y = Mathf.Clamp(c.y, min.y, max.y);
        return c;
    }
}
