using UnityEngine;
using UnityEditor;
using System.Collections;
using System.IO;

namespace GlobalSnowEffect {
    [CustomEditor(typeof(GlobalSnowProfile))]
    public class GlobalSnowProfileEditor : Editor {

        static GUIStyle titleLabelStyle, sectionHeaderStyle;
        static Color titleColor;
        readonly static bool[] expandSection = new bool[6];
        const string SECTION_PREFS = "GlobalSnowExpandSection";
        readonly static string[] sectionNames = new string[] {
                                                "Scene Setup", "Quality", "Coverage", "Appearance", "Features", "Mask Editor"
                                };
        const int SCENE_SETTINGS = 0;
        const int QUALITY_SETTINGS = 1;
        const int COVERAGE_SETTINGS = 2;
        const int APPEARANCE_SETTINGS = 3;
        const int FEATURE_SETTINGS = 4;
        const int MASK_EDITOR = 5;

        SerializedProperty layerMask, excludedCastShadows, zenithalMask, minimumAltitude, altitudeScatter, minimumAltitudeVegetationOffset;
        SerializedProperty smoothCoverage, coverageResolution, coverageExtension, snowQuality, reliefAmount;
        SerializedProperty coverageMask, coverageMaskTexture, coverageMaskWorldSize, coverageMaskWorldCenter, groundCoverage, coverageUpdateMethod;
        SerializedProperty slopeThreshold, slopeSharpness, slopeNoise;
        SerializedProperty occlusion, occlusionIntensity, glitterStrength, maxExposure;
        SerializedProperty snowTint;
        SerializedProperty snowNormalsTex, snowNormalsStrength, noiseTex, noiseTexScale;
        SerializedProperty distanceOptimization, detailDistance, distanceSnowColor, distanceIgnoreNormals, distanceIgnoreCoverage, distanceSlopeThreshold;
        SerializedProperty snowfall, snowfallIntensity, snowfallSpeed, snowfallWind, snowfallDistance, snowfallUseIllumination, snowfallReceiveShadows;
        SerializedProperty snowdustIntensity, snowdustVerticalOffset;
        SerializedProperty cameraFrost, cameraFrostIntensity, cameraFrostSpread, cameraFrostDistortion, cameraFrostTintColor;
        SerializedProperty smoothness, snowAmount, altitudeBlending;
        SerializedProperty billboardCoverage, grassCoverage;

        void OnEnable() {
            titleColor = EditorGUIUtility.isProSkin ? new Color(0.52f, 0.66f, 0.9f) : new Color(0.12f, 0.16f, 0.4f);
            for (int k = 0; k < expandSection.Length; k++) {
                expandSection[k] = EditorPrefs.GetBool(SECTION_PREFS + k, false);
            }

            smoothness = serializedObject.FindProperty("smoothness");
            snowAmount = serializedObject.FindProperty("snowAmount");
            layerMask = serializedObject.FindProperty("layerMask");
            excludedCastShadows = serializedObject.FindProperty("excludedCastShadows");
            zenithalMask = serializedObject.FindProperty("zenithalMask");
            minimumAltitude = serializedObject.FindProperty("minimumAltitude");
            altitudeScatter = serializedObject.FindProperty("altitudeScatter");
            snowTint = serializedObject.FindProperty("snowTint");
            snowNormalsTex = serializedObject.FindProperty("snowNormalsTex");
            snowNormalsStrength = serializedObject.FindProperty("snowNormalsStrength");
            noiseTex = serializedObject.FindProperty("noiseTex");
            noiseTexScale = serializedObject.FindProperty("noiseTexScale");
            altitudeBlending = serializedObject.FindProperty("altitudeBlending");
            minimumAltitudeVegetationOffset = serializedObject.FindProperty("minimumAltitudeVegetationOffset");
            distanceOptimization = serializedObject.FindProperty("distanceOptimization");
            detailDistance = serializedObject.FindProperty("detailDistance");
            distanceSnowColor = serializedObject.FindProperty("distanceSnowColor");
            distanceIgnoreNormals = serializedObject.FindProperty("distanceIgnoreNormals");
            distanceIgnoreCoverage = serializedObject.FindProperty("distanceIgnoreCoverage");
            distanceSlopeThreshold = serializedObject.FindProperty("distanceSlopeThreshold");
            smoothCoverage = serializedObject.FindProperty("smoothCoverage");
            coverageResolution = serializedObject.FindProperty("coverageResolution");
            coverageExtension = serializedObject.FindProperty("coverageExtension");
            coverageMask = serializedObject.FindProperty("coverageMask");
            coverageUpdateMethod = serializedObject.FindProperty("coverageUpdateMethod");
            groundCoverage = serializedObject.FindProperty("groundCoverage");
            coverageMaskTexture = serializedObject.FindProperty("coverageMaskTexture");
            coverageMaskWorldSize = serializedObject.FindProperty("coverageMaskWorldSize");
            coverageMaskWorldCenter = serializedObject.FindProperty("coverageMaskWorldCenter");
            slopeThreshold = serializedObject.FindProperty("slopeThreshold");
            slopeSharpness = serializedObject.FindProperty("slopeSharpness");
            slopeNoise = serializedObject.FindProperty("slopeNoise");
            snowQuality = serializedObject.FindProperty("snowQuality");
            reliefAmount = serializedObject.FindProperty("reliefAmount");
            occlusion = serializedObject.FindProperty("occlusion");
            occlusionIntensity = serializedObject.FindProperty("occlusionIntensity");
            glitterStrength = serializedObject.FindProperty("glitterStrength");
            snowfall = serializedObject.FindProperty("snowfall");
            snowfallIntensity = serializedObject.FindProperty("snowfallIntensity");
            snowfallSpeed = serializedObject.FindProperty("snowfallSpeed");
            snowfallWind = serializedObject.FindProperty("snowfallWind");
            snowfallDistance = serializedObject.FindProperty("snowfallDistance");
            snowfallUseIllumination = serializedObject.FindProperty("snowfallUseIllumination");
            snowfallReceiveShadows = serializedObject.FindProperty("snowfallReceiveShadows");
            snowdustIntensity = serializedObject.FindProperty("snowdustIntensity");
            snowdustVerticalOffset = serializedObject.FindProperty("snowdustVerticalOffset");
            maxExposure = serializedObject.FindProperty("maxExposure");
            cameraFrost = serializedObject.FindProperty("cameraFrost");
            cameraFrostIntensity = serializedObject.FindProperty("cameraFrostIntensity");
            cameraFrostSpread = serializedObject.FindProperty("cameraFrostSpread");
            cameraFrostDistortion = serializedObject.FindProperty("cameraFrostDistortion");
            cameraFrostTintColor = serializedObject.FindProperty("cameraFrostTintColor");
            billboardCoverage = serializedObject.FindProperty("billboardCoverage");
            grassCoverage = serializedObject.FindProperty("grassCoverage");
        }

        void OnDestroy() {
            // Save folding sections state
            for (int k = 0; k < expandSection.Length; k++) {
                EditorPrefs.SetBool(SECTION_PREFS + k, expandSection[k]);
            }
        }

        public override void OnInspectorGUI() {

            serializedObject.Update();

            if (sectionHeaderStyle == null) {
                sectionHeaderStyle = new GUIStyle(EditorStyles.foldout);
            }
            sectionHeaderStyle.SetFoldoutColor();

            if (titleLabelStyle == null) {
                titleLabelStyle = new GUIStyle(EditorStyles.label);
            }
            titleLabelStyle.normal.textColor = titleColor;
            titleLabelStyle.fontStyle = FontStyle.Bold;

            expandSection[QUALITY_SETTINGS] = EditorGUILayout.Foldout(expandSection[QUALITY_SETTINGS], sectionNames[QUALITY_SETTINGS], sectionHeaderStyle);
            if (expandSection[QUALITY_SETTINGS]) {

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Preset", GUILayout.Width(120));
                if (GUILayout.Button(new GUIContent("Best Quality", "Enables relief, occlusion and better coverage quality."))) {
                    coverageResolution.intValue = 3;
                    smoothCoverage.boolValue = true;
                    snowQuality.intValue = (int)SNOW_QUALITY.ReliefMapping;
                    reliefAmount.floatValue = 0.3f;
                    occlusion.boolValue = true;
                    occlusionIntensity.floatValue = 1.2f;
                    glitterStrength.floatValue = 0.75f;
                    distanceOptimization.boolValue = false;
                }
                if (GUILayout.Button(new GUIContent("Medium", "Enables relief and occlusion, normal coverage quality and medium distance optimization."))) {
                    coverageResolution.intValue = 2;
                    smoothCoverage.boolValue = true;
                    snowQuality.intValue = (int)SNOW_QUALITY.ReliefMapping;
                    reliefAmount.floatValue = 0.3f;
                    occlusion.boolValue = true;
                    occlusionIntensity.floatValue = 1.2f;
                    glitterStrength.floatValue = 0.75f;
                    distanceOptimization.boolValue = true;
                    detailDistance.floatValue = 500f;
                }
                if (GUILayout.Button(new GUIContent("Faster", "Enables flat shading and use a faster coverage computation plus shorter distance optimization."))) {
                    coverageExtension.intValue = 1;
                    coverageResolution.intValue = 1;
                    snowQuality.intValue = (int)SNOW_QUALITY.FlatShading;
                    smoothCoverage.boolValue = false;
                    distanceOptimization.boolValue = true;
                    detailDistance.floatValue = 100f;
                }

                if (GUILayout.Button(new GUIContent("Fastest", "Uses optimized snow renderer for distance snow on entire scene."))) {
                    coverageExtension.intValue = 1;
                    coverageResolution.intValue = 1;
                    snowQuality.intValue = (int)SNOW_QUALITY.FlatShading;
                    smoothCoverage.boolValue = false;
                    distanceOptimization.boolValue = true;
                    detailDistance.floatValue = 0f;
                }
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(distanceOptimization, new GUIContent("Distance Optimization", "Reduces snow detail beyond a given distance from the camera."));
                if (distanceOptimization.boolValue) {
                    EditorGUILayout.HelpBox("Please note that distance optimization is only available in forward rendering path.", MessageType.Info);
                    EditorGUILayout.PropertyField(detailDistance, new GUIContent("   Detail Distance", "Beyond this limit the snow will be rendered in a simplified way, reducing GPU usage."));
                    EditorGUILayout.PropertyField(distanceIgnoreCoverage, new GUIContent("   Ignore Coverage", "Ignore coverage computation checking while rendering distant snow."));
                    EditorGUILayout.PropertyField(distanceIgnoreNormals, new GUIContent("   Ignore Normals", "Ignore surface normal on distance snow (makes distance snow rendering faster)."));
                    if (!distanceIgnoreNormals.boolValue) {
                        EditorGUILayout.PropertyField(distanceSlopeThreshold, new GUIContent("   Slope Threshold", "Custom slope threshold for distance snow."));
                    }
                    EditorGUILayout.PropertyField(distanceSnowColor, new GUIContent("   Tint", "Snow color on the distance."));
                }
            }

            EditorGUILayout.Separator();
            expandSection[COVERAGE_SETTINGS] = EditorGUILayout.Foldout(expandSection[COVERAGE_SETTINGS], sectionNames[COVERAGE_SETTINGS], sectionHeaderStyle);
            if (expandSection[COVERAGE_SETTINGS]) {
                EditorGUILayout.PropertyField(layerMask, new GUIContent("Layer Mask", "Optionally exclude some objects from being covered by snow. Alternatively you can add the script GlobalSnowIgnoreCoverage to any number of gameobjects to be exluded without changing their layer."));
                EditorGUILayout.PropertyField(excludedCastShadows, new GUIContent("Excluded Cast Shadows", "If set to false, excluded objects from the layer mask won't cast shadows on snow (improves performance)."));
                EditorGUILayout.PropertyField(zenithalMask, new GUIContent("Zenithal Mask", "Specify which objects are considered for top-down occlusion. Objects on top prevent snow on objects beneath them. Make sure to exclude any particle system to improve performance and avoid coverage issues."));
                EditorGUILayout.PropertyField(minimumAltitude, new GUIContent("Minimum Altitude", "Specify snow level."));
                string t1, t2;
                EditorGUILayout.LabelField("Billboard Only Options");
                t1 = "   Tree Coverage";
                t2 = "   Grass Coverage";
                EditorGUILayout.PropertyField(minimumAltitudeVegetationOffset, new GUIContent("   Altitude Offset", "Applies a vertical offset to the minimum altitude only to grass and trees. This option is useful to avoid showing full grass or trees covered with snow when altitude scattered is used and there's little snow on ground which causes unnatural visuals."));
                EditorGUILayout.PropertyField(billboardCoverage, new GUIContent(t1, "Amount of snow over tree billboards."));
                EditorGUILayout.PropertyField(grassCoverage, new GUIContent(t2, "Amount of snow over grass objects."));
                EditorGUILayout.PropertyField(groundCoverage, new GUIContent("Ground Coverage", "Increase or reduce snow coverage under opaque objects."));
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(coverageExtension, new GUIContent("Coverage Extension", "Area included in the snow coverage. 1 = 512 meters, 2 = 1024 meters. Note that greater extension reduces quality."));
                GUILayout.Label(Mathf.Pow(2, 8f + coverageExtension.intValue).ToString(), GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.PropertyField(coverageResolution, new GUIContent("Coverage Quality", "Resolution of the coverage texture (1=512 pixels, 2=1024 pixels, 3=2048 pixels)."));
                GUILayout.Label(Mathf.Pow(2, 8f + coverageResolution.intValue).ToString(), GUILayout.Width(50));
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.PropertyField(smoothCoverage, new GUIContent("Smooth Coverage", "Increase snow converage quality."));
                EditorGUILayout.PropertyField(coverageMask, new GUIContent("Coverage Mask", "Uses alpha channel of a custom texture as snow coverage mask."));
                if (coverageMask.boolValue) {
                    EditorGUILayout.PropertyField(coverageMaskTexture, new GUIContent("   Texture (A)", "Snow coverage mask. A value of alpha of zero means no snow."));
                    EditorGUILayout.PropertyField(coverageMaskWorldSize, new GUIContent("   World Size", "Mapping of the texture against the world in world units. Usually this should match terrain size."));
                    EditorGUILayout.PropertyField(coverageMaskWorldCenter, new GUIContent("   World Center", "Mapping of the texture center against the world in world units. Use this as an offset to apply coverage mask over a certain area."));
                }
                EditorGUILayout.PropertyField(coverageUpdateMethod, new GUIContent("Coverage Update", "Specifies when the snow coverage needs to be computed. Every frame, Discrete (every 50 meters of player movement), or Manual (requires manual call to UpdateSnowCoverage function)."));
            }

            EditorGUILayout.Separator();
            expandSection[APPEARANCE_SETTINGS] = EditorGUILayout.Foldout(expandSection[APPEARANCE_SETTINGS], sectionNames[APPEARANCE_SETTINGS], sectionHeaderStyle);

            if (expandSection[APPEARANCE_SETTINGS]) {
                EditorGUILayout.PropertyField(snowAmount, new GUIContent("Snow Amount", "Global snow threshold."));
                EditorGUILayout.PropertyField(snowQuality, new GUIContent("Snow Complexity", "Choose the rendering scheme for the snow."));
                if (snowQuality.intValue == (int)SNOW_QUALITY.ReliefMapping) {
                    EditorGUILayout.PropertyField(reliefAmount, new GUIContent("   Relief Amount", "Relief intensity."));
                    EditorGUILayout.PropertyField(occlusion, new GUIContent("   Occlusion", "Enables occlusion effect."));
                    if (occlusion.boolValue) {
                        EditorGUILayout.PropertyField(occlusionIntensity, new GUIContent("      Intensity", "Occlusion intensity."));
                    }
                }
                if (snowQuality.intValue != (int)SNOW_QUALITY.FlatShading) {
                    EditorGUILayout.PropertyField(glitterStrength, new GUIContent("   Glitter Strength", "Snow glitter intensity. Set to zero to disable."));
                }

                EditorGUILayout.LabelField("Slope Options (DX11 only)");
                EditorGUILayout.PropertyField(slopeThreshold, new GUIContent("   Threshold", "The maximum slope where snow can accumulate."));
                EditorGUILayout.PropertyField(slopeSharpness, new GUIContent("   Sharpness", "The sharpness (or smoothness) of the snow at terrain borders."));
                EditorGUILayout.PropertyField(slopeNoise, new GUIContent("   Noise", "Amount of randomization to fill the transient area between low and high slope (determined by slope threshold)."));
                GUI.enabled = true;
                EditorGUILayout.PropertyField(altitudeScatter, new GUIContent("Altitude Scatter", "Defines random snow scattering around minimum altitude level."));
                EditorGUILayout.PropertyField(altitudeBlending, new GUIContent("Altitude Blending", "Defines vertical gradient length for snow blending."));
                EditorGUILayout.PropertyField(snowTint, new GUIContent("Snow Tint", "Snow tint color."));
                EditorGUILayout.PropertyField(smoothness, new GUIContent("Roughness", "Snow PBR roughness."));
                EditorGUILayout.PropertyField(maxExposure, new GUIContent("Max Exposure", "Controls maximum snow brightness."));

                if (snowQuality.intValue != (int)SNOW_QUALITY.FlatShading) {
                    EditorGUILayout.ObjectField(snowNormalsTex, new GUIContent("Snow Normals Texture"));
                    EditorGUILayout.PropertyField(snowNormalsStrength, new GUIContent("Snow Normals Strength"));
                    EditorGUILayout.ObjectField(noiseTex, new GUIContent("Noise Texture"));
                    EditorGUILayout.PropertyField(noiseTexScale, new GUIContent("Noise Texture Scale"));
                }
            }

            EditorGUILayout.Separator();
            expandSection[FEATURE_SETTINGS] = EditorGUILayout.Foldout(expandSection[FEATURE_SETTINGS], sectionNames[FEATURE_SETTINGS], sectionHeaderStyle);

            if (expandSection[FEATURE_SETTINGS]) {
                EditorGUILayout.PropertyField(snowfall, new GUIContent("Snowfall", "Enable snowfall."));
                if (snowfall.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(snowfallIntensity, new GUIContent("Intensity", "Snowflakes emission rate."));
                    EditorGUILayout.PropertyField(snowfallSpeed, new GUIContent("Speed", "Snowfall speed."));
                    EditorGUILayout.PropertyField(snowfallWind, new GUIContent("Wind", "Horizontal wind speed."));
                    EditorGUILayout.PropertyField(snowfallDistance, new GUIContent("Emission Distance", "Emission box scale. Reduce to produce more dense snowfall."));
                    EditorGUILayout.PropertyField(snowfallReceiveShadows, new GUIContent("Receive Shadows", "If enabled, snow particles will receive and cast shadows (affected by illumination in general)."));
                    if (snowfallReceiveShadows.boolValue) {
                        GUI.enabled = false;
                    }
                    EditorGUILayout.PropertyField(snowfallUseIllumination, new GUIContent("Use Illumination", "If enabled, snow particles will be affected by light."));
                    GUI.enabled = true;
                    EditorGUILayout.HelpBox("You can customize particle system prefab located in GlobalSnow/Resources/Prefab folder.", MessageType.Info);
                    EditorGUI.indentLevel--;
                }
                EditorGUILayout.PropertyField(snowdustIntensity, new GUIContent("Snow Dust", "Snow dust intensity."));
                EditorGUI.indentLevel++;
                EditorGUILayout.PropertyField(snowdustVerticalOffset, new GUIContent("Vertical Offset", "Vertical offset for the emission volume with respect to the camera altitude."));
                EditorGUI.indentLevel--;
                if (snowdustIntensity.floatValue > 0) {
                    EditorGUILayout.HelpBox("Customize additional options like gravity or collision of snow dust in the SnowDustSystem prefab inside GlobalSnow/Resources/Common/Prefabs folder.", MessageType.Info);
                }

                bool prevBool = cameraFrost.boolValue;
                EditorGUILayout.PropertyField(cameraFrost, new GUIContent("Camera Frost", "Enable camera frost effect."));
                if (cameraFrost.boolValue) {
                    EditorGUI.indentLevel++;
                    EditorGUILayout.PropertyField(cameraFrostIntensity, new GUIContent("Intensity", "Intensity of camera frost effect."));
                    EditorGUILayout.PropertyField(cameraFrostSpread, new GUIContent("Spread", "Amplitude of camera frost effect."));
                    EditorGUILayout.PropertyField(cameraFrostDistortion, new GUIContent("Distortion", "Distortion magnitude."));
                    EditorGUILayout.PropertyField(cameraFrostTintColor, new GUIContent("Tint Color", "Tinting color for the frost effect."));
                    EditorGUI.indentLevel--;
                }
            }
            EditorGUILayout.Separator();

            if (serializedObject.ApplyModifiedProperties()) {
                GlobalSnow gs = GlobalSnow.instance;
                GlobalSnowProfile profile = (GlobalSnowProfile)target;
                if (gs != null && gs.profile == profile) {
                    profile.ApplyTo(gs);
                }
            }
        }

    }

}
