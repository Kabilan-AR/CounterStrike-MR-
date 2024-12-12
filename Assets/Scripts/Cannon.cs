using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

[RequireComponent(typeof(LineRenderer))]
public class Cannon : MonoBehaviour
{
    public Animator animator;
    [Header("Cannon Settings")]
    [SerializeField] private GameObject enemyTarget;
    [SerializeField] private Transform _cannonBarrel;
    
    [SerializeField] private GameObject _Bomb;
   //[SerializeField] private AudioClip _reloadSound;
    [SerializeField] private AudioClip _fireSound;
    [SerializeField] private AudioClip _destroyedSound;

    [SerializeField]
    [Range(0, 180)]
    private float firingAngle = 45f;
    [SerializeField]
    [Range(1, 100)]
    private int speed=10;
    private float gravity = -9.81f;

    private LineRenderer _lineRenderer;
    private AudioSource _source;
   
    [Header("Display Controls")]
    
    [SerializeField]
    [Range(10, 100)]
    private int LinePoints = 25;
    [SerializeField]
    [Range(0.01f, 0.25f)]
    private float TimeBetweenPoints = 0.1f;

    private bool isPositioning = false;
    private bool isFiring = false;

   [HideInInspector] public bool isReloading = false;
    [HideInInspector] public bool CannonDestroyed = false;

    private float fireTimer;
    private float fireTiming = 0f;
    private Rigidbody _grenadeRB;
    void Start()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Player"))
            {
                enemyTarget = obj;
                break; 
            }
           
        }
        fireTimer = Random.Range(5, 8);
        _grenadeRB = _Bomb.GetComponent<Rigidbody>();
        _lineRenderer = GetComponent<LineRenderer>();
        _source=GetComponent<AudioSource>();
    }

    public void Fire()
    {
        if(!CannonDestroyed)
        {
           
            isReloading = true;
            StartCoroutine(FireCannonBall());
        }

    }

    void Update()
    {
        Vector3 directionToTarget = (enemyTarget.transform.position - _cannonBarrel.position).normalized;
        float angle = Vector3.Angle(_cannonBarrel.forward, directionToTarget);
        Vector3 velocity = CalculateLaunchVelocity();
        if(fireTiming>fireTimer && !CannonDestroyed)
        {
            Fire();
            fireTiming = 0f;
        }
        fireTiming += Time.deltaTime;
        if (angle > 2f && !CannonDestroyed) 
        {

              StartCoroutine(CannonAlignment());     
        }
        if (isReloading) { RenderTrajectory(velocity); }

    }
    private IEnumerator FireCannonBall()
    {
        Vector3 velocity = CalculateLaunchVelocity();
        isFiring = true;
        float timer = 0f;
        if(!CannonDestroyed)
        {
            animator.SetBool("isFiring", isFiring);
            while (timer < 4.35f)
            {
                
                timer += Time.deltaTime;
                yield return null;
            }
        }
       
        timer= 0f;
        isFiring= false;
        animator.SetBool("isFiring", isFiring);
        _source.clip = _fireSound;
        _source.Play();
        isReloading = false;
        _lineRenderer.enabled = false;
        var bomb = Instantiate(_Bomb, _cannonBarrel.position, Quaternion.identity)
                                .GetComponent<Rigidbody>();
        bomb.velocity = velocity;
        //Reference of Velocity
        bomb.GetComponent<Bomb>().velocity = velocity;
    }
    private IEnumerator CannonAlignment()
    {
        float timer = 0f;
        isPositioning = true;
       
        if(!CannonDestroyed)
        {
            Quaternion initialRotation = transform.rotation;

            Vector3 directionToTarget = (enemyTarget.transform.position - _cannonBarrel.position).normalized;

            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);

            animator.SetBool("isPositioning", isPositioning);

            while (timer < 4.2f)
            {
                
                timer += Time.deltaTime;
                transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, timer / 4.2f);
                yield return null;
            }
        }
       
        
        isPositioning = false;
        animator.SetBool("isPositioning", isPositioning);
        timer = 0f;
    }
    private void OnTriggerEnter(Collider collision)
    {
       
        if(collision.gameObject.layer == LayerMask.NameToLayer("PlayerBomb"))
        {
            Debug.Log("Cannon destroyed");
            _source.clip = _destroyedSound;
            _source.Play();
            CannonDestroyed = true;
            isReloading = false;
            animator.SetBool("isPositioning", false);
            animator.SetBool("isFiring", false);
            animator.SetBool("isBroken", true);
            Destroy(gameObject, 0.7f);
        }
    }
  
    // Function to calculate the velocity needed to hit the target
    private Vector3 CalculateLaunchVelocity()
    {
        Vector3 directionToTarget = enemyTarget.transform.position - _cannonBarrel.position;

        float distanceToTarget = directionToTarget.magnitude;
        float targetHeightDifference = enemyTarget.transform.position.y - _cannonBarrel.position.y;

        // Convert the firing angle to radians
        float angleInRadians = firingAngle * Mathf.Deg2Rad;

        // Calculate the initial velocity using projectile motion formula
        float velocitySquared = (distanceToTarget * -gravity) / (Mathf.Sin(2 * angleInRadians));
        float initialVelocity = Mathf.Sqrt(Mathf.Abs(velocitySquared));

        // Calculate the velocity components in x and y directions
        Vector3 velocity = directionToTarget.normalized * initialVelocity * Mathf.Cos(angleInRadians);
        velocity.y = initialVelocity * Mathf.Sin(angleInRadians);

        return velocity;
    }

  
    private void RenderTrajectory(Vector3 initialVelocity)
    {
        _lineRenderer.enabled = true;
        _lineRenderer.positionCount = Mathf.CeilToInt(LinePoints / TimeBetweenPoints) + 1;

        Vector3 startPosition = _cannonBarrel.position;

        int i = 0;
        _lineRenderer.SetPosition(i, startPosition);

        //Simulate the trajectory
        for (float time = 0; time < LinePoints; time += TimeBetweenPoints)
        {
            i++;

            // Calculate the position of the projectile at the given time
            Vector3 point = startPosition + initialVelocity * time;
            point.y = startPosition.y + initialVelocity.y * time + (0.5f * gravity * time * time);

            _lineRenderer.SetPosition(i, point);

            // Check for collision using a raycast
            Vector3 lastPosition = _lineRenderer.GetPosition(i - 1);
            if (Physics.Raycast(lastPosition, (point - lastPosition).normalized, out RaycastHit hit, (point - lastPosition).magnitude))
            {
                _lineRenderer.SetPosition(i, hit.point);
                _lineRenderer.positionCount = i + 1;
                return;
            }
        }
        
    }

    }
