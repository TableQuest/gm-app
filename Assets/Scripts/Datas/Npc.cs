using System;


public class Npc
{
    public string id;
    public string name;
    public int lifeMax;
    public int life;
    public string description;
    public string image;
    public string pawnCode;

    public Npc(string id, string name, int lifeMax, int life, string description, string image)
    {
        this.id = id;
        this.name = name;
        this.life = life;
        this.lifeMax = lifeMax;
        this.description = description;
        this.image = image;
    }
}

