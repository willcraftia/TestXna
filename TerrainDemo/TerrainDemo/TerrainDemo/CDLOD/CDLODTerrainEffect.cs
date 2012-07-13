#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class CDLODTerrainEffect : IEffectMatrices
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

        /// <summary>
        /// Effect の実体を取得します。
        /// </summary>
        public Effect BackingEffect { get; private set; }

        // I/F
        /// <summary>
        /// このエフェクトでは World を使用しません。
        /// </summary>
        public Matrix World
        {
            // NOTE
            // インスタンシングでは頂点ストリームから変換行列を得るため未使用となります。
            get { return Matrix.Identity; }
            set { }
        }

        // I/F
        /// <summary>
        /// View を取得または設定します。
        /// View の設定では、EyePosition を内部で算出して設定します。
        /// </summary>
        public Matrix View
        {
            get { return view.GetValueMatrix(); }
            set
            {
                view.SetValue(value);

                Matrix inverse;
                Matrix.Invert(ref value, out inverse);
                eyePosition.SetValue(inverse.Translation);
            }
        }

        // I/F
        /// <summary>
        /// Projection を取得または設定します。
        /// </summary>
        public Matrix Projection
        {
            get { return projection.GetValueMatrix(); }
            set { projection.SetValue(value); }
        }

        /// <summary>
        /// AmbientLightColor を取得または設定します。
        /// </summary>
        public Vector3 AmbientLightColor
        {
            get { return ambientLightColor.GetValueVector3(); }
            set { ambientLightColor.SetValue(value); }
        }

        /// <summary>
        /// LightDirection を取得または設定します。
        /// </summary>
        public Vector3 LightDirection
        {
            get { return lightDirection.GetValueVector3(); }
            set { lightDirection.SetValue(value); }
        }

        /// <summary>
        /// DiffuseLightColor を取得または設定します。
        /// </summary>
        public Vector3 DiffuseLightColor
        {
            get { return diffuseLightColor.GetValueVector3(); }
            set { diffuseLightColor.SetValue(value); }
        }

        /// <summary>
        /// TerrainOffset を取得または設定します。
        /// </summary>
        public Vector3 TerrainOffset
        {
            get { return terrainOffset.GetValueVector3(); }
            set { terrainOffset.SetValue(value); }
        }

        /// <summary>
        /// TerrainScale を取得または設定します。
        /// </summary>
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

        /// <summary>
        /// HeightMap を取得または設定します。
        /// HeightMap の設定では、
        /// HeightMapSize、TwoHeightMapSize、
        /// HeightMapTexelSize、TwoHeightMapTexelSize、
        /// samplerWorldToTextureScale
        /// を内部で算出して設定します。
        /// </summary>
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

        /// <summary>
        /// PatchGridSize を取得または設定します。
        /// </summary>
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

        /// <summary>
        /// WhiteSolid technique を取得します。
        /// </summary>
        public EffectTechnique WhiteSolidTequnique { get; private set; }

        /// <summary>
        /// HeightColor technique を取得します。
        /// </summary>
        public EffectTechnique HeightColorTequnique { get; private set; }

        /// <summary>
        /// Wireframe technique を取得します。
        /// </summary>
        public EffectTechnique WireframeTequnique { get; private set; }

        /// <summary>
        /// backingEffect の CurrentTechnique を取得または設定します。
        /// </summary>
        public EffectTechnique CurrentTechnique
        {
            get { return BackingEffect.CurrentTechnique; }
            set { BackingEffect.CurrentTechnique = value; }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// backingEffect の共有を発生させたくない場合は、呼び出し元で Effect の複製を設定してください。
        /// なお、backingEffect で設定する Effect の Dispose() は呼び出し元で管理してください。
        /// </summary>
        /// <param name="backingEffect">インスタンシングに対応した Effect。</param>
        public CDLODTerrainEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");

            this.BackingEffect = backingEffect;

            InitializeParameters();
            InitializeTequniques();
        }

        /// <summary>
        /// プロパティからのアクセスに使用する EffectParameter の初期化を行います。
        /// </summary>
        void InitializeParameters()
        {
            view = BackingEffect.Parameters["View"];
            projection = BackingEffect.Parameters["Projection"];
            // View の設定時に [M41, M42, M43] を EyePosition へ設定します。
            // このため、専用のプロパティによるアクセスを提供しません。
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
        /// 使用する EffectTechnique の初期化を行います。
        /// </summary>
        void InitializeTequniques()
        {
            WhiteSolidTequnique = BackingEffect.Techniques["WhiteSolid"];
            HeightColorTequnique = BackingEffect.Techniques["HeightColor"];
            WireframeTequnique = BackingEffect.Techniques["Wireframe"];
        }
    }
}
