using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [System.Serializable]
    public struct Position
    {
        public Vector3 position;
        public Quaternion rotation;
    }
    public Position black;
    public Position white;
    public Position blackMobile;
    public Position whiteMobile;
    public bool isBlack;
    public Toggle reverseWhite;

    // Update is called once per frame
    void Update()
    {
        if (reverseWhite.isOn)
        {
            if (isBlack)
            {
                transform.position = Vector3.Lerp(transform.position, blackMobile.position, Time.deltaTime * 5);
                transform.rotation = Quaternion.Lerp(transform.rotation, blackMobile.rotation, Time.deltaTime * 5);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, whiteMobile.position, Time.deltaTime * 5);
                transform.rotation = Quaternion.Lerp(transform.rotation, whiteMobile.rotation, Time.deltaTime * 5);
            }
        }
        else
        {
            if (isBlack)
            {
                transform.position = Vector3.Lerp(transform.position, black.position, Time.deltaTime * 5);
                transform.rotation = Quaternion.Lerp(transform.rotation, black.rotation, Time.deltaTime * 5);
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, white.position, Time.deltaTime * 5);
                transform.rotation = Quaternion.Lerp(transform.rotation, white.rotation, Time.deltaTime * 5);
            }
        }
    }
}