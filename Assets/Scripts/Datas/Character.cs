using System;
using UnityEngine;

public class Character : MonoBehaviour
{
    public int id;
    public string name;
    public int life;
    public int lifeMax;
    public string description;
    public GameObject panel;
    public Character(int id, string name, int life, int lifeMax, string description)
    {
        this.life = life;
        this.id = id;
        this.name = name;
        this.lifeMax = lifeMax;
        this.description = description;
    }

    public void LinkPanel(GameObject panel)
    {
        this.panel = panel;
    }
}
