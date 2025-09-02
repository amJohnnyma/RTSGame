using UnityEngine;

[CreateAssetMenu(menuName = "Entity/EntityMoveable")]
public class EntityMoveable : Entity
{
    public float moveSpeed = 5f;
    public float rotationSpeed = 5f;
    public float gravityStrength = 50f;
    public float planetRadius = 10f;
}   