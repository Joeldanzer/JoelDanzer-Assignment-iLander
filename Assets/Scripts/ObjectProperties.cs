using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

static public class DefinedValues
{
    public const float Gravity  = 9.81f / 4.0f;
    public const float Friction = 1.0f;
}
public abstract class ObjectProperties : MonoBehaviour
{
    protected Vector2 m_velocity;
    protected Vector2 m_maxVelocity;

    [SerializeField]
    protected float m_weight;

    protected bool m_active;

    public bool Active
    {
        get { return m_active;  }
        set { m_active = value; }
    }
    public Vector2 Velocity {
        get { return m_velocity;  }
        set { m_velocity = value; } 
    }
    public Vector2 MaxVelocity
    {
        get { return m_maxVelocity; }
    }
    public float Weight
    {
        get { return m_weight; }
    }

    public abstract void Instantiate();
    public abstract void UpdateObject();
} 

// Object Properties will hold the information on the objects current weight, velocity 
// to be used when calculating the physics later on
