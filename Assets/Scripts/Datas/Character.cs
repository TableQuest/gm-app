using System;
using UnityEngine;

public class Character
{
    public string playerId;
    public int id;
    public string name;
    public int life;
    public int lifeMax;
    public string description;
    public GameObject panel;
    public Character(string playerId, int id, string name, int life, int lifeMax, string description)
    {
        this.playerId = playerId;
        this.life = life;
        this.id = id;
        this.name = name;
        this.lifeMax = lifeMax;
        this.description = description;
        this.panel = null;
    }

    public void LinkPanel(GameObject panel)
    {
        this.panel = panel;
    }

}
