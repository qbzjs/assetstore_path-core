using System;

using UnityEngine;
using UnityEditor;


namespace AmazingAssets.BeastEditor
{
    internal class StandardShaderGUI : ShaderGUI
    {
        private enum WorkflowMode
        {
            Specular,
            Metallic,
            Dielectric
        }

        public enum BlendMode
        {
            Opaque,
            Cutout,
            Fade,   // Old school alpha-blending mode, fresnel does not affect amount of transparency
            Transparent // Physically plausible transparency mode, implemented as alpha pre-multiply
        }

        public enum SmoothnessMapChannel
        {
            SpecularMetallicAlpha,
            AlbedoAlpha,
        } 

        private static class Styles
        {
            public static GUIContent uvSetLabel = new GUIContent("UV Set");

            public static GUIContent albedoText = new GUIContent("Albedo", "Albedo (RGB) and Transparency (A)");
            public static GUIContent alphaCutoffText = new GUIContent("Alpha Cutoff", "Threshold for alpha cutoff");
            public static GUIContent specularMapText = new GUIContent("Specular", "Specular (RGB) and Smoothness (A)");
            public static GUIContent metallicMapText = new GUIContent("Metallic", "Metallic (R) and Smoothness (A)");
            public static GUIContent smoothnessText = new GUIContent("Smoothness", "Smoothness value");
            public static GUIContent smoothnessScaleText = new GUIContent("Smoothness", "Smoothness scale factor");
            public static GUIContent smoothnessMapChannelText = new GUIContent("Source", "Smoothness texture and channel");
            public static GUIContent highlightsText = new GUIContent("Specular Highlights", "Specular Highlights");
            public static GUIContent reflectionsText = new GUIContent("Reflections", "Glossy Reflections");
            public static GUIContent normalMapText = new GUIContent("Normal Map", "Normal Map");
            public static GUIContent heightMapText = new GUIContent("Height Map", "Height Map (G)");
            public static GUIContent occlusionText = new GUIContent("Occlusion", "Occlusion (G)");
            public static GUIContent emissionText = new GUIContent("Color", "Emission (RGB)");
            public static GUIContent detailMaskText = new GUIContent("Detail Mask", "Mask for Secondary Maps (A)");
            public static GUIContent detailAlbedoText = new GUIContent("Detail Albedo x2", "Albedo (RGB) multiplied by 2");
            public static GUIContent detailNormalMapText = new GUIContent("Normal Map", "Normal Map");

            public static string primaryMapsText = "Main Maps";
            public static string secondaryMapsText = "Secondary Maps";
            public static string forwardText = "Forward Rendering Options";
            public static string renderingMode = "Rendering Mode";
            public static string advancedText = "Advanced Options";
            public static readonly string[] blendNames = Enum.GetNames(typeof(BlendMode));
        }

        MaterialProperty blendMode = null;
        MaterialProperty albedoMap = null;
        MaterialProperty albedoColor = null;
        MaterialProperty alphaCutoff = null;
        MaterialProperty specularMap = null;
        MaterialProperty specularColor = null;
        MaterialProperty metallicMap = null;
        MaterialProperty metallic = null;
        MaterialProperty smoothness = null;
        MaterialProperty smoothnessScale = null;
        MaterialProperty smoothnessMapChannel = null;
        MaterialProperty highlights = null;
        MaterialProperty reflections = null;
        MaterialProperty bumpScale = null;
        MaterialProperty bumpMap = null;
        MaterialProperty occlusionStrength = null;
        MaterialProperty occlusionMap = null;
        MaterialProperty heigtMapScale = null;
        MaterialProperty heightMap = null;
        MaterialProperty emissionColorForRendering = null;
        MaterialProperty emissionMap = null;
        MaterialProperty detailMask = null;
        MaterialProperty detailAlbedoMap = null;
        MaterialProperty detailNormalMapScale = null;
        MaterialProperty detailNormalMap = null;
        MaterialProperty uvSetSecondary = null;


        //Beast        
        MaterialProperty _Beast_Tessellation_Type = null;
        MaterialProperty _Beast_TessellationFactor = null;
        MaterialProperty _Beast_TessellationMinDistance = null;
        MaterialProperty _Beast_TessellationMaxDistance = null;
        MaterialProperty _Beast_TessellationEdgeLength = null;
        MaterialProperty _Beast_TessellationPhong = null;
        MaterialProperty _Beast_TessellationDisplaceMap = null;
        MaterialProperty _Beast_TessellationDisplaceMapUVSet = null;
        MaterialProperty _Beast_TessellationDisplaceMapChannel = null;
        MaterialProperty _Beast_TessellationDisplaceStrength = null;
        MaterialProperty _Beast_TessellationShadowPassLOD = null;
        MaterialProperty _Beast_TessellationDepthPassLOD = null;
        MaterialProperty _Beast_TessellationUseSmoothNormals = null;
        MaterialProperty _Beast_Generate = null;
        MaterialProperty _Beast_TessellationNormalCoef;
        MaterialProperty _Beast_TessellationTangentCoef = null;

        MaterialProperty _CurvedWorldBendSettings = null;


        MaterialEditor m_MaterialEditor;
        WorkflowMode m_WorkflowMode = WorkflowMode.Specular;

        bool m_FirstTimeApply = true;

        public void FindProperties(MaterialProperty[] props)
        {
            blendMode = FindProperty("_Mode", props);
            albedoMap = FindProperty("_MainTex", props);
            albedoColor = FindProperty("_Color", props);
            alphaCutoff = FindProperty("_Cutoff", props);
            specularMap = FindProperty("_SpecGlossMap", props, false);
            specularColor = FindProperty("_SpecColor", props, false);
            metallicMap = FindProperty("_MetallicGlossMap", props, false);
            metallic = FindProperty("_Metallic", props, false);
            if (specularMap != null && specularColor != null)
                m_WorkflowMode = WorkflowMode.Specular;
            else if (metallicMap != null && metallic != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
            smoothness = FindProperty("_Glossiness", props);
            smoothnessScale = FindProperty("_GlossMapScale", props, false);
            smoothnessMapChannel = FindProperty("_SmoothnessTextureChannel", props, false);
            highlights = FindProperty("_SpecularHighlights", props, false);
            reflections = FindProperty("_GlossyReflections", props, false);
            bumpScale = FindProperty("_BumpScale", props);
            bumpMap = FindProperty("_BumpMap", props);
            heigtMapScale = FindProperty("_Parallax", props);
            heightMap = FindProperty("_ParallaxMap", props);
            occlusionStrength = FindProperty("_OcclusionStrength", props);
            occlusionMap = FindProperty("_OcclusionMap", props);
            emissionColorForRendering = FindProperty("_EmissionColor", props);
            emissionMap = FindProperty("_EmissionMap", props);
            detailMask = FindProperty("_DetailMask", props);
            detailAlbedoMap = FindProperty("_DetailAlbedoMap", props);
            detailNormalMapScale = FindProperty("_DetailNormalMapScale", props);
            detailNormalMap = FindProperty("_DetailNormalMap", props);
            uvSetSecondary = FindProperty("_UVSec", props);


            //Beast
            _Beast_Tessellation_Type = FindProperty("_Beast_Tessellation_Type", props);
            _Beast_TessellationFactor = FindProperty("_Beast_TessellationFactor", props);
            _Beast_TessellationMinDistance = FindProperty("_Beast_TessellationMinDistance", props);
            _Beast_TessellationMaxDistance = FindProperty("_Beast_TessellationMaxDistance", props);
            _Beast_TessellationEdgeLength = FindProperty("_Beast_TessellationEdgeLength", props);
            _Beast_TessellationPhong = FindProperty("_Beast_TessellationPhong", props);
            _Beast_TessellationDisplaceMap = FindProperty("_Beast_TessellationDisplaceMap", props);
            _Beast_TessellationDisplaceMapUVSet = FindProperty("_Beast_TessellationDisplaceMapUVSet", props);
            _Beast_TessellationDisplaceMapChannel = FindProperty("_Beast_TessellationDisplaceMapChannel", props);
            _Beast_TessellationDisplaceStrength = FindProperty("_Beast_TessellationDisplaceStrength", props);
            _Beast_TessellationShadowPassLOD = FindProperty("_Beast_TessellationShadowPassLOD", props);
            _Beast_TessellationDepthPassLOD = FindProperty("_Beast_TessellationDepthPassLOD", props);
            _Beast_TessellationUseSmoothNormals = FindProperty("_Beast_TessellationUseSmoothNormals", props);
            _Beast_Generate = FindProperty("_Beast_Generate", props);
            _Beast_TessellationNormalCoef = FindProperty("_Beast_TessellationNormalCoef", props);
            _Beast_TessellationTangentCoef = FindProperty("_Beast_TessellationTangentCoef", props);

            _CurvedWorldBendSettings = FindProperty("_CurvedWorldBendSettings", props, false);
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] props)
        {
            FindProperties(props); // MaterialProperties can be animated so we do not cache them but fetch them every event to ensure animated values are updated correctly
            m_MaterialEditor = materialEditor;
            Material material = materialEditor.target as Material;

            // Make sure that needed setup (ie keywords/renderqueue) are set up if we're switching some existing
            // material to a standard shader.
            // Do this before any GUI code has been issued to prevent layout issues in subsequent GUILayout statements (case 780071)
            if (m_FirstTimeApply)
            {
                MaterialChanged(material, m_WorkflowMode);
                m_FirstTimeApply = false;
            }

            ShaderPropertiesGUI(material);
        }

        public void ShaderPropertiesGUI(Material material)
        {
            // Use default labelWidth
            UnityEditor.EditorGUIUtility.labelWidth = 0f;

            // Detect any changes to the material
            EditorGUI.BeginChangeCheck();
            {
                DoCurvedWorld(material, m_MaterialEditor);

                BlendModePopup();


                // Primary properties
                GUILayout.Label(Styles.primaryMapsText, EditorStyles.boldLabel);
                DoAlbedoArea(material);
                DoSpecularMetallicArea();
                m_MaterialEditor.TexturePropertySingleLine(Styles.normalMapText, bumpMap, bumpMap.textureValue != null ? bumpScale : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.heightMapText, heightMap, heightMap.textureValue != null ? heigtMapScale : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.occlusionText, occlusionMap, occlusionMap.textureValue != null ? occlusionStrength : null);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailMaskText, detailMask);
                DoEmissionArea(material);
                EditorGUI.BeginChangeCheck();
                m_MaterialEditor.TextureScaleOffsetProperty(albedoMap);
                if (EditorGUI.EndChangeCheck())
                    emissionMap.textureScaleAndOffset = albedoMap.textureScaleAndOffset; // Apply the main texture scale and offset to the emission texture as well, for Enlighten's sake

                EditorGUILayout.Space();

                // Secondary properties
                GUILayout.Label(Styles.secondaryMapsText, EditorStyles.boldLabel);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailAlbedoText, detailAlbedoMap);
                m_MaterialEditor.TexturePropertySingleLine(Styles.detailNormalMapText, detailNormalMap, detailNormalMapScale);
                m_MaterialEditor.TextureScaleOffsetProperty(detailAlbedoMap);
                m_MaterialEditor.ShaderProperty(uvSetSecondary, Styles.uvSetLabel.text);

                // Third properties
                GUILayout.Label(Styles.forwardText, EditorStyles.boldLabel);
                if (highlights != null)
                    m_MaterialEditor.ShaderProperty(highlights, Styles.highlightsText);
                if (reflections != null)
                    m_MaterialEditor.ShaderProperty(reflections, Styles.reflectionsText);               
            }
            if (EditorGUI.EndChangeCheck())
            {
                foreach (var obj in blendMode.targets)
                    MaterialChanged((Material)obj, m_WorkflowMode);
            }

            EditorGUILayout.Space();

            // NB renderqueue editor is not shown on purpose: we want to override it based on blend mode
            GUILayout.Label(Styles.advancedText, EditorStyles.boldLabel);
            m_MaterialEditor.EnableInstancingField();
            m_MaterialEditor.DoubleSidedGIField();



            //Beast
            GUILayout.Space(10);
            DoBeast(material, m_MaterialEditor);
        }

        internal void DetermineWorkflow(MaterialProperty[] props)
        {
            if (FindProperty("_SpecGlossMap", props, false) != null && FindProperty("_SpecColor", props, false) != null)
                m_WorkflowMode = WorkflowMode.Specular;
            else if (FindProperty("_MetallicGlossMap", props, false) != null && FindProperty("_Metallic", props, false) != null)
                m_WorkflowMode = WorkflowMode.Metallic;
            else
                m_WorkflowMode = WorkflowMode.Dielectric;
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }

            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));
                return;
            }

            BlendMode blendMode = BlendMode.Opaque;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                blendMode = BlendMode.Cutout;
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                blendMode = BlendMode.Fade;
            }
            material.SetFloat("_Mode", (float)blendMode);

            DetermineWorkflow(MaterialEditor.GetMaterialProperties(new Material[] { material }));
            MaterialChanged(material, m_WorkflowMode);
        }

        void BlendModePopup()
        {
            EditorGUI.showMixedValue = blendMode.hasMixedValue;
            var mode = (BlendMode)blendMode.floatValue;

            EditorGUI.BeginChangeCheck();
            mode = (BlendMode)EditorGUILayout.Popup(Styles.renderingMode, (int)mode, Styles.blendNames);
            if (EditorGUI.EndChangeCheck())
            {
                m_MaterialEditor.RegisterPropertyChangeUndo("Rendering Mode");
                blendMode.floatValue = (float)mode;
            }

            EditorGUI.showMixedValue = false;
        }

        void DoAlbedoArea(Material material)
        {
            m_MaterialEditor.TexturePropertySingleLine(Styles.albedoText, albedoMap, albedoColor);
            if (((BlendMode)material.GetFloat("_Mode") == BlendMode.Cutout))
            {
                m_MaterialEditor.ShaderProperty(alphaCutoff, Styles.alphaCutoffText.text, MaterialEditor.kMiniTextureFieldLabelIndentLevel + 1);
            }
        }

        void DoEmissionArea(Material material)
        {
            // Emission for GI?
            if (m_MaterialEditor.EmissionEnabledProperty())
            {
                bool hadEmissionTexture = emissionMap.textureValue != null;

                // Texture and HDR color controls
                m_MaterialEditor.TexturePropertyWithHDRColor(Styles.emissionText, emissionMap, emissionColorForRendering, false);

                // If texture was assigned and color was black set color to white
                float brightness = emissionColorForRendering.colorValue.maxColorComponent;
                if (emissionMap.textureValue != null && !hadEmissionTexture && brightness <= 0f)
                    emissionColorForRendering.colorValue = Color.white;

                // change the GI flag and fix it up with emissive as black if necessary
                m_MaterialEditor.LightmapEmissionFlagsProperty(MaterialEditor.kMiniTextureFieldLabelIndentLevel, true);
            }
        }

        void DoSpecularMetallicArea()
        {
            bool hasGlossMap = false;
            if (m_WorkflowMode == WorkflowMode.Specular)
            {
                hasGlossMap = specularMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.specularMapText, specularMap, hasGlossMap ? null : specularColor);
            }
            else if (m_WorkflowMode == WorkflowMode.Metallic)
            {
                hasGlossMap = metallicMap.textureValue != null;
                m_MaterialEditor.TexturePropertySingleLine(Styles.metallicMapText, metallicMap, hasGlossMap ? null : metallic);
            }

            bool showSmoothnessScale = hasGlossMap;
            if (smoothnessMapChannel != null)
            {
                int smoothnessChannel = (int)smoothnessMapChannel.floatValue;
                if (smoothnessChannel == (int)SmoothnessMapChannel.AlbedoAlpha)
                    showSmoothnessScale = true;
            }

            int indentation = 2; // align with labels of texture properties
            m_MaterialEditor.ShaderProperty(showSmoothnessScale ? smoothnessScale : smoothness, showSmoothnessScale ? Styles.smoothnessScaleText : Styles.smoothnessText, indentation);

            ++indentation;
            if (smoothnessMapChannel != null)
                m_MaterialEditor.ShaderProperty(smoothnessMapChannel, Styles.smoothnessMapChannelText, indentation);
        }

        public static void SetupMaterialWithBlendMode(Material material, BlendMode blendMode)
        {
            switch (blendMode)
            {
                case BlendMode.Opaque:
                    material.SetOverrideTag("RenderType", "");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = -1;
                    break;
                case BlendMode.Cutout:
                    material.SetOverrideTag("RenderType", "TransparentCutout");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
                    material.SetInt("_ZWrite", 1);
                    material.EnableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.AlphaTest;
                    break;
                case BlendMode.Fade:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.EnableKeyword("_ALPHABLEND_ON");
                    material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
                case BlendMode.Transparent:
                    material.SetOverrideTag("RenderType", "Transparent");
                    material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                    material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    material.SetInt("_ZWrite", 0);
                    material.DisableKeyword("_ALPHATEST_ON");
                    material.DisableKeyword("_ALPHABLEND_ON");
                    material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
                    material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
                    break;
            }
        }

        static SmoothnessMapChannel GetSmoothnessMapChannel(Material material)
        {
            int ch = (int)material.GetFloat("_SmoothnessTextureChannel");
            if (ch == (int)SmoothnessMapChannel.AlbedoAlpha)
                return SmoothnessMapChannel.AlbedoAlpha;
            else
                return SmoothnessMapChannel.SpecularMetallicAlpha;
        }

        static void SetMaterialKeywords(Material material, WorkflowMode workflowMode)
        {
            // Note: keywords must be based on Material value not on MaterialProperty due to multi-edit & material animation
            // (MaterialProperty value might come from renderer material property block)
            SetKeyword(material, "_NORMALMAP", material.GetTexture("_BumpMap") || material.GetTexture("_DetailNormalMap"));
            if (workflowMode == WorkflowMode.Specular)
                SetKeyword(material, "_SPECGLOSSMAP", material.GetTexture("_SpecGlossMap"));
            else if (workflowMode == WorkflowMode.Metallic)
                SetKeyword(material, "_METALLICGLOSSMAP", material.GetTexture("_MetallicGlossMap"));
            SetKeyword(material, "_PARALLAXMAP", material.GetTexture("_ParallaxMap"));
            SetKeyword(material, "_DETAIL_MULX2", material.GetTexture("_DetailAlbedoMap") || material.GetTexture("_DetailNormalMap"));

            // A material's GI flag internally keeps track of whether emission is enabled at all, it's enabled but has no effect
            // or is enabled and may be modified at runtime. This state depends on the values of the current flag and emissive color.
            // The fixup routine makes sure that the material is in the correct state if/when changes are made to the mode or color.
            MaterialEditor.FixupEmissiveFlag(material);
            bool shouldEmissionBeEnabled = (material.globalIlluminationFlags & MaterialGlobalIlluminationFlags.EmissiveIsBlack) == 0;
            SetKeyword(material, "_EMISSION", shouldEmissionBeEnabled);

            if (material.HasProperty("_SmoothnessTextureChannel"))
            {
                SetKeyword(material, "_SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A", GetSmoothnessMapChannel(material) == SmoothnessMapChannel.AlbedoAlpha);
            }
        }

        static void MaterialChanged(Material material, WorkflowMode workflowMode)
        {
            SetupMaterialWithBlendMode(material, (BlendMode)material.GetFloat("_Mode"));

            SetMaterialKeywords(material, workflowMode);
        }

        static void SetKeyword(Material m, string keyword, bool state)
        {
            if (state)
                m.EnableKeyword(keyword);
            else
                m.DisableKeyword(keyword);
        }



        //Beast//////////////////////////////////////////////////////////////////////////////////////////////////////
        public enum TessellationMode
        {
            Fixed,
            DistanceBased,
            EdgeLength,
            Phong
        }
        public enum Recalculate
        {
            None,
            Normals,
            Tangents,
        }

        static string[] tessellationTypeNames = new string[] { "Fixed", "Distance Based", "Edge Length", "Phong" };


        void DoCurvedWorld(Material material, MaterialEditor editor)
        {
            if(_CurvedWorldBendSettings != null)
            {
                using (new EditorGUIHelper.EditorGUILayoutBeginVertical(EditorStyles.helpBox))
                {
                    EditorGUILayout.LabelField("Curved World", EditorStyles.boldLabel);
                    editor.ShaderProperty(_CurvedWorldBendSettings, string.Empty);
                }

                GUILayout.Space(10);
            }
        }
        void DoBeast(Material material, MaterialEditor editor)
        {
            DrawGroupHeader("Beast (Tessellation)");


            EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(_Beast_Tessellation_Type, "Type");
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material, m_WorkflowMode);
            }


            TessellationMode mode = (TessellationMode)_Beast_Tessellation_Type.floatValue;

            switch (mode)
            {
                case TessellationMode.Fixed:
                    editor.RangeProperty(_Beast_TessellationFactor, "Factor");
                    break;

                case TessellationMode.DistanceBased:
                    editor.RangeProperty(_Beast_TessellationFactor, "Factor");

                    using (new EditorGUIHelper.EditorGUIIndentLevel(1))
                    {
                        EditorGUI.BeginChangeCheck();
                        editor.FloatProperty(_Beast_TessellationMinDistance, "Min Distance");
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_Beast_TessellationMinDistance.floatValue < 0)
                                _Beast_TessellationMinDistance.floatValue = 0;

                            if (_Beast_TessellationMinDistance.floatValue > _Beast_TessellationMaxDistance.floatValue)
                                _Beast_TessellationMaxDistance.floatValue = _Beast_TessellationMinDistance.floatValue;
                        }

                        EditorGUI.BeginChangeCheck();
                        editor.FloatProperty(_Beast_TessellationMaxDistance, "Max Distance");
                        if (EditorGUI.EndChangeCheck())
                        {
                            if (_Beast_TessellationMaxDistance.floatValue < 0)
                                _Beast_TessellationMaxDistance.floatValue = 0;
                            if (_Beast_TessellationMaxDistance.floatValue < _Beast_TessellationMinDistance.floatValue)
                                _Beast_TessellationMinDistance.floatValue = _Beast_TessellationMaxDistance.floatValue;
                        }
                    }
                    break;

                case TessellationMode.EdgeLength:
                    editor.RangeProperty(_Beast_TessellationEdgeLength, "Edge Length");
                    break;

                case TessellationMode.Phong:
                    editor.RangeProperty(_Beast_TessellationEdgeLength, "Edge Length");
                    editor.RangeProperty(_Beast_TessellationPhong, "Phong");
                    break;
            }

            if (mode != TessellationMode.Phong)
            {
                using (new EditorGUIHelper.EditorGUIUtilityFieldWidth(UnityEditor.EditorGUIUtility.fieldWidth * 2))
                {
                    editor.TexturePropertySingleLine(new GUIContent("Displace Map"), _Beast_TessellationDisplaceMap);
                }
                editor.ShaderProperty(_Beast_TessellationDisplaceMapChannel, new GUIContent("Channel"), 1);
                editor.ShaderProperty(_Beast_TessellationDisplaceStrength, "Strength", 1);

                editor.TextureScaleOffsetProperty(_Beast_TessellationDisplaceMap);
                editor.ShaderProperty(_Beast_TessellationDisplaceMapUVSet, "UV Set");

            }

            GUILayout.Space(5);
            EditorGUI.BeginChangeCheck();
            editor.ShaderProperty(_Beast_Generate, "Recalculate");
            if (EditorGUI.EndChangeCheck())
            {
                MaterialChanged(material, m_WorkflowMode);
            }

            switch ((Recalculate)_Beast_Generate.floatValue)
            {
                case Recalculate.Normals:
                    editor.ShaderProperty(_Beast_TessellationNormalCoef, "   Normal Coef");
                    break;

                case Recalculate.Tangents:
                    editor.ShaderProperty(_Beast_TessellationNormalCoef, "   Normal Coef");
                    editor.ShaderProperty(_Beast_TessellationTangentCoef, "   Tangent Coef");
                    break;
            }

            editor.RangeProperty(_Beast_TessellationShadowPassLOD, "Shadow Pass LOD");
            editor.RangeProperty(_Beast_TessellationDepthPassLOD, "Depth Pass LOD");


            GUILayout.Space(5);
            bool useSmoothNormals = _Beast_TessellationUseSmoothNormals.floatValue > 0.5f;
            EditorGUI.BeginChangeCheck();
            useSmoothNormals = EditorGUILayout.Toggle("Use Smooth Normals", useSmoothNormals);
            if (EditorGUI.EndChangeCheck())
            {
                _Beast_TessellationUseSmoothNormals.floatValue = useSmoothNormals ? 1 : 0;
            }

            if (useSmoothNormals)
            {
                if (editor.HelpBoxWithButton(new GUIContent("Shader will use smooth normals from mesh UV4."), new GUIContent("Bake")))
                {
                    using (new EditorGUIHelper.GUIEnabled(Selection.activeGameObject != null))
                    {
                        BeastEditorWindow.ShowWindow();
                    }
                }
            }
        }
        void SetupBeastKeywords(Material material)
        {
            switch ((Recalculate)_Beast_Generate.floatValue)
            {
                case Recalculate.None:
                    material.DisableKeyword("_BEAST_GENERATE_NORMALS");
                    material.DisableKeyword("_BEAST_GENERATE_NORMALS_AND_TANGENT");
                    break;

                case Recalculate.Normals:
                    material.EnableKeyword("_BEAST_GENERATE_NORMALS");
                    material.DisableKeyword("_BEAST_GENERATE_NORMALS_AND_TANGENT");
                    break;

                case Recalculate.Tangents:
                    material.DisableKeyword("_BEAST_GENERATE_NORMALS");
                    material.EnableKeyword("_BEAST_GENERATE_NORMALS_AND_TANGENT");
                    break;
            }



            switch ((TessellationMode)_Beast_Tessellation_Type.floatValue)
            {
                case TessellationMode.Fixed:
                    material.EnableKeyword("_BEAST_TESSELLATION_TYPE_FIXED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_DISTANCE_BASED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_EDGE_LENGTH");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_PHONG");
                    break;

                case TessellationMode.DistanceBased:
                    material.EnableKeyword("_BEAST_TESSELLATION_TYPE_DISTANCE_BASED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_FIXED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_EDGE_LENGTH");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_PHONG");
                    break;

                case TessellationMode.EdgeLength:
                    material.EnableKeyword("_BEAST_TESSELLATION_TYPE_EDGE_LENGTH");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_FIXED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_DISTANCE_BASED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_PHONG");
                    break;

                case TessellationMode.Phong:
                    material.EnableKeyword("_BEAST_TESSELLATION_TYPE_PHONG");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_EDGE_LENGTH");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_FIXED");
                    material.DisableKeyword("_BEAST_TESSELLATION_TYPE_DISTANCE_BASED");
                    break;
            }
        }


        void DrawGroupHeader(string label)
        {
            Rect labelRect = EditorGUILayout.GetControlRect();


            Rect headerRect = labelRect;
            headerRect.xMin = 10;
            headerRect.yMax -= 2;
            EditorGUI.DrawRect(headerRect, Color.black * 0.6f);


            Rect lineRect = headerRect;
            lineRect.yMin = lineRect.yMax;
            lineRect.height = 2;
            EditorGUI.DrawRect(lineRect, new Color(0.92f, 0.65f, 0, 1));


            EditorGUI.LabelField(labelRect, label, EditorStyles.whiteLabel);
        }
    }
} // namespace UnityEditor
