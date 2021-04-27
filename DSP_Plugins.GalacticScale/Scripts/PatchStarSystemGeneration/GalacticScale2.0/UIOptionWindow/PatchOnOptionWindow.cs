﻿using HarmonyLib;
using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using NGPT;
using System.Reflection;
using System.Collections.Generic;

namespace GalacticScale
{
    public class PatchOnOptionWindow
    {
        private static RectTransform tabLine;
        private static RectTransform galacticButton;
        private static UIButton[] tabButtons;
        private static Text[] tabTexts;
        private static Tweener[] tabTweeners;

        private static RectTransform details;
        private static RectTransform templateOptionsCanvas;
        private static RectTransform templateUIComboBox;
        private static RectTransform templateCheckBox;
        private static RectTransform templateInputField;
        private static RectTransform templateSlider;

        private static List<RectTransform> optionRects = new List<RectTransform>();
        private static List<RectTransform> generatorCanvases = new List<RectTransform>();
        private static List<List<RectTransform>> generatorPluginOptionRects = new List<List<RectTransform>>();
        private static List<List<GS2.GSOption>> generatorPluginOptions = new List<List<GS2.GSOption>>();
        private static float anchorX;
        private static float anchorY;

        private static List<GS2.GSOption> options = new List<GS2.GSOption>();

        public static UnityEvent OptionsUIPostfix = new UnityEvent();


        [HarmonyPostfix, HarmonyPatch(typeof(UIOptionWindow), "_OnOpen")]
        public static void PatchMainMenu(ref UIButton[] ___tabButtons, ref Text[] ___tabTexts, ref Tweener[] ___tabTweeners, ref Image ___tabSlider)
        {
            GameObject overlayCanvas = GameObject.Find("Overlay Canvas");
            if (overlayCanvas == null || overlayCanvas.transform.Find("Top Windows") == null) return;

            //Grab the tabgroup and store the relevant data in this class
            tabLine = GameObject.Find("Top Windows/Option Window/tab-line").GetComponent<RectTransform>();
            tabButtons = ___tabButtons;
            tabTexts = ___tabTexts;
            tabTweeners = ___tabTweeners;
            //GS2.Log("TEST");
            //Get out of the patch, and start running our own code
            var contentGS = GameObject.Find("Option Window/details/content-gs");
            if (contentGS == null)
                CreateGalacticScaleSettingsPage();
            
    }

        private static void CreateGalacticScaleSettingsPage()
        {
            GS2.Log("TEST2");
            //Add Tab Button
            Transform tabParent = GameObject.Find("Option Window/tab-line/tab-button-5").GetComponent<RectTransform>().parent;
            RectTransform tabButtonTemplate = tabParent.GetChild(tabParent.childCount - 1).GetComponent<RectTransform>();
            galacticButton = Object.Instantiate(tabButtonTemplate, tabLine, false);
            galacticButton.name = "tab-button-gs";
            galacticButton.anchoredPosition = new Vector2(galacticButton.anchoredPosition.x + 160, galacticButton.anchoredPosition.y);
            Object.Destroy(galacticButton.GetComponentInChildren<Localizer>());
            galacticButton.GetComponent<Button>().onClick.RemoveAllListeners();
            galacticButton.GetComponentInChildren<Text>().text = "Galactic Scale";
            galacticButton.GetComponent<Button>().onClick.AddListener(new UnityAction(GalacticScaleTabClick));
            tabButtons.AddItem<UIButton>(galacticButton.GetComponent<UIButton>());
            tabTexts.AddItem<Text>(galacticButton.GetComponentInChildren<Text>());
            tabTweeners.AddItem<Tweener>(galacticButton.GetComponent<Tweener>());

            //Create the galactic scale settings panel
            RectTransform detailsTemplate = GameObject.Find("Option Window/details/content-5").GetComponent<RectTransform>();
            details = Object.Instantiate(detailsTemplate, GameObject.Find("Option Window/details").GetComponent<RectTransform>(), false);
            details.gameObject.SetActive(false);
            details.gameObject.name = "content-gs";

            //Destroy surplus ui elements
            Transform temp = details.Find("tiplevel");
            if (temp != null) Object.Destroy(temp.gameObject);
            temp = details.Find("revert-button");
            if (temp != null) Object.Destroy(temp.gameObject);
            //Copy original combobox as a template, then get rid of it
            RectTransform generatorPicker = details.Find("language").GetComponent<RectTransform>();
            anchorX = generatorPicker.anchoredPosition.x;
            anchorY = generatorPicker.anchoredPosition.y;
            //templateUIComboBox = Object.Instantiate<RectTransform>(details.Find("language").GetComponent<RectTransform>());
            //templateUIComboBox.gameObject.SetActive(false);
            GS2.Log("TEST3");
            templateUIComboBox = CreateTemplate(details.Find("language").GetComponent<RectTransform>());
            GS2.Log("TEST4");
            Object.Destroy(generatorPicker.gameObject);
            GS2.Log("TEST5");

            templateOptionsCanvas = Object.Instantiate(details, details, false);
            templateOptionsCanvas.anchoredPosition = details.anchoredPosition + new Vector2(750f,0);
            templateOptionsCanvas.gameObject.name = "templateCanvasPanel";
            while (templateOptionsCanvas.transform.childCount > 0)
            {
                Object.DestroyImmediate(templateOptionsCanvas.transform.GetChild(0).gameObject);
            }

            RectTransform checkBoxProto = GameObject.Find("Option Window/details/content-1/fullscreen").GetComponent<RectTransform>(); //need to remove localizer, has textcomponent, and child called Checkbox with a UIToggle and a unityengine.ui.toggle
            templateCheckBox = CreateTemplate(checkBoxProto);
            RectTransform sliderProto = GameObject.Find("Option Window/details/content-1/dofblur").GetComponent<RectTransform>(); // localizer,  a textcomponent, has child called slider which has a UI.Slider component ,
            templateSlider = CreateTemplate(sliderProto);
            RectTransform inputProto = GameObject.Find("UI Root/Overlay Canvas/Galaxy Select/galaxy-seed").GetComponent<RectTransform>(); //localizer, has a ui.text comp, a child called inputfield which has a ui.inputfield, a uibutton and a eventsystems.eventtrigger
            templateInputField = CreateTemplate(inputProto);


            //Get a list of all loaded generators, and add a combobox to select between them.
            //List<string> generatorNames = GS2.generators.ConvertAll<string>((iGenerator iGen) => { return iGen.Name; });
            //options.Add(new GS2.GSOption("GS2", "Generator", "UIComboBox", generatorNames, new GS2.GSOptionCallback(GeneratorSelected)));
            CreateOwnOptions();
            ImportCustomGeneratorOptions();
            GS2.Log("Test9");
            CreateOptionsUI();
            
            GS2.Log("Test10");
            OptionsUIPostfix.Invoke();
            GS2.Log("Test11");
        }
        private static void ImportCustomGeneratorOptions()
        {
            for (var i = 0; i < GS2.generators.Count; i++)
            {
                List<GS2.GSOption> pluginOptions = new List<GS2.GSOption>();
                if (GS2.generators[i] is iConfigurableGenerator gen) foreach (GS2.GSOption o in gen.Options) pluginOptions.Add(o);
                generatorPluginOptions.Add(pluginOptions);
            }
        }
        private static void CreateOwnOptions()
        {
            GS2.Log("CreateOwnOptions()");
            List<string> generatorNames = GS2.generators.ConvertAll<string>((iGenerator iGen) => { return iGen.Name; });
            options.Add(new GS2.GSOption("Generator", "UIComboBox", generatorNames, new GS2.GSOptionCallback(GeneratorSelected), new UnityAction(CreateOwnOptionsPostFix)));
        }
        private static void CreateOwnOptionsPostFix()
        {
            GS2.Log("CreateGeneratorOptionsPostFix");
            var generatorIndex = 0;
            List<string> generatorNames = GS2.generators.ConvertAll<string>((iGenerator iGen) => { return iGen.Name; });
            for (var i = 0; i < generatorNames.Count; i++) if (generatorNames[i] == GS2.generator.Name) { GS2.Log("index found!" + i); generatorIndex = i; }
            if (optionRects[0] != null)
            {
                GS2.Log("Setting combobox for generator index to " + generatorIndex);
                optionRects[0].GetComponentInChildren<UIComboBox>().itemIndex = generatorIndex;
            }
            else GS2.Log("optionRects[0] == null!@#");
        }
        private static RectTransform CreateTemplate(RectTransform original)
        {
            RectTransform template = Object.Instantiate(original, GameObject.Find("Option Window/details").GetComponent<RectTransform>(), false);
            template.gameObject.SetActive(false);
            return template;
        }
        
        // Method that handles creation of the settings tab
        private static void CreateOptionsUI()
        {
            GS2.Log("CreateOptionsUI");
            for (var i = 0; i < options.Count; i++)
            {
                switch (options[i].type)
                {
                    case "UIComboBox": CreateComboBox(options[i], details, i); break;
                    default: break;
                }
            }
            int currentGenIndex = GS2.GetCurrentGeneratorIndex();
            GS2.Log("CreateGeneratorOptionsCanvases: currentGenIndex = "+ currentGenIndex);
            for (var i = 0; i < generatorPluginOptions.Count; i++)
            { //for each canvas
                GS2.Log("Creating Canvas " + i);
                RectTransform canvas = Object.Instantiate(templateOptionsCanvas, details, false);
                canvas.name = "testCanvas" + i;
                generatorCanvases.Add(canvas);
                canvas.name = "generatorCanvas-"+GS2.generators[i].Name;
                if (currentGenIndex == i)
                {
                    GS2.Log("Setting canvas active");
                    canvas.gameObject.SetActive(true);
                }
                else canvas.gameObject.SetActive(false);
                AddGeneratorPluginUIElements(canvas, i);

            }
        }



        /// Iterate through all the plugins that have elements to add to the UI, add them,// then add their postfixes to the event listener
        private static void AddGeneratorPluginUIElements(RectTransform canvas, int genIndex)
        {
            GS2.Log("AddGeneratorOptions listener count=" + OptionsUIPostfix.GetPersistentEventCount());
            List<GS2.GSOption> options = generatorPluginOptions[genIndex];
            for (int i = 0;i < options.Count; i++)
            {
                switch (options[i].type)
                {
                    case "UIComboBox": CreateComboBox(options[i], canvas, i); break;
                    default: break;
                }
                //if (options[i].postfix != null) OptionsUIPostfix.AddListener(options[i].postfix);
            }
            GS2.Log("AddGeneratorOptions listener count end=" + OptionsUIPostfix.GetPersistentEventCount());
        }

        // Create a combobox from a GSOption definition
        private static void CreateComboBox(GS2.GSOption o, RectTransform canvas, int index)
        {
            GS2.Log("CreateComboBox");
            RectTransform comboBoxRect = Object.Instantiate(templateUIComboBox, canvas);
            comboBoxRect.name = o.label;
            comboBoxRect.gameObject.SetActive(true);
            optionRects.Add(comboBoxRect);
            o.rectTransform = comboBoxRect;
            int offset = index * -40;
            comboBoxRect.anchoredPosition = new Vector2(anchorX , anchorY + offset);
            UIComboBox comboBoxUI = comboBoxRect.GetComponentInChildren<UIComboBox>();
            comboBoxUI.name = o.label + "_comboBox";
            comboBoxUI.Items = o.data as List<string>;
            comboBoxUI.UpdateItems();
            comboBoxUI.itemIndex = 0;
            comboBoxUI.onItemIndexChange.RemoveAllListeners();
            if (o.callback != null) comboBoxUI.onItemIndexChange.AddListener(delegate { o.callback(comboBoxUI.itemIndex); });
            comboBoxRect.GetComponentInChildren<Text>().text = o.label;
            comboBoxRect.GetComponentInChildren<Text>().text = o.label;
            RectTransform tipTransform = comboBoxRect.GetChild(0).GetComponent<RectTransform>();
            tipTransform.gameObject.name = "optionTip-" + (index);
            Object.Destroy(tipTransform.GetComponent<Localizer>());
            tipTransform.GetComponent<Text>().text = o.tip;
            if (o.postfix != null) OptionsUIPostfix.AddListener(o.postfix);
            GS2.Log("Finished Creating ComboBox");
        }

        // Callback for own Generator ComboBox Selection Event
        private static void GeneratorSelected(object result)
        {
            GS2.Log("Result = " + result + GS2.generators[(int)result].Name);
            GS2.generator = GS2.generators[(int)result];
            GS2.Log("Set the generator, trying to disable every canvas");
            for (var i = 0; i < generatorCanvases.Count; i++) generatorCanvases[i].gameObject.SetActive(false);
            GS2.Log("tryuing to setactive");
            generatorCanvases[(int)result].gameObject.SetActive(true);
            GS2.Log("trying to save prefs");
            GS2.SavePreferences();
        }
        private static void GalacticScaleTabClick()
        {
            UIRoot.instance.optionWindow.SetTabIndex(5, false);
            details.gameObject.SetActive(true);
        }
        public static void DisableDetails()
        {
            if (details != null && details.gameObject != null) details.gameObject.SetActive(false);
        }

        
    }
}