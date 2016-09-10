using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Procedural_engines
{
    public class ProceduralEngine : PartModule
    {
        static string[] enginelist = new string[5];

        #region callbacks
        public override void OnInitialize()
        {
            UpdateConfiguration();
        }

        public void Update()
        {
            if (HighLogic.LoadedSceneIsEditor)
                OnUpdateEditor();
        }
        public void OnUpdateEditor()
        {
            UpdateConfiguration();
        }
        [KSPField]
        public float costMultiplier = 1.0f;

        #region IPartCostModifier implementation

        public float GetModuleCost(float defaultCost, ModifierStagingSituation sit)
        {
            return thrust * 0.5f * costMultiplier;
        }

        public ModifierChangeWhen GetModuleCostChangeWhen()
        {
            return ModifierChangeWhen.CONSTANTLY;
        }

        #endregion

        #endregion

        #region Thrust 

        [KSPField(isPersistant = true, guiName = "Thrust", guiActive = false, guiActiveEditor = true, guiFormat = "F3", guiUnits = "N"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = 1f, maxValue = float.PositiveInfinity, incrementLarge = 1000000f, incrementSmall = 10000, incrementSlide = 1f, sigFigs = 3, unit = "N", useSI = true)]
        public float thrust = 2500;
        private float oldThrust = 0;


        [KSPField(isPersistant = true, guiName = "Throttle", guiActive = false, guiActiveEditor = true, guiFormat = "F3", guiUnits = "%"),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = 1f, maxValue = 100f, incrementLarge = 10f, incrementSmall = 1f, incrementSlide = 1f, sigFigs = 3, unit = "", useSI = false)]
        public float minthrottle = 100;
        private float oldminthrottle = 0;


        [KSPField(isPersistant = true, guiName = "ignitions", guiActive = false, guiActiveEditor = true, guiFormat = "F3", guiUnits = ""),
         UI_FloatEdit(scene = UI_Scene.Editor, minValue = 0f, maxValue = 100f, incrementLarge = 10f, incrementSmall = 1f, incrementSlide = 1f, sigFigs = 0, unit = "", useSI = false)]
        public float ignitions = 0;
        private float oldignitions = 0;


        [KSPField(guiActiveEditor = true, isPersistant = true, guiName = "Fuel Type")]
        [UI_ChooseOption(affectSymCounterparts = UI_Scene.Editor, options = new string[] { "Methalox", "Kerlox", "Hydrolox", "Hydrazine" }, scene = UI_Scene.Editor, suppressEditorShipModified = true)]
        public string fueltype = "Hydrazine";
        public string oldfueltype = "";


        private void UpdateConfiguration()
        {
            if (oldThrust != thrust || oldfueltype != fueltype || oldignitions != ignitions || oldminthrottle != minthrottle)
            {
                int twr = 0;
                int surfaceisp = 0;
                int vacuumisp = 0;
                string fuel1 = "Hydrazine";
                string fuel2 = "Hydrazine";
                float fuel1ratio = 1;
                float fuel2ratio = 1;
                if (fueltype == "Methalox")
                {
                    twr = 150;
                    surfaceisp = 321;
                    vacuumisp = 363;
                    fuel1 = "LqdMethane";
                    fuel1ratio = 0.4165f;
                    fuel2 = "LqdOxygen";
                    fuel2ratio = 0.5835f;

                }
                else if (fueltype == "Kerlox")
                {
                    twr = 150;
                    surfaceisp = 282;
                    vacuumisp = 311;
                    fuel1 = "Kerosene";
                    fuel1ratio = 1f;
                    fuel2 = "LqdOxygen";
                    fuel2ratio = 2.56f;
                }
                else if (fueltype == "Hydrolox")
                {
                    twr = 150;
                    surfaceisp = 391;
                    vacuumisp = 451;
                    fuel1 = "LqdHydrogen";
                    fuel1ratio = 1;
                    fuel2 = "LqdOxygen";
                    fuel2ratio = 6;
                }
                else if (fueltype == "Hydrazine")
                {
                    twr = 1500;
                    surfaceisp = 240;
                    vacuumisp = 240;
                    fuel1 = "Hydrazine";
                    fuel1ratio = 1;
                    fuel2 = null;
                    fuel2ratio = 0;
                }
                ConfigNode config = new ConfigNode("ModuleEngines");
                config.name = "ModuleEngines";
                config.SetValue("maxThrust", (thrust / 1000).ToString("0.0000"), true);
                config.SetValue("minThrust", ((thrust / 1000)*(minthrottle/100)).ToString("0.0000"), true);
                config.SetValue("ignitionThreshold", "0.1");
                config.SetValue("ignitions", ignitions.ToString());

                ConfigNode curve = new ConfigNode("atmosphereCurve");
                FloatCurve newAtmoCurve = new FloatCurve();
                newAtmoCurve.Add(0, vacuumisp);
                newAtmoCurve.Add(1, surfaceisp);
                newAtmoCurve.Save(curve);
                config.AddNode(curve);

                ConfigNode prop = new ConfigNode("PROPELLANT");
                Propellant propellant = new Propellant();
                propellant.name = fuel1;
                propellant.ratio = fuel1ratio;
                propellant.drawStackGauge = true;
                propellant.Save(prop);
                config.AddNode(prop);
                if (fuel2!= null)
                {
                    ConfigNode prop2 = new ConfigNode("PROPELLANT");
                    Propellant propellant2 = new Propellant();
                    propellant2.name = fuel2;
                    propellant2.ratio = fuel2ratio;
                    propellant2.Save(config);
                    propellant2.Save(prop2);
                    config.AddNode(prop2);
                }
                part.Modules["ModuleEngines"].Load(config);
                part.prefabMass = ((thrust/1000) / twr);
                oldThrust = thrust;
                oldfueltype = fueltype;
                oldignitions = ignitions;
                oldminthrottle = minthrottle;
            }


            if (HighLogic.LoadedSceneIsEditor)
                GameEvents.onEditorShipModified.Fire(EditorLogic.fetch.ship);

        }
        #endregion
    }
}