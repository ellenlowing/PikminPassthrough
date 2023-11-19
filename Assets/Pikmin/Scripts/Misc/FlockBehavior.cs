using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{
    public int index;
    public GameObject gameObject;
    public Vector3 positionOffset;
    public Rigidbody rb;
    public Animator anim;

    public Boid (int _index, GameObject obj, Vector3 offset)
    {
        index = _index;
        gameObject = obj;
        positionOffset = offset;
        rb = obj.GetComponent<Rigidbody>();
        anim = obj.GetComponent<Animator>();
    }
}

public class FlockBehavior : MonoBehaviour
{
    public enum BoidFormation
    {
        Radial,
        Grid,
        Line
    }

    [Header("References")]
    public GameObject boidPrefab;
    public Transform leaderTransform;

    [Header("Behavior variables")]
    [SerializeField] private float _speed;
    [SerializeField] private float _distanceFromLeader;
    [SerializeField] private float _distanceThreshold;
    [SerializeField] private Vector2 _gridSize;
    [SerializeField] private float _gridSpace;
    [SerializeField] private int _numBoids;
    [SerializeField] private float _leaderChangeRate;
    [SerializeField] private float _noise;
    [SerializeField] private BoidFormation _boidFormation;

    private List<Boid> boids;

    private Vector3 lastLeaderPosition;
    private GameObject leaderGhost;

    void Start()
    {
        InitializeBoids();
        leaderGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaderGhost.transform.position = GetNewPosition(Vector3.zero);
        leaderGhost.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Rigidbody ghostRb = leaderGhost.AddComponent<Rigidbody>();
        ghostRb.interpolation = RigidbodyInterpolation.Interpolate;
        ghostRb.useGravity = false;
    }

    void Update()
    {
        float step = _speed * Time.deltaTime;

        Vector3 groundedTransformPosition = new Vector3(
            leaderTransform.position.x,
            0,
            leaderTransform.position.z
        );
        bool leaderAwayFromPack = Vector3.Distance(leaderGhost.transform.position, groundedTransformPosition) > _distanceThreshold; 

        if(leaderAwayFromPack) 
        {
            Vector3 newGhostPosition = GetNewPosition(Vector3.zero);
            leaderGhost.transform.position = Vector3.MoveTowards(leaderGhost.transform.position, newGhostPosition, step);
        }

        for(int i = 0; i < _numBoids; i++)
        {
            Boid boid = boids[i];
            Transform boidTransform = boid.gameObject.transform;

            Vector3 newPosition = GetNewPosition(boid.positionOffset);

            if(!leaderAwayFromPack)
            {
                // If pikmin has arrived at new position
                boidTransform.rotation = Quaternion.LookRotation(groundedTransformPosition - boidTransform.position, Vector3.up);
                boid.anim.SetInteger("state", 0);
            }
            else
            {
                // If pikmin is still walking
                boidTransform.position = Vector3.MoveTowards(boidTransform.position, newPosition, step);
                boidTransform.rotation = Quaternion.LookRotation(newPosition - boidTransform.position, Vector3.up);
                boid.anim.SetInteger("state", 1);
            }
        }
    }
    
    private void InitializeBoids()
    {
        boids = new List<Boid>();
        for(int i = 0; i < _numBoids; i++)
        {
            Vector3 offset = GetPositionOffset(i);
            GameObject obj = Instantiate(boidPrefab, GetNewPosition(offset), Quaternion.identity);
            Boid boid = new Boid(i, obj, offset);
            boids.Add(boid);
        }
    }

    private void UpdateFormation()
    {
        for(int i = 0; i < _numBoids; i++)
        {
            boids[i].positionOffset = GetPositionOffset(i);
        }
    }

    private Vector3 GetNewPosition(Vector3 positionOffset)
    {
        Vector3 newOffset = leaderTransform.forward * positionOffset.z + leaderTransform.right * positionOffset.x;
        Vector3 newPosition = leaderTransform.position + newOffset;
        if(_boidFormation == BoidFormation.Radial)
        {
            newPosition += newOffset * (_distanceFromLeader - 1f);
        }
        else
        {
            newPosition += newOffset - leaderTransform.forward * _distanceFromLeader;
        }

        newPosition.y = 0;
        
        return newPosition;
    }

    private Vector3 GetPositionOffset(int index)
    {
        if(_boidFormation == BoidFormation.Line)
        {
            return new Vector3(0, 0, -index * _gridSpace);
        }
        else if (_boidFormation == BoidFormation.Grid)
        {
            float x = Mathf.Floor(index % _gridSize.x);
            float z = Mathf.Floor(index / _gridSize.x);
            return new Vector3(-x * _gridSpace, 0f, -z * _gridSpace);
        }
        else if (_boidFormation == BoidFormation.Radial)
        {
            float angle = (float)index / (float)_numBoids * Mathf.PI * 2f;
            float x = Mathf.Cos(angle);
            float z = Mathf.Sin(angle);
            return new Vector3(x, 0f, z);
        }
        return Vector3.zero;
    }

    private float GetRateOfChange(Vector3 curr, Vector3 prev)
    {
        Vector3 rate = (curr - prev) / Time.deltaTime;
        return rate.magnitude;
    }

    private Vector3 GetNoise(Vector3 pos)
    {
        var noise = Mathf.PerlinNoise(pos.x * _noise, pos.z * _noise);
        return new Vector3(noise, 0, noise);
    }
}
