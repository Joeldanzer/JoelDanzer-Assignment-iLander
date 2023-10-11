using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatyRock : ObjectProperties
{
    [SerializeField]
    private Transform m_p1, m_p2;
    private Transform m_targetPosition; 

    [SerializeField]
    private float m_speed;

    Vector3 m_dir;

    public override void Instantiate()
    {
        // Calculate middle position
       
        transform.position = (m_p2.position - m_p1.position) / 2.0f  + m_p1.position;
        m_dir              = GetVector3Direction(m_p1.position, m_p2.position);
        m_targetPosition   = m_p1;
    }
    public override void UpdateObject()
    {
        // Not the most pretty code & it kinda works, has problems moving if the either x or y is somewhat not alligned
        m_dir = m_targetPosition.position - transform.position;

        if (Vector3.Distance(transform.position, m_targetPosition.position) < 0.5f)
        {
            if(m_targetPosition == m_p1)
                m_targetPosition = m_p2;   
            else if (m_targetPosition == m_p2)
                m_targetPosition = m_p1;
        }
 
        m_velocity.y += m_speed * m_dir.y * Time.deltaTime;
        m_velocity.x += m_speed * m_dir.x * Time.deltaTime;

    }
    Vector3 DivideVector3(Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }
    Vector3 GetVector3Direction(Vector3 v1, Vector3 v2)
    {
        Vector3 dir = v1 - v2;
        return dir.normalized;
    }
}
