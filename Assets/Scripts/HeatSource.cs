using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class HeatSource : MonoBehaviour {

    public float temperature;
    public float coefficient;

    [SerializeField]
    public RangeInt rang = new RangeInt(0,5);
    
    private void OnTriggerStay(Collider other)
    {
        WeatherParticlePresure otherParticle = other.gameObject.GetComponent<WeatherParticlePresure>();

        if (otherParticle)
        {
            float difTemp = otherParticle.temperature - this.temperature;
            if (difTemp < 1f)
            {
                otherParticle.ChangeTemperature(this.coefficient * Mathf.Abs(difTemp));
            }
        }
    }
    //private void OnCollisionStay(Collision collision)
    //{
    //    WeatherParticle otherParticle = collision.gameObject.GetComponent<WeatherParticle>();

    //    if (otherParticle)
    //    {
    //        float difTemp = otherParticle.temperature - this.temperature;
    //        if (difTemp < 1f)
    //        {
    //            otherParticle.ChangeTemperature(this.coefficient * Mathf.Abs(difTemp));
    //        }
    //    }
    //}    
}
