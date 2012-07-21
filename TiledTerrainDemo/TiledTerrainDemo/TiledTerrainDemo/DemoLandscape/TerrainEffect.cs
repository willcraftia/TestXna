#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.DemoLandscape
{
    public sealed class TerrainEffect : IEffectMatrices
    {
        public const int DefinedMaxLevelCount = 15;

        public const int DefinedMaxColorCount = 8;

        EffectParameter view;

        EffectParameter projection;

        EffectParameter eyePosition;

        EffectParameter ambientLightColor;

        EffectParameter lightDirection;

        EffectParameter diffuseLightColor;

        EffectParameter terrainOffset;

        EffectParameter terrainEyePosition;

        EffectParameter terrainScale;

        EffectParameter inverseTerrainScale;

        EffectParameter levelCount;

        EffectParameter morphConsts;

        EffectParameter heightMapSize;

        EffectParameter twoHeightMapSize;

        EffectParameter heightMapTexelSize;

        EffectParameter twoHeightMapTexelSize;

        EffectParameter heightMapOverlapSize;

        EffectParameter halfPatchGridSize;

        EffectParameter twoOverPatchGridSize;

        EffectParameter heightMap;

        EffectParameter heightColorCount;

        EffectParameter heightColors;

        EffectParameter heightColorPositions;

        EffectParameter lightEnabled;

        float patchGridSizeValue;

        public Effect BackingEffect { get; private set; }

        // I/F
        public Matrix World
        {
            get { throw new NotSupportedException("This effect never use a world matrix."); }
            set { throw new NotSupportedException("This effect never use a world matrix."); }
        }

        // I/F
        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set { view.SetValue(value); }
        }

        // I/F
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        public Vector3 EyePosition
        {
            get { return eyePosition.GetValueVector3(); }
            set { eyePosition.SetValue(value); }
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

        public Vector3 TerrainOffset
        {
            get { return terrainOffset.GetValueVector3(); }
            set { terrainOffset.SetValue(value); }
        }

        public Vector3 TerrainEyePosition
        {
            get { return terrainEyePosition.GetValueVector3(); }
            set { terrainEyePosition.SetValue(value); }
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

        public Vector2 HeightMapSize
        {
            get { return heightMapSize.GetValueVector2(); }
        }

        public float HeightMapOverlapSize
        {
            get { return heightMapOverlapSize.GetValueSingle(); }
        }

        public Texture2D HeightMap
        {
            get { return heightMap.GetValueTexture2D(); }
            set { heightMap.SetValue(value); }
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

        public bool LightEnabled
        {
            get { return lightEnabled.GetValueBoolean(); }
            set { lightEnabled.SetValue(value); }
        }

        public EffectTechnique WhiteSolidTequnique { get; private set; }

        public EffectTechnique HeightColorTequnique { get; private set; }

        public EffectTechnique WireframeTequnique { get; private set; }

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

        public void SetHeightMapInfo(float width, float height, float overlapSize)
        {
            // heightMapSize
            var size = new Vector2(width + 2 * overlapSize, height + 2 * overlapSize);
            heightMapSize.SetValue(size);
            // 2 * heightMapSize
            twoHeightMapSize.SetValue(2 * size);

            // texelSize
            var texelSize = new Vector2
            {
                X = 1 / size.X,
                Y = 1 / size.Y
            };
            heightMapTexelSize.SetValue(texelSize);
            // 2 * texelSize
            twoHeightMapTexelSize.SetValue(2 * texelSize);

            // overlapSize
            heightMapOverlapSize.SetValue(overlapSize);
        }

        /// <summary>
        /// Cache effect parameter accessors.
        /// </summary>
        void CacheEffectParameters()
        {
            view = BackingEffect.Parameters["View"];
            projection = BackingEffect.Parameters["Projection"];
            eyePosition = BackingEffect.Parameters["EyePosition"];

            ambientLightColor = BackingEffect.Parameters["AmbientLightColor"];
            lightDirection = BackingEffect.Parameters["LightDirection"];
            diffuseLightColor = BackingEffect.Parameters["DiffuseLightColor"];

            terrainOffset = BackingEffect.Parameters["TerrainOffset"];
            terrainEyePosition = BackingEffect.Parameters["TerrainEyePosition"];
            terrainScale = BackingEffect.Parameters["TerrainScale"];
            inverseTerrainScale = BackingEffect.Parameters["InverseTerrainScale"];

            levelCount = BackingEffect.Parameters["LevelCount"];
            morphConsts = BackingEffect.Parameters["MorphConsts"];

            heightMapSize = BackingEffect.Parameters["HeightMapSize"];
            twoHeightMapSize = BackingEffect.Parameters["TwoHeightMapSize"];
            heightMapTexelSize = BackingEffect.Parameters["HeightMapTexelSize"];
            twoHeightMapTexelSize = BackingEffect.Parameters["TwoHeightMapTexelSize"];
            heightMapOverlapSize = BackingEffect.Parameters["HeightMapOverlapSize"];

            halfPatchGridSize = BackingEffect.Parameters["HalfPatchGridSize"];
            twoOverPatchGridSize = BackingEffect.Parameters["TwoOverPatchGridSize"];

            heightMap = BackingEffect.Parameters["HeightMap"];

            heightColorCount = BackingEffect.Parameters["HeightColorCount"];
            heightColors = BackingEffect.Parameters["HeightColors"];
            heightColorPositions = BackingEffect.Parameters["HeightColorPositions"];
            lightEnabled = BackingEffect.Parameters["LightEnabled"];
        }

        /// <summary>
        /// Cache effect technique accessors.
        /// </summary>
        void CacheEffectTequniques()
        {
            WhiteSolidTequnique = BackingEffect.Techniques["WhiteSolid"];
            HeightColorTequnique = BackingEffect.Techniques["HeightColor"];
            WireframeTequnique = BackingEffect.Techniques["Wireframe"];
        }
    }
}
