using UnityEngine;
using UnityEngine.AI;

public class EnemyChaseScript : MonoBehaviour
{
    

    public Transform target;
    public NavMeshAgent agent;

    private void Update()
    {

        agent.SetDestination(target.position);

    }








}
