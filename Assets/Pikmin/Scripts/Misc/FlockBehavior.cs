using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boid
{
    public int index;
    public GameObject gameObject;
    public Vector3 positionOffset;

    public Boid (int _index, GameObject obj, Vector3 offset)
    {
        index = _index;
        gameObject = obj;
        positionOffset = offset;
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

    public GameObject boidPrefab;
    public Transform leaderTransform;
    [SerializeField] private float _speed;
    [SerializeField] private float _distanceFromLeader;
    [SerializeField] private float _noise;

    public List<Boid> boids;
    public int numBoids;
    public BoidFormation boidFormation;
    public Vector2 gridSize;
    public float gridSpace;

    void Start()
    {
        InitializeBoids();
    }

    void Update()
    {
        float step = _speed * Time.deltaTime;
        for(int i = 0; i < numBoids; i++)
        {
            Boid boid = boids[i];
            Transform boidTransform = boid.gameObject.transform;
            
            Vector3 newOffset = leaderTransform.forward * boid.positionOffset.z + leaderTransform.right * boid.positionOffset.x;
            newOffset += GetNoise(newOffset);

            Vector3 newPosition = leaderTransform.position + newOffset + leaderTransform.forward * _distanceFromLeader;

            boidTransform.position = Vector3.MoveTowards(boidTransform.position, newPosition, step);
            boidTransform.rotation = Quaternion.RotateTowards(boidTransform.rotation, Quaternion.LookRotation(boidTransform.forward, Vector3.up), step);

            _noise = Mathf.Lerp(0, 1, Vector3.Distance(boidTransform.position, newPosition));
        }
    }

    private void InitializeBoids()
    {
        boids = new List<Boid>();
        for(int i = 0; i < numBoids; i++)
        {
            GameObject obj = Instantiate(boidPrefab);
            Vector3 offset = GetPositionOffset(i);

            obj.transform.position = offset;
            Boid boid = new Boid(i, obj, offset);
            boids.Add(boid);
        }
    }

    private Vector3 GetPositionOffset(int index)
    {
        if(boidFormation == BoidFormation.Line)
        {
            return new Vector3(0, 0, -index * gridSpace);
        }
        else if (boidFormation == BoidFormation.Grid)
        {
            float x = Mathf.Floor(index % gridSize.x);
            float z = Mathf.Floor(index / gridSize.x);
            return new Vector3(-x * gridSpace, 0f, -z * gridSpace);
        }
        return Vector3.zero;
    }

    private Vector3 GetNoise(Vector3 pos)
    {
        var noise = Mathf.PerlinNoise(pos.x * _noise, pos.z * _noise);
        return new Vector3(noise, 0, noise);
    }
}
