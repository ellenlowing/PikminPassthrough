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
        Line,
        Ring,
        Phyllotaxis
    }

    [Header("References")]
    public GameObject boidPrefab;
    public Transform leaderTransform;

    [Header("Behavior variables")]
    [SerializeField] private float _speed;
    [SerializeField] private float _angularSpeed;
    [SerializeField] private float _distanceThreshold;
    [SerializeField] private float _noiseScale;
    [SerializeField] private float _noiseWeight;
    [SerializeField] private float _leaderMoveDistanceThreshold;
    [SerializeField] private float _distanceFromDestinationThreshold;

    [Header("Formation")]
    [SerializeField] private BoidFormation _boidFormation;
    [SerializeField] private int _numBoids;
    [SerializeField] private Vector2 _gridSize;
    [SerializeField] private float _gridSpace;
    [SerializeField] private float _radius;
    [SerializeField] private float _ringRadius;
    [SerializeField] private float _phylloc;

    private List<Boid> boids;

    private Vector3 lastLeaderPosition;
    private GameObject leaderGhost;
    [SerializeField] bool leaderAwayFromPack;

    void Start()
    {
        InitializeBoids();
        leaderGhost = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        leaderGhost.transform.position = GetNewPosition(leaderTransform, Vector3.zero);
        leaderGhost.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        Rigidbody ghostRb = leaderGhost.AddComponent<Rigidbody>();
        ghostRb.interpolation = RigidbodyInterpolation.Interpolate;
        ghostRb.useGravity = false;
        ghostRb.isKinematic = true;
        Destroy(leaderGhost.GetComponent<SphereCollider>());
        // Destroy(leaderGhost.GetComponent<MeshRenderer>());
        lastLeaderPosition = leaderTransform.position;
    }

    void Update()
    {
        float step = _speed * Time.deltaTime;
        float angularStep = _angularSpeed * Time.deltaTime;

        Vector3 groundedLeaderPosition = new Vector3(
            leaderTransform.position.x,
            0,
            leaderTransform.position.z
        );
        leaderAwayFromPack = Vector3.Distance(leaderGhost.transform.position, groundedLeaderPosition) > _distanceThreshold; 

        float leaderMoveDistance = Vector3.Distance(leaderTransform.position, lastLeaderPosition);

        if(leaderAwayFromPack)
        {
            Vector3 newGhostPosition = GetNewPosition(leaderTransform, Vector3.zero);
            leaderGhost.transform.position = Vector3.MoveTowards(leaderGhost.transform.position, newGhostPosition, step);

            // Comment out below if formation is dependent on leader rotation
            Quaternion leaderYRotation = Quaternion.identity;
            leaderYRotation.eulerAngles = new Vector3(0, leaderTransform.rotation.eulerAngles.y, 0);
            leaderGhost.transform.rotation = Quaternion.RotateTowards(leaderGhost.transform.rotation, leaderYRotation, angularStep);
        }
        
        // method 1. get new position offset (with leader ghost as center) based on formation style
        // for(int i = 0; i < _numBoids; i++)
        // {
        //     Boid boid = boids[i];
        //     Transform boidTransform = boid.gameObject.transform;

        //     Vector3 newPosition = GetNewPosition(leaderGhost.transform, boid.positionOffset);
        //     float distanceFromDestination = Vector3.Distance(boidTransform.position, newPosition);
            
        //     if(leaderAwayFromPack || 
        //         (distanceFromDestination > _distanceFromDestinationThreshold) || 
        //         (distanceFromDestination <= _distanceFromDestinationThreshold && leaderMoveDistance > _leaderMoveDistanceThreshold)
        //     )
        //     {
        //         // If pikmin is still walking
        //         boidTransform.position = Vector3.MoveTowards(boidTransform.position, newPosition, step);
        //         boidTransform.rotation = Quaternion.LookRotation(newPosition - boidTransform.position, Vector3.up);
        //         boid.anim.SetInteger("state", 1);
        //     }
        //     else 
        //     {
        //         // If pikmin has arrived at new position
        //         boidTransform.rotation = Quaternion.LookRotation(groundedLeaderPosition - boidTransform.position, Vector3.up);
        //         boid.anim.SetInteger("state", 0);
        //     }
        // }

        // method 2. adopt leader movement when leader is in motion, and move towards formation when leader stop
        for(int i = 0; i < _numBoids; i++)
        {
            Boid boid = boids[i];
            Transform boidTransform = boid.gameObject.transform;

            float groundedLeaderMoveDistance = Vector3.Distance(groundedLeaderPosition, new Vector3(lastLeaderPosition.x, 0, lastLeaderPosition.z));
            
            if(groundedLeaderMoveDistance > _leaderMoveDistanceThreshold)
            {
                boid.anim.SetInteger("state", 1);
                Vector3 newPosition = boidTransform.position + (leaderTransform.position - lastLeaderPosition);
                boidTransform.rotation = Quaternion.LookRotation(newPosition - boidTransform.position, Vector3.up);
                boidTransform.position = newPosition;
            }
            else
            {
                Vector3 newPosition = GetNewPosition(leaderGhost.transform, boid.positionOffset) ;
                float distanceFromDestination = Vector3.Distance(boidTransform.position, newPosition);

                if(distanceFromDestination > _distanceFromDestinationThreshold)
                {
                    boid.anim.SetInteger("state", 1);
                    boidTransform.rotation = Quaternion.LookRotation(newPosition - boidTransform.position, Vector3.up);
                    boidTransform.position = Vector3.MoveTowards(boidTransform.position, newPosition, step);
                }
                else
                {
                    boid.anim.SetInteger("state", 0);
                    boidTransform.rotation = Quaternion.LookRotation(groundedLeaderPosition - boidTransform.position, Vector3.up);
                }
            }

        }


        lastLeaderPosition = leaderTransform.position;
    }
    
    private void InitializeBoids()
    {
        boids = new List<Boid>();
        for(int i = 0; i < _numBoids; i++)
        {
            Vector3 offset = GetPositionOffset(i);
            GameObject obj = Instantiate(boidPrefab, GetNewPosition(leaderTransform, offset), Quaternion.identity);
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

    private Vector3 GetNewPosition(Transform refTransform, Vector3 positionOffset)
    {
        Vector3 newOffset = refTransform.forward * positionOffset.z + refTransform.right * positionOffset.x;
        Vector3 newPosition = refTransform.position + newOffset;
        if(_boidFormation == BoidFormation.Radial || _boidFormation == BoidFormation.Phyllotaxis)
        {
            newPosition += newOffset * (_radius - 1f);
        }
        else
        {
            newPosition += newOffset - refTransform.forward * _radius;
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
        else if (_boidFormation == BoidFormation.Ring)
        {
            int rowNum = GetPositiveRoot(1, 1, -2 * (index + 1));
            int numBoidsPerRow = rowNum * (rowNum + 1) / 2 - (rowNum - 1) * rowNum / 2;
            int rowStartIndex = rowNum * (rowNum + 1) / 2 - numBoidsPerRow ;

            float angle = -0.5f;
            if(numBoidsPerRow > 1)
            {
                angle += Mathf.Lerp(-0.0833f, 0.0833f, (float)(index - rowStartIndex) / (float)(numBoidsPerRow - 1));
            }
            angle *= Mathf.PI;
            float radius = (float)rowNum * _ringRadius;
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);

            return new Vector3(x, 0, z);
        }
        else if (_boidFormation == BoidFormation.Phyllotaxis)
        {
            float a = (float)index * 137.5f * Mathf.Deg2Rad;
            float r = _phylloc * Mathf.Sqrt(index);
            float x = r * Mathf.Cos(a);
            float z = r * Mathf.Sin(a);
            return new Vector3(x, 0, z);
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
        var noise = Mathf.PerlinNoise(pos.x * _noiseScale, pos.z * _noiseScale);
        return new Vector3(noise, 0, noise) * _noiseWeight;
    }

    private int GetPositiveRoot(float a, float b, float c)
    {
        float d = Mathf.Sqrt(b * b - 4 * a * c);
        float x1 = (-b + d) / (2 * a);
        float x2 = (-b - d) / (2 * a);
        float xpos = x1 >= 0 ? x1 : x2;
        return Mathf.CeilToInt(xpos);
    }
}
