using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavigateBetweenMenu : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (Transform child in gameObject.transform)
        {
            if (child.name == "SceneButton"){
                child.GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene("Scene");});
            }
            if (child.name == "PlayerButton")
            {
                child.GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene("Player"); });
            }
            if (child.name == "NPCButton")
            {
                child.GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene("NPC"); });
            }
            if (child.name == "DiceButton")
            {
                child.GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene("Dices"); });
            }
            if (child.name == "LogsButton")
            {
                child.GetComponent<Button>().onClick.AddListener(delegate { SceneManager.LoadScene("Logs"); });
            }
        }
    }

}
