using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skill 
{
    public int id;
    public string name;
    public int manaCost;
    public int range;
    public int maxTarget;
    public string type;
    public int statModifier;
    public bool healing;
    public string image;

    public Skill(int id, string name, int manaCost, 
        int range, int maxTarget, string type,
        int statModifier, bool healing, string image)
    {
        this.id = id;
        this.name = name;
        this.manaCost = manaCost;
        this.range = range;
        this.maxTarget = maxTarget;
        this.type = type;
        this.statModifier = statModifier;
        this.healing = healing;
        this.image = image;
    }


}
