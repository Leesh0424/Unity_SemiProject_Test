using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterCamera : MonoBehaviour
{
    private Transform cameraTransform = null;
    public GameObject objTarget = null;
    private Transform objTargetTransform = null;
    public float distance = 6.0f;
    public float height = 1.75f;
    public float heightDamping = 2.0f;
    public float rotationDamping = 3.0f;

    void Start()
    {
        cameraTransform = GetComponent<Transform>();

        if (objTarget != null)
        {
            objTargetTransform = objTarget.transform;
        }
    }

    void ThirdCamera()
    {
        float objTargetRotationAngle = objTargetTransform.eulerAngles.y;
        float objHeight = objTargetTransform.position.y + height;
        float nowRotationAngle = cameraTransform.eulerAngles.y;
        float nowHeight = cameraTransform.position.y;

        nowRotationAngle = Mathf.LerpAngle(nowRotationAngle, objTargetRotationAngle, rotationDamping * Time.deltaTime);
        nowHeight = Mathf.Lerp(nowHeight, objHeight, heightDamping * Time.deltaTime);
        Quaternion nowRotation = Quaternion.Euler(0f, nowRotationAngle, 0f);

        cameraTransform.position = objTargetTransform.position;
        cameraTransform.position -= nowRotation * Vector3.forward * distance;
        cameraTransform.position = new Vector3(cameraTransform.position.x, nowHeight, cameraTransform.position.z);
        cameraTransform.LookAt(objTargetTransform);
    }

    private void LateUpdate()
    {
        if (objTarget == null)
        {
            return;
        }

        if (objTargetTransform == null)
        {
            objTargetTransform = objTarget.transform;
        }

        ThirdCamera();
    }
}