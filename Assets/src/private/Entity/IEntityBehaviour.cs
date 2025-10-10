using UnityEngine;
using System.Collections.Generic;
public interface IEntityBehaviour
{
    void ComputeMove(
        EntityRuntime entity,
        Vector3 entityPosition
        );

        void OnTargetReached(EntityRuntime entity);

}

public class ScoutBehavior : IEntityBehaviour
{
    private World world;
    private float visionRadius;



    public ScoutBehavior(World world, float visionRadius = 1f)
    {
        this.world = world;
        this.visionRadius = visionRadius * 4;
    }

    public void ComputeMove(EntityRuntime entity, Vector3 entityPos)
    {
        entity.currentTask.UpdateTask(entityPos);
        
    }

    public void OnTargetReached(EntityRuntime entity)
    {
        entity.currentTask.OnTargetReached();
    }
}

public class HarvestBehaviour : IEntityBehaviour
{

    private World world;
    private float visionRadius;



    public HarvestBehaviour(World world, float visionRadius = 1f)
    {
        this.world = world;
        this.visionRadius = visionRadius * 4;
    }


    public void ComputeMove(EntityRuntime entity, Vector3 entityPos)
    {

        entity.currentTask.UpdateTask(entityPos);
 
    }

     public void OnTargetReached(EntityRuntime entity)
    {
        entity.currentTask.OnTargetReached();

    }
}
