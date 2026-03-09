using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;
public class MainMenuManager : MonoBehaviour
{
    public GameObject PlayButton;
    [SerializeField] TMP_InputField PlayerName;
    public void SelectOption(string option)
    {
        PlayerPrefs.SetString("Controller", option);
        PlayerPrefs.Save();
        PlayButton.SetActive(true);
    }

    private void Start()
    {
        if( PlayerPrefs.GetString("Controller") != null)
        {
            PlayButton.SetActive(true);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }
    public void PlayGame()
    {
        PlayerPrefs.SetString("Username", PlayerName.text ?? "RandomPlayer"+Random.Range(0,1000)); PlayerPrefs.Save();

        SceneManager.LoadScene(1);
    }
}
