using UnityEngine;
using System;

public class CameraMovement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    [SerializeField] private Transform target;
    [SerializeField] private float smoothAcc;
    [SerializeField] private float thresholdX = 2f;
    [SerializeField] public float smoothTime = 2f;
    private Vector3 velocity = Vector3.zero;

    // Use LateU`pdate so the camera follows after target movement
    void LateUpdate()
    {
        if (target == null) return;

        if (Mathf.Abs(transform.position.x - target.position.x) >= thresholdX)
        {
            float smoothSpeed = target.position.x-transform.position.x;
            if(smoothSpeed>0){
                smoothSpeed-= thresholdX;
            }else{
                smoothSpeed+= thresholdX;
            }
            /*if(Mathf.Abs(transform.position.x - target.position.x)>=4f){
                if(smoothSpeed>0){
                    smoothSpeed=4f;
                }else{
                    smoothSpeed=-4f;
                }
            }*/
            Debug.Log("smoothSpeed: "+smoothSpeed);
            Debug.Log("thresholdX: "+thresholdX);
            Debug.Log("deltaTime: "+Time.deltaTime);
            //float newX = Mathf.Lerp(transform.position.x, target.position.x, smoothSpeed);
            transform.position = new Vector3(transform.position.x+(smoothSpeed*smoothAcc*Time.deltaTime), transform.position.y, transform.position.z);
        }

        /*if (Mathf.Abs(transform.position.x - target.position.x) >= thresholdX){
            Vector3 targetPosition = new Vector3(
                target.position.x,
                transform.position.y,
                transform.position.z
            );

            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime
            );
        }*/
    }
}
