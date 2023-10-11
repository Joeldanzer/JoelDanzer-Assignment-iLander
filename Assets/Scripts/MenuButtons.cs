using System.Collections;
using System.Collections.Generic;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuButtons : MonoBehaviour
{
    // Easy tracker though very much not necessary for this project lol
    public enum Scenes
    {
        MainMenu = 0,
        Level1 = 1,
        Level2 = 2,
        Level3 = 3,
    };

    Button[] m_buttons;

    void Start()
    {
        m_buttons = gameObject.GetComponentsInChildren<Button>();
        m_buttons[0].onClick.AddListener(delegate { LoadScene((int)Scenes.Level1); });
        m_buttons[1].onClick.AddListener(QuitGame);
    }

    void LoadScene(int scene)
    {
        SceneManager.LoadScene(scene);
    }

    void QuitGame()
    {
        Application.Quit();
    }
}
