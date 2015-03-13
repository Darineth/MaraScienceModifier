using System;
using System.Collections.Generic;
using UnityEngine;

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
        }

        void LoadScienceModifiers()
        {
            /*try
            {
                Debug.Log("[MaraScienceModifier]: Agents List:");

                foreach (Contracts.Agents.Agent agent in Contracts.Agents.AgentList.Instance.Agencies)
                {
                    Debug.Log(agent.Name);
                    foreach (Contracts.Agents.AgentMentality mentality in agent.Mentality)
                    {
                        Debug.Log("\t- " + mentality.Description);
                    }
                }
            }
            catch
            {
            }*/

            /*try
            {
                for (int ii = 1; ii < 100; ++ii)
                {
                    Debug.Log("[MaraScienceModifier]: Testing Contract:");

                    var contract = Contracts.ContractSystem.Instance.GenerateContract(ii, Contracts.Contract.ContractPrestige.Significant);

                    if (contract != null)
                    {
                        Debug.Log(contract.MissionControlTextRich());

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
            }*/

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

                    /*Debug.Log("[MaraScienceModifier]: Default Body Science Data:\n" +
                              "LandedDataValue = " + cb.scienceValues.LandedDataValue + "\n" +
                              "SplashedDataValue = " + cb.scienceValues.SplashedDataValue + "\n" +
                              "InSpaceLowDataValue = " + cb.scienceValues.InSpaceLowDataValue + "\n" +
                              "InSpaceHighDataValue = " + cb.scienceValues.InSpaceHighDataValue + "\n" +
                              "FlyingHighDataValue = " + cb.scienceValues.FlyingHighDataValue + "\n" +
                              "FlyingLowDataValue = " + cb.scienceValues.FlyingLowDataValue + "\n" +
                              "RecoveryValue = " + cb.scienceValues.RecoveryValue);*/

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
            Toolbar.IButton btn = Toolbar.ToolbarManager.Instance.add("MaraScienceModifier", "Reload");
            btn.ToolTip = "Reload MaraScienceModifier Config";
            btn.TexturePath = "MaraScienceModifier/Textures/ToolbarButton";
            btn.OnClick += (e) =>
            {
                LoadScienceModifiers();
            };
        }
    }
}
