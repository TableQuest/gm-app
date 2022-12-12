using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player 
{
    private string id;
    private Character character;

    public Player(string id, Character character)
    {
        this.id = id;  
        this.character = character; 
    }

    public string getID()
    {
        return this.id;
    }

    public Character getCharacter()
    {
        return this.character;
    }
}
