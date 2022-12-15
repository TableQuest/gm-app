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
    public int statModifier;

    public Skill(int id, string name, int manaCost, int range, int maxTarget, int statModifier)
    {
        this.id = id;
        this.name = name;
        this.manaCost = manaCost;
        this.range = range;
        this.maxTarget = maxTarget;
        this.statModifier = statModifier;
    }


}
