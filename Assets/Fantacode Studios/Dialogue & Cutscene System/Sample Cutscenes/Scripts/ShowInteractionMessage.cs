using FC_CutsceneSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FC_CutsceneSystem {

    public class ShowInteractionMessage : MonoBehaviour
    {
        [SerializeField] GameObject objectToShow;
        [SerializeField] GameObject objectToHide;
        [SerializeField] Transform thirdPersonCam;

        private void Start()
        {
            CutsceneManager.instance.OnCutsceneStart += () => objectToShow.SetActive(false);
        }

        private void Update()
        {
            if (objectToShow.activeSelf)
            {
                objectToShow.transform.LookAt(thirdPersonCam, Vector3.up); 
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                objectToShow.SetActive(true);
                objectToHide.SetActive(false);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
                objectToShow.SetActive(false);
        }
    }

}
