using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Restart2048_2 : MonoBehaviour
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
        game2048_2.SetActive(false);
        SceneManager.LoadScene(0);
        //mainScreen_2.SetActive(false);

        
        game2048_2.SetActive(true);
    }
}
