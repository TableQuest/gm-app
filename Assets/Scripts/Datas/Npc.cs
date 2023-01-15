using System;
public class Npc
{
    public string id;
    public string name;
    private int lifeMax;
    private int life;
    private string description;

    public Npc(string id, string name, int lifeMax, int life, string description)
    {
        this.id = id;
        this.name = name;
        this.life = life;
        this.lifeMax = lifeMax;
        this.description = description;
    }
    
    
}
