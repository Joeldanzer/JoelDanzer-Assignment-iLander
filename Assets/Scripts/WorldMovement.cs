using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldMovement : MonoBehaviour
{
    public float m_speed;
    public ObjectProperties m_target;
    public Transform        m_center;


    void Update()
    {   
        // Because of how i made the shadowmap shadows only work in -1 to 1 space so
        // i have to move the whole world
        if(m_target.transform.position.x > m_center.position.x)
        {
            // We only want to check distance on the x coordinate
            Vector2 center = new Vector2(m_center.transform.position.x, m_target.transform.position.y);
            float d = Vector2.Distance(m_target.transform.position, center);
            transform.position += new Vector3(m_speed * Mathf.Clamp(d, 0.0f, 0.75f) * Time.deltaTime, 0, 0);

        }

    }
}
