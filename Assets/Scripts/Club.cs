﻿

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Club : MonoBehaviour
{
    public Transform ballTarget;

    // Cached references
    public Ball ball;
    public Rigidbody2D clubRb;
    public Rigidbody2D clubHookRb;
    public GameObject clubHook;

    // Public variables
    [SerializeField] public float speed = 5f;
    [SerializeField] public float releaseTime = 0.5f;
    [SerializeField] public float maxDragDistance = 2f;
    public bool allowCameraMove = false;

    // Private variables
    private bool isPressed = false;
    private bool alreadyExecuted = false;
    private Vector3 ballPos;

    // Start
    private void Start()
    {
        ball = FindObjectOfType<Ball>();
        clubHook.gameObject.transform.position = ball.transform.position;
    }

    // Update
    private void Update()
    {
        if (isPressed)
        {
            PreparingShoot();
        }

        if (ball != null)
        {
            if (!alreadyExecuted && ball.rb.velocity.magnitude <= 0.02f)
            // alreadyExecuted prevents it from running every frame
            {
                MakeClubInvisible(false);
                UpdateHookPosition();
            }
        }

        Physics2D.IgnoreLayerCollision(8, 9);

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        FindObjectOfType<GameSystem>().AddShot();
        MakeClubInvisible(true);
    }

    // Updating hook's position into ball's position, needed
    // for shooting the ball again
    private void UpdateHookPosition()
    {
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);

        Debug.Log("UpdateHookPosition() running");
        ballPos = ballTarget.position;
        clubHook.gameObject.transform.position = ballPos;
        ballPos += new Vector3(0f, -0.5f, 0f);
        transform.position = ballPos;
        alreadyExecuted = true;
    }

    // Updating the position, rotation etc for the club in realtime while preparing the shoot
    private void PreparingShoot()
    {
        clubHook.gameObject.transform.position = ball.transform.position;

        Vector2 direction = ballTarget.position - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        rotation *= Quaternion.Euler(0, 0, -90);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, speed * Time.deltaTime);

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (Vector3.Distance(mousePos, clubHookRb.position) > maxDragDistance)
            clubRb.position = clubHookRb.position + (mousePos - clubHookRb.position).normalized * maxDragDistance;
        else
            clubRb.position = mousePos;
    }

    // Executes as soon as mouse click is down
    private void OnMouseDown()
    {
        if (clubRb.velocity.magnitude <= 0.02f) // Checks if ball is not moving
        {
            GetComponent<SpringJoint2D>().enabled = true;
            allowCameraMove = false;
            isPressed = true;
            clubRb.isKinematic = true;
        }
    }

    // Executes as soon as mouse click is released
    private void OnMouseUp()
    {
        if (clubRb.velocity.magnitude <= 0.02f)
        {
            allowCameraMove = true;
            isPressed = false;
            clubRb.isKinematic = false;

            StartCoroutine(Release());
        }
    }

    // This coroutine shoots the ball, using component "SpringJoint2D"
    IEnumerator Release()
    {
        yield return new WaitForSeconds(releaseTime);
        GetComponent<SpringJoint2D>().enabled = false;
        yield return new WaitForSeconds(2f);

        alreadyExecuted = false;
    }

    public void MakeClubInvisible(bool status)
    {
        if (status)
            GameObject.Find("Club").transform.localScale = new Vector3(0, 0, 0);
        else if (!status)
            GameObject.Find("Club").transform.localScale = new Vector3(5, 1, 1);
    }
}
