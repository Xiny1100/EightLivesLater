using UnityEngine;
using System.Collections;

public class TestCouroutine : MonoBehaviour
{
    public Renderer rend;
    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            StartCoroutine("Fade");
        }
    }

    IEnumerator Fade() {
        yield return new WaitForSeconds(1.0f);
        rend.material.color = Color.red;
    }

}


