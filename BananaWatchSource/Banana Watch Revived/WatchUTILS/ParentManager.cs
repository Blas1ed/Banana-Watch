using System.Collections.Generic;
using UnityEngine;

namespace Banana_Watch_Revived.WatchUTILS
{
    public class ParentManager : MonoBehaviour
    {
        public List<GameObject> Buttons;
        public void OnDisable()
        {
            foreach (var button in Buttons) 
            {
                button.GetComponent<ButtonScript>().CDown = false;
            }
        }
    }
}
