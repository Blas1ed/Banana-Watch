using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Banana_Watch_Revived.WatchUTILS
{
    public class ButtonScript : MonoBehaviour
    {
        public int ButtonInt;
        public bool ArrowKey;
        public bool EnterFunc;
        public bool Up;
        public bool CDown;
        public void OnTriggerEnter(Collider other)
        {
            if (other.name == "RightHandTriggerCollider")
            {
                if (!ArrowKey && !CDown && !EnterFunc)
                {
                    Plugin.Instance.ModChange(Up);

                    StartCoroutine(Cooldown());
                }
                else if (!CDown && !EnterFunc)
                {
                    if (!Up && !Plugin.InHomePage)
                    {
                        if (Plugin.Instance.CurrentPageInt == 0)
                        {
                            Plugin.OpenNewPage("Home", Plugin.Instance.Buttons);
                        }
                    }
                    else if (!Up) 
                    {
                        if (Plugin.CurrentPage == "Info" || Plugin.CurrentPage == "Mods")
                        {
                            if (Plugin.CurrentPage == "Info")
                            {
                            Plugin.OpenNewPage("Home", new List<GameObject>() { Plugin.Infopart.transform.parent.gameObject });
                            }
                            else if (Plugin.CurrentPage == "Mods" && Plugin.Instance.PageIndexes[1] == 0)
                            {
                                Plugin.OpenNewPage("Home", Plugin.Instance.ModButtons);
                            }
                           

                        }
                    }

                    Plugin.TabMenu(Up);


                    StartCoroutine(Cooldown());
                }
                else if (EnterFunc)
                {
                    if (!Plugin.InHomePage)
                    {
                        Plugin.SetFunction(Plugin.CurrentSelectedMod, null);
                    }
                    else
                    {
                        if (Plugin.CurrentPage != "Mods")
                        {
                       Plugin.SetFunction(Plugin.CurrentActiveHomeTab, Plugin.Instance.HomeButtons, Plugin.Instance.HomeButtons[Plugin.CurrentActiveHomeTab].name);
                        }
                        else
                        {
                            Plugin.SetFunction(Plugin.Instance.PageIndexes[0], Plugin.Instance.HomeButtons, "");
                        }
                    }
                    StartCoroutine(Cooldown());

                }


                    Plugin.Instance.RedButton(true, gameObject, false);
                Plugin.ActiveMenuPrefab.GetComponent<AudioSource>().Play();
            }
        }

        public void OnTriggerExit(Collider other) 
        { 
            if (other.name == "RightHandTriggerCollider")
            {
            Plugin.Instance.RedButton(false, gameObject, false);
            }

        }

        public IEnumerator Cooldown()
        {
             CDown = true;
            yield return new WaitForSeconds(0.6f);
            CDown = false;
        }
    }
}
