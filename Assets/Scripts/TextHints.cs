using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextHint : MonoBehaviour
{
    public static bool textOn = false;
    public static string message;
    private float timer = 0.0f;
    TextMeshProUGUI Hint;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Hint = GetComponent<TextMeshProUGUI>();
        timer = 0.0f;
        textOn = false;
        Hint.text = "";
    }

    // Update is called once per frame
    void Update()
    {
        if (textOn)
        {
            Hint.enabled = true;
            Hint.text = message;
            timer += Time.deltaTime;
        }
        if (timer >= 5.0f)
        {
            Hint.enabled = false;
            timer = 0.0f;
        }
    }
}
