using Banana_Watch_Revived.WatchUTILS;
using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using Utilla;
using ComputerPlusPlus.Tools;
using GorillaLocomotion;
using Photon.Pun;
using Photon.Realtime;
using Photon;

namespace Banana_Watch_Revived
{
    [System.Serializable]
    public class Button
    {
        public string ButtonId = "";
        public string ButtonFunctionId = "";
        public string Description;
        public bool Active = false;
    }

    [System.Serializable]
    public class ModButton
    {
       public BepInEx.PluginInfo PluginInfo = new BepInEx.PluginInfo();
    }

    [System.Serializable]
    public class Page
    {
       public List<Button> ActiveButtons = new List<Button>();
    }

    [System.Serializable]
    public class ModPage
    {
        public List<ModButton> ActiveButtons = new List<ModButton>();
    };

    [BepInPlugin(PluginInfo.GUID, PluginInfo.Name, PluginInfo.Version)]
    [BepInDependency("com.kylethescientist.gorillatag.computerplusplus", "1.0.1")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.11")]
    public class Plugin : BaseUnityPlugin
    {
        public static AssetBundle MainBundle;
        public static GameObject Prefab;
        public static GameObject ActiveMenuPrefab;
        public static GameObject MenuObj;
        public static Plugin Instance { get; private set; }
        public List<string> MenuButtons = new List<string> { "Low Grav", "Fly", "Teleport", "Grapple" };
        public List<GameObject> HomeButtons = new List<GameObject>();
        public List<string> MenuButtonDescriptions = new List<string> { "Turn Off Your Gravity!", "Fly around the map!", "Teleport around the map!", "Grapple Gun!" };
        public static string InfoPage { get; } = "Banana Menu Info\n" + 
                                                            "Menu made by Blas1ed you can contact me in the gorilla tag modding discord @Blas1ed";
        public List<GameObject> Buttons = new List<GameObject>();
        public List<ModPage> ModPages = new List<ModPage>();
        public List<GameObject> ModButtons = new List<GameObject>();
        public List<string> ModStringButtons = new List<string>();
        public int CurrentPageInt = 0;
        public List<Page> Pages = new List<Page>();
        public XRNode RNode = XRNode.RightHand;
        public static int CurrentSelectedMod = 0;
        public static int CurrentActiveHomeTab = 0;
        public static TextMeshPro Infopart = new TextMeshPro();
        private SpringJoint joint = new SpringJoint();
        public static LineRenderer lr = new LineRenderer();
        private Vector3 grapplePoint = new Vector3();
        private LayerMask whatIsGrappleable;
        private float maxDistance = 100f;
        private bool CanGrapple = false;
        private Transform gunTip;
        public List<int> PageIndexes = new List<int>() { 0, 0 };
        public static bool InHomePage = true;
        private Material RedMat;
        private Material WhiteMat;
        private Material BlackMat;
        public float RightTriggerValue = 0f;
        public static string CurrentPage = "Home";
        public List<BepInEx.PluginInfo> pinfos = new List<BepInEx.PluginInfo>();
        public static bool InModded { get; private set; }
        public static GameObject Teleprefab;

/*        public static bool TestingMode { get; } = true;*/

        public void Start()
        {
            Utilla.Events.GameInitialized += Init;
            Instance = this;
        }

        public void Init(object sender, EventArgs e)
        {
            WatchUTILS.WatchUTIL.SetupAssetBundle();
            Buttons = WatchUTILS.WatchUTIL.ButtonObjects();
            WhiteMat = WatchUTIL.GetMaterialAssetBundle("Banana_Watch_Revived.Resources.wristwatch", "TestMat");
            BlackMat = WatchUTIL.GetMaterialAssetBundle("Banana_Watch_Revived.Resources.wristwatch", "TestMat 1");
            RedMat = WatchUTIL.GetMaterialAssetBundle("Banana_Watch_Revived.Resources.wristwatch", "redclicked");
           whatIsGrappleable = 1 << 9;

            gunTip = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform;

            foreach (var plugin in BepInEx.Bootstrap.Chainloader.PluginInfos.Values)
            {
                try
                {
                    Debug.Log("Found plugin:" + plugin.Metadata.Name);
                    if (plugin.Metadata.Name == "Computer++")
                    {
                        continue;
                    }
                    if (plugin.Metadata.Name == "Utilla")
                    {
                        continue;
                    }
                    if (plugin.Metadata.Name == PluginInfo.Name)
                    {
                        continue;
                    }

                    var entry = ComputerPlusPlus.Plugin.Instance.Config.Bind("EnabledMods", plugin.Metadata.Name, true);
                    plugin.Instance.enabled = entry.Value;
                    pinfos.Add(plugin);
                }
                catch (Exception ee)
                {
                    Logging.Exception(ee);
                }
            }

            foreach (string button in MenuButtons) 
            {
                if (Pages.Count == 0)
                {
                    Pages.Add(new Page());
                }
                foreach (Page page in Pages.ToList()) 
                {
                    if (page.ActiveButtons.Count < Buttons.Count)
                    {
                        Button Btn = new Button();
                        Btn.ButtonId = button;
                        Btn.ButtonFunctionId = button;
                        Btn.Description = MenuButtonDescriptions[MenuButtons.IndexOf(button)];
                        page.ActiveButtons.Add(Btn);
                    }
                    else if (Pages.IndexOf(page) == Pages.Count - 1)
                    {
                        Button Btn = new Button();
                        Btn.ButtonId = button;
                        Btn.ButtonFunctionId = button;
                        Btn.Description = MenuButtonDescriptions[MenuButtons.IndexOf(button)];
                        Pages.Add(new Page());
                        Pages[Pages.IndexOf(page) + 1].ActiveButtons.Add(Btn);
                    }
                }
            }

            /*foreach (GameObject button in ModButtons)
            {
                if (ModPages.Count == 0)
                {
                    ModPages.Add(new ModPage());
                }
                foreach (ModPage page in ModPages.ToList())
                {
                    if (page.ActiveButtons.Count < ModButtons.Count)
                    {
                        page.ActiveButtons.Add(button);
                    }
                    else if (ModPages.IndexOf(page) == ModPages.Count - 1)
                    {
                        ModPages.Add(new ModPage());
                        ModPages[ModPages.IndexOf(page) + 1].ActiveButtons.Add(button);
                    }
                }
            }*/

            foreach (BepInEx.PluginInfo pinfo in pinfos)
            {
                if (ModPages.Count == 0)
                {
                    ModPages.Add(new ModPage());
                }
                foreach (ModPage page in ModPages.ToList())
                {
                    if (page.ActiveButtons.Count < ModButtons.Count)
                    {
                        ModButton Btn = new ModButton();
                        Btn.PluginInfo = pinfo;
                        page.ActiveButtons.Add(Btn);
                    }
                    else if (ModPages.IndexOf(page) == ModPages.Count - 1)
                    {
                        ModButton Btn = new ModButton();
                        Btn.PluginInfo = pinfo;
                        ModPages.Add(new ModPage());
                        ModPages[ModPages.IndexOf(page) + 1].ActiveButtons.Add(Btn);
                    }
                }
            }

            foreach (GameObject obj in Buttons)
            {
                obj.SetActive(false);
            }

            foreach (GameObject obj in HomeButtons) 
            {
                obj.SetActive(true);
            }

            var value = ComputerPlusPlus.Plugin.Instance.Config.Bind("EnabledMods", PluginInfo.Name, true);
            bool PluginEnabled = value.Value;

            if (!PluginEnabled)
            {
                ActiveMenuPrefab.SetActive(false);
            }
        }

        public void ModChange(bool up)
        {
                if (!InHomePage)
                {
           if (up && CurrentSelectedMod < Pages[CurrentPageInt].ActiveButtons.Count - 1) 
            {            
               CurrentSelectedMod++;
            }
           else if (!up && CurrentSelectedMod != 0)
            {
                CurrentSelectedMod--;
            }
                }
                else
            {
                if (CurrentPage == "Home")
                {
                if (up && CurrentActiveHomeTab < HomeButtons.Count - 1)
                {
                    CurrentActiveHomeTab++;
                }
                else if (!up && CurrentActiveHomeTab != 0)
                {
                    CurrentActiveHomeTab--;
                }
                }
                else if (CurrentPage == "Mods")
                {
                    if (up && PageIndexes[0] < ModPages[PageIndexes[1]].ActiveButtons.Count - 1)
                    {
                        PageIndexes[0]++;
                    }
                    else if (!up && PageIndexes[0] != 0)
                    {
                        PageIndexes[0]--;
                    }
                }

            }
        }

        public void RedButton(bool on, GameObject obj, bool Whitemat)
        {
            if (on)
            {
                obj.GetComponent<Renderer>().material = RedMat;
            }
            else
            {
                if (Whitemat)
                {
                obj.GetComponent<Renderer>().material = WhiteMat;
                }
                else
                {
                    obj.GetComponent<Renderer>().material = BlackMat;
                }
            }
        }

        public void Teleportation()
        {
            int layerIndex = LayerMask.NameToLayer("Gorilla Object");
            LayerMask layerMask = 1 << layerIndex;

            Vector3 rayStart = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.position;

          rayStart =  GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.position;

            Vector3 rayDirection = GameObject.Find("Player Objects/Local VRRig/Local Gorilla Player/rig/body/shoulder.R/upper_arm.R/forearm.R/hand.R").transform.up;

                Ray ray = new Ray(rayStart, rayDirection);

                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
            {
                GameObject hitObject = hit.collider.gameObject;
                Teleprefab.SetActive(true);
                Teleprefab.transform.position = hit.point;
                }
                else
            {
                Teleprefab.SetActive(false);
            }

                if (Teleprefab.activeSelf)
            {
                if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5)
                {
                    GameObject GorillaPlayer = GameObject.Find("GorillaPlayer");
                    GorillaPlayer.GetComponent<Rigidbody>().velocity = Vector3.zero;
                    GorillaPlayer.transform.position = hit.point;
                }
            }
            }

        public void Grapple()
        {
            DrawRope();
            if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5 && CanGrapple)
            {
                StartGrapple();
            }
            else if (ControllerInputPoller.instance.rightControllerIndexFloat < 0.5)
            {
                StopGrapple();
            }
        }

        void DrawRope()
        {
            if (!joint) return;

            currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 8f);

            lr.SetPosition(0, gunTip.position);
            lr.SetPosition(1, currentGrapplePosition);
        }

        void StartGrapple()
        {
            RaycastHit hit;

            if (Physics.Raycast(gunTip.position, gunTip.up, out hit, maxDistance, whatIsGrappleable))
            {
                grapplePoint = hit.point;
                joint = GameObject.Find("GorillaPlayer").gameObject.AddComponent<SpringJoint>();
                joint.autoConfigureConnectedAnchor = false;
                joint.connectedAnchor = grapplePoint;

                float distanceFromPoint = Vector3.Distance(gunTip.position, grapplePoint);

                joint.maxDistance = distanceFromPoint * 0.6f;
                joint.minDistance = distanceFromPoint * 0.15f;

                joint.spring = 18f;
                joint.damper = 2f;
                joint.massScale = 10f;

                lr.positionCount = 2;
                currentGrapplePosition = gunTip.position;
                CanGrapple = false;
            }
        }

        void StopGrapple()
        {
            lr.positionCount = 0;
            Destroy(joint);
            CanGrapple = true;
        }

        private Vector3 currentGrapplePosition;

        public void Awake()
        {

        }

        public void ButtonFunctions()
        {

            foreach (Page page in Pages)
            {
                foreach (Button button in page.ActiveButtons)
                {
                    if (Pages.IndexOf(page) == CurrentPageInt)
                    {
                    if (button.Active)
                    {
                        SendModFunction(button.ButtonFunctionId);

                            WatchUTIL.FindInParent("ButtonMod", Buttons[page.ActiveButtons.IndexOf(button)].transform).GetComponent<TextMeshPro>().color = Color.red;
                            WatchUTIL.FindInParent("ButtonStatus", Buttons[page.ActiveButtons.IndexOf(button)].transform).GetComponent<TextMeshPro>().color = Color.green;
                            WatchUTIL.FindInParent("ButtonStatus", Buttons[page.ActiveButtons.IndexOf(button)].transform).GetComponent<TextMeshPro>().text = "[ON]";
                        }
                        else
                        {

                            WatchUTIL.FindInParent("ButtonMod", Buttons[page.ActiveButtons.IndexOf(button)].transform).GetComponent<TextMeshPro>().color = Color.white;
                            WatchUTIL.FindInParent("ButtonStatus", Buttons[page.ActiveButtons.IndexOf(button)].transform).GetComponent<TextMeshPro>().color = Color.white;
                            WatchUTIL.FindInParent("ButtonStatus", Buttons[page.ActiveButtons.IndexOf(button)].transform).GetComponent<TextMeshPro>().text = "[OFF]";
                            EndFunction(page.ActiveButtons.IndexOf(button));
                    }
                    }

                }
            }
        }

        public void EndFunction(int ButtonID)
        {
            
            Pages[CurrentPageInt].ActiveButtons[ButtonID].Active = false;
            switch (Pages[CurrentPageInt].ActiveButtons[ButtonID].ButtonFunctionId)
            {
                case "Low Grav":
                    FunctionsStatus.LowGrav = false;
                    break;
                case "Fly":
                    FunctionsStatus.Fly = false;
                    break;
                case "Teleport":
                    FunctionsStatus.Teleport = false;
                    Plugin.Teleprefab.SetActive(false);
                    break;
                case "Grapple":
                    FunctionsStatus.Grapple = false;
                    StopGrapple();
                    break;
            }




        }

        public void OnEnable()
        {
            if (ActiveMenuPrefab != null) 
            {
                ActiveMenuPrefab.SetActive(true);
            }
        }

        public void OnDisable()
        {
            if (ActiveMenuPrefab != null)
            {
                ActiveMenuPrefab.SetActive(false);
            }
        }

        public void SendModFunction(string FunctionId)
        {
            switch (FunctionId)
            {
                case "Low Grav":
                    if (!FunctionsStatus.LowGrav) { LowGrav(); }
                    break;
                case "Fly":
                    FunctionsStatus.Fly = true;
                    break;
                case "Teleport":
                    FunctionsStatus.Teleport = true;
                    break;
                case "Grapple":
                    FunctionsStatus.Grapple = true;
                    break;
            }
        }

        public static void TabMenu(bool Up)
        {
            if (!InHomePage)
            {
            if (Up && Instance.CurrentPageInt < Instance.Pages.Count - 1)
            {
                Instance.CurrentPageInt++;
                if (CurrentSelectedMod > Instance.Pages[Instance.CurrentPageInt].ActiveButtons.Count - 1)
                {
                    CurrentSelectedMod = Instance.Pages[Instance.CurrentPageInt].ActiveButtons.Count - 1;
                }
            }
            else if (!Up && Instance.CurrentPageInt != 0)
            {
                Instance.CurrentPageInt--;
            }
            }
            else
            {
                if (CurrentPage == "Mods")
                {
                if (Up && Instance.PageIndexes[1] < Instance.ModPages.Count - 1)
                {
                    Instance.PageIndexes[1]++;
                    if (Instance.PageIndexes[0] > Instance.ModPages[Instance.PageIndexes[1]].ActiveButtons.Count - 1)
                    {
                        Instance.PageIndexes[0] = Instance.ModPages[Instance.PageIndexes[1]].ActiveButtons.Count - 1;
                    }
                }
                else if (!Up && Instance.PageIndexes[1] != 0)
                {
                    Instance.PageIndexes[1]--;
                }
                }

            }
        }

        public static void SetFunction(int ButtonID, List<GameObject> OldTabList, string NewPage = "")
        {
            if (!InHomePage)
            {
             if (!Instance.Pages[Instance.CurrentPageInt].ActiveButtons[ButtonID].Active)
            {
            Instance.Pages[Instance.CurrentPageInt].ActiveButtons[ButtonID].Active = true;
            }
            else
            {
                Instance.Pages[Instance.CurrentPageInt].ActiveButtons[ButtonID].Active = false;
            }               
            }
            else
            {
                if (NewPage != "")
                {
                OpenNewPage(NewPage, OldTabList);
                    Debug.Log("New Page == null");
                }
                else
                {
                    Instance.ModPages[Instance.PageIndexes[1]].ActiveButtons[ButtonID].PluginInfo.Instance.enabled = !Instance.ModPages[Instance.PageIndexes[1]].ActiveButtons[ButtonID].PluginInfo.Instance.enabled;
                    ComputerPlusPlus.Plugin.Instance.Config.Bind("EnabledMods", Instance.ModPages[Instance.PageIndexes[1]].ActiveButtons[ButtonID].PluginInfo.Metadata.Name, true).Value = Instance.ModPages[Instance.PageIndexes[1]].ActiveButtons[ButtonID].PluginInfo.Instance.enabled;
                    Debug.Log("New Page == somthin");    
                }
            }

        }

        

        public static void OpenNewPage(string Page, List<GameObject> objs)
        {
            List<string> cases = new List<string>()
            {
                "ActivateMods",
                "Info",
                "Home",
                "Mods"
            };
            switch (Page)
            {
                case "ActivateMods":
            foreach (GameObject obj in Instance.Buttons)
                {
                    obj.SetActive(true);
                }
            Plugin.InHomePage = false;
                    CurrentPage = Page;
                    break;
                case "Info":
                    Plugin.InHomePage = true;
                    Infopart.transform.parent.gameObject.SetActive(true);
                    CurrentPage = Page;
                    break;
                case "Home":
                    Plugin.InHomePage = true;
                    foreach (GameObject obj in Instance.HomeButtons)
                    {
                        obj.SetActive(true);
                    }
                    CurrentPage = Page;
                    break;
                case "Mods":
                    Plugin.InHomePage = true;
                    foreach (GameObject obj in Instance.ModButtons)
                    {
                        obj.SetActive(true);
                    }
                    CurrentPage = Page;
                    break;

            }

            foreach (string str in cases)
            {
                if (str == Page)
                {
            foreach (GameObject obj in objs)
            {
                obj.SetActive(false);
            }

                }
            }

            

        }

        public void LowGrav()
        {
            if (!FunctionsStatus.LowGrav)
            {
                FunctionsStatus.LowGrav = true;
            }
        }
        public void FunctionHandling()
        {
            Rigidbody rb = GameObject.Find("GorillaPlayer").GetComponent<Rigidbody>();
            
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.CurrentRoom.CustomProperties.TryGetValue("gameMode", out var gamemodeObject) && (gamemodeObject as string).Contains("MODDED"))
                {
                    if (FunctionsStatus.Fly == true)
                    {
                        Collider HeadCollider = GameObject.Find("GorillaPlayer").GetComponent<GorillaLocomotion.Player>().headCollider;
                        GameObject gp = GameObject.Find("GorillaPlayer");
                        XRController RightHandController = GameObject.Find("RightHand Controller").GetComponent<XRController>();
                        if (ControllerInputPoller.instance.rightControllerIndexFloat > 0.5)
                        {
                            gp.transform.position += (HeadCollider.transform.forward * Time.deltaTime) * 12f;
                            rb.velocity = Vector3.zero;

                        }
                    }
                    if (FunctionsStatus.LowGrav)
                    {
                        rb.useGravity = false;
                    }
                    else
                    {
                        rb.useGravity = true;
                    }
                    if (FunctionsStatus.Teleport == true)
                    {
                        Teleportation();
                    }
                    if (FunctionsStatus.Grapple == true)
                    {
                        Grapple();
                    }
                }
                else
                {
                    rb.useGravity = true;
                }
            }
            else
            {
                rb.useGravity = true;
            }


        }

        public void Update()
        {
            if (MenuObj.activeSelf && !InHomePage)
            {
                foreach (Page page in Pages)
                {
                    if (CurrentPageInt == Pages.IndexOf(page))
                    {
                        for (int i = 0; i < Buttons.Count; i++)
                        {
                            if (i <= Pages[CurrentPageInt].ActiveButtons.Count - 1)
                            {
                                Buttons[i].SetActive(true);
                                WatchUTILS.WatchUTIL.FindInParent("ButtonMod", Buttons[i].transform).GetComponent<TextMeshPro>().text = Pages[CurrentPageInt].ActiveButtons[i].ButtonId;
                                WatchUTILS.WatchUTIL.FindInParent("ButtonDescription", Buttons[i].transform).GetComponent<TextMeshPro>().text = Pages[CurrentPageInt].ActiveButtons[i].Description;

                            }
                            else
                            {
                                Buttons[i].SetActive(false);
                            }

                            if (i == CurrentSelectedMod)
                            {
                                WatchUTILS.WatchUTIL.FindInParent("ButtonSelect", Buttons[i].transform).gameObject.SetActive(true);
                            }
                            else
                            {
                                WatchUTILS.WatchUTIL.FindInParent("ButtonSelect", Buttons[i].transform).gameObject.SetActive(false);
                            }
                        }
                    }
                }





            }

            if (InHomePage)
                {
                for (int i = 0; i < HomeButtons.Count; i++)
                {
                    if (i == CurrentActiveHomeTab)
                    {
                        WatchUTILS.WatchUTIL.FindInParent("ButtonSelect", HomeButtons[i].transform).gameObject.SetActive(true);
                    }
                    else
                    {
                        WatchUTILS.WatchUTIL.FindInParent("ButtonSelect", HomeButtons[i].transform).gameObject.SetActive(false);
                    }
                }
             
                
                    int CorrectPinfoIn = 0;
                if (CurrentPage == "Mods")
                {
                foreach (ModPage page in ModPages)
                {
                    if (PageIndexes[1] == ModPages.IndexOf(page))
                    {
                        for (int i = 0; i < ModButtons.Count; i++)
                        {
                            if (i <= ModPages[PageIndexes[1]].ActiveButtons.Count - 1)
                            {
                                    BepInEx.PluginInfo newPinfo = null;

                                 if (CorrectPinfoIn != pinfos.Count)
                                    {
                                        CorrectPinfoIn += ModPages[PageIndexes[1]].ActiveButtons.Count;
                                    }

                                ModButtons[i].SetActive(true);
                                
                                WatchUTILS.WatchUTIL.FindInParent("ButtonMod", ModButtons[i].transform).GetComponent<TextMeshPro>().text = ModPages[ModPages.IndexOf(page)].ActiveButtons[i].PluginInfo.Metadata.Name;

                                    if (ModPages[ModPages.IndexOf(page)].ActiveButtons[i].PluginInfo.Instance.enabled)
                                    {
                                        WatchUTILS.WatchUTIL.FindInParent("ButtonStatus", ModButtons[i].transform).GetComponent<TextMeshPro>().text = "[ON]";
                                        WatchUTILS.WatchUTIL.FindInParent("ButtonStatus", ModButtons[i].transform).GetComponent<TextMeshPro>().color = Color.green;
                                        WatchUTILS.WatchUTIL.FindInParent("ButtonMod", ModButtons[i].transform).GetComponent<TextMeshPro>().color = Color.white;
                                    }
                                    else
                                    {
                                        WatchUTILS.WatchUTIL.FindInParent("ButtonStatus", ModButtons[i].transform).GetComponent<TextMeshPro>().text = "[OFF]";
                                        WatchUTILS.WatchUTIL.FindInParent("ButtonStatus", ModButtons[i].transform).GetComponent<TextMeshPro>().color = Color.red;
                                        WatchUTILS.WatchUTIL.FindInParent("ButtonMod", ModButtons[i].transform).GetComponent<TextMeshPro>().color = Color.red;
                                    }
                                    /* Debug.Log(ModPages[ModPages.IndexOf(page)].ActiveButtons[i].PluginInfo.Metadata.Name);*/
                                }
                            else
                            {
                                ModButtons[i].SetActive(false);
                            }

                            if (i == PageIndexes[0])
                            {
                                WatchUTILS.WatchUTIL.FindInParent("ButtonSelect", ModButtons[i].transform).gameObject.SetActive(true);
                            }
                            else
                            {
                                WatchUTILS.WatchUTIL.FindInParent("ButtonSelect", ModButtons[i].transform).gameObject.SetActive(false);
                            }
                        }
                    
                }
                }
                }




                }
            
            ButtonFunctions();
            FunctionHandling();
        }
    }
}
