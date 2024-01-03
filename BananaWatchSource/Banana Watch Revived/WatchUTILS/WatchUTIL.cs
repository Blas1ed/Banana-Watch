using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using TMPro;

namespace Banana_Watch_Revived.WatchUTILS
{
    public static class WatchUTIL
    {
        public static void SetupAssetBundle()
        {
            Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Banana_Watch_Revived.Resources.wristwatch");
            Plugin.MainBundle = AssetBundle.LoadFromStream(stream);
            Plugin.Prefab = Plugin.MainBundle.LoadAsset<GameObject>("WristWatch");

            GameObject obj = UnityEngine.Object.Instantiate(Plugin.Prefab, GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.L/upper_arm.L/forearm.L/hand.L").transform);
            Transform Parent = FindInParent("Watch", obj.transform);
            GameObject Collider = FindInParent("MenuCollider", Parent).gameObject;
            GameObject Menu = FindInParent("MenuStuff", Parent).gameObject;
            Plugin.ActiveMenuPrefab = obj;
            Plugin.MenuObj = Menu;
           WatchCollider col = Collider.AddComponent<WatchCollider>();
            Plugin.Teleprefab = GameObject.Instantiate(Plugin.MainBundle.LoadAsset<GameObject>("TeleSphere"));
            Plugin.lr = GameObject.Instantiate(Plugin.MainBundle.LoadAsset<GameObject>("Line")).GetComponent<LineRenderer>();
            col.WatchObject = Menu;
            stream.Close();
        }

        public static List<GameObject> ButtonObjects()
        {
            Transform Parent = FindInParent("Watch", Plugin.ActiveMenuPrefab.transform);
            Transform Menu = FindInParent("MenuStuff", Parent);
            Transform ButtonsParent = FindInParent("Buttons", Menu);

            List<GameObject> Buttons = new List<GameObject>
            {
                FindInParent("Button 1", ButtonsParent).gameObject,
                FindInParent("Button 2", ButtonsParent).gameObject,
                FindInParent("Button 3", ButtonsParent).gameObject
            };

            List<GameObject> HomeButtons = new List<GameObject>()
            {
                FindInParent("ActivateMods", ButtonsParent).gameObject,
                FindInParent("Info", ButtonsParent).gameObject,
                FindInParent("Mods", ButtonsParent).gameObject
            };
            List<GameObject> ModButtons = new List<GameObject>()
            {
                FindInParent("ModPage 1", ButtonsParent).gameObject,
                FindInParent("ModPage 2", ButtonsParent).gameObject,
                FindInParent("ModPage 3", ButtonsParent).gameObject
            };

            List<GameObject> ArrowKeys = new List<GameObject>()
            {
                FindInParent("TabUp", ButtonsParent).gameObject,
                FindInParent("TabDown", ButtonsParent).gameObject,
                FindInParent("ModUp", ButtonsParent).gameObject,
                FindInParent("ModDown", ButtonsParent).gameObject,
                FindInParent("ModEnter", ButtonsParent).gameObject
            };

            Transform InfoParent = FindInParent("InfoPage", ButtonsParent);

            Plugin.Instance.ModButtons = ModButtons;

            TextMeshPro Versionpart = FindInParent("WatchVersion", InfoParent).GetComponent<TextMeshPro>();

            Plugin.Infopart = FindInParent("Info", InfoParent).GetComponent<TextMeshPro>();

            Versionpart.text = $"Banana Watch V{PluginInfo.Version}";

            Plugin.Infopart.text = Plugin.InfoPage;

            Plugin.Instance.HomeButtons = HomeButtons;

           ParentManager man = Plugin.MenuObj.AddComponent<ParentManager>();

            man.Buttons = ArrowKeys.ToList();

            foreach (GameObject Key in ArrowKeys)
            {
               ButtonScript Func = Key.AddComponent<ButtonScript>();

                if (Key.name == "TabUp" || Key.name == "TabDown")
                {
                Func.ArrowKey = true;
                    if (Func.name == "TabUp")
                    {
                        Func.Up = true;
                    }
                    else if (Func.name == "TabDown")
                    {
                        Func.Up = false;
                    }
                }
                else if (Key.name == "ModUp" || Key.name == "ModDown")
                {
                    if (Func.name == "ModUp")
                    {
                        Func.Up = true;
                    }
                    else if (Func.name == "ModDown")
                    {
                        Func.Up = false;
                    }
                }
                else
                {
                    Func.EnterFunc = true;
                }

            }

            foreach (GameObject button in Buttons)
            {
               ButtonScript bs = button.AddComponent<ButtonScript>();
               bs.ButtonInt = Buttons.IndexOf(button);
            }

            return Buttons;
        }

        public static Material GetMaterialAssetBundle(string path, string MatName)
        {
            Material mat = Plugin.MainBundle.LoadAsset<Material>(MatName);
            return mat;
        }

        public static TextMeshPro GetTextFromParent(string TextName, Transform Parent)
        {
            foreach (Transform child in Parent)
            {
                if (child.name == TextName)
                {
                    return child.GetComponent<TextMeshPro>();
                }
            }
            return null;
        }

        public static Transform FindInParent(string ChildName, Transform Parent)
        {
            foreach (Transform child in Parent)
            {
                if (child.name == ChildName)
                {
                    return child;
                }
            }

            return null;
        }
    }
}
