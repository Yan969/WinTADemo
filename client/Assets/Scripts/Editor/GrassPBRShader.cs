
using UnityEngine;

namespace UnityEditor.TADemo
{
    public class GrassPBRShader : TADemoBasePBRShaderEditor
    {
        public MaterialProperty bottomBlendMap;
        public MaterialProperty bottomColor;
        public MaterialProperty thicknessMap;
        public MaterialProperty diffuseDistortion;
        public MaterialProperty power;
        public MaterialProperty scale;
        //public MaterialProperty attention;
        public MaterialProperty ambient;
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);
            bottomBlendMap = FindProperty("_BottomBlendMap", properties);
            bottomColor = FindProperty("_BottomColor", properties);
            thicknessMap = FindProperty("_ThicknessMap", properties);
            diffuseDistortion = FindProperty("_DiffuseDistortion", properties);
            power = FindProperty("_Power", properties);
            scale = FindProperty("_Scale", properties);
            //attention = FindProperty("_Attention", properties);
            ambient = FindProperty("_Ambient", properties);
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

            if (thicknessMap != null)
            {
                materialEditor.TextureProperty(thicknessMap, "Thickness Map");
            }

            if (diffuseDistortion != null)
            {
                materialEditor.RangeProperty(diffuseDistortion, "Diffuse Distortion");
            }

            if (power != null)
            {
                materialEditor.FloatProperty(power, "Power");
            }

            if (scale != null)
            {
                materialEditor.FloatProperty(scale, "Scale");
            }

            // if (attention != null)
            // {
            //     materialEditor.RangeProperty(attention, "Attention");
            // }

            if (ambient != null)
            {
                materialEditor.ColorProperty(ambient, "Ambient");
            }
        }
    }
}


