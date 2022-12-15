using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character
{
    public string playerId;
    public int id;
    public string name;
    public int life;
    public int lifeMax;
    public int mana;
    public int manaMax;
    public string description;

    public List<Skill> skills;


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
        //this.skills = listSkill;
    }

    public void LinkPanel(GameObject panel)
    {
        this.panel = panel;
    }

    public void addSkill(Skill skill)
    {
        skills.Add(skill);
    }

}
