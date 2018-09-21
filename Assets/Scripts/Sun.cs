using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sun : MonoBehaviour
{
    /// <summary>
    /// Duration of day in seconds
    /// </summary>
    public int dayDuration;
    /// <summary>
    /// Duration of year in days
    /// </summary>
    public int yearDuration;
    /// <summary>
    /// winter day duration time percentage compare with summer day duration time
    /// </summary>
    /// 
    [Range(0,100f)]
    public float timeDifferencePercentage;

    public Light sunLight;

    private Vector3 axisRotation;
    private float lightTime;

    [Range(0f,1f)]
    public float smoothness;
    public int iterations;

    

    private Quaternion originalRotation;

    private Vector3 originVectorRight;

    private Vector3 originVectorForward;
	// Use this for initialization
	void Start ()
	{
        axisRotation = Vector3.right;
	    //iterations = Mathf.Clamp(Mathf.FloorToInt(smoothness * iterations), 1, _iterations);
	    StartCoroutine(Movement(1f));
	    originalRotation = this.transform.rotation;
	    originVectorRight = this.transform.right;
	    originVectorForward = this.transform.up;
	}
	
	
	
    /*  Funcion senoidal para el año:
        y=((d/4)*(1-p)*(cos(i*360/a)) + (d/4)*(1+p))

        y = horas de luz;
        d = duracion del dia (horas);
        p = porcentaje de duracion del dia en invierno frente al dia en verano
        i = dia del año
        a = dias que dura el año

    */
    IEnumerator Movement(float period)
    {
        while (true)
        {
            for (int i = 0; i < yearDuration; i++)
            {
                lightTime = ((dayDuration / 4f) * (1f - timeDifferencePercentage/100f) * (Mathf.Cos(i * 2f* Mathf.PI / yearDuration)) + (dayDuration / 4f) * (1f + timeDifferencePercentage/100f));
                //Debug.Log("hours of light: " + lightTime + " at day " + i);
                for (int j = 0; j < iterations; j++)
                {
                    float angleZ = (1f-(lightTime * 2f / (float)dayDuration))*90f;
                    Quaternion rotantionOnZaxis =  Quaternion.AngleAxis(angleZ,originVectorForward);
                    float angleX = (float)j / ((float)iterations) * 180f;
                    //Debug.Log(angleX);
                    Quaternion rotantionOnXaxis = Quaternion.AngleAxis(angleX, originVectorRight);
                    Quaternion newRotation = originalRotation * rotantionOnXaxis * rotantionOnZaxis;
                    this.transform.rotation = newRotation;
                    yield return new WaitForSeconds((float)dayDuration/(float)iterations);
                }
                yield return new WaitForSeconds(dayDuration-lightTime);                
            }
        }
    }
}
