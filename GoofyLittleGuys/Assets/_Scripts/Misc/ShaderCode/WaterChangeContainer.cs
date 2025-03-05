using System;
using System.Collections.Generic;
using UnityEngine;
//using UnityEditor;

namespace _Scripts.Misc.ShaderCode
{
    //[CreateAssetMenu(fileName = "WaterContainer", menuName = "New Water Container", order = 0)]
    public class WaterChangeContainer : ScriptableObject
    {
        private static readonly int WaterColor = Shader.PropertyToID("_WaterColor");
        private static readonly int LightFoamColor = Shader.PropertyToID("_LightFoamColor");
        private static readonly int DarkFoamColor = Shader.PropertyToID("_DarkFoamColor");
        public List<Material> waterMaterials = new List<Material>();
        
        [HideInInspector] public Color baseWaterColor = Color.white;
        [HideInInspector] public Color baseLightFoamColor = Color.white;
        [HideInInspector] public Color baseDarkFoamColor = Color.white;
        
        public Color waterColor = new Color(0.8f, 0.8f, 0.8f);
        public Color lightFoamColor = new Color(0.8f, 0.8f, 0.8f);
        public Color darkFoamColor = new Color(0.8f, 0.8f, 0.8f);

        private void OnEnable()
        {
            //EditorApplication.playModeStateChanged += ResetColors;
            
            baseWaterColor = waterMaterials[0].GetColor(WaterColor);
            baseLightFoamColor = waterMaterials[0].GetColor(LightFoamColor);
            baseDarkFoamColor = waterMaterials[0].GetColor(DarkFoamColor);
        }

        private void OnDisable()
        {
            //EditorApplication.playModeStateChanged -= ResetColors;
        }
        /*
        private void ResetColors(PlayModeStateChange playModeStateChange)
        {
            if (playModeStateChange == PlayModeStateChange.ExitingPlayMode)
                SwapColors(baseWaterColor, baseLightFoamColor, baseDarkFoamColor);
        }
        */

        /// <summary>
        /// Use to change color of all water material properties, meant to be used with a lerp function but can be standalone.
        /// </summary>
        /// <param name="c1">Base color</param>
        /// <param name="c2">Light foam color</param>
        /// <param name="c3">Dark foam color</param>
        public void SwapColors(Color c1, Color c2, Color c3)
        {
            foreach (Material m in waterMaterials)
            {
                m.SetColor(WaterColor, c1);
                m.SetColor(LightFoamColor, c2);
                m.SetColor(DarkFoamColor, c3);
            }
        }
    }
}