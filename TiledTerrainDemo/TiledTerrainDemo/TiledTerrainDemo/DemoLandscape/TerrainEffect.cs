#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class TerrainEffect : IEffectFog
    {
        public const int DefinedMaxLevelCount = 15;

        public const int DefinedMaxColorCount = 8;

        EffectParameter projection;

        EffectParameter lightEnabled;

        EffectParameter ambientLightColor;

        EffectParameter lightDirection;

        EffectParameter diffuseLightColor;

        EffectParameter fogEnabled;

        EffectParameter fogStart;

        EffectParameter fogEnd;
        
        EffectParameter fogColor;

        EffectParameter terrainEyePosition;

        EffectParameter terrainView;

        EffectParameter terrainScale;

        EffectParameter inverseTerrainScale;

        EffectParameter levelCount;

        EffectParameter morphConsts;

        EffectParameter heightMapSize;

        EffectParameter twoHeightMapSize;

        EffectParameter heightMapTexelSize;

        EffectParameter halfPatchGridSize;

        EffectParameter twoOverPatchGridSize;

        EffectParameter heightMap;

        EffectParameter normalMap;

        EffectParameter heightColorCount;

        EffectParameter heightColors;

        EffectParameter heightColorPositions;

        EffectParameter diffuseMap0;
        EffectParameter diffuseMap1;
        EffectParameter diffuseMap2;
        EffectParameter diffuseMap3;

        EffectParameter diffuseRange0;
        EffectParameter diffuseRange1;
        EffectParameter diffuseRange2;
        EffectParameter diffuseRange3;

        EffectParameter diffuseMapScale;

        float patchGridSizeValue;

        public Effect BackingEffect { get; private set; }

        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        public bool LightEnabled
        {
            get { return lightEnabled.GetValueBoolean(); }
            set { lightEnabled.SetValue(value); }
        }

        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor.GetValueVector3(); }
            set { ambientLightColor.SetValue(value); }
        }

        public Vector3 LightDirection
        {
            get { return lightDirection.GetValueVector3(); }
            set { lightDirection.SetValue(value); }
        }

        public Vector3 DiffuseLightColor
        {
            get { return diffuseLightColor.GetValueVector3(); }
            set { diffuseLightColor.SetValue(value); }
        }

        // I/F
        public bool FogEnabled
        {
            get { return fogEnabled.GetValueSingle() != 0; }
            set { fogEnabled.SetValue(value ? 1 : 0); }
        }

        // I/F
        public float FogStart
        {
            get { return fogStart.GetValueSingle(); }
            set { fogStart.SetValue(value); }
        }

        // I/F
        public float FogEnd
        {
            get { return fogEnd.GetValueSingle(); }
            set { fogEnd.SetValue(value); }
        }

        // I/F
        public Vector3 FogColor
        {
            get { return fogColor.GetValueVector3(); }
            set { fogColor.SetValue(value); }
        }

        public Vector3 TerrainEyePosition
        {
            get { return terrainEyePosition.GetValueVector3(); }
            set { terrainEyePosition.SetValue(value); }
        }

        public Matrix TerrainView
        {
            get { return terrainView.GetValueMatrix(); }
            set { terrainView.SetValue(value); }
        }

        public Vector3 TerrainScale
        {
            get { return terrainScale.GetValueVector3(); }
            set
            {
                terrainScale.SetValue(value);

                var inverse = new Vector3
                {
                    X = 1 / value.X,
                    Y = 1 / value.Y,
                    Z = 1 / value.Z
                };
                inverseTerrainScale.SetValue(inverse);
            }
        }

        public float LevelCount
        {
            get { return levelCount.GetValueSingle(); }
            set { levelCount.SetValue(value); }
        }

        public Vector2[] MorphConsts
        {
            get { return morphConsts.GetValueVector2Array(DefinedMaxLevelCount); }
            set { morphConsts.SetValue(value); }
        }

        /// <summary>
        /// X = the width of a height map.
        /// Y = the height of a height map.
        /// </summary>
        public Vector2 HeightMapSize
        {
            get { return heightMapSize.GetValueVector2(); }
            set
            {
                heightMapSize.SetValue(value);
                // 2 * heightMapSize
                twoHeightMapSize.SetValue(2 * value);

                // texelSize
                var texelSize = new Vector2
                {
                    X = 1 / value.X,
                    Y = 1 / value.Y
                };
                heightMapTexelSize.SetValue(texelSize);
            }
        }

        public Texture2D HeightMap
        {
            get { return heightMap.GetValueTexture2D(); }
            set { heightMap.SetValue(value); }
        }

        public Texture2D NormalMap
        {
            get { return normalMap.GetValueTexture2D(); }
            set { normalMap.SetValue(value); }
        }

        public float PatchGridSize
        {
            get { return patchGridSizeValue; }
            set
            {
                patchGridSizeValue = value;

                halfPatchGridSize.SetValue(patchGridSizeValue * 0.5f);
                twoOverPatchGridSize.SetValue(2 / patchGridSizeValue);
            }
        }

        public float HeightColorCount
        {
            get { return heightColorCount.GetValueSingle(); }
            set { heightColorCount.SetValue(value); }
        }

        public Vector4[] HeightColors
        {
            get { return heightColors.GetValueVector4Array(DefinedMaxColorCount); }
            set { heightColors.SetValue(value); }
        }

        public float[] HeightColorPositions
        {
            get { return heightColorPositions.GetValueSingleArray(DefinedMaxColorCount); }
            set { heightColorPositions.SetValue(value); }
        }

        public Texture2D DiffuseMap0
        {
            get { return diffuseMap0.GetValueTexture2D(); }
            set { diffuseMap0.SetValue(value); }
        }

        public Texture2D DiffuseMap1
        {
            get { return diffuseMap1.GetValueTexture2D(); }
            set { diffuseMap1.SetValue(value); }
        }

        public Texture2D DiffuseMap2
        {
            get { return diffuseMap2.GetValueTexture2D(); }
            set { diffuseMap2.SetValue(value); }
        }

        public Texture2D DiffuseMap3
        {
            get { return diffuseMap3.GetValueTexture2D(); }
            set { diffuseMap3.SetValue(value); }
        }

        public Vector3 DiffuseRange0
        {
            get { return diffuseRange0.GetValueVector3(); }
            set { diffuseRange0.SetValue(value); }
        }

        public Vector3 DiffuseRange1
        {
            get { return diffuseRange1.GetValueVector3(); }
            set { diffuseRange1.SetValue(value); }
        }

        public Vector3 DiffuseRange2
        {
            get { return diffuseRange2.GetValueVector3(); }
            set { diffuseRange2.SetValue(value); }
        }

        public Vector3 DiffuseRange3
        {
            get { return diffuseRange3.GetValueVector3(); }
            set { diffuseRange3.SetValue(value); }
        }

        public float DiffuseMapScale
        {
            get { return diffuseMapScale.GetValueSingle(); }
            set { diffuseMapScale.SetValue(value); }
        }

        public EffectTechnique WhiteSolidTequnique { get; private set; }

        public EffectTechnique NormalTequnique { get; private set; }

        public EffectTechnique HeightColorTequnique { get; private set; }

        public EffectTechnique WireframeTequnique { get; private set; }

        public EffectTechnique TextureTequnique { get; private set; }

        /// <summary>
        /// If not share a backing effect, clone it before specifing to this constructor.
        /// This class does not the backing effect's Dispose().
        /// </summary>
        /// <param name="backingEffect">An effect supporting the CDLOD instancing.</param>
        public TerrainEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            BackingEffect = backingEffect;

            CacheEffectParameters();
            CacheEffectTequniques();
        }

        public void SetDiffuseRange0(float min, float max)
        {
            diffuseRange0.SetValue(new Vector3(min, max, max - min));
        }

        public void SetDiffuseRange1(float min, float max)
        {
            diffuseRange1.SetValue(new Vector3(min, max, max - min));
        }

        public void SetDiffuseRange2(float min, float max)
        {
            diffuseRange2.SetValue(new Vector3(min, max, max - min));
        }

        public void SetDiffuseRange3(float min, float max)
        {
            diffuseRange3.SetValue(new Vector3(min, max, max - min));
        }

        /// <summary>
        /// Cache effect parameter accessors.
        /// </summary>
        void CacheEffectParameters()
        {
            projection = BackingEffect.Parameters["Projection"];

            lightEnabled = BackingEffect.Parameters["LightEnabled"];
            ambientLightColor = BackingEffect.Parameters["AmbientLightColor"];
            lightDirection = BackingEffect.Parameters["LightDirection"];
            diffuseLightColor = BackingEffect.Parameters["DiffuseLightColor"];

            fogEnabled = BackingEffect.Parameters["FogEnabled"];
            fogStart = BackingEffect.Parameters["FogStart"];
            fogEnd = BackingEffect.Parameters["FogEnd"];
            fogColor = BackingEffect.Parameters["FogColor"];

            terrainEyePosition = BackingEffect.Parameters["TerrainEyePosition"];
            terrainView = BackingEffect.Parameters["TerrainView"];
            terrainScale = BackingEffect.Parameters["TerrainScale"];
            inverseTerrainScale = BackingEffect.Parameters["InverseTerrainScale"];

            levelCount = BackingEffect.Parameters["LevelCount"];
            morphConsts = BackingEffect.Parameters["MorphConsts"];

            heightMapSize = BackingEffect.Parameters["HeightMapSize"];
            twoHeightMapSize = BackingEffect.Parameters["TwoHeightMapSize"];
            heightMapTexelSize = BackingEffect.Parameters["HeightMapTexelSize"];

            halfPatchGridSize = BackingEffect.Parameters["HalfPatchGridSize"];
            twoOverPatchGridSize = BackingEffect.Parameters["TwoOverPatchGridSize"];

            heightMap = BackingEffect.Parameters["HeightMap"];
            normalMap = BackingEffect.Parameters["NormalMap"];

            heightColorCount = BackingEffect.Parameters["HeightColorCount"];
            heightColors = BackingEffect.Parameters["HeightColors"];
            heightColorPositions = BackingEffect.Parameters["HeightColorPositions"];

            diffuseMap0 = BackingEffect.Parameters["DiffuseMap0"];
            diffuseMap1 = BackingEffect.Parameters["DiffuseMap1"];
            diffuseMap2 = BackingEffect.Parameters["DiffuseMap2"];
            diffuseMap3 = BackingEffect.Parameters["DiffuseMap3"];
            diffuseRange0 = BackingEffect.Parameters["DiffuseRange0"];
            diffuseRange1 = BackingEffect.Parameters["DiffuseRange1"];
            diffuseRange2 = BackingEffect.Parameters["DiffuseRange2"];
            diffuseRange3 = BackingEffect.Parameters["DiffuseRange3"];

            diffuseMapScale = BackingEffect.Parameters["DiffuseMapScale"];
        }

        /// <summary>
        /// Cache effect technique accessors.
        /// </summary>
        void CacheEffectTequniques()
        {
            WhiteSolidTequnique = BackingEffect.Techniques["WhiteSolid"];
            NormalTequnique = BackingEffect.Techniques["Normal"];
            HeightColorTequnique = BackingEffect.Techniques["HeightColor"];
            WireframeTequnique = BackingEffect.Techniques["Wireframe"];
            TextureTequnique = BackingEffect.Techniques["Texture"];
        }
    }
}
