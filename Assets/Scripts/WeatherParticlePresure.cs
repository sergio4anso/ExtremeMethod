using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeatherParticlePresure : MonoBehaviour, IClient<UniversalGridPresure> {

    public float temperature;
    public static float transmissionCoefficient = 0.01f;
    public static float sizeIncrement = 3f;
    private MeshRenderer myRenderer;
    private Rigidbody myRig;
    public Color ColdColor;
    public Color HotColor;
    private Material pivotMat;
    private UniversalGridPresure universalGrid;

    protected void Awake()
    {
        UniversalGridPresure.AddClient(this);
    }

    void OnDestroy()
    {
        UniversalGridPresure.RemoveClient(this);
    }

    private void Start()
    {
        myRenderer = this.GetComponent<MeshRenderer>();
        myRig = this.GetComponent<Rigidbody>();
        pivotMat = Material.Instantiate(myRenderer.material);
    }

    public void ChangeTemperature(float degrees)
    {
        this.temperature += degrees;
        ChangeColor();
        ChangeSize();
    }
    public void ChangeColor()
    {
        float coeficient = Mathf.Clamp((this.temperature + 20f) / 40f, 0f, 1f);
        pivotMat.color = Color.Lerp(ColdColor, HotColor, coeficient);
        this.myRenderer.material = pivotMat;
    }
    public void ChangeSize()
    {
        float sizeFactor = ((-this.temperature * sizeIncrement) / 100f) - 0.5f - sizeIncrement;

        int m = /*UniversalGridPresure.altitudeTopTemp.start*/ -50;
        int M = /*UniversalGridPresure.altitudeDownTemp.end*/ 50;
        float r = universalGrid.particleRadious;
        float t = this.temperature;
        float c = sizeIncrement;

        float sizeFactor2 = (r * (c * c * (m - t) - M + t)) / (c * (m - M));
        this.transform.localScale = Vector3.one * sizeFactor2;
    }

    private void OnCollisionStay(Collision collision)
    {
        WeatherParticlePresure otherParticle = collision.gameObject.GetComponent<WeatherParticlePresure>();
        if (otherParticle)
        {
            float difTemp = this.temperature - otherParticle.temperature;
            if (Mathf.Abs(difTemp) > 1f)
            {
                otherParticle.ChangeTemperature(WeatherParticlePresure.transmissionCoefficient * Mathf.Clamp(difTemp / 2f, 1f, 10f));
                this.ChangeTemperature(-WeatherParticlePresure.transmissionCoefficient * Mathf.Clamp(difTemp / 2f, 1f, 10f));
            }
        }
    }

    #region IClient calls

    /// <summary>
    /// Injects the reference to a service into this client
    /// </summary>
    /// <param name="service">The reference to a required service, which will hold null 
    /// if that service is no longer available</param>
    public void Serve(UniversalGridPresure service)
    {
        if (service != null)
        {
            universalGrid = service;
        }
    }

    #endregion
}
