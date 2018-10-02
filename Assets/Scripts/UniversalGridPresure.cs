using System.Collections;
using System.Collections.Generic;
using Byn.Common;
using UnityEngine;
using UnityEditor;

public class UniversalGridPresure : /*UnitySingleton<UniversalGridPresure>*/ CService<UniversalGridPresure> {

    public enum Face
    {
        Top = 0,
        Down = 1,
        Front = 2,
        Back = 3,
        Right = 4,
        Left = 5,
        Non =6
    }

    public int widthSize;
    public int longSize;
    public int heightSize;
    public float sideBoxSize;
    public int particles;
    
    public float particleRadious;

    public GameObject weatherParticlePrefab;
    private WeatherParticlePresure[] weatherParticles;
    public float periodApplyPhysix;
    private float periodApplyOnUnitGridPhysix;
    private float periodColdPhysix;
    public float loseTemperatureCoefficient;
    public float incrementTempVelocityCoefficient;
    //[Range(-40,-50)]
    public RangeInt altitudeTopTempRange;

    private int currentAltitudeTopTemp;
    [Range(0, 15)]
    public RangeInt altitudeDownTempRange;

    private int currentAltitudeDownTemp;

    private float yTopPosition;
    private float yDownPosition;


    public class GridElement
    {
        public Vector3 centerPosition;
        public Vector3 airVelocity;
        public float averageTemperature;
        public int presure;


        public GridElement(Vector3 newPostion)
        {
            this.centerPosition = newPostion;
        }
    }

    //first index width, second index, long, third index height
    //float[,,] gridTempValues;
    GridElement[,,] gridPresureValues;

    private BoxCollider topCollider;
    private BoxCollider downCollider;
    private BoxCollider frontCollider;
    private BoxCollider backCollider;
    private BoxCollider leftCollider;
    private BoxCollider RightCollider;
    private const float colliderThickness = 5f;

    float CheckAverageTemperature(int x, int y, int z)
    {
        float averageTemperature = 0;
        RaycastHit[] m_HitDetect;
        m_HitDetect = Physics.BoxCastAll(gridPresureValues[x,y,z].centerPosition, sideBoxSize / 2f * Vector3.one, transform.forward, transform.rotation, 0f, 1 << 8);
        if (m_HitDetect.Length > 0)
        {
            foreach (RaycastHit particle in m_HitDetect)
            {
                averageTemperature += particle.transform.GetComponent<WeatherParticlePresure>().temperature;
            }

            averageTemperature /= m_HitDetect.Length; 

            //foreach (RaycastHit particle in m_HitDetect)
            //{
            //    float difTemp = particle.transform.GetComponent<WeatherParticlePresure>().temperature - averageTemperature;

            //    if (Mathf.Abs(difTemp) > 1f)
            //    {
            //        particle.transform.GetComponent<Rigidbody>()
            //            .AddForce(Vector3.up * difTemp * incrementTempVelocityCoefficient, ForceMode.Force);
            //    }
            //}
            //ApplyGridTempForces(x, y, z, m_HitDetect);
            return averageTemperature;
        }
        else return float.NaN;
    }

    int CheckGridPresure(int x, int y, int z)
    {
        RaycastHit[] m_HitDetect;
        m_HitDetect = Physics.BoxCastAll(gridPresureValues[x, y, z].centerPosition, sideBoxSize / 2f * Vector3.one, transform.forward, transform.rotation, 0f, 1 << 8);
        if (m_HitDetect.Length > 0)
        {
            ApplyGridPresureForces(x, y, z, m_HitDetect);
            return m_HitDetect.Length;
        }
        else return 0;
    }

    //void ApplyGridTempForces(int x, int y, int z, RaycastHit[] particles)
    //{
    //    Vector3 gridForces = Vector3.zero;

    //    if (x < gridPresureValues.GetLength(0) - 2)
    //    {
    //        float difTempRight = gridPresureValues[x + 1, y, z].averageTemperature - gridPresureValues[x, y, z].averageTemperature;
    //        if (difTempRight > 0)
    //        {
    //            gridForces += Vector3.right * difTempRight;
    //        }
    //    }
    //    if (x > 0)
    //    {
    //        float difTempLeft = gridPresureValues[x - 1, y, z].averageTemperature - gridPresureValues[x, y, z].averageTemperature;
    //        if (difTempLeft > 0)
    //        {
    //            gridForces -= Vector3.right * difTempLeft;
    //        }
    //    }
    //    if (y < gridPresureValues.GetLength(1) - 2)
    //    {
    //        float difTempTop = gridPresureValues[x, y + 1, z].averageTemperature - gridPresureValues[x, y, z].averageTemperature;
    //        if (difTempTop > 0)
    //        {
    //            gridForces += Vector3.up * difTempTop;
    //        }
    //    }
    //    if (y > 0)
    //    {
    //        float difTempDown = gridPresureValues[x, y - 1, z].averageTemperature - gridPresureValues[x, y, z].averageTemperature;
    //        if (difTempDown > 0)
    //        {
    //            gridForces -= Vector3.up * difTempDown;
    //        }
    //    }
    //    if (z < gridPresureValues.GetLength(2) - 2)
    //    {
    //        float difTempFront = gridPresureValues[x, y, z + 1].averageTemperature - gridPresureValues[x, y, z].averageTemperature;
    //        if (difTempFront > 0)
    //        {
    //            gridForces += Vector3.forward * difTempFront;
    //        }
    //    }
    //    if (z > 0)
    //    {
    //        float difTempBack = gridPresureValues[x, y, z - 1].averageTemperature - gridPresureValues[x, y, z].averageTemperature;
    //        if (difTempBack > 0)
    //        {
    //            gridForces -= Vector3.forward * difTempBack;
    //        }
    //    }

    //    foreach (RaycastHit particle in particles)
    //    {
    //        particle.transform.GetComponent<Rigidbody>().AddForce(gridForces * incrementTempVelocityCoefficient);
    //    }
    //}

    void ApplyGridPresureForces(int x, int y, int z, RaycastHit[] particles)
    {
        //int topPres, downPres, frontPres, backPres, rightPres, LeftPres;
        int maxDifPresure = 0;
        Face maxDifPresureFace = Face.Non;
        
        if (x < gridPresureValues.GetLength(0) - 2)
        {
            int difPresure = gridPresureValues[x, y, z].presure - gridPresureValues[x + 1, y, z].presure;
            if (difPresure > 0)
            {
                maxDifPresure = difPresure;
                maxDifPresureFace = Face.Right;
                Debug.Log("Right forces");
                //gridForces += Vector3.right * difPresure;
            }
        }
        if (x > 0)
        {
            int difPresure = gridPresureValues[x, y, z].presure - gridPresureValues[x - 1, y, z].presure;
            if (difPresure > 0 & difPresure > maxDifPresure)
            {
                maxDifPresure = difPresure;
                maxDifPresureFace = Face.Left;
                Debug.Log("Left forces");
                //gridForces -= Vector3.right * difPresure;
            }
        }
        if (y < gridPresureValues.GetLength(1) - 2)
        {
            int difPresure = gridPresureValues[x, y, z].presure - gridPresureValues[x, y + 1, z].presure;
            if (difPresure > 0 & difPresure > maxDifPresure)
            {
                maxDifPresure = difPresure;
                maxDifPresureFace = Face.Top;
                Debug.Log("Top forces");
                //gridForces += Vector3.up * difPresure;
            }
        }
        if (y > 0)
        {
            int difPresure = gridPresureValues[x, y, z].presure - gridPresureValues[x, y - 1, z].presure;
            if (difPresure > 0 & difPresure > maxDifPresure)
            {
                maxDifPresure = difPresure;
                maxDifPresureFace = Face.Down;
                Debug.Log("Down forces");
                //gridForces -= Vector3.up * difPresure;
            }
        }
        if (z < gridPresureValues.GetLength(2) - 2)
        {
            int difPresure = gridPresureValues[x, y, z].presure - gridPresureValues[x, y, z + 1].presure;
            if (difPresure > 0 & difPresure > maxDifPresure)
            {
                maxDifPresure = difPresure;
                maxDifPresureFace = Face.Front;
                Debug.Log("Front forces");
                //gridForces += Vector3.forward * difPresure;
            }
        }
        if (z > 0)
        {
            int difPresure = gridPresureValues[x, y, z].presure - gridPresureValues[x, y, z - 1].presure;
            if (difPresure > 0 & difPresure > maxDifPresure)
            {
                maxDifPresure = difPresure;
                maxDifPresureFace = Face.Back;
                Debug.Log("Back forces");
                //gridForces -= Vector3.forward * difPresure;
            }
        }

        maxDifPresure *= (int)incrementTempVelocityCoefficient;

        switch ((int)maxDifPresureFace)
        {
            case 0:
                foreach (RaycastHit particle in particles)
                {
                    particle.transform.GetComponent<Rigidbody>().AddForce(Vector3.up * maxDifPresure);
                    //Debug.Log("Applying difPresure");
                }
                break;
            case 1:
                foreach (RaycastHit particle in particles)
                {
                    particle.transform.GetComponent<Rigidbody>().AddForce(Vector3.down * maxDifPresure);
                    //Debug.Log("Applying difPresure");
                }
                break;
            case 2:
                foreach (RaycastHit particle in particles)
                {
                    particle.transform.GetComponent<Rigidbody>().AddForce(Vector3.forward * maxDifPresure);
                    //Debug.Log("Applying difPresure");
                }
                break;
            case 3:
                foreach (RaycastHit particle in particles)
                {
                    particle.transform.GetComponent<Rigidbody>().AddForce(Vector3.back * maxDifPresure);
                    //Debug.Log("Applying difPresure");
                }
                break;
            case 4:
                foreach (RaycastHit particle in particles)
                {
                    particle.transform.GetComponent<Rigidbody>().AddForce(Vector3.right * maxDifPresure);
                    //Debug.Log("Applying difPresure");
                }
                break;
            case 5:
                foreach (RaycastHit particle in particles)
                {
                    particle.transform.GetComponent<Rigidbody>().AddForce(Vector3.left * maxDifPresure);
                    //Debug.Log("Applying difPresure");
                }
                break;
            default:
                break;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ServeClients(this);
    }

    // Use this for initialization
    void Start()
    {

        int xLength = Mathf.FloorToInt(widthSize / sideBoxSize);
        int yLength = Mathf.FloorToInt(heightSize / sideBoxSize);
        int zLength = Mathf.FloorToInt(longSize / sideBoxSize);

        yTopPosition = yLength * sideBoxSize + transform.position.y;
        yDownPosition = transform.position.y;

        //gridTempValues = new float[xLength, yLength, zLength];
        gridPresureValues = new GridElement[xLength, yLength, zLength];

        Vector3 centerForColliders;

        topCollider = this.gameObject.AddComponent<BoxCollider>();
        centerForColliders = new Vector3((xLength * sideBoxSize / 2f), (yLength * sideBoxSize) + colliderThickness / 2, (zLength * sideBoxSize / 2f));
        topCollider.center = centerForColliders;
        topCollider.size = new Vector3(xLength * sideBoxSize, colliderThickness, zLength * sideBoxSize);

        downCollider = this.gameObject.AddComponent<BoxCollider>();
        centerForColliders = new Vector3((xLength * sideBoxSize / 2f), -colliderThickness / 2, (zLength * sideBoxSize / 2f));
        downCollider.center = centerForColliders;
        downCollider.size = new Vector3(xLength * sideBoxSize, colliderThickness, (zLength * sideBoxSize) + colliderThickness / 2);

        frontCollider = this.gameObject.AddComponent<BoxCollider>();
        centerForColliders = new Vector3((xLength * sideBoxSize / 2f), (yLength * sideBoxSize / 2f), (zLength * sideBoxSize) + colliderThickness / 2);
        frontCollider.center = centerForColliders;
        frontCollider.size = new Vector3(xLength * sideBoxSize, yLength * sideBoxSize, colliderThickness);

        backCollider = this.gameObject.AddComponent<BoxCollider>();
        centerForColliders = new Vector3((xLength * sideBoxSize / 2f), (yLength * sideBoxSize / 2f), -colliderThickness / 2);
        backCollider.center = centerForColliders;
        backCollider.size = new Vector3(xLength * sideBoxSize, yLength * sideBoxSize, colliderThickness);

        RightCollider = this.gameObject.AddComponent<BoxCollider>();
        centerForColliders = new Vector3((xLength * sideBoxSize) + colliderThickness / 2, (yLength * sideBoxSize / 2f), (zLength * sideBoxSize / 2f));
        RightCollider.center = centerForColliders;
        RightCollider.size = new Vector3(colliderThickness, yLength * sideBoxSize, zLength * sideBoxSize);

        leftCollider = this.gameObject.AddComponent<BoxCollider>();
        centerForColliders = new Vector3(-colliderThickness / 2, (yLength * sideBoxSize / 2f), (zLength * sideBoxSize / 2f));
        leftCollider.center = centerForColliders;
        leftCollider.size = new Vector3(colliderThickness, yLength * sideBoxSize, zLength * sideBoxSize);

        int particlesPerX = Mathf.FloorToInt((xLength * sideBoxSize) / (particleRadious /** WeatherParticlePresure.sizeIncrement*/));
        int particlesPerY = Mathf.FloorToInt((yLength * sideBoxSize) / (particleRadious /** WeatherParticlePresure.sizeIncrement*/));
        int particlesPerZ = Mathf.FloorToInt((zLength * sideBoxSize) / (particleRadious /** WeatherParticlePresure.sizeIncrement*/));

        if (particles > particlesPerX * particlesPerY * particlesPerZ)        {
            
            Debug.Log("To many particles!!");
        }
        Debug.Log("Particles per x = " + particlesPerX + ", y= " + particlesPerY + ", z= " + particlesPerZ);
        int count = 0;
        GameObject weatherParticlesParent = new GameObject();
        weatherParticlesParent.name = "WeatherParticles";
        weatherParticlesParent.transform.parent = this.transform;

        for (int y = particlesPerY - 1; y >= 0; y--)
        {
            for (int x = 0; x < particlesPerX; x++)
            {
                for (int z = 0; z < particlesPerZ; z++)
                {
                    Vector3 particlePosition = new Vector3((x * particleRadious /** WeatherParticlePresure.sizeIncrement*/), (y * particleRadious /** WeatherParticlePresure.sizeIncrement*/), (z * particleRadious /** WeatherParticlePresure.sizeIncrement*/)) + Vector3.one * particleRadious /** WeatherParticlePresure.sizeIncrement*/;
                    GameObject newWeatherParticle = Instantiate(weatherParticlePrefab, weatherParticlesParent.transform);
                    newWeatherParticle.transform.position = particlePosition;
                    newWeatherParticle.transform.localScale = Vector3.one * particleRadious;
                    //newWeatherParticle.transform.GetComponent<Rigidbody>().AddForce(Random.onUnitSphere * Random.Range(500,3000f),ForceMode.Force);
                    if (++count >= particles)
                    {
                        break;
                    }
                }
                if (++count >= particles)
                {
                    break;
                }
            }
            if (++count >= particles)
            {
                break;
            }
        }
        Debug.Log("Instantiated " + count + " particles of " + particles + " particles requested.");
        weatherParticles = weatherParticlesParent.GetComponentsInChildren<WeatherParticlePresure>();

        
        periodColdPhysix = periodApplyPhysix / weatherParticles.Length;
        periodApplyOnUnitGridPhysix = periodApplyPhysix / (xLength * yLength * zLength);

        Debug.Log("PeriodColdPhysix: " + periodColdPhysix);
        Debug.Log("PeriodApplyOnGrid: " + periodApplyOnUnitGridPhysix);

        for (int x = 0; x < Mathf.FloorToInt(widthSize / sideBoxSize); x++)
        {
            for (int y = 0; y < Mathf.FloorToInt(heightSize / sideBoxSize); y++)
            {
                for (int z = 0; z < Mathf.FloorToInt(longSize / sideBoxSize); z++)
                {
                    Vector3 posUnitGrid = new Vector3(x * sideBoxSize + sideBoxSize / 2f, y * sideBoxSize + sideBoxSize / 2f, z * sideBoxSize + sideBoxSize / 2f);
                    gridPresureValues[x, y, z] = new GridElement(posUnitGrid);
                }
            }
        }


        StartCoroutine(ApplyCold(periodColdPhysix));
        StartCoroutine(ApplyOnUnitGridPhysix(periodApplyOnUnitGridPhysix));
    }

    void OnDrawGizmosSelected()
    {
        // Draw a semitransparent blue cube at the transforms position
        Gizmos.color = new Color(1, 1, 1, 1f);

        for (int x = 0; x < Mathf.FloorToInt(widthSize / sideBoxSize); x++)
        {
            for (int y = 0; y < Mathf.FloorToInt(heightSize / sideBoxSize); y++)
            {
                for (int z = 0; z < Mathf.FloorToInt(longSize / sideBoxSize); z++)
                {
                    Vector3 posUnitGrid = new Vector3(x * sideBoxSize + sideBoxSize / 2f, y * sideBoxSize + sideBoxSize / 2f, z * sideBoxSize + sideBoxSize / 2f);
                    Gizmos.DrawWireCube(transform.position + posUnitGrid, Vector3.one * sideBoxSize);
                    if (gridPresureValues != null)
                    {
                        GUI.color = Color.white;
                        Handles.Label(gridPresureValues[x, y, z].centerPosition,
                            "Temp: " + gridPresureValues[x, y, z].averageTemperature.ToString() + " Pres: " + gridPresureValues[x, y, z].presure.ToString());
                    }
                }
            }
        }
    }

    IEnumerator ApplyCold(float period)
    {
        yield return null;
        while (true)
        {
            foreach (WeatherParticlePresure particle in weatherParticles)
            {
                float altitudeFactor = (particle.transform.position.y - yDownPosition) / yTopPosition;

                if (altitudeFactor >= 0.5f)
                {
                    float limitCoefficient = Mathf.Clamp(particle.temperature + 50, 0f, 100f) / 100;
                    particle.ChangeTemperature(-loseTemperatureCoefficient * (altitudeFactor - 0.5f) * 2f * limitCoefficient);
                    Debug.Log("Applying Cold");
                }
                yield return new WaitForSeconds(period);
            }
        }
    }


    void ColdPhysix()
    {
        foreach (WeatherParticlePresure particle in weatherParticles)
        {
            float altitudeFactor = (particle.transform.position.y - yDownPosition) / yTopPosition;

            if (altitudeFactor >= 0.5f)
            {
                float limitCoefficient = Mathf.Clamp(particle.temperature + 50, 0f, 100f) / 100;
                particle.ChangeTemperature(-loseTemperatureCoefficient * (altitudeFactor - 0.5f) * 2f * limitCoefficient);
                Debug.Log("Applying Cold");
            }
        }
    }

    void GridPhysix()
    {
        for (int x = 0; x < gridPresureValues.GetLength(0); x++)
        {
            for (int y = 0; y < gridPresureValues.GetLength(1); y++)
            {
                for (int z = 0; z < gridPresureValues.GetLength(2); z++)
                {
                    float temp = CheckAverageTemperature(x, y, z);
                    gridPresureValues[x, y, z].averageTemperature = temp != float.NaN ? temp : gridPresureValues[x, y, z].averageTemperature;
                    gridPresureValues[x, y, z].presure = CheckGridPresure(x, y, z);
                    Debug.Log("Applying physics on grid");
                }
            }
        }
    }

    IEnumerator ApplyOnUnitGridPhysix(float period)
    {
        yield return null;
        while (true)
        {
            for (int x = 0; x < gridPresureValues.GetLength(0); x++)
            {
                for (int y = 0; y < gridPresureValues.GetLength(1); y++)
                {
                    for (int z = 0; z < gridPresureValues.GetLength(2); z++)
                    {
                        float temp = CheckAverageTemperature(x, y, z);
                        gridPresureValues[x, y, z].averageTemperature = temp != float.NaN ? temp : gridPresureValues[x, y, z].averageTemperature;
                        gridPresureValues[x, y, z].presure = CheckGridPresure(x, y, z);
                        Debug.Log("Applying physics on grid");

                        yield return new WaitForSeconds(period);
                    }
                }
            }
        }
    }
}
