using System.Collections;
using UnityEngine;

namespace Banana_Watch_Revived.WatchUTILS
{
    public class WatchCollider : MonoBehaviour
    {
        public GameObject WatchObject;
        private bool delayed;
        public void OnTriggerEnter(Collider other)
        {
            if (other.name == "RightHandTriggerCollider")
            {
            if (!WatchObject.activeSelf && !delayed)
            {
                WatchObject.SetActive(true);
                    StartCoroutine(Delay(0.6f));
            }
            else if (!delayed)
            {
                WatchObject.SetActive(false);
                StartCoroutine(Delay(0.6f));
                }
            }

        }

        public IEnumerator Delay(float delay)
        {
            delayed = true;
            yield return new WaitForSeconds(delay);
            delayed = false;
        }
    }
}
