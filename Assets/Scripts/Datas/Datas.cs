using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Datas : MonoBehaviour
{
    public List<Character> charactersList;
    private const string URL = "localhost:3000";

    public Datas()
    {
        charactersList = new List<Character>();
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
