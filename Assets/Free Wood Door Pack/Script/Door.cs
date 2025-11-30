using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace DoorScript
{
	[RequireComponent(typeof(AudioSource))]


public class Door : MonoBehaviour {

	public bool open;
	public float smooth = 1.0f;
	public float DoorOpenAngle = -90.0f;
    public float DoorCloseAngle = 0.0f;

	public AudioSource asource;
	public AudioClip openDoor,closeDoor;

    private Quaternion targetRotation;
        // Use this for initialization
    void Start () {
		asource = GetComponent<AudioSource> ();
            targetRotation = Quaternion.Euler(0, DoorCloseAngle, 0);
        }
	
	// Update is called once per frame
	void Update () {
            transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, smooth * Time.deltaTime);
        }

	public void OpenDoor(){
		open =!open;
		asource.clip = open?openDoor:closeDoor;
		asource.Play ();
	}

        // 🔥 Auto open when player enters
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                open = true;
                targetRotation = Quaternion.Euler(0, DoorOpenAngle, 0);

                if (openDoor != null && !asource.isPlaying)
                {
                    asource.PlayOneShot(openDoor);
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                open = false;
                targetRotation = Quaternion.Euler(0, DoorCloseAngle, 0);

                if (closeDoor != null && !asource.isPlaying)
                {
                    asource.PlayOneShot(closeDoor);
                }
            }
        }
    }
}