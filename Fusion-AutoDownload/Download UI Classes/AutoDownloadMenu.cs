﻿using BoneLib.BoneMenu;
using BoneLib.BoneMenu.Elements;
using BoneLib.BoneMenu.UI;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using Il2CppColGen = Il2CppSystem.Collections.Generic;
using UnhollowerBaseLib;
using System.Collections;
using UnityEngine.Networking;

namespace FusionAutoDownload.Download_UI_Classes
{
    public class AutoDownloadMenuUI : ProgressUI
    {
        public AutoDownloadMenuUI() : base()
        {

        }

        public void ChangeMod(ModWrapper mod)
        {
            _mod = mod;
            Refrencer = null;
        }


        protected override void OnMyCrateAdded()
        {
            
        }
        // 4 - SelectedModMenu EventTrigger
        // - 0 - ModTitle TextMeshProUGUI
        // - 1 - Version TextMeshProUGUI
        // - 2 - Author Name TextMeshProUGUI
        // - 3 - Description TextMeshProUGUI
        // - 4 - "In this mod ({X}mb):" size label TextMeshProUGUI
        // - 5 - Levels List RectTransform
        // - 6 - Spawnables List RectTransform
        // - 7 - Avatars List RectTransform
        // - 8 - Install/Uninstall mod Button
        // - 9 - block/unblock mod Button
        // -10 - will delete/wont delete mod Button
        // -11 - download bar RectTransform
        // -12 - download bar % TextMeshProUGUI
        // -13 - download MB TextMeshProUGUI
        // -14 - download bar Pallet TextMeshProUGUI
        // -15 - Thumbnail Image
        // -16 - Auto-Update Button
        private EventTrigger Refrencer;
        private RectTransform downloadBar;
        private TextMeshProUGUI downloadBarPercent;
        private TextMeshProUGUI downloadBarMB;
        private Button installButton;
        private TextMeshProUGUI installButtonText;
        private Button removeButton;
        private TextMeshProUGUI removeButtonText;
        private Button blockButton;
        private TextMeshProUGUI blockButtonText;
        private Button autoButton;
        private TextMeshProUGUI autoButtonText;
        protected override IEnumerator UpdateLoop()
        {
            while (true)
            {
                yield return null;

                if (Refrencer == null && AutoDownloadMenu.SpawnedMenuRefs != null)
                {
                    Refrencer = AutoDownloadMenu.SpawnedMenuRefs.GetPersistant<EventTrigger>(4);
                    downloadBar = Refrencer.GetPersistant<RectTransform>(11);
                    downloadBarPercent = Refrencer.GetPersistant<TextMeshProUGUI>(12);
                    downloadBarMB = Refrencer.GetPersistant<TextMeshProUGUI>(13);

                    installButton = Refrencer.GetPersistant<Button>(8);
                    installButton.onClick = new Button.ButtonClickedEvent();
                    installButton.onClick.AddListener(new Action(() =>
                    {
                        AutoDownloadMenu.SpawnedAudio.PlayOneShot(AutoDownloadMenu.UIAssetMenuButtonSFX);
                        if (!ModNull)
                        {
                            if (!_mod.Installed && !_mod.Downloading)
                            {
                                _mod.TryDownload();
                                _mod.Keeping = true;

                                installButtonText.text = _mod.Installed ? "Installed!" : "Install";
                            }
                        }
                    }));
                    installButtonText = installButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (!ModNull)
                        installButtonText.text = _mod.Installed ? "Installed!" : "Install";


                    removeButton = Refrencer.GetPersistant<Button>(10);
                    removeButton.onClick = new Button.ButtonClickedEvent();
                    removeButton.onClick.AddListener(new Action(() =>
                    {
                        AutoDownloadMenu.SpawnedAudio.PlayOneShot(AutoDownloadMenu.UIAssetMenuButtonSFX);
                        if (!ModNull)
                        {
                            if (_mod.Installed && !_mod.Downloading)
                            {
                                _mod.Keeping = !_mod.Keeping;
                                removeButtonText.text = _mod.Keeping ? "Wont Delete" : "Will Delete";
                            }
                        }
                    }));
                    removeButtonText = removeButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (!ModNull)
                        removeButtonText.text = _mod.Keeping ? "Wont Delete" : "Will Delete";

                    blockButton = Refrencer.GetPersistant<Button>(9);
                    blockButton.onClick = new Button.ButtonClickedEvent();
                    blockButton.onClick.AddListener(new Action(() =>
                    {
                        AutoDownloadMenu.SpawnedAudio.PlayOneShot(AutoDownloadMenu.UIAssetMenuButtonSFX);
                        if (!ModNull)
                        {
                            _mod.Blocked = !_mod.Blocked;
                            blockButtonText.text = _mod.Blocked ? "Blocked" : "Not Blocked";
                        }
                    }));
                    blockButtonText = blockButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (!ModNull)
                        blockButtonText.text = _mod.Blocked ? "Blocked" : "Not Blocked";

                    autoButton = Refrencer.GetPersistant<Button>(16);
                    autoButton.onClick = new Button.ButtonClickedEvent();
                    autoButton.onClick.AddListener(new Action(() =>
                    {
                        AutoDownloadMenu.SpawnedAudio.PlayOneShot(AutoDownloadMenu.UIAssetMenuButtonSFX);
                        if (!ModNull && _mod.Installed)
                        {
                            _mod.AutoUpdate = !_mod.AutoUpdate;
                            autoButtonText.text = _mod.AutoUpdate ? "Will AutoUpdate" : "Won't AutoUpdate";
                        }
                    }));
                    autoButtonText = autoButton.transform.GetChild(1).GetComponent<TextMeshProUGUI>();
                    if (!ModNull)
                    {
                        if(_mod.Installed)
                            autoButtonText.text = _mod.AutoUpdate ? "Will AutoUpdate" : "Won't AutoUpdate";
                        else autoButtonText.text = "Won't AutoUpdate";
                    }
                }

                if (Refrencer == null)
                    continue;

                if (ModNull)
                    continue;

                // Update Download UI
                downloadBarMB.text = _mod.MB ?? "-1mb / -1mb";
                downloadBarPercent.text = _mod.Downloading ? _mod.Percent ?? "100%" : _mod.Installed ? "100%" : "0%";

                Vector2 sizeDelta = downloadBar.sizeDelta;
                sizeDelta.x = _mod.Downloading ? _mod.Progress * 1140 : _mod.Installed ? 1140 : 0;
                downloadBar.sizeDelta = sizeDelta;
            }
        }
    }
    public static class AutoDownloadMenu
    {
        public static GameObject UIAssetMenu
        {
            get
            {
                if (s_uiAssetMenuInternal == null)
                {
                    s_uiAssetMenuInternal = RepoWrapper.UIBundle.LoadAsset("Assets/UI/AutoDownloadMenu.prefab").Cast<GameObject>();
                }
                return s_uiAssetMenuInternal;
            }
        }
        private static GameObject s_uiAssetMenuInternal;
        public static AudioClip UIAssetMenuButtonSFX
        { 
            get
            {
                if (s_uiAssetMenuButtonSFX == null)
                {
                    s_uiAssetMenuButtonSFX = RepoWrapper.UIBundle.LoadAsset("Assets/UI/ButtonSFX.mp3").Cast<AudioClip>();
                    Msg(s_uiAssetMenuButtonSFX == null);
                }
                return s_uiAssetMenuButtonSFX;
            }
        }
        private static AudioClip s_uiAssetMenuButtonSFX;
        public static GameObject UIModEntry
        {
            get
            {
                if (s_uiModEntry == null)
                {
                    s_uiModEntry = RepoWrapper.UIBundle.LoadAsset("Assets/UI/ModEntry.prefab").Cast<GameObject>();
                }
                return s_uiModEntry;
            }
        }
        private static GameObject s_uiModEntry;

        public static GameObject UICrateEntry
        {
            get
            {
                if (s_uiCrateEntry == null)
                {
                    s_uiCrateEntry = RepoWrapper.UIBundle.LoadAsset("Assets/UI/CrateListing.prefab").Cast<GameObject>();
                }
                return s_uiCrateEntry;
            }
        }
        private static GameObject s_uiCrateEntry;

        public static MenuCategory Category;
        public static GameObject SpawnedMenu;

        public static EventTrigger SpawnedMenuRefs;
        public static Animator SpawnedAnimator;
        public static AudioSource SpawnedAudio;

        public static Toggle[] ModFiltersToggles;
        public static bool NoFiltersEnabled { get => ModFiltersToggles.All(filter => !filter.isOn); }
        public static Predicate<ModWrapper>[] ModFilters =
        {
            mod => mod.Installed,
            mod => mod.Keeping,
            mod => mod.Downloading,
            mod => mod.Blocked,
            mod => mod.AutoUpdate && mod.Installed
        };

        public static AutoDownloadMenuUI ProgressUI;

        public static void Setup()
        {
            Category = MenuManager.CreateCategory("Fusion Autodownloader", Color.blue);
            ProgressUI = new AutoDownloadMenuUI();
        }
        

        [HarmonyPatch(typeof(UIManager), "OnCategoryUpdated")]
        public class Patch_UIManager_OnCategoryUpdated
        {
            public static TMP_FontAsset FoundFont;
            [HarmonyPostfix]
            public static void Postfix(UIManager __instance, MenuCategory category)
            {
                if (category == Category)
                {
                    if (SpawnedMenu == null)
                    {
                        Category.SetName("");
                        SpawnedMenu = UnityEngine.Object.Instantiate(UIAssetMenu);
                        SpawnedMenu.transform.parent = __instance.MainPage.transform;
                        SpawnedMenu.transform.localPosition = Vector3.forward;
                        SpawnedMenu.transform.localRotation = Quaternion.identity;
                        SpawnedMenu.transform.localScale = Vector3.one;
                        RectTransform spawnedRectTransform = SpawnedMenu.transform.TryCast<RectTransform>();
                        spawnedRectTransform.anchoredPosition3D = new Vector3(90, 13, 0);
                        spawnedRectTransform.sizeDelta = new Vector2(600, 700);

                        if (FoundFont == null)
                            FoundFont = __instance.MainPage?.transform.FindChild("Name").GetComponent<TextMeshPro>().font;

                        foreach (TextMeshProUGUI text in SpawnedMenu.GetComponentsInChildren<TextMeshProUGUI>(true))
                            text.font = FoundFont;

                        SpawnedMenuRefs = SpawnedMenu.transform.GetChild(1).GetComponent<EventTrigger>();
                        SpawnedAnimator = SpawnedMenu.GetComponent<Animator>();
                        SpawnedAudio = SpawnedMenu.GetComponent<AudioSource>();

                        SetupMenuFunctionality();
                    }

                    SpawnedMenu.transform.parent.Find("Name").gameObject.SetActive(false);
                    SpawnedMenu.transform.parent.Find("ScrollUp").gameObject.SetActive(false);
                    SpawnedMenu.transform.parent.Find("ScrollDown").gameObject.SetActive(false);
                    SpawnedMenu.transform.parent.Find("Collider Mask Top").gameObject.SetActive(false);
                    SpawnedMenu.transform.parent.Find("Collider Mask Bottem").gameObject.SetActive(false);
                }
                else 
                {
                    if (SpawnedMenu != null)
                    {
                        UnityEngine.Object.Destroy(SpawnedMenu);
                        Category.SetName("Fusion Autodownloader");

                        SpawnedMenu.transform.parent.Find("Name").gameObject.SetActive(true);
                        SpawnedMenu.transform.parent.Find("ScrollUp").gameObject.SetActive(true);
                        SpawnedMenu.transform.parent.Find("ScrollDown").gameObject.SetActive(true);
                        SpawnedMenu.transform.parent.Find("Collider Mask Top").gameObject.SetActive(true);
                        SpawnedMenu.transform.parent.Find("Collider Mask Bottem").gameObject.SetActive(true);
                    }
                } 
            }
        }

        #region Extension Methods
        public static Il2CppColGen.List<PersistentCall> GetPersistantCalls(this EventTrigger eventTrigger) =>
            eventTrigger.delegates[0].callback.m_PersistentCalls.m_Calls;
        public static T GetPersistant<T>(this EventTrigger eventTrigger, int i) where T : Il2CppObjectBase =>
            eventTrigger.GetPersistantCalls()[i].target.Cast<T>();
        public static void DestroyAllChildren(this Transform parent)
        {
            for (int i = parent.childCount - 1; i >= 0; i--)
                UnityEngine.Object.Destroy(parent.GetChild(i).gameObject);
        }
        #endregion
        // A single "EventTrigger" component holds most of the references to components we want to quickly mess with.
        // basically a fast List<object> for us, setup in the unity editor.
        // 0 - Filters EventTrigger
        // - 0 - Installed Toggle
        // - 1 - Dont Delete Toggle
        // - 2 - Downloading Toggle
        // - 3 - Blocked Toggle
        // - 4 - Auto-Update Toggle
        // - 5 - <25mb Toggle
        // - 6 - <50mb Toggle
        // - 7 - <100mb Toggle
        // - 8 - <500mb Toggle
        // 1 - Mod List RectTransform
        // 2 - Mod List Up Button
        // 3 - Mod List Down Button
        // 4 - SelectedModMenu EventTrigger
        // - 0 - ModTitle TextMeshProUGUI
        // - 1 - Version TextMeshProUGUI
        // - 2 - Author Name TextMeshProUGUI
        // - 3 - Description TextMeshProUGUI
        // - 4 - "In this mod ({X}mb):" size label TextMeshProUGUI
        // - 5 - Levels List RectTransform
        // - 6 - Spawnables List RectTransform
        // - 7 - Avatars List RectTransform
        // - 8 - Install/Uninstall mod Button
        // - 9 - block/unblock mod Button
        // -10 - will delete/wont delete mod Button
        // -11 - download bar RectTransform
        // -12 - download bar % TextMeshProUGUI
        // -13 - download MB TextMeshProUGUI
        // -14 - download bar Pallet TextMeshProUGUI
        // -15 - Thumbnail Image
        // 5 - Safetey menu EventTrigger
        // - 0 - < mod max file size Button
        // - 1 - > mod max file size Button
        // - 2 - mod max file size ScrollBar
        // - 3 - mod will delete Toggle
        // - 4 - mod will update Toggle
        // - 5 - mod max file size display TextMeshProUGUI
        // - 6 - discord Button
        // - 7 - Set all mods to Update Button
        // - 8 - Set all mods to not Update Button
        // - 9 - Set all mods to delete Button
        // - 10 - Set all mods to not delete Button
        // 6 - Search menu EventTrigger
        // - 0 - ModsList RectTransform
        // - 1 - Sort Mode TextMeshProUGUI
        // - 2 - Search Button
        // - 3 - ModList Up Button
        // - 4 - ModList down Button
        // - 5 - Keyboard Lower Keys holder gameobject
        // - 6 - Keyboard Lower Keys holder gameobject
        // - 7 - Keyboard backspace button
        // - 8 - Keyboard enter button
        // - 9 - Keyboard clear button
        // - 10 - Search Bar Text TextMeshProUGUI

        public static void SetupMenuFunctionality()
        {
            // Add click to ALL buttons (rip perf)
            foreach (Button bt in SpawnedMenu.GetComponentsInChildren<Button>(true))
                bt.onClick.AddListener(new Action(() => { SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX); }));
            foreach (Toggle bt in SpawnedMenu.GetComponentsInChildren<Toggle>(true))
                bt.onValueChanged.AddListener(new Action<bool>(value => { SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX); }));

            // Setting up Up/Down Mod Scroll
            RectTransform modList = SpawnedMenuRefs.GetPersistant<RectTransform>(1);

            SpawnedMenuRefs.GetPersistant<Button>(3).onClick.AddListener(new Action(() =>
            {
                ModWrapper foundMod = ModManagerMenu.FindNextModEntry(true, RepoWrapper.AllMods, true, ref ModManagerMenu.RightModNum);
                ModManagerMenu.LeftModNum = ModManagerMenu.RightModNum - 1;
                for (int i = 0; i < 6; i++)
                    ModManagerMenu.FindNextModEntry(false, RepoWrapper.AllMods, false, ref ModManagerMenu.LeftModNum);

                ModManagerMenu.AddModEntry(foundMod, modList, true);
            }));

            SpawnedMenuRefs.GetPersistant<Button>(2).onClick.AddListener(new Action(() =>
            {
                ModWrapper foundMod = ModManagerMenu.FindNextModEntry(false, RepoWrapper.AllMods, true, ref ModManagerMenu.LeftModNum);
                ModManagerMenu.RightModNum = ModManagerMenu.LeftModNum + 1;
                for (int i = 0; i < 6; i++)
                    ModManagerMenu.FindNextModEntry(true, RepoWrapper.AllMods, false, ref ModManagerMenu.RightModNum);

                ModManagerMenu.AddModEntry(foundMod, modList, false);
            }));

            // Setup Mod Filter list
            ModFiltersToggles = SpawnedMenuRefs.GetPersistant<EventTrigger>(0)
                                               .GetPersistantCalls()
                                               .ToArray()
                                               .Select(pc => pc.target.Cast<Toggle>())
                                               .ToArray();

            foreach (Toggle filterTog in ModFiltersToggles)
                filterTog.onValueChanged.AddListener(new Action<bool>(state => { ModManagerMenu.RefreshMods(modList, RepoWrapper.AllMods, 6, true, ref ModManagerMenu.RightModNum); }));
            ModManagerMenu.RefreshMods(modList, RepoWrapper.AllMods, 6, true, ref ModManagerMenu.RightModNum);

            // Setting up Safetey Menu
            EventTrigger safeteyMenuRefs = SpawnedMenuRefs.GetPersistant<EventTrigger>(5);

            // - Max Mod Size Slider
            Scrollbar modSizeSlider = safeteyMenuRefs.GetPersistant<Scrollbar>(2);
            modSizeSlider.value = AutoDownloadMelon.ModSizeLimit == -1 ? 1 : AutoDownloadMelon.ModSizeLimit / 1000f;

            TextMeshProUGUI maxModSizeTitle = safeteyMenuRefs.GetPersistant<TextMeshProUGUI>(5);

            Action<float> changeMaxModSize = (change) =>
            {
                //Msg(modSizeSlider.value);
                modSizeSlider.value += change;
                modSizeSlider.value = Mathf.Clamp01(modSizeSlider.value);
                if (modSizeSlider.value > .9f)
                {
                    AutoDownloadMelon.ModSizeLimit = -1;
                    maxModSizeTitle.text = "Max Mod Size<size=20> (infinite)";
                }
                else
                {
                    AutoDownloadMelon.ModSizeLimit = Mathf.RoundToInt(modSizeSlider.value * 1000);
                    maxModSizeTitle.text = $"Max Mod Size<size=20> ({AutoDownloadMelon.ModSizeLimit}mb)";
                }
            };
            changeMaxModSize.Invoke(0);
            safeteyMenuRefs.GetPersistant<Button>(0).onClick.AddListener(new Action(() => { changeMaxModSize.Invoke(-0.025f); }));
            safeteyMenuRefs.GetPersistant<Button>(1).onClick.AddListener(new Action(() => { changeMaxModSize.Invoke(0.025f); }));
            safeteyMenuRefs.GetPersistant<Button>(6).onClick.AddListener(new Action(() => Application.OpenURL(@"https://discord.gg/Kaqvh4Jw")));

            // - Default autodownloaded mod's states settings
            Toggle willDelete = safeteyMenuRefs.GetPersistant<Toggle>(3);
            willDelete.isOn = AutoDownloadMelon.WillDeleteDefault;
            willDelete.onValueChanged.AddListener(new Action<bool>((state) =>
            {
                AutoDownloadMelon.WillDeleteDefault = state;
            }));
            Toggle willUpdate = safeteyMenuRefs.GetPersistant<Toggle>(4);
            willUpdate.isOn = AutoDownloadMelon.WillUpdateDefault;
            willUpdate.onValueChanged.AddListener(new Action<bool>((state) =>
            {
                AutoDownloadMelon.WillUpdateDefault = state;
            }));

            // set all mods to autoupdate / delete bts
            safeteyMenuRefs.GetPersistant<Button>(7).onClick.AddListener(new Action(() =>
            {
                foreach (ModWrapper mod in RepoWrapper.AllMods)
                    if (mod.Installed)
                        mod.AutoUpdate = true;
            }));
            safeteyMenuRefs.GetPersistant<Button>(8).onClick.AddListener(new Action(() =>
            {
                foreach (ModWrapper mod in RepoWrapper.AllMods)
                    if (mod.Installed)
                        mod.AutoUpdate = false;
            }));
            safeteyMenuRefs.GetPersistant<Button>(9).onClick.AddListener(new Action(() =>
            {
                foreach (ModWrapper mod in RepoWrapper.AllMods)
                    if (mod.Installed)
                        mod.Keeping = false;
            }));
            safeteyMenuRefs.GetPersistant<Button>(10).onClick.AddListener(new Action(() =>
            {
                foreach (ModWrapper mod in RepoWrapper.AllMods)
                    if (mod.Installed)
                        mod.Keeping = true;
            }));


            // Keyboard / Mod Search setup
            EventTrigger searchMenuRefList = SpawnedMenuRefs.GetPersistant<EventTrigger>(6);

            RectTransform searchList = searchMenuRefList.GetPersistant<RectTransform>(0);
            searchList.DestroyAllChildren();
            
            TextMeshProUGUI sortMode = searchMenuRefList.GetPersistant<TextMeshProUGUI>(1);
            TextMeshProUGUI searchText = searchMenuRefList.GetPersistant<TextMeshProUGUI>(10);

            Button modListUpBT = searchMenuRefList.GetPersistant<Button>(3);
            Button modListDownBT = searchMenuRefList.GetPersistant<Button>(4);

            // Search button pressed
            Action search = new Action(() => 
            {
                searchList.DestroyAllChildren();

                int leftModNum = 0;
                int rightModNum = 0;
                switch (sortMode.text.Substring(9))
                {
                    case "Search Bar":
                        ModWrapper[] searchedMods = FuzzySearches.FuzzySearchMods(RepoWrapper.AllMods, searchText.text).ToArray();

                        ModManagerMenu.RefreshMods(searchList, searchedMods, 8, false, ref rightModNum);

                        modListDownBT.onClick = new Button.ButtonClickedEvent();
                        modListDownBT.onClick.AddListener(new Action(() => 
                        {
                            SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX);
                            ModWrapper foundMod = ModManagerMenu.FindNextModEntry(true, searchedMods, false, ref rightModNum);
                            leftModNum = rightModNum - 1;
                            for (int i = 0; i < 8; i++)
                                ModManagerMenu.FindNextModEntry(false, searchedMods, false, ref leftModNum);

                            ModManagerMenu.AddModEntry(foundMod, searchList, true);
                        }));

                        modListUpBT.onClick = new Button.ButtonClickedEvent();
                        modListUpBT.onClick.AddListener(new Action(() =>
                        {
                            SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX);
                            ModWrapper foundMod = ModManagerMenu.FindNextModEntry(false, searchedMods, false, ref leftModNum);
                            rightModNum = leftModNum + 1;
                            for (int i = 0; i < 8; i++)
                                ModManagerMenu.FindNextModEntry(true, searchedMods, false, ref rightModNum);

                            ModManagerMenu.AddModEntry(foundMod, searchList, false);
                        }));
                        
                        break;
                    case "Downloads":
                        List<(ModWrapper, int)> searchedMods2 = RepoWrapper.AllMods.Where(mod => mod.ModListing.Title.Contains("\n  <mspace=-0.2>\u25ac\ua71c</mspace>    "))
                                                                        .Select(mod =>
                                                                        {
                                                                            (ModWrapper, int)? o = null;

                                                                            string[] split = mod.ModListing.Title.Split(new string[] { "\n  <mspace=-0.2>\u25ac\ua71c</mspace>    " }, StringSplitOptions.RemoveEmptyEntries).ToArray();
                                                                            if (int.TryParse(split[split.Length - 1], out int downloads))
                                                                                o = (mod, downloads);
                                                                            return o;
                                                                        }).Where(tup => tup.HasValue)
                                                                        .Select(tup => tup.Value)
                                                                        .ToList();

                        searchedMods2.Sort((a, b) => b.Item2.CompareTo(a.Item2));

                        ModWrapper[] searchedMods3 = searchedMods2.Select((tup) => tup.Item1).ToArray();

                        ModManagerMenu.RefreshMods(searchList, searchedMods3, 8, false, ref rightModNum);

                        modListDownBT.onClick = new Button.ButtonClickedEvent();
                        modListDownBT.onClick.AddListener(new Action(() =>
                        {
                            SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX);
                            ModWrapper foundMod = ModManagerMenu.FindNextModEntry(true, searchedMods3, false, ref rightModNum);
                            leftModNum = rightModNum - 1;
                            for (int i = 0; i < 8; i++)
                                ModManagerMenu.FindNextModEntry(false, searchedMods3, false, ref leftModNum);

                            ModManagerMenu.AddModEntry(foundMod, searchList, true);
                        }));

                        modListUpBT.onClick = new Button.ButtonClickedEvent();
                        modListUpBT.onClick.AddListener(new Action(() =>
                        {
                            SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX);
                            ModWrapper foundMod = ModManagerMenu.FindNextModEntry(false, searchedMods3, false, ref leftModNum);
                            rightModNum = leftModNum + 1;
                            for (int i = 0; i < 8; i++)
                                ModManagerMenu.FindNextModEntry(true, searchedMods3, false, ref rightModNum);

                            ModManagerMenu.AddModEntry(foundMod, searchList, false);
                        }));
                        break;
                }

            });

            searchMenuRefList.GetPersistant<Button>(2).onClick.AddListener(search);
            searchMenuRefList.GetPersistant<Button>(8).onClick.AddListener(search);

            // Keyboard setup
            bool firstPress = true;
            Transform keyHolder = searchMenuRefList.GetPersistant<GameObject>(5).transform;
            for (int i = 0; i < keyHolder.childCount; i++)
            {
                Transform curKeyTrans = keyHolder.GetChild(i);

                if (curKeyTrans.gameObject.name.Length == 1)
                    curKeyTrans.GetComponent<Button>().onClick.AddListener(new Action(() =>
                    {
                        if (firstPress)
                            searchText.text = "";
                        searchText.text += curKeyTrans.gameObject.name;
                        firstPress = false;
                    }));
            }
            Transform keyHolder2 = searchMenuRefList.GetPersistant<GameObject>(6).transform;
            for (int i = 0; i < keyHolder2.childCount; i++)
            {
                Transform curKeyTrans = keyHolder2.GetChild(i);

                if (curKeyTrans.gameObject.name.Length == 1)
                    curKeyTrans.GetComponent<Button>().onClick.AddListener(new Action(() =>
                    {
                        if (firstPress)
                            searchText.text = "";
                        searchText.text += curKeyTrans.gameObject.name;
                        firstPress = false;
                    }));
            }

            
            searchMenuRefList.GetPersistant<Button>(7).onClick.AddListener(new Action(() => 
            { 
                searchText.text = searchText.text.Substring(0, searchText.text.Length-1);
                if (searchText.text == "")
                {
                    searchText.text = "...";
                    firstPress = true;
                }
            }));

            searchMenuRefList.GetPersistant<Button>(9).onClick.AddListener(new Action(() => { searchText.text = "..."; firstPress = true; }));
        }
        // 6 - Search menu EventTrigger
        // - 0 - ModsList RectTransform
        // - 1 - Sort Mode TextMeshProUGUI
        // - 2 - Search Button
        // - 3 - ModList Up Button
        // - 4 - ModList down Button
        // - 5 - Keyboard Lower Keys holder gameobject
        // - 6 - Keyboard Lower Keys holder gameobject
        // - 7 - Keyboard backspace button
        // - 8 - Keyboard enter button
        // - 9 - Keyboard clear button
        // - 10 - Search Bar Text TextMeshProUGUI

        public static class ModManagerMenu
        {
            public static void RefreshMods(RectTransform modList, ModWrapper[] mods, int generateCount, bool useFilters, ref int rightNum)
            {
                modList.DestroyAllChildren();

                LeftModNum = RightModNum = NoFiltersEnabled ? UnityEngine.Random.RandomRangeInt(0, RepoWrapper.AllMods.Length-generateCount) : 0;
                LeftModNum--;
                for (int i = 0; i < generateCount; i++)
                {
                    ModWrapper foundMod = FindNextModEntry(true, mods, useFilters, ref rightNum);

                    AddModEntry(foundMod, modList, null);
                }

                modList.anchoredPosition = new Vector2(0, 0);
            }
            public static bool CheckFilters(ModWrapper mod)
            {
                bool valid = true;

                for (int i = 0; i < ModFiltersToggles.Length; i++)
                {
                    Toggle filter = ModFiltersToggles[i];
                    if (filter.gameObject.activeInHierarchy)
                    {
                        if (filter.isOn)
                        {
                            valid &= ModFilters[i].Invoke(mod);
                            if (!valid)
                                break;
                        }
                    }
                }
                return valid;
            }

            public static int LeftModNum;
            public static int RightModNum;
            public static ModWrapper FindNextModEntry(bool dir, ModWrapper[] mods, bool useFilters, ref int storedIndex)
            {
                for (int i = storedIndex; i < mods.Length && i > -1; i += dir ? 1 : -1)
                {
                    ModWrapper mod = mods[i];
                    if (!useFilters || CheckFilters(mod))
                    {
                        if (dir)
                            storedIndex = i+1;
                        else
                            storedIndex = i-1;
                        return mod;
                    }
                }
                return null;
            }
            public static void AddModEntry(ModWrapper mod, RectTransform parent, bool? bottem)
            {
                if (mod == null)
                    return;
                Transform newEntry = UnityEngine.Object.Instantiate(UIModEntry, parent).transform;
                if (bottem.HasValue)
                {
                    if (!(bool)bottem)
                    {
                        newEntry.SetAsFirstSibling();
                        UnityEngine.Object.Destroy(parent.GetChild(parent.childCount - 1).gameObject);
                    }
                    else
                    {
                        newEntry.SetAsLastSibling();
                        UnityEngine.Object.Destroy(parent.GetChild(0).gameObject);
                    }
                }
                newEntry.GetComponent<Button>().onClick.AddListener(new Action(() =>
                {
                    SpawnedAudio.PlayOneShot(UIAssetMenuButtonSFX);
                    SpawnedAnimator.SetTrigger("OpenModSelectionMenu");
                    fillSelectedModMenu(mod);
                }));
                var name = newEntry.GetChild(0).GetComponent<TextMeshProUGUI>();
                name.text = mod.ModListing.Title;
                name.font = Patch_UIManager_OnCategoryUpdated.FoundFont;
                var desc = newEntry.GetChild(1).GetComponent<TextMeshProUGUI>();
                desc.text = mod.ModListing.Description;
                desc.font = Patch_UIManager_OnCategoryUpdated.FoundFont;
                var imgFailText = newEntry.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>();
                imgFailText.font = Patch_UIManager_OnCategoryUpdated.FoundFont;

                Image modImg = newEntry.GetChild(2).GetChild(1).GetComponent<Image>();
                if (mod.Thumbnail == null)
                    thumbnailDownload(mod.ModListing.ThumbnailUrl, thumb =>
                    {
                        // On Thumbnail Downloaded
                        mod.Thumbnail = thumb;
                        if (modImg != null)
                        {
                            modImg.sprite = thumb;
                            modImg.enabled = true;
                        }
                    });
                else
                {
                    modImg.sprite = mod.Thumbnail;
                    modImg.enabled = true;
                }
            }
            private static void thumbnailDownload(string url, Action<Sprite> onDownload)
            {
                //Msg("Getting thumb: " + url);
                UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
                UnityWebRequestAsyncOperation data = request.SendWebRequest();

                data.m_completeCallback += new Action<AsyncOperation>(operation =>
                {
                    AutoDownloadMelon.UnityThread.Enqueue(() =>
                    {
                        if (request.result == UnityWebRequest.Result.Success)
                        {
                            // Get downloaded asset bundle
                            var texture = request.downloadHandler.Cast<DownloadHandlerTexture>().texture;

                            // Convert Texture2D to Sprite
                            Sprite sprite = Sprite.Create
                            (
                                texture,
                                new Rect(0, 0, texture.width, texture.height),
                                new Vector2(0.5f, 0.5f)
                            );

                            // Set Image component's sprite
                            onDownload.Invoke(sprite);
                        }
                        else Msg("failed: " + request.result.ToString() + " - " + url);
                    });
                });
            }

            private static void fillSelectedModMenu(ModWrapper mod)
            {
                EventTrigger selectedRefs = SpawnedMenuRefs.GetPersistant<EventTrigger>(4);
                selectedRefs.GetPersistant<TextMeshProUGUI>(0).text = mod.ModListing.Title;
                selectedRefs.GetPersistant<TextMeshProUGUI>(1).text = $"v({mod.ModListing.Version})";
                selectedRefs.GetPersistant<TextMeshProUGUI>(2).text = mod.ModListing.Author;
                selectedRefs.GetPersistant<TextMeshProUGUI>(3).text = mod.ModListing.Description;
                selectedRefs.GetPersistant<TextMeshProUGUI>(14).text = mod.Barcode;

                RepoWrapper.GetURLFileSize(mod.Url, size =>
                {
                    int mb = Mathf.RoundToInt(size / 1e+6f);
                    if (selectedRefs != null)  
                        selectedRefs.GetPersistant<TextMeshProUGUI>(4).text = $"In this mod ({mb}mb):";
                    if (!mod.Downloading)
                        mod.MB = $"{(mod.Installed ? mb : 0)}mb / {mb}mb";
                });


                RectTransform levelsList = selectedRefs.GetPersistant<RectTransform>(5);
                levelsList.DestroyAllChildren();
                RectTransform spawnablesList = selectedRefs.GetPersistant<RectTransform>(6);
                spawnablesList.DestroyAllChildren();
                RectTransform avatarsList = selectedRefs.GetPersistant<RectTransform>(7);
                avatarsList.DestroyAllChildren();

                RepoWrapper.GetPalletFromURL(mod.ModListing.ManifestUrl, pallet =>
                {
                    if (selectedRefs != null)
                    {
                        foreach (RepoWrapper.SmallCrate crate in pallet)
                        {
                            switch (crate.CrateType.Split(',')[0])
                            {
                                case "SLZ.Marrow.Warehouse.LevelCrate":
                                    addCrateListing(crate, levelsList);
                                    break;
                                case "SLZ.Marrow.Warehouse.SpawnableCrate":
                                    addCrateListing(crate, spawnablesList);
                                    break;
                                case "SLZ.Marrow.Warehouse.AvatarCrate":
                                    addCrateListing(crate, avatarsList);
                                    break;
                            }
                        }
                    }
                });

                Image thumb = selectedRefs.GetPersistant<Image>(15);
                thumb.sprite = mod.Thumbnail;
                thumb.enabled = mod.Thumbnail != null;

                // Download Bar :skull:
                ProgressUI.ChangeMod(mod);
            }
            private static void addCrateListing(RepoWrapper.SmallCrate crate, RectTransform parent)
            {
                Transform newCrate = UnityEngine.Object.Instantiate(UICrateEntry, parent).transform;

                var title = newCrate.GetChild(0).GetComponent<TextMeshProUGUI>();
                title.font = Patch_UIManager_OnCategoryUpdated.FoundFont;

                if (!string.IsNullOrEmpty(crate.CrateTitle))
                    title.text = crate.CrateTitle;
                else title.text = "No Name";

                var desc = newCrate.GetChild(1).GetComponent<TextMeshProUGUI>();
                desc.font = Patch_UIManager_OnCategoryUpdated.FoundFont;

                if (!string.IsNullOrEmpty(crate.CrateDescription))
                    desc.text = crate.CrateDescription;
            }
            // 4 - SelectedModMenu EventTrigger
            // - 0 - ModTitle TextMeshProUGUI
            // - 1 - Version TextMeshProUGUI
            // - 2 - Author Name TextMeshProUGUI
            // - 3 - Description TextMeshProUGUI
            // - 4 - "In this mod ({X}mb):" size label TextMeshProUGUI
            // - 5 - Levels List RectTransform
            // - 6 - Spawnables List RectTransform
            // - 7 - Avatars List RectTransform
            // - 8 - Install/Uninstall mod Button
            // - 9 - block/unblock mod Button
            // -10 - will delete/wont delete mod Button
            // -11 - download bar RectTransform
            // -12 - download bar % TextMeshProUGUI
            // -13 - download MB TextMeshProUGUI
            // -14 - download bar Pallet TextMeshProUGUI
            // -15 - Thumbnail Image
        }
        public static void Msg(object msg) => AutoDownloadMelon.Msg(msg);
    }


    public static class FuzzySearches
    {
        private static double CalculateDiceCoefficient(string source, string target)
        {
            var sourceBigrams = GetCharacterBigrams(source);
            var targetBigrams = GetCharacterBigrams(target);

            var intersectionCount = sourceBigrams.Intersect(targetBigrams).Count();
            var sourceCount = sourceBigrams.Count();
            var targetCount = targetBigrams.Count();

            return (2.0 * intersectionCount) / (sourceCount + targetCount);
        }

        private static IEnumerable<string> GetCharacterBigrams(string input)
        {
            if (string.IsNullOrEmpty(input) || input.Length < 2)
                return Enumerable.Empty<string>();

            return Enumerable.Range(0, input.Length - 1)
                .Select(i => input.Substring(i, 2));
        }

        public static List<ModWrapper> FuzzySearchMods(ModWrapper[] sourceList, string keyword)
        {
            var rankedList = new List<(ModWrapper, double)>();

            foreach (ModWrapper source in sourceList)
            {
                double similarity = CalculateDiceCoefficient(source.ModListing.Title.ToLower(), keyword.ToLower());
                rankedList.Add((source, similarity));
            }

            rankedList.Sort((a, b) => b.Item2.CompareTo(a.Item2));

            var orderedList = new List<ModWrapper>();
            foreach ((ModWrapper source, double similarity) in rankedList)
            {
                orderedList.Add(source);
            }

            return orderedList;
        }

        /*public static List<ModWrapper> FuzzySearchMods(ModWrapper[] sourceList, string keyword)
        {
            List<(ModWrapper, int)> rankedList = new List<(ModWrapper, int)>();

            foreach (ModWrapper source in sourceList)
            {
                int distance = CalculateLevenshteinDistance(source.ModListing.Title, keyword);
                rankedList.Add((source, distance));
            }

            rankedList.Sort((a, b) => a.Item2.CompareTo(b.Item2));

            var orderedList = new List<ModWrapper>();
            foreach ((ModWrapper source, int distance) in rankedList)
            {
                orderedList.Add(source);
            }

            return orderedList;
        }*/
    }
}
