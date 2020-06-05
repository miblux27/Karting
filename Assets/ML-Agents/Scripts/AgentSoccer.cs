using System;
using UnityEngine;
using MLAgents;

public class AgentSoccer : Agent
{
    // Note that that the detectable tags are different for the blue and purple teams. The order is
    // * ball
    // * own goal
    // * opposing goal
    // * wall
    // * own teammate
    // * opposing player
    public enum Team
    {
        Blue = 0,
        Purple = 1
    }

    [HideInInspector]
    public Team team;
    float m_KickPower;
    int m_PlayerIndex;
    public SoccerFieldArea area;

    [HideInInspector]
    public Rigidbody agentRb;
    SoccerSettings m_SoccerSettings;
    BehaviorParameters m_BehaviorParameters;
    DecisionRequester m_DecisionRequester;
    KartMovement m_KartMovement;
    Vector3 m_Transform;

    public override void InitializeAgent()
    {
        base.InitializeAgent();
        m_BehaviorParameters = gameObject.GetComponent<BehaviorParameters>();
        m_DecisionRequester = gameObject.GetComponent<DecisionRequester>();
        m_KartMovement = gameObject.GetComponent<KartMovement>();
        m_Transform = transform.position;

        if (m_BehaviorParameters.m_TeamID == (int)Team.Blue)
        {
            team = Team.Blue;
        }
        else
        {
            team = Team.Purple;
        }

        m_SoccerSettings = FindObjectOfType<SoccerSettings>();
        agentRb = GetComponent<Rigidbody>();
        agentRb.maxAngularVelocity = 500;

        var playerState = new PlayerState
        {
            agentRb = agentRb,
            startingPos = transform.position,
            agentScript = this,
        };
        area.playerStates.Add(playerState);
        m_PlayerIndex = area.playerStates.IndexOf(playerState);
        playerState.playerIndex = m_PlayerIndex;
    }

    /*public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        m_KickPower = 0f;

        var forwardAxis = (int)act[0];
        var rightAxis = (int)act[1];
        var rotateAxis = (int)act[2];

        switch (forwardAxis)
        {
            case 1:
                dirToGo = transform.forward * 1f;
                m_KickPower = 1f;
                break;
            case 2:
                dirToGo = transform.forward * -1f;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                dirToGo = transform.right * 0.3f;
                break;
            case 2:
                dirToGo = transform.right * -0.3f;
                break;
        }

        switch (rotateAxis)
        {
            case 1:
                rotateDir = transform.up * -1f;
                break;
            case 2:
                rotateDir = transform.up * 1f;
                break;
        }

        transform.Rotate(rotateDir, Time.deltaTime * 100f);
        agentRb.AddForce(dirToGo * m_SoccerSettings.agentRunSpeed,
            ForceMode.VelocityChange);
    }*/

    public void MoveKart(float[] act)
    {
        var steering = 0f;
        var accel = 0f;
        var footbrake = 0f;

        var forwardAxis = (int)act[0];
        var rightAxis = (int)act[1];
        var jump = (int)act[2];

        switch (forwardAxis)
        {
            case 1:
                accel = 1f;
                break;
            case 2:
                footbrake = 1f;
                break;
        }

        switch (rightAxis)
        {
            case 1:
                steering = 1f;
                break;
            case 2:
                steering = -1f;
                break;
        }

        m_KartMovement.Move(steering, accel, footbrake, jump == 1);
    }

    public override void CollectObservations()
    {
        AddVectorObs(m_KartMovement.IsGrounded);
        if (!m_KartMovement.HasSecondJump)
            SetActionMask(2, 1);
    }

    public override void AgentAction(float[] vectorAction)
    {
        // Existential penalty for strikers.
        AddReward(-1f / 3000f);
        // MoveAgent(vectorAction);
        MoveKart(vectorAction);
        // Tweak the decision period depending on whether the kart is in the air or not
        // AdjustDecisionConfidence();
    }

    public void AdjustDecisionConfidence()
    {
        if (!m_KartMovement.IsGrounded)
            m_DecisionRequester.DecisionPeriod = 1;
        else
            m_DecisionRequester.DecisionPeriod = 5;
    }

    public override float[] Heuristic()
    {
        var action = new float[3];
        //forward
        if (Input.GetKey(KeyCode.W))
        {
            action[0] = 1f;
        }
        if (Input.GetKey(KeyCode.S))
        {
            action[0] = 2f;
        }
        //right
        if (Input.GetKey(KeyCode.D))
        {
            action[1] = 1f;
        }
        if (Input.GetKey(KeyCode.A))
        {
            action[1] = 2f;
        }
        //jump
        if (Input.GetKeyDown(KeyCode.Space))
        {
            action[2] = 1f;
        }
        return action;
    }

    /// <summary>
    /// Used to provide a "kick" to the ball.
    /// </summary>
    void OnCollisionEnter(Collision c)
    {
        m_KickPower = m_KartMovement.LocalSpeed > 0f ? 1f : 0f;
        if (c.gameObject.CompareTag("ball") && !m_KartMovement.IsGrounded)
        {
            var force = 2000f * m_KickPower;
            var dir = c.GetContact(0).point - transform.position;
            dir = dir.normalized;
            c.gameObject.GetComponent<Rigidbody>().AddForce(dir * force);
        }
    }

    public override void AgentReset()
    {
        if (team == Team.Purple)
        {
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, 90f, 0f);
        }
        transform.position = m_Transform;
        agentRb.velocity = Vector3.zero;
        agentRb.angularVelocity = Vector3.zero;
        m_KartMovement.Reset();
        SetResetParameters();
    }

    public void SetResetParameters()
    {
        area.ResetBall();
    }
}
