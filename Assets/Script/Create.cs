using UnityEngine;

public class Create : MonoBehaviour
{
    public Transform newObject;
    private int sphereCount;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetButtonDown("Fire1")) {
            Instantiate(newObject, new Vector3(Random.Range(-10.0f,10.0f), Random.Range(-10.0f, 10.0f), 0), transform.rotation);
            sphereCount++;
            Debug.Log("new object created :" + sphereCount);
        }
    }
}
