using System.Collections;
using System.Collections.Generic;                        //Generic algorithm
using UnityEngine;
using System.Linq;

public class SpawnCars : MonoBehaviour                   //here MonoBehaviour is a Parent class and i created a child class of name SpawnCars
{
    public int numCars;                             //public means we can change it from the software directly , no need to change it in script directly
    public GameObject CarPrefab;
    public List<GameObject> Cars;
    int generationTime = 20;  
    float startTime = 0;
    public int generation = 1;

    public TMPro.TextMeshProUGUI textMesh;
    void Start(){                                        //not confirm : can be anything but start with uppercase letter
        for (int i=0;i<numCars;i++){
            GameObject c = Instantiate(CarPrefab,this.transform.position,this.transform.rotation);            //these Instantiate like functions are present in MonoBehaviour class which is my child class inherits
            AIController ai = c.GetComponent<AIController>();
            ai.steeringSensitivity = Random.Range(0.01f,0.03f);
            ai.lookAhead = Random.Range(18.0f, 22.0f);
            ai.maxTorque = Random.Range(180.0f,220.0f);
            ai.maxSteerAngle = Random.Range(50.0f, 70.0f);
            ai.maxBrakeTorque = Random.Range(4500.0f, 5000.0f);
            ai.accelCornerMax = Random.Range(18.0f,22.0f);
            ai.brakeCornerMax = Random.Range(2.0f,7.0f);
            ai.accelVelocityThreshold = Random.Range(18.0f,22.0f);
            ai.brakeVelocityThreshold = Random.Range(8.0f,22.0f);
            ai.antiroll = Random.Range(4500.0f,5500.0f);
            Cars.Add(c);
        }
        Time.timeScale = 5;    //to increase time speed ,we will see fast  what is going on 
        textMesh.text = "Trial: " + generation;

    }
    
    GameObject GeneSwap(AIController parent1, AIController parent2){
        GameObject c = Instantiate(CarPrefab, this.transform.position, this.transform.rotation);
        AIController ai = c.GetComponent<AIController>();

        ai.steeringSensitivity = (parent1.steeringSensitivity + parent2.steeringSensitivity)/2.0f;
        ai.lookAhead = (parent1.lookAhead + parent2.lookAhead)/2.0f;
        ai.maxTorque = (parent1.maxTorque + parent2.maxTorque)/2.0f;
        ai.maxSteerAngle = (parent1.maxSteerAngle + parent2.maxSteerAngle)/2.0f;
        ai.maxBrakeTorque = (parent1.maxBrakeTorque + parent2.maxBrakeTorque)/2.0f;
        ai.accelCornerMax = (parent1.accelCornerMax + parent2.accelCornerMax)/2.0f;
        ai.brakeCornerMax = (parent1.brakeCornerMax + parent2.brakeCornerMax)/2.0f;
        ai.accelVelocityThreshold = (parent1.accelVelocityThreshold + parent2.accelVelocityThreshold)/2.0f;
        ai.brakeVelocityThreshold = (parent1.brakeVelocityThreshold + parent2.brakeVelocityThreshold)/2.0f;
        ai.antiroll = (parent1.antiroll + parent2.antiroll)/2.0f;
        return c;
   }

   void Breed(){
    startTime = Time.realtimeSinceStartup;
    List<GameObject> sortedCars = Cars.OrderByDescending(o=> o.GetComponent<AIController>().fitness).ToList();
    
    int halfCars = (int)(sortedCars.Count / 2.0f);
    Cars.Clear();
    for(int i=0; i<halfCars; i++){
        Cars.Add(GeneSwap(sortedCars[i].GetComponent<AIController>(), sortedCars[i + 1].GetComponent<AIController>()));
        Cars.Add(GeneSwap(sortedCars[i + 1].GetComponent<AIController>(), sortedCars[i].GetComponent<AIController>()));
    }

    for(int i=0; i<sortedCars.Count; i++){
        Destroy(sortedCars[i]);
    }

    generation++;
    textMesh.text = "Trial: " + generation;
   }


    void Update(){
        if(Time.realtimeSinceStartup > startTime + generationTime){
            Breed();
        }
       
    }
}
