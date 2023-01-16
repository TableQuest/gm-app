using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


public class Datas : MonoBehaviour
{
    public List<Character> charactersList;
    public List<Npc> placedNpcList;
    public List<Npc> npcList;
    public GameState gameState;
    public Datas()
    {
        charactersList = new List<Character>();
        npcList = new List<Npc>();
        placedNpcList = new List<Npc>();
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
