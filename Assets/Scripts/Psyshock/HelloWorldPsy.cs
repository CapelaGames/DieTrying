/*
using UnityEngine;
using Latios.Psyshock;
using SphereCollider = Latios.Psyshock.SphereCollider;
using Unity.Mathematics;
using Collider = Latios.Psyshock.Collider;
using Unity.Entities;

public class HelloWorldPsy : Subsystem
{

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //How to create Collider

        //Step 1: Create a sphere collider
        var sphere = new SphereCollider(float3.zero, 1f);

        //Step 2: Assign it to a Collider (union of all collider types)
        Collider collider = sphere;

        //Step 3: Attach the Collider to an entity
        EntityManager.AddComponentData(sceneBlackboardEntity, collider);

        //How to extract sphere collider

        //Step 1: Check type
        if (collider.type == ColliderType.Sphere)
        {
            //Step 2: Assign to a variable of the specialized type
            SphereCollider sphere2 = collider;

            //Note: With safety checks enabled, you will get an exception if you cast to the wrong type.
        }

        //EZ PZ, right?
    }

    // Update is called once per frame
    void Update()
    {

    }
}
*/
