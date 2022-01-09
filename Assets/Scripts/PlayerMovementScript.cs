using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum PlayerState
{
    Standing,
    Walking,
    Sprinting,
    Jump,
    DoubleJump,
    Walled,
    Boosting
}

public class PlayerStateEvent : UnityEvent<PlayerState>  { };
public class JetpackEvent : UnityEvent<float> { };

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField]
    private float jetpackVerticalSpeedLimit = 4f;

    [SerializeField]
    private bool slowMode = false;

    public static PlayerStateEvent PlayerStateEvent;
    public static JetpackEvent JetpackEvent;

    public PlayerState state;
    private Rigidbody2D rb2d;
    private GroundCheck gc;
    private Animator anim;
    private List<WallCheck> wcs;
    ParticleSystem ps;

    
    private float x, y;
    private bool sprinting = false;
    private bool doubleJumpUsed = false;
    
    private const float jumpMod = 6f;
    private const float lowSpeedCap = 3f;
    private const float sprintAmp = 1.55f;

    //Introduces the high speedcap
    private const float highSpeedCap = lowSpeedCap * sprintAmp;

    private const float fixedDeltaFactor = 500f;
    private const float deltaFactor = 50f;
    [Range(-0.2f, 1.2f)] [SerializeField]
    private float boost = 1f;
    [SerializeField]
    private bool boostDrain = false;
    bool firstDrain = false;
    Coroutine boostHandler;

    private void Awake()
    {
        PlayerStateEvent = new PlayerStateEvent();
        JetpackEvent = new JetpackEvent();
        
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        rb2d = GetComponent<Rigidbody2D>();
        gc = GetComponentInChildren<GroundCheck>();
        wcs = new List<WallCheck>( GetComponentsInChildren<WallCheck>());
        boostHandler = StartCoroutine(BoostDrainer());
        ps = GetComponent<ParticleSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        x = Input.GetAxis("Horizontal");
        sprinting = Input.GetKey(KeyCode.LeftShift);

        state = StateChecker();
        handleInput(state);
        
    }

    private void FixedUpdate()
    {
        handleX(x);
    }


    private void handleInput(PlayerState state)
    {

        
        handleJump();
        handleBoost();
    }

    private void handleBoost()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && boost > 0f)
        {
            AlterDrain(true);
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            AlterDrain(false);
        }
    }

    private void AlterDrain(bool state)
    {
        boostDrain = state;
        if (state)
        {
            ps.Play();
        }
        else
        {
            ps.Stop();
        }
    }

    IEnumerator BoostDrainer()
    {
        
        while (true)
        {
            yield return new WaitForFixedUpdate();
            //Jetpack only activates if speed isn't too high already.
            if(rb2d.velocity.y <= jetpackVerticalSpeedLimit)
            {
                if (boostDrain && boost >= 0)
                {
                    boost -= 1f * Time.deltaTime;
                    if(rb2d.velocity.y < 0.1f)
                    {
                        rb2d.AddForce(Vector2.up * Time.fixedDeltaTime * deltaFactor * (slowMode ? 10f : 7f), firstDrain ? ForceMode2D.Impulse : ForceMode2D.Force);
                        if (firstDrain)
                        {
                            firstDrain = false;
                        }
                    }
                    else
                    {
                        rb2d.AddForce(Vector2.up * Time.fixedDeltaTime * deltaFactor * (slowMode ? 15f : 3.5f));
                        if (!firstDrain)
                        {
                            firstDrain = true;
                        }
                    }
                    
                    if(boost < 0)
                    {
                        AlterDrain(false);
                    }
                }
                else if ((state == PlayerState.Standing || state == PlayerState.Walking) && !boostDrain && boost <= 1)
                {
                    boost += 0.6f * Time.deltaTime;
                }

                JetpackEvent.Invoke(boost);
            }
            
        }
        
    }

    private void handleX(float x)
    {
        //Handles movement on ground and in the air
        
        if ( (state != PlayerState.DoubleJump || state != PlayerState.Jump) && Mathf.Abs(rb2d.velocity.x + x) < highSpeedCap)
        {
            //Debug.Log("1");
            
            rb2d.AddForce(Vector2.right * 3 * fixedDeltaFactor * x * Time.fixedDeltaTime);
        }
        //Handles movement in the air if speed is too high (I.e. after a walljump)
        else if((state == PlayerState.Jump || state == PlayerState.DoubleJump || state == PlayerState.Boosting) && (rb2d.velocity.x > lowSpeedCap && x < 0) || (rb2d.velocity.x < -lowSpeedCap && x > 0))
        {
            //Debug.Log("2");
            rb2d.AddForce(Vector2.right * fixedDeltaFactor * x * Time.fixedDeltaTime, ForceMode2D.Force);
        }
        //Handles movement in the air below the high speedcap
        else if((state == PlayerState.DoubleJump || state == PlayerState.Jump) && Mathf.Abs(rb2d.velocity.x + x) < lowSpeedCap)
        {
            //Debug.Log("3");
            rb2d.AddForce(Vector2.right * 3 * fixedDeltaFactor * x * Time.fixedDeltaTime);
        }

        
        if(state == PlayerState.Walking && x == 0f)
        {
            rb2d.velocity = new Vector2(0, 0);
        }
        
    }

    private void handleJump()
    {

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (wallStatus(out bool left, out bool right))
            {
                if (left)
                {
                    rb2d.velocity = new Vector2(0, 0);
                    rb2d.AddForce((Vector2.up * 0.9f + Vector2.right * 1.3f) * Time.fixedDeltaTime * deltaFactor * jumpMod, ForceMode2D.Impulse);
                    Debug.Log(rb2d.velocity); //(7.80, 5.40)



                }
                else if (right)
                {
                    rb2d.velocity = new Vector2(0, 0);
                    rb2d.AddForce((Vector2.up * 0.9f + -Vector2.right * 1.3f) * Time.fixedDeltaTime * deltaFactor * jumpMod, ForceMode2D.Impulse);
                    Debug.Log(rb2d.velocity); //(7.80, 5.40)
                }
            }
            else if (!doubleJumpUsed)
            {
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(Vector2.up * deltaFactor * Time.fixedDeltaTime * jumpMod , ForceMode2D.Impulse);
            }

            if (gc.colliding)
            {
                gc.colliding = false;
            }
            //If you are not attached to a wall; expend the double jump
            else if (!left && !right && !doubleJumpUsed)
            {
                doubleJumpUsed = true;
            }
        }
    }

    private PlayerState StateChecker()
    {
        PlayerState tempState;
        if (gc.colliding)
        {
            doubleJumpUsed = false;
            if (!firstDrain)
            {
                firstDrain = true;
            }
            if (rb2d.velocity.x != 0)
            {
                if(sprinting)
                {
                    tempState = PlayerState.Sprinting;
                }
                else
                {
                    tempState = PlayerState.Walking;
                }   
            }
            else
            {
                tempState = PlayerState.Standing;
            }
        }
        else
        {
            if(walled())
            {
                tempState = PlayerState.Walled;
            }
            else if (boostDrain)
            {
                tempState = PlayerState.Boosting;
            }
            else if (doubleJumpUsed)
            {
                tempState = PlayerState.DoubleJump;
            }
            else
            {
                tempState = PlayerState.Jump;
            }
        }

        if(tempState != state)
        {
            PlayerStateEvent.Invoke(tempState);
            anim.SetInteger("State", (int)tempState);
            anim.SetTrigger("Changed");
            
        }

        return tempState;


    }

    private bool wallStatus(out bool left, out bool right)
    {
        left = false;
        right = false;

        List<WallCheck> tempwcs = wcs.FindAll(x => x.walled);

        if(tempwcs.Count == 1)
        {
            left = tempwcs[0].transform.localPosition.x < 0f;
            right =! left;
            return true;
        }
        else if(tempwcs.Count == 2)
        {
            right = true;
            left = true;
            return true;
        }

        return false;

    }

    private bool walled()
    {
        return wcs.Exists(x => x.walled);
    }
}
