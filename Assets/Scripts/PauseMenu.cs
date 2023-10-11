using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    bool m_paused = true;
    public bool Paused
    {
        get { return m_paused; }
    }

    Button[] m_buttons;
    RectTransform[] m_children;

    private void Start()
    {
        m_children = GetComponentsInChildren<RectTransform>();

        m_buttons = gameObject.GetComponentsInChildren<Button>(); // 0 = Resume, 1 = Quit
        m_buttons[0].onClick.AddListener(PauseOrResumeGame);
        m_buttons[1].onClick.AddListener(QuitGame);

        PauseOrResumeGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            PauseOrResumeGame();
        
    }

    //public void OnPause(InputAction.CallbackContext context)
    //{
    //    if (context.performed)
    //        PauseOrResumeGame();
    //        
    //}
    private void PauseOrResumeGame()
    {
        m_paused = !m_paused;
        for (int i = 1; i < m_children.Length; i++)
        {
            m_children[i].gameObject.SetActive(m_paused);
            
        }
    }
    private void QuitGame() {
        SceneManager.LoadScene(0);
    }

}
