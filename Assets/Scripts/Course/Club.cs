﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Club : MonoBehaviour
{
    // Cached references
    public Ball theBall;
    public Rigidbody2D clubRb;
    public Rigidbody2D clubHookRb;
    public GameObject clubHook;

    // Public variables
    [SerializeField] public float speed = 5f;
    [SerializeField] public float releaseTime = 0.5f;
    [SerializeField] public float maxDragDistance = 2f;
    public bool allowCameraMove = true;
    public bool ongoingShoot = false;
    public bool isPressed = false;

    // Private variables
    private bool alreadyExecuted = false;
    private bool shootIsReleased = false;
    private Vector3 ballPos;
    private Vector2 inputPos;
    private float offset = 0.4f; // Use 0f for PC/Mac build

    // --- START ---
    private void Start()
    {
        // Club's collider ignores Course's colliders
        Physics2D.IgnoreLayerCollision(8, 9);

        // Locking the hook into ball's position
        PositionClubHook();
    }

    // --- UPDATE ---
    private void Update()
    {
        if (isPressed)
        {
            PreparingShoot();
        }

        if (theBall != null)
        {
            // Need to have this if statement for Course 3 and 4 to prevent a bug
            if (SceneManager.GetActiveScene().buildIndex == 3 || SceneManager.GetActiveScene().buildIndex == 4)
            {
                if (!alreadyExecuted && theBall.rb.velocity.magnitude <= 0.00f)
                {
                    ongoingShoot = false;
                    MakeClubInvisible(false);
                    UpdateHookPosition();
                }
            }
            // When ball is still, this executes. Why 0.02: if 0, player would wait too long to the ball to stop rolling
            else if (!alreadyExecuted && theBall.rb.velocity.magnitude <= 0.02f)
            // alreadyExecuted prevents it from running every frame
            {
                ongoingShoot = false;
                MakeClubInvisible(false);
                UpdateHookPosition();
            }
        }
    }

    // --- METHODS ---

    // When club colliders with ball
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (shootIsReleased)
        {
            FindObjectOfType<GameSystem>().AddShot();
            MakeClubInvisible(true);
        }
    }

    public void PositionClubHook()
    {
        clubHook.gameObject.transform.position = theBall.transform.position;
    }

    // Updating hook into ball's position, needed
    // for shooting the ball again
    public void UpdateHookPosition()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Debug.Log("UpdateHookPosition() running");
        ballPos = theBall.transform.position;
        PositionClubHook();
        ballPos += new Vector3(0f, -0.5f, 0f);
        transform.position = ballPos;
        alreadyExecuted = true;
    }

    // Updating the position, rotation etc for the club in realtime while preparing the shoot
    private void PreparingShoot()
    {
        // Attching hook onto the ball 
        PositionClubHook();

        // Facing the club to ball
        Vector3 direction = theBall.transform.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        rotation *= Quaternion.Euler(0, 0, -90);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);

        // Positions of player's input
        inputPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // Vector3 position = wished position to compare, this case: input
        Vector3 position = (Vector3)Vector2.Lerp(clubRb.position, inputPos,
                            Mathf.Clamp(Vector3.Distance(clubRb.position, inputPos), 0f, 0.5f)) +
                            (theBall.transform.position - (Vector3)inputPos).normalized * offset;

        // Max drag distance
        if (Vector3.Distance(position, theBall.transform.position) > maxDragDistance)
            transform.position = clubHookRb.position + ((inputPos) - clubHookRb.position).normalized * maxDragDistance;
        else
            transform.position = position;
    }

    // Executes as soon as mouse click is down
    private void OnMouseDown()
    {
        theBall.rb.bodyType = RigidbodyType2D.Static;

        isPressed = true;
        ongoingShoot = true;
        allowCameraMove = false;
        shootIsReleased = false;
        clubRb.isKinematic = true;
    }

    // Executes as soon as mouse click is released, activating the component SpringJoint2D
    private void OnMouseUp()
    {
        theBall.rb.bodyType = RigidbodyType2D.Dynamic;
        GetComponent<SpringJoint2D>().enabled = true;

        isPressed = false;
        allowCameraMove = true;
        shootIsReleased = true;
        clubRb.isKinematic = false;

        StartCoroutine(Release());
    }

    // This coroutine shoots the ball, deactivating the component SpringJoint2D
    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);
        GetComponent<SpringJoint2D>().enabled = false;
        clubRb.velocity = Vector2.zero;
        alreadyExecuted = false;
    }

    // "Removing" the club gameobject, 
    // executing when the club colliders with the ball (after released shoot)
    public void MakeClubInvisible(bool status)
    {
        if (status)
            GameObject.Find("Club").transform.localScale = new Vector3(0, 0, 0);
        else if (!status)
            GameObject.Find("Club").transform.localScale = new Vector3(6, 1, 1);
    }
}
