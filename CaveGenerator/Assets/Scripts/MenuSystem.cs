using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;


public class MenuSystem : MonoBehaviour
{
    public GameObject mainMenuHolder;
    public GameObject startOptionsMenuHolder;

    public Text widthText;
    public Text heightText;
    public Text chankXText;
    public Text chankYText;


    void Start()
    {
        widthText.text = "64";
        heightText.text = "64";
        chankXText.text = "8";
        chankYText.text = "8";
    }

    public void StartButton()
    {
        mainMenuHolder.SetActive(false);
        startOptionsMenuHolder.SetActive(true);
    }

    public void LoadGame()
    {
        mainMenuHolder.SetActive(false);
        startOptionsMenuHolder.SetActive(true);
    }

    public void Back()
    {
        mainMenuHolder.SetActive(true);
        startOptionsMenuHolder.SetActive(false);
    }

    public void ExitButton()
    {
        Application.Quit();
    }

    public void ChangedWidthSliderValue(float value)
    {
        MapGenerator.width = (int)value;
        widthText.text = value.ToString();
    }
    public void ChangedHeightSliderValue(float value)
    {
        MapGenerator.height = (int)value;
        heightText.text = value.ToString();
    }
    public void ChangedChankXSliderValue(float value)
    {
        MapGenerator.chankSizeX = (int)value;
        chankXText.text = value.ToString();
    }
    public void ChangedChankYSliderValue(float value)
    {
        MapGenerator.chankSizeY = (int)value;
        chankYText.text = value.ToString();
    }
}
