﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class player : MonoBehaviour
{
    public Vector3 movement_direction;
    public float walking_velocity = 5f;
    public float velocity;
    public float max_velocity;
    public float acceleration = 10.0f;
    public float turn_speed = 1.5f;

    public string state;

    private Animator animation_controller;
    private CharacterController character_controller;
    private GameObject playerModel;
    private bool upKey;
    private bool downKey;
    private bool leftKey;
    private bool rightKey;
    private bool ctrlDown;
    private bool shiftDown;
    private bool spaceDown;

    // tiny helper to save time, if forward is true update forwards, else backwards
    // if turn is true, update rotation, if not do not
    void VelocityUpdate(bool forward)
    {
        if (forward)
        {
            velocity += acceleration;
            if (velocity > max_velocity)
                velocity = max_velocity;
        }
        else
        {
            velocity -= acceleration;
            if (velocity < - max_velocity)
                velocity = - max_velocity;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        animation_controller = GetComponentInChildren<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        velocity = 0.0f;
        state = "idle";
        playerModel = GameObject.Find("PlayerModel");
    }

    // Update is called once per frame
    void Update()
    {
        // Check all keys first for convenience
        upKey = Input.GetKey("w") || Input.GetKey(KeyCode.UpArrow);
        downKey = Input.GetKey("s") || Input.GetKey(KeyCode.DownArrow);
        leftKey = Input.GetKey("a") || Input.GetKey(KeyCode.LeftArrow);
        rightKey = Input.GetKey("d") || Input.GetKey(KeyCode.RightArrow);
        ctrlDown = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
        shiftDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        spaceDown = Input.GetKey(KeyCode.Space);

        // set state based on input
        /* in animation controller, states controlled by int
         * 0 = idle
         * 1 = walk forward
         * 2 = walk backward
         * 3 = run forward
         * 4 = jumping
        */

        if (state != "jump" && spaceDown)
            state = "jump";
        else if (upKey && shiftDown)
            state = "run";
        else if (leftKey || rightKey)
        {
            if (downKey)
                state = "backwardWalk";
            else
                state = "forwardWalk";
        }
        else if (upKey)
            state = "forwardWalk";
        else if (downKey)
            state = "backwardWalk";
        else
            state = "idle";

        // FSM for character behavior, also update velocity and handle turning
        switch (state)
        {
            case "idle":
                animation_controller.SetInteger("state", 0);
                max_velocity = walking_velocity;
                VelocityUpdate(false);
                break;
            case "forwardWalk":
                animation_controller.SetInteger("state", 1);
                max_velocity = walking_velocity;
                VelocityUpdate(true);
                break;
            case "backwardWalk":
                animation_controller.SetInteger("state", 2);
                max_velocity = walking_velocity;
                VelocityUpdate(false);
                break;
            case "jump":
                animation_controller.SetInteger("state", 4);
                max_velocity = walking_velocity;
                if (upKey)
                    VelocityUpdate(true);
                else if (downKey)
                    VelocityUpdate(false);
                else
                {
                    max_velocity = walking_velocity;
                    VelocityUpdate(true);
                }
                break;
            case "run":
                animation_controller.SetInteger("state", 3);
                max_velocity = 2.0f * walking_velocity;
                VelocityUpdate(true);
                break;
            default:
                break;
        }

        // update movement direction based on keys
        float xdirection = 0.0f;
        float zdirection = 0.0f;
        // dir is the direction to rotate based on movement

        string dir = "north";
        if (upKey || downKey)
        {
            // case where no strafing, go straight, also do this if both keys are held
            xdirection = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
            zdirection = Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
            if (downKey)
                dir = "south";
            else
                dir = "north";
        }
        if (leftKey && !rightKey && (upKey || downKey))
        {
            // strafing left case
            if (upKey)
            {
                xdirection = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y - 45.0f));
                zdirection = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y - 45.0f));
                dir = "northEast";

            }
            else if (downKey)
            {
                xdirection = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45.0f));
                zdirection = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45.0f));
                dir = "southEast";
            }
        }
        else if (rightKey && !leftKey && (upKey || downKey))
        {
            // strafing right case
            if (upKey)
            {
                xdirection = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45.0f));
                zdirection = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 45.0f));
                dir = "northWest";
            }
            else if (downKey)
            {
                xdirection = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y - 45.0f));
                zdirection = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y - 45.0f));
                dir = "southWest";
            }
        }
        else if (leftKey && !rightKey)
        {
            // moving left case
            xdirection = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y - 90.0f));
            zdirection = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y - 90.0f));
            dir = "east";
        }
        else if (rightKey && !leftKey)
        {
            // moving right case
            xdirection = Mathf.Sin(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 90.0f));
            zdirection = Mathf.Cos(Mathf.Deg2Rad * (transform.rotation.eulerAngles.y + 90.0f));
            dir = "west";
        }

        movement_direction = new Vector3(xdirection, 0.0f, zdirection);
        float newY = RotationUpdate(dir);
        playerModel.transform.eulerAngles = new Vector3(0, newY, 0);

        character_controller.Move(movement_direction * velocity * Time.deltaTime);
    }


    // helper to rotate player model
    // if turn is true, update rotation, if not do not
    float RotationUpdate(string dir)
    {
        float result;
        switch (dir)
        {
            case "north":
                result = transform.rotation.eulerAngles.y;
                break;
            case "south":
                result = transform.rotation.eulerAngles.y + 180;
                break;
            case "east":
                result = transform.rotation.eulerAngles.y - 90;
                break;
            case "west":
                result = transform.rotation.eulerAngles.y + 90;
                break;
            case "northEast":
                result = transform.rotation.eulerAngles.y - 45;
                break;
            case "northWest":
                result = transform.rotation.eulerAngles.y + 45;
                break;
            case "southEast":
                result = transform.rotation.eulerAngles.y - 135;
                break;
            case "southWest":
                result = transform.rotation.eulerAngles.y + 135;
                break;
            default:
                result = transform.rotation.eulerAngles.y;
                break;
        }
        return result;
    }
}
