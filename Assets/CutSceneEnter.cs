using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CutSceneEnter : MonoBehaviour
{
    public GameObject thePlayer;
    public GameObject cutscene;
    public GameObject cutsceneCam;
    public GameObject MainsceneCam;

    void OnTriggerEnter2D(Collider2D collision)
    {
        this.gameObject.GetComponent<BoxCollider2D>().enabled = false;
        thePlayer.SetActive(false);
        cutscene.SetActive(true);
        cutsceneCam.SetActive(true);
        MainsceneCam.SetActive(false);
        StartCoroutine(FinishCut());

    
    }

    IEnumerator FinishCut()
    {
        yield return new WaitForSeconds(4);
        thePlayer.SetActive(true);
        cutscene.SetActive(false);
        cutsceneCam.SetActive(false);
        MainsceneCam.SetActive(true);
    }

     
    // Start is called before the first frame update
}
