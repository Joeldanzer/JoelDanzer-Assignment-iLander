using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelTransitioner : MonoBehaviour
{
    [SerializeField]
    private MenuButtons.Scenes m_newScene = MenuButtons.Scenes.MainMenu;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log(collision.gameObject.transform.position);
        SceneManager.LoadScene((int)m_newScene);
    }

}
