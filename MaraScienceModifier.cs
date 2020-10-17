using System;
using System.Collections.Generic;
using UnityEngine;
using KSP.UI.Screens;

namespace Mara
{
    [KSPAddon(KSPAddon.Startup.MainMenu, false)]
    public class MaraScienceModifier : UnityEngine.MonoBehaviour
    {
        #region Members
        static bool _blnLoadedScienceValues = false;
        static bool s_blnToolbarButtonCreated = false;
        #endregion

        void Start()
        {
            if (_blnLoadedScienceValues) { return; }

            _blnLoadedScienceValues = true;

            LoadScienceModifiers();

            GameEvents.onGUIRnDComplexSpawn.Add(new EventVoid.OnEvent(OnGUIRnDComplexSpawn));
            GameEvents.onGameSceneLoadRequested.Add(new EventData<GameScenes>.OnEvent(OnGameSceneLoadRequested));
            RDTechTree.OnTechTreeSpawn.Add(new EventData<RDTechTree>.OnEvent(OnTechTreeSpawn));
        }

        private void OnGUIRnDComplexSpawn()
        {
            Debug.Log("[MaraScienceModifier.OnGUIRnDComplexSpawn]");

            System.Text.StringBuilder sbParts = new System.Text.StringBuilder();
            foreach (AvailablePart p in PartLoader.LoadedPartsList)
            {
                string configName1 = "?";
                string configName2 = "?";
                string configName3 = "?";
                if (p.partConfig != null && !string.IsNullOrEmpty(p.partConfig.GetValue("name")))
                {
                    configName1 = p.partConfig.GetValue("name");
                }
                if (p.internalConfig != null && !string.IsNullOrEmpty(p.internalConfig.GetValue("name")))
                {
                    configName2 = p.internalConfig.GetValue("name");
                }
                if (p.partUrlConfig != null && !string.IsNullOrEmpty(p.partUrlConfig.name))
                {
                    configName3 = p.partUrlConfig.name;
                }
                sbParts
                    .Append(p.name).Append("\t")
                    .Append(configName1).Append("\t")
                    .Append(configName2).Append("\t")
                    .Append(configName3).Append("\t")
                    .Append(p.TechRequired).Append("\t")
                    .Append(p.title).AppendLine();
            }

            Debug.Log(sbParts.ToString());

            Debug.Log("[MaraScienceModifier.OnGUIRnDComplexSpawn.Unassigned]");

            sbParts = new System.Text.StringBuilder();
            foreach (AvailablePart p in PartLoader.LoadedPartsList)
            {
                if (p.TechRequired == "Unassigned")
                {
                    string configName1 = "?";
                    string configName2 = "?";
                    string configName3 = "?";
                    if (p.partConfig != null && !string.IsNullOrEmpty(p.partConfig.GetValue("name")))
                    {
                        configName1 = p.partConfig.GetValue("name");
                    }
                    if (p.internalConfig != null && !string.IsNullOrEmpty(p.internalConfig.GetValue("name")))
                    {
                        configName2 = p.internalConfig.GetValue("name");
                    }
                    if (p.partUrlConfig != null && !string.IsNullOrEmpty(p.partUrlConfig.name))
                    {
                        configName3 = p.partUrlConfig.name;
                    }
                    sbParts
                        .Append(p.name).Append("\t")
                        .Append(configName1).Append("\t")
                        .Append(configName2).Append("\t")
                        .Append(configName3).Append("\t")
                        .Append(p.TechRequired).Append("\t")
                        .Append(p.title).AppendLine();
                }
            }

            Debug.Log(sbParts.ToString());
        }

        private void OnTechTreeSpawn(RDTechTree tree)
        {

            Debug.Log("[MaraScienceModifier.OnTechTreeSpawn]");

            System.Text.StringBuilder sbNodes = new System.Text.StringBuilder();
            foreach (RDNode n in tree.controller.nodes)
            {
                sbNodes.Append(n.name).Append(",").Append(n.tech.techID).Append(",").Append(n.tech.partsAssigned != null ? n.tech.partsAssigned.Count.ToString() : "<null>").AppendLine();
            }

            Debug.Log(sbNodes.ToString());
        }

        private void OnGameSceneLoadRequested(GameScenes scene)
        {
            if (scene == GameScenes.SPACECENTER)
            {
                Debug.Log("[MaraScienceModifier.OnGameSceneLoadRequested]: " + scene.ToString());
            }
        }

        void LoadScienceModifiers()
        {
            try
            {
                Debug.Log("[MaraScienceModifier]: Agents List:");

                foreach (Contracts.Agents.Agent agent in Contracts.Agents.AgentList.Instance.Agencies)
                {
                    Debug.Log("Name: " + agent.Name + ", Title: " + (agent.Title == null ? "<NULL>" : agent.Title) );
                    foreach (Contracts.Agents.AgentMentality mentality in agent.Mentality)
                    {
                        Debug.Log("\t- " + mentality.Description);
                    }
                }
            }
            catch
            {
            }

            try
            {
                for (int ii = 1; ii < 100; ++ii)
                {
                    Debug.Log("[MaraScienceModifier]: Testing Contract:");

                    var contract = Contracts.ContractSystem.Instance.GenerateContract(ii, Contracts.Contract.ContractPrestige.Significant);

                    if (contract != null)
                    {
                        Debug.Log(contract.MissionControlTextRich());

                        if(contract.Agent == null)
                        {
                            Debug.Log("Generated contract with NULL Agent: " + contract.Title);
                        }

                        foreach (Contracts.Agents.Agent agent in Contracts.Agents.AgentList.Instance.Agencies)
                        {
                            Debug.Log(agent.Name + ": " + agent.ScoreAgentSuitability(contract));
                        }

                        for (int jj = 1; jj < 100; ++jj)
                        {
                            Debug.Log(jj + ": " + Contracts.Agents.AgentList.Instance.GetSuitableAgentForContract(contract).Name);
                        }

                        break;
                    }
                    else
                    {
                        Debug.Log("[MaraScienceModifier]: No Contract Generated");
                    }
                }
            }
            catch(Exception ex)
            {
                Debug.LogException(ex);
            }

            try
            {
                Debug.Log("[MaraScienceModifier]: Loading Science Modifiers...");

                string strConfigPath = KSP.IO.IOUtils.GetFilePathFor(this.GetType(), "MaraScienceModifier.cfg");

                if (!KSP.IO.File.Exists<MaraScienceModifier>("MaraScienceModifier.cfg"))
                {
                    Debug.Log("[MaraScienceModifier]: Config file not found: " + strConfigPath);
                    return;
                }

                //string cstrSettingsPath = KSPUtil.ApplicationRootPath + "GameData/MaraScienceModifier/MaraScienceModifier.cfg";

                ConfigNode cnSettings;
                try
                {
                    cnSettings = ConfigNode.Load(strConfigPath);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[MaraScienceModifier]: Error loading configuration:\r\n" + ex.ToString());
                    return;
                }

                if (cnSettings == null || !cnSettings.HasNode("MaraScienceModifierSettings"))
                {
                    Debug.Log("[MaraScienceModifier]: MaraScienceModifierSettings Config Node not found.");
                    return;
                }

                cnSettings = cnSettings.GetNode("MaraScienceModifierSettings");

                if (cnSettings == null)
                {
                    Debug.Log("[MaraScienceModifier]: MaraScienceModifierSettings node not found.");
                    return;
                }

                if (!s_blnToolbarButtonCreated && (String.Equals(cnSettings.GetValue("CreateReloadButton"), "true", StringComparison.OrdinalIgnoreCase)))
                {
                    try
                    {
                        CreateToolbarButton();
                        s_blnToolbarButtonCreated = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.Log("[MaraScienceModifier]: Failed to create toolbar button: " + ex.ToString());
                    }
                }

                foreach (CelestialBody cb in FlightGlobals.Bodies)
                {
                    Debug.Log("[MaraScienceModifier]: Checking for Body Config: " + cb.gameObject.name + "...");

                    Debug.Log("[MaraScienceModifier]: Default Body Science Data:\n" +
                              "LandedDataValue = " + cb.scienceValues.LandedDataValue + "\n" +
                              "SplashedDataValue = " + cb.scienceValues.SplashedDataValue + "\n" +
                              "InSpaceLowDataValue = " + cb.scienceValues.InSpaceLowDataValue + "\n" +
                              "InSpaceHighDataValue = " + cb.scienceValues.InSpaceHighDataValue + "\n" +
                              "FlyingHighDataValue = " + cb.scienceValues.FlyingHighDataValue + "\n" +
                              "FlyingLowDataValue = " + cb.scienceValues.FlyingLowDataValue + "\n" +
                              "RecoveryValue = " + cb.scienceValues.RecoveryValue);

                    if (cnSettings.HasNode(cb.gameObject.name))
                    {
                        Debug.Log("[MaraScienceModifier]: Applying Custom Config Values: " + cb.gameObject.name + "...");
                        ConfigNode cnBody = cnSettings.GetNode(cb.gameObject.name);
                        float fltValue;
                        if (cnBody.HasValue("LandedDataValue") && float.TryParse(cnBody.GetValue("LandedDataValue"), out fltValue)) cb.scienceValues.LandedDataValue = fltValue;
                        if (cnBody.HasValue("SplashedDataValue") && float.TryParse(cnBody.GetValue("SplashedDataValue"), out fltValue)) cb.scienceValues.SplashedDataValue = fltValue;
                        if (cnBody.HasValue("InSpaceLowDataValue") && float.TryParse(cnBody.GetValue("InSpaceLowDataValue"), out fltValue)) cb.scienceValues.InSpaceLowDataValue = fltValue;
                        if (cnBody.HasValue("InSpaceHighDataValue") && float.TryParse(cnBody.GetValue("InSpaceHighDataValue"), out fltValue)) cb.scienceValues.InSpaceHighDataValue = fltValue;
                        if (cnBody.HasValue("FlyingHighDataValue") && float.TryParse(cnBody.GetValue("FlyingHighDataValue"), out fltValue)) cb.scienceValues.FlyingHighDataValue = fltValue;
                        if (cnBody.HasValue("FlyingLowDataValue") && float.TryParse(cnBody.GetValue("FlyingLowDataValue"), out fltValue)) cb.scienceValues.FlyingLowDataValue = fltValue;
                        if (cnBody.HasValue("RecoveryValue") && float.TryParse(cnBody.GetValue("RecoveryValue"), out fltValue)) cb.scienceValues.RecoveryValue = fltValue;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError("MaraScienceModifier Start(): " + ex.ToString());
            }
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.M) && Input.GetKeyDown(KeyCode.LeftAlt))
            {
                LoadScienceModifiers();
            }
        }

        void CreateToolbarButton()
        {
            //Toolbar.IButton btn = Toolbar.ToolbarManager.Instance.add("MaraScienceModifier", "Reload");
            //btn.ToolTip = "Reload MaraScienceModifier Config";
            //btn.TexturePath = "MaraScienceModifier/Textures/ToolbarButton";
            //btn.OnClick += (e) =>
            //{
            //    //LoadScienceModifiers();
            //    GC.Collect();
            //    GC.Collect();
            //    GC.Collect();
            //};
        }
    }
}
