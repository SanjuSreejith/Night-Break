using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class GhostTeleportManager : MonoBehaviour
{
    public GameObject ghost;
    public GameObject player;
    public Transform ghostTeleportPosition;
    public Transform playerTeleportPosition;
    public Transform patrolRootMainMaze;

    private NavMeshAgent ghostAgent;
    private GhostAI ghostAI;

    void Start()
    {
        if (GameSettings.CurrentMode == GameSettings.GameMode.Endless)
        {
            StartCoroutine(SetupDelayed());
        }
    }

    IEnumerator SetupDelayed()
    {
        yield return new WaitForSeconds(0.1f); // Let scene load

        if (!ghost || !player || !ghostTeleportPosition || !playerTeleportPosition || !patrolRootMainMaze)
        {
          
            yield break;
        }

        if (!ghost.activeInHierarchy)
        {
            ghost.SetActive(true);
            yield return new WaitForSeconds(0.05f); // Ensure activation
        }

        // 1. Teleport Player
        player.transform.position = playerTeleportPosition.position;
        player.transform.rotation = playerTeleportPosition.rotation;

        // 2. Setup Ghost
        ghostAgent = ghost.GetComponent<NavMeshAgent>();
        ghostAI = ghost.GetComponent<GhostAI>();

        if (ghostAgent != null)
        {
            ghostAgent.Warp(ghostTeleportPosition.position); // Safe teleport
            ghostAgent.ResetPath();
        }
        else
        {
            ghost.transform.position = ghostTeleportPosition.position;
        }

        ghost.transform.rotation = ghostTeleportPosition.rotation;

        if (ghostAI == null)
        {
         
            yield break;
        }

        // 3. Update Patrol Points
        List<Transform> newPatrolList = new List<Transform>();
        foreach (Transform point in patrolRootMainMaze)
        {
            newPatrolList.Add(point);
        }

        if (newPatrolList.Count == 0)
        {
            
            yield break;
        }

        ghostAI.patrolPoints = newPatrolList.ToArray();

        // 4. Reset Patrol Index
        var patrolIndexField = typeof(GhostAI).GetField("currentPatrolIndex",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (patrolIndexField != null)
        {
            patrolIndexField.SetValue(ghostAI, 0);
        }

      
    }
}
