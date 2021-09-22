using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class Seawolf8Control : MonoBehaviour
{

    public Camera realSense;
    public float simSpeed = 0.05f;
    public float simAngleSpeed = 0.05f;
    public float realWorldScale = 1.0f / 10.0f;
    ROSConnection ros;

    //rate of movement
    float forwardRate = 0.0f;
    float strafeRate = 0.0f;
    float verticalRate = 0.0f;

    //rate of rotation
    float pitchRate = 0.0f;
    float rollRate = 0.0f;
    float yawRate = 0.0f;

    
    // Start is called before the first frame update
    void Start()
    {
        ros = ROSConnection.instance;
        ros.Subscribe<Float32MultiArrayMsg>("wolf_RC_output", RCCallback);
        ros.RegisterPublisher<Float64Msg>("wolf_gazebo/global_alt");
        ros.RegisterPublisher<Float64Msg>("wolf_gazebo/compass_hdg");
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //update motion based on RC rates
        this.gameObject.GetComponent<Rigidbody>().velocity = this.gameObject.transform.worldToLocalMatrix * new Vector3(strafeRate, forwardRate, -verticalRate) * simSpeed;
        this.gameObject.GetComponent<Rigidbody>().angularVelocity = new Vector3(pitchRate, -yawRate, rollRate);

        //send state data
        ros.Send<Float64Msg>("wolf_gazebo/global_alt", new Float64Msg(this.transform.position.y * realWorldScale));
        ros.Send<Float64Msg>("wolf_gazebo/compass_hdg", new Float64Msg(Mathf.Deg2Rad * this.transform.rotation.eulerAngles.y));
    }


    void RCCallback(Float32MultiArrayMsg rates)
    {
        //only accepts a 6 member 1D float array
        if (rates.data.Length != 6)
        {
            return;
        }

        pitchRate = rates.data[0];
        rollRate = rates.data[1];
        verticalRate = rates.data[2];
        yawRate = rates.data[3];
        forwardRate = rates.data[4];
        strafeRate = rates.data[5];
    }
}
