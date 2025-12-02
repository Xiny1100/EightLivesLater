using UnityEngine;

public class Switch : MonoBehaviour
{
    public Transform target1;
    public Transform target2;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Switch")){
            //  transform.lookAt(target1);

            //target1 = GameObject.Find("Cube").transform;
            //target2 = GameObject.Find("Sphere").transform;
            target1 = GameObject.FindWithTag("Cube").transform;
            target2 = GameObject.FindWithTag("Sphere").transform;

            if (GetComponent<FollowScript>().target == target1)
            {
                GetComponent<FollowScript>().target = target2;
            }
            else
            {
                GetComponent<FollowScript>().target = target1;
            }
        }
        // add this later if let go button, spotlight look at target 2
    }
}
