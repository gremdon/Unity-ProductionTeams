﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Events;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovementBehaviour : MonoBehaviour, IDamager
{
    public NavMeshAgent _NavMeshAgent;
    [SerializeField]
    public Stats EnemyStats;
    [SerializeField]
    public Transform TargetTower;
    public float MovementSpeed;
    public float DistanceFromTarget;    
    [Tooltip("Distance the enemy must be from target to trigger a state change")]
    public float DistanceToTrigger;
   
    private enum States
    {
        idle, walk, attack
    }

    [SerializeField]
    private States CurrentState;

    public EventEnemyLarvaAttack OnEnemyLarvaAttack;

    void Awake()
    {
        _NavMeshAgent = GetComponent<NavMeshAgent>();                
        _NavMeshAgent.speed = MovementSpeed;
    }

    private float attackTimer;
    private bool canAttack = true;

    private void Update()
    {
        if (TargetTower.GetComponent<IDamageable>() == null || CurrentState != States.attack)
            return;

        if (canAttack)
            attackTimer += Time.deltaTime;

        if (attackTimer >= EnemyStats.Items["larvaenemyattackspeed"].Value)
        {
            canAttack = false;            
            OnEnemyLarvaAttack.Invoke(this.gameObject);
        }
    }

    IEnumerator Idle()
    {
        if (TargetTower == null)
            yield return null;
        int LoopCounter = 0;
        CurrentState = States.idle;
        _NavMeshAgent.isStopped = true;                        
        while (LoopCounter <= 1000)
        {
            LoopCounter++;
            DistanceFromTarget = Vector3.Distance(transform.position, _NavMeshAgent.destination);
            transform.LookAt(TargetTower.position);
            if (DistanceFromTarget > DistanceToTrigger)
            {                
                yield return StartCoroutine("Walk");
            }
            else if(CurrentState != States.attack)
                CurrentState = States.attack;
            yield return null;
        }        
    }

    IEnumerator Walk()
    {
        int LoopCounter = 0;
        CurrentState = States.walk;
        _NavMeshAgent.isStopped = false;
        while (LoopCounter <= 1000)
        {
            LoopCounter++;
            DistanceFromTarget = Vector3.Distance(transform.position, _NavMeshAgent.destination);
            if (DistanceFromTarget <= DistanceToTrigger)
                yield return StartCoroutine("Idle");
            yield return null;
        }
    }

    public void ChangeTarget(GameObject otherTarget)
    {
        StopAllCoroutines();
        TargetTower = otherTarget.transform;
        StartCoroutine("Idle");
    }

    public void ResetAttackTimer()
    {
        attackTimer = 0;
        canAttack = false;
    }

    public void StartAttackTimer()
    {
        canAttack = true;
    }

    public void Attack()
    {
        DoDamage(TargetTower.GetComponent<IDamageable>());
    }  

    public void DoDamage(IDamageable target)
    {
        target.TakeDamage(10);
    }

    [System.Serializable]
    public class EventEnemyLarvaAttack : UnityEvent<GameObject>
    {

    }
}
