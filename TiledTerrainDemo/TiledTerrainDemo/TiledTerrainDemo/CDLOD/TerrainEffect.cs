#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TiledTerrainDemo.CDLOD
{
    public sealed class TerrainEffect : IEffectMatrices
    {
        const int definedMaxLevelCount = 15;

        EffectParameter view;

        EffectParameter projection;

        EffectParameter eyePosition;

        EffectParameter ambientLightColor;

        EffectParameter lightDirection;

        EffectParameter diffuseLightColor;

        EffectParameter terrainOffset;

        EffectParameter terrainScale;

        EffectParameter levelCount;

        EffectParameter morphConsts;

        EffectParameter samplerWorldToTextureScale;

        EffectParameter heightMapSize;

        EffectParameter twoHeightMapSize;

        EffectParameter heightMapTexelSize;

        EffectParameter twoHeightMapTexelSize;

        EffectParameter halfPatchGridSize;

        EffectParameter twoOverPatchGridSize;

        EffectParameter heightMap;

        EffectParameter lightEnabled;

        float patchGridSize;

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
            set
            {
                view.SetValue(value);

                // eyePosition.
                Matrix inverse;
                Matrix.Invert(ref value, out inverse);
                eyePosition.SetValue(inverse.Translation);
            }
        }

        // I/F
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
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

        public Vector3 TerrainScale
        {
            get { return terrainScale.GetValueVector3(); }
            set { terrainScale.SetValue(value); }
        }

        public float LevelCount
        {
            get { return levelCount.GetValueSingle(); }
            set { levelCount.SetValue(value); }
        }

        public Vector2[] MorphConsts
        {
            get { return morphConsts.GetValueVector2Array(definedMaxLevelCount); }
            set { morphConsts.SetValue(value); }
        }

        public Texture2D HeightMap
        {
            get { return heightMap.GetValueTexture2D(); }
            set
            {
                heightMap.SetValue(value);

                // heightMapSize
                // twoHeightMapSize
                var size = new Vector2(value.Width, value.Height);
                heightMapSize.SetValue(size);
                twoHeightMapSize.SetValue(2 * size);

                // heightMapTexelSize
                // twoHeightMapTexelSize
                var texelSize = new Vector2
                {
                    X = 1 / size.X,
                    Y = 1 / size.Y
                };
                heightMapTexelSize.SetValue(texelSize);
                twoHeightMapTexelSize.SetValue(2 * texelSize);

                // samplerWorldToTextureScale
                var worldToTextureScale = new Vector2
                {
                    X = (size.X - 1) * texelSize.X,
                    Y = (size.Y - 1) * texelSize.Y
                };
                samplerWorldToTextureScale.SetValue(worldToTextureScale);
            }
        }

        public float PatchGridSize
        {
            get { return patchGridSize; }
            set
            {
                patchGridSize = value;

                halfPatchGridSize.SetValue(patchGridSize * 0.5f);
                twoOverPatchGridSize.SetValue(2 / patchGridSize);
            }
        }

        public bool LightEnabled
        {
            get { return lightEnabled.GetValueBoolean(); }
            set { lightEnabled.SetValue(value); }
        }

        public EffectTechnique WhiteSolidTequnique { get; private set; }

        public EffectTechnique HeightColorTequnique { get; private set; }

        public EffectTechnique WireframeTequnique { get; private set; }

        public EffectTechnique CurrentTechnique
        {
            get { return BackingEffect.CurrentTechnique; }
            set { BackingEffect.CurrentTechnique = value; }
        }

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
            terrainScale = BackingEffect.Parameters["TerrainScale"];

            levelCount = BackingEffect.Parameters["LevelCount"];
            morphConsts = BackingEffect.Parameters["MorphConsts"];

            samplerWorldToTextureScale = BackingEffect.Parameters["SamplerWorldToTextureScale"];
            heightMapSize = BackingEffect.Parameters["HeightMapSize"];
            twoHeightMapSize = BackingEffect.Parameters["TwoHeightMapSize"];
            heightMapTexelSize = BackingEffect.Parameters["HeightMapTexelSize"];
            twoHeightMapTexelSize = BackingEffect.Parameters["TwoHeightMapTexelSize"];

            halfPatchGridSize = BackingEffect.Parameters["HalfPatchGridSize"];
            twoOverPatchGridSize = BackingEffect.Parameters["TwoOverPatchGridSize"];

            heightMap = BackingEffect.Parameters["HeightMap"];

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
