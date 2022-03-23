using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainScreen_2 : MonoBehaviour
{
    [SerializeField] GameObject mainScreen_2;
    [SerializeField] GameObject game2048_2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void restart2048()
    {
        SceneManager.LoadSceneAsync(0);
        //mainScreen_2.SetActive(true);
        //game2048_2.SetActive(false);
        mainScreen_2.SetActive(false);
        game2048_2.SetActive(true);
    }

    public void display2048()
    {
        mainScreen_2.SetActive(false);
        game2048_2.SetActive(true);
    }

    public void backMain()
    {
        SceneManager.LoadSceneAsync(0);
        game2048_2 .SetActive(false);
        mainScreen_2.SetActive(true);
    }
}
