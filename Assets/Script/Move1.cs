using UnityEngine;

public class Move1 : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    
        public float speed;
   

    // Update is called once per frame
    void Update()
    {
        //transform.Translate(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical");
        float x = Input.GetAxis("Horizontal") * Time.deltaTime;
        float z = Input.GetAxis("Vertical") * Time.deltaTime;
        transform.Translate(x*speed, 0, z * speed);
    }
}
