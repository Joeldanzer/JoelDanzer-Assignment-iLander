using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using Unity.VisualScripting;

public class SubmarineController : ObjectProperties
{

    public TMPro.TMP_Text m_hullText;
    [SerializeField]
    private float m_speed, m_lampRotationSpeed, m_lampMaxAngle;

    [SerializeField]
    private Transform m_lampTransform, m_startPositon, m_cameraTransform;

    private float m_currentLampRotation = 0.0f;
    private float m_lampDir = 0.0f;
    private float m_yDir    = 0.0f;
    private float m_xDir    = 0.0f;

    [SerializeField]
    private float m_hull, m_defaultHullDamage, m_invulnerableTimer;
    private float m_currentInvulTimer;

    private BoxCollider2D m_collider;

    // Start is called before the first frame update
    public override void Instantiate() {
        m_hull = 100.0f;
        m_hullText.SetText("Hull: " + m_hull + "%");
        m_collider = GetComponent<BoxCollider2D>();
        transform.position = m_startPositon.position;
    }

    void ResetSubmarine()
    {
        m_hull = 100.0f;
        m_hullText.SetText("Hull: " + m_hull + "%");
        transform.position         = m_startPositon.position;
        m_cameraTransform.position = new Vector3(0.0f, 0.0f, -10.0f);
        m_velocity.x = 0.0f;
        m_velocity.y = 0.0f;
    }

    // Update is called once per frame
    public override void UpdateObject()
    {
        m_velocity.x += (m_speed * m_xDir) * Time.deltaTime;
        m_velocity.y += (m_speed * m_yDir) * Time.deltaTime;

        m_currentLampRotation     = Mathf.Clamp(m_currentLampRotation + (m_lampRotationSpeed * Time.deltaTime) * m_lampDir, -m_lampMaxAngle, m_lampMaxAngle);
        m_lampTransform.rotation  = Quaternion.Euler(new Vector3(0.0f, 0.0f, m_currentLampRotation));

        m_currentInvulTimer -= Time.deltaTime;
        

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        LayerMask mask = (1 << 3);

        float hullDamage = 0.0f;

        // want to detect specificlally where we collided so we can knock the submarine in the opposite direction.
        if (Physics2D.Raycast(new Vector2(m_collider.bounds.max.x - 0.1f, m_collider.bounds.max.y), Vector2.up, 0.1f, mask.value) ||
            Physics2D.Raycast(new Vector2(m_collider.bounds.min.x + 0.1f, m_collider.bounds.max.y), Vector2.up, 0.1f, mask.value))
        {
            hullDamage = m_defaultHullDamage * m_velocity.y;
            m_velocity.y = -(m_velocity.y / 1.5f); 
            
        }
        else if (Physics2D.Raycast(new Vector2(m_collider.bounds.max.x - 0.1f, m_collider.bounds.min.y), Vector2.down, 0.1f, mask.value) ||
            Physics2D.Raycast(new Vector2(m_collider.bounds.min.x + 0.1f, m_collider.bounds.min.y), Vector2.down, 0.1f, mask.value))
        {
            hullDamage = m_defaultHullDamage * Mathf.Abs(m_velocity.y);
            m_velocity.y = Mathf.Abs(m_velocity.y) / 1.5f;

        }   
        
        if (Physics2D.Raycast(new Vector2(m_collider.bounds.min.x, m_collider.bounds.max.y - 0.1f), Vector2.left, 0.1f, mask.value) ||
            Physics2D.Raycast(new Vector2(m_collider.bounds.min.x, m_collider.bounds.min.y + 0.1f), Vector2.left, 0.1f, mask.value))
        {
            hullDamage = m_defaultHullDamage * Mathf.Abs(m_velocity.x);
            m_velocity.x = Mathf.Abs(m_velocity.x) / 1.5f; 

        }
        else if (Physics2D.Raycast(new Vector2(m_collider.bounds.max.x, m_collider.bounds.max.y - 0.1f), Vector2.right, 0.1f, mask.value) ||
                 Physics2D.Raycast(new Vector2(m_collider.bounds.max.x, m_collider.bounds.min.y + 0.1f), Vector2.right, 0.1f, mask.value))
        {
            hullDamage = m_defaultHullDamage * m_velocity.x;
            m_velocity.x = -(m_velocity.x) / 1.5f;
        }


        if(m_currentInvulTimer <= 0.0f)
        {
            m_hull -= hullDamage;
            m_hullText.SetText("Hull: " + m_hull.ToString("0.0") + "%");
            if(m_hull < 0.0f)
            {
                ResetSubmarine();
            }
            m_currentInvulTimer = m_invulnerableTimer;
        }
    }

    public void MoveYAxis(InputAction.CallbackContext context)
    {
        m_yDir = context.canceled ? 0.0f : context.ReadValue<float>();
    }
    public void MoveXAxis(InputAction.CallbackContext context)
    {
        m_xDir = context.canceled ? 0.0f : context.ReadValue<float>();
    }
    public void RotateLamp(InputAction.CallbackContext context)
    {
        m_lampDir = context.canceled ? 0.0f : context.ReadValue<float>(); 
    }
}
