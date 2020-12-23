﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShpdAnimal : Actor
{
    UIShpdAnimal ui;

    private float hungerPercentage = 100f;   
    public float hungerDecrease = 10f;

    //order of behaviours in the movebeahviour options for sheep should match the oder of states in this enum
    public enum State
    {
        Stop,
        Dawdle,
        FollowShepard,
        FindFood
    }
    public State state;
    State prevState;

    float attentionTime;
    float aTimer = 0;

    public override  void Begin()
    {

        ui = GetComponent<UIShpdAnimal>();
        ui.Initialize(hungerPercentage);
        state = State.Dawdle;
        currentMoveBehaviour = moveBehaviourOptions[(int)State.Dawdle];
        base.Begin();
    }
    public void SetShepard(Shepard _shpd)
    {
        leader = _shpd;
    }

    void Update()
    {
        ui.SetValues(hungerPercentage);
        ui.StateDebugging(state);
        CheckStatus();
      
        //Debug.Log(target);
        switch (state)
        {
            case State.Stop:
                OnStop();
                break;
            case State.Dawdle:
                OnDawdle();
                break;
            case State.FollowShepard:
                OnFollowShepard();
                break;
            case State.FindFood:
                OnFindFood();
                break;
        }

       


        BUpdate();
    }

    // checks sheeps statitiscs for the need to change its state
    void CheckStatus()
    {       
        if(hungerPercentage < 100)
        {
            if(ContextFilter.FilterContext(ItemsInView, "Plant").Count != 0)
            {
                state = State.FindFood;
                return;
            }           
        }

        if (ItemsInProximity.Contains(interest))
        {
            state = State.Stop;
            return;
        }
        else if (state == State.Stop)
        {
            if(prevState == State.Stop)
            {
                state = State.Dawdle;
            }
            state = prevState;
        }

        // state = State.Dawdle;
    }

    #region StateFunctions
    void OnStop()
    {
        AttentionTimer();
        currentMoveBehaviour = moveBehaviourOptions[(int)State.Stop];                   
        //prevState = state; stop shouldntreally ever be a previous state;

    }

    void OnDawdle()
    {
        //if(prevState != state)
        //{
            prevState = state;
            currentMoveBehaviour = moveBehaviourOptions[(int)State.Dawdle];
            interest = transform;
        //}
       
    }

    void OnFollowShepard()
    {
        Debug.Log("Whistle");
        AttentionTimer();
        //if(prevState != state)
        //{
            currentMoveBehaviour = moveBehaviourOptions[(int)State.FollowShepard];
            interest = leader.transform;
            prevState = state;
        //}
        
    }

    void OnFindFood()
    {
        //Debug.Log(interest.gameObject.tag);
        if(interest.gameObject.tag != "Plant") // need to handle in case of no food
        {
            List<Transform> foodInView = ContextFilter.FilterContext(ItemsInView, "Plant");
            foreach (Transform item in foodInView)
            {
                Food food = item.GetComponent<Food>();
                if (!food.taken)
                {

                    food.taken = true;
                    interest = item;
                    currentMoveBehaviour = moveBehaviourOptions[(int)State.FindFood];
                    break;
                }
            }
            
          
            prevState = state;
        }
      
    }

    #endregion

    public void DecreaseHunger()
    {
        Debug.Log("HUngedecreaase");
        hungerPercentage -= hungerDecrease;
    }

    void AttentionTimer()
    {
        aTimer += Time.deltaTime;
        if (aTimer > attentionTime)
        {
            aTimer = 0;
            state = State.Dawdle; //will need to be changed to whatever occurs when lost shepards attention;
        }
    }

    public void NotifyWhistle(Shepard shpd)
    {
        
        state = State.FollowShepard;       
        attentionTime = Random.Range(8, 15);
    }
}
