using System;
using UnityEditor.Rendering.Universal.ShaderGUI;
using UnityEngine;

namespace UnityEditor.TADemo
{
    public class GrassShaderEditor : TADemoBaseShaderEditor
    {
        public MaterialProperty bottomBlendMap;
        public MaterialProperty bottomColor;
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            bottomBlendMap = BaseShaderGUI.FindProperty("_BottomBlendMap", properties);
            bottomColor = BaseShaderGUI.FindProperty("_BottomColor", properties);
        }

        public override void ValidateMaterial(Material material)
        {
            base.ValidateMaterial(material);
        }

        public override void DrawSurfaceOptions(Material material)
        {
            base.DrawSurfaceOptions(material);
        }

        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            if (bottomBlendMap != null && bottomColor != null)
            {
                materialEditor.TexturePropertySingleLine(new GUIContent("Bottom Color"), bottomBlendMap, bottomColor);
            }
        }
    }
}

