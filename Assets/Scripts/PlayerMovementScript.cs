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
    private float jetpackVerticalSpeedLimit = 6f;

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
    
    private float jumpMod = 6f;
    private float speedCap = 3f;
    private float sprintAmp = 1.55f;

    [Range(-0.2f, 1.2f)] [SerializeField]
    private float boost = 1f;
    [SerializeField]
    private bool boostDrain = false;
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

    private void handleInput(PlayerState state)
    {

        handleX(x);
        handleJump();
        handleBoost();
    }

    private void handleBoost()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            boostDrain = true;
            ps.Play();
        }
        else if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            boostDrain = false;
            ps.Stop();
        }
    }

    IEnumerator BoostDrainer()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            //Jetpack only activates if speed isn't too high already.
            if(rb2d.velocity.y <= jetpackVerticalSpeedLimit)
            {
                if (boostDrain && boost >= 0)
                {
                    boost -= 1f * Time.deltaTime;
                    if(rb2d.velocity.y < 0f)
                    {
                        rb2d.AddForce(Vector2.up * (slowMode ? 3.5f : 7f));
                    }
                    else
                    {
                        rb2d.AddForce(Vector2.up * (slowMode ? 2f : 3.5f));
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
        //Handles sprinting
        float _speedCap = speedCap * sprintAmp;
        // Debug.Log(rb2d.velocity.x); //Wall kick velocity is ~6
        if (state != PlayerState.DoubleJump && Mathf.Abs(rb2d.velocity.x + x) < _speedCap)
        {
            rb2d.AddForce(Vector2.right * 3 * x);
        }
        else if((state == PlayerState.Jump || state == PlayerState.DoubleJump || state == PlayerState.Boosting) && (rb2d.velocity.x > speedCap && x < 0) || (rb2d.velocity.x < -speedCap && x > 0))
        {
            rb2d.AddForce(Vector2.right * (slowMode ? 1f : 3f) * x, ForceMode2D.Force);
        }
        else if(state == PlayerState.DoubleJump && Mathf.Abs(rb2d.velocity.x + x) < speedCap)
        {
            rb2d.AddForce(Vector2.right * 3 * x);
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
                    rb2d.AddForce((Vector2.up + Vector2.right) * jumpMod, ForceMode2D.Impulse);
                }
                else if (right)
                {
                    rb2d.velocity = new Vector2(0, 0);
                    rb2d.AddForce((Vector2.up + -Vector2.right) * jumpMod, ForceMode2D.Impulse);
                }
            }
            else if (!doubleJumpUsed)
            {
                //Velocity reset if you are falling and jump again
                if(rb2d.velocity.y < 0f)
                {
                    
                }
                rb2d.velocity = new Vector2(rb2d.velocity.x, 0f);
                rb2d.AddForce(Vector2.up * jumpMod , ForceMode2D.Impulse);
            }

            if (gc.colliding)
            {
                gc.colliding = false;
            }
            else if (!doubleJumpUsed)
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
            if(rb2d.velocity.x != 0)
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
