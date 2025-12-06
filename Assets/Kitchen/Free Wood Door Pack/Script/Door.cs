using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DoorScript
{
    [RequireComponent(typeof(AudioSource))]


    public class Door : MonoBehaviour
    {
        public bool open;
        public float smooth = 1.0f;
        float DoorOpenAngle = -90.0f;
        float DoorCloseAngle = 0.0f;
        public AudioSource asource;
        public AudioClip openDoor, closeDoor;

        private Quaternion targetRotation;
        // Use this for initialization
        void Start()
        {
            asource = GetComponent<AudioSource>();
            targetRotation = Quaternion.Euler(0, DoorCloseAngle, 0);

        }

        // Update is called once per frame
        void Update()
        {
            if (open)
            {
                var target = Quaternion.Euler(0, DoorOpenAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target, Time.deltaTime * 5 * smooth);

            }
            else
            {
                var target1 = Quaternion.Euler(0, DoorCloseAngle, 0);
                transform.localRotation = Quaternion.Slerp(transform.localRotation, target1, Time.deltaTime * 5 * smooth);

            }
        }

        public void OpenDoor()
        {
            open = !open;

            asource.clip = open ? openDoor : closeDoor;

            if (asource.clip != null)
            {
                Debug.Log("🔊 Playing sound: " + asource.clip.name);
            }
            else
            {
                Debug.Log("❌ No sound clip assigned!");
            }

            asource.Play();
        }

        // 🔥 Auto open when player enters
        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player") && CheeseCollect.cheeseCount >= 4)
            {
                open = true;
                targetRotation = Quaternion.Euler(0, DoorOpenAngle, 0);

                Debug.Log("🚪 Door opened! Player has enough cheese.");

                if (openDoor != null && !asource.isPlaying)
                {
                    Debug.Log("🔊 Playing openDoor sound: " + openDoor.name);
                    asource.PlayOneShot(openDoor);
                }
                else
                {
                    Debug.Log("❌ openDoor sound is missing!");
                }
            }
            else if (other.CompareTag("Player") && CheeseCollect.cheeseCount < 4)
            {
                Debug.Log("⚠ Player does NOT have enough cheese. Need 4.");

                TextHint.message = "You need 4 pieces of cheese to open this door!";
                TextHint.textOn = true;
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                open = false;
                targetRotation = Quaternion.Euler(0, DoorCloseAngle, 0);

                Debug.Log("🚪 Door closed.");

                if (closeDoor != null && !asource.isPlaying)
                {
                    Debug.Log("🔊 Playing closeDoor sound: " + closeDoor.name);
                    asource.PlayOneShot(closeDoor);
                }
                else
                {
                    Debug.Log("❌ closeDoor sound is missing!");
                }
            }
        }
    }
}