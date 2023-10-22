using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    public GameObject agentObject;
    public newAgent agent;
    void Start(){
        agent = agentObject.GetComponent<newAgent>();
    }
   public void OnTriggerEnter(Collider other) {
    if(other.CompareTag("Block")){
        agent.targetEntry(this.gameObject);
    }
   }

   public void OnTriggerExit(Collider other) {
    if(other.CompareTag("Block")){
        agent.targetExit(this.gameObject);
    }
   }
}