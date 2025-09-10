using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Type
{
    NONE,
    BUILDING,
    HARVESTABLE
}

[System.Serializable]
public struct TeamData //this doesnt seem too efficient. -> Finding alliances is a for loop. Maybe a B tree? Something scalable and easy to loop through. Undirected graph?
{
    string name;
    List<string> alliances;

    public override string ToString()
    {
        string allies = (alliances != null && alliances.Count > 0)
            ? string.Join(", ", alliances)
            : "None";
        return $"Team(Name={name}, Alliances=[{allies}])";
    }
}

// damage calculation
// Damage = (Base * DamageType) / (1 + other.defense) -> Defense of this damage type

[System.Serializable]
public struct Attack // what type of attack does it do
{
    string type; // name of the attack
    float damage; //base damage
    float attackRange; // range
    float attackAngle; //control cone, AOE, etc.
    Transform attackPos; // where does the attack start
    DamageType damageType; // what type of damage does it do
                           // 
    
    public override string ToString()
    {
        string pos = attackPos != null ? attackPos.name : "None";
        return $"Attack(Type={type}, Damage={damage}, Range={attackRange}, Angle={attackAngle}, Pos={pos}, {damageType})";
    }
    
}

[System.Serializable]
public struct DamageType
{
    string type; // the type of damage
    float value; // damage modifier of this type 0 to inf

    public override string ToString()
    {
        return $"DamageType(Type={type}, Value={value})";
    }


}

[System.Serializable]
public struct DefenseType
{
    string type; // type of defense
    float value; // resistance against a type -inf to inf

    public override string ToString()
    {
        return $"DefenseType(Type={type}, Value={value})";
    }
}


[System.Serializable]
public class EntityStats : MonoBehaviour
{

    public string entityName = "default";
    public int moveable = 0; // incase i want different moveable types
    public float health = 10; // entity hit points

    public Type type = Type.NONE; // building, creature, etc.
    public string eTag = "EntitySelectable";

    public TeamData teamData; //teams name, alliances
    public List<Attack> attacks;
    public List<DefenseType> defences;


    void Awake()
    {
        this.gameObject.tag = eTag;

    }
    public override string ToString()
    {
        string atkStr = (attacks != null && attacks.Count > 0)
            ? string.Join("\n    ", attacks)
            : "None";

        string defStr = (defences != null && defences.Count > 0)
            ? string.Join("\n    ", defences)
            : "None";

        return $"EntityStats(Name={entityName}, Type={type}, Health={health}, Moveable={moveable})\n" +
               $"  {teamData}\n" +
               $"  Attacks:\n    {atkStr}\n" +
               $"  Defences:\n    {defStr}";
    }
   

}
