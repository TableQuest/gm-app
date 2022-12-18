using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Datas : MonoBehaviour
{
    public List<Character> charactersList;
    public GameState gameState;
    public Datas()
    {
        charactersList = new List<Character>();
        gameState = GameState.INIT;
    }

    public void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}

public enum GameState
{
    INIT, FREE, RESTRICTED, TURN
}
