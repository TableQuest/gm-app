using UnityEngine;
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
    public int speed;
    public string description;
    public string image;

    public List<Skill> skills;


    public GameObject panel;
    public Character(string playerId, int id, string name, int life, int lifeMax, int mana, int manaMax, int speed, string description, List<Skill> skills, string image)
    {
        this.playerId = playerId;
        this.id = id;
        this.name = name;
        this.life = life;
        this.lifeMax = lifeMax;
        this.mana = mana;
        this.manaMax = manaMax;
        this.speed = speed;
        this.description = description;
        this.panel = null;
        this.skills = skills;
        this.image = image;
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
