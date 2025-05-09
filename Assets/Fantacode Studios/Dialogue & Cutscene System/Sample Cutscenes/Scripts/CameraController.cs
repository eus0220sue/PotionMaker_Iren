using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem
{

    public class CameraController : MonoBehaviour
    {
        [SerializeField] Transform followTarget;

        [SerializeField] float rotationSpeed = 2f;
        [SerializeField] float distance = 2.5f;

        [SerializeField] float minVerticalAngle = -45;
        [SerializeField] float maxVerticalAngle = 45;

        [SerializeField] Vector2 framingOffset;

        [SerializeField] bool invertX;
        [SerializeField] bool invertY;


        float rotationX = 0f;
        float rotationY = -90f;

        float invertXVal;
        float invertYVal;

        private void Update()
        {
            invertXVal = (invertX) ? -1 : 1;
            invertYVal = (invertY) ? -1 : 1;

            rotationX += Input.GetAxis("Mouse Y") * invertYVal * rotationSpeed;
            rotationX = Mathf.Clamp(rotationX, minVerticalAngle, maxVerticalAngle);

            rotationY += Input.GetAxis("Mouse X") * invertXVal * rotationSpeed;

            var targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

            var focusPostion = followTarget.position + new Vector3(framingOffset.x, framingOffset.y);

            transform.position = focusPostion - targetRotation * new Vector3(0, 0, distance);
            transform.rotation = targetRotation;
        }

        public Quaternion PlanarRotation => Quaternion.Euler(0, rotationY, 0);
    }
}
