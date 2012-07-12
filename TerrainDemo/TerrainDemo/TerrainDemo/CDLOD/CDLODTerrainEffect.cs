#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace TerrainDemo.CDLOD
{
    public sealed class CDLODTerrainEffect : IEffectMatrices
    {
        /// <summary>
        /// インスタンシングに対応した Effect。
        /// </summary>
        Effect backingEffect;

        /// <summary>
        /// View パラメータ。
        /// </summary>
        EffectParameter view;

        /// <summary>
        /// Projection パラメータ。
        /// </summary>
        EffectParameter projection;

        /// <summary>
        /// EyePosition パラメータ。
        /// </summary>
        EffectParameter eyePosition;

        /// <summary>
        /// AmbientLightColor パラメータ。
        /// </summary>
        EffectParameter ambientLightColor;

        /// <summary>
        /// LightDirection パラメータ。
        /// </summary>
        EffectParameter lightDirection;

        /// <summary>
        /// DiffuseLightColor パラメータ。
        /// </summary>
        EffectParameter diffuseLightColor;

        /// <summary>
        /// TerrainOffset パラメータ。
        /// </summary>
        EffectParameter terrainOffset;

        /// <summary>
        /// TerrainScale パラメータ。
        /// </summary>
        EffectParameter terrainScale;

        /// <summary>
        /// SamplerWorldToTextureScale パラメータ。
        /// </summary>
        EffectParameter samplerWorldToTextureScale;

        /// <summary>
        /// HeightMapSize パラメータ。
        /// </summary>
        EffectParameter heightMapSize;

        /// <summary>
        /// TwoHeightMapSize パラメータ。
        /// </summary>
        EffectParameter twoHeightMapSize;

        /// <summary>
        /// HeightMapTexelSize パラメータ。
        /// </summary>
        EffectParameter heightMapTexelSize;

        /// <summary>
        /// TwoHeightMapTexelSize パラメータ。
        /// </summary>
        EffectParameter twoHeightMapTexelSize;

        /// <summary>
        /// HalfPatchGridSize パラメータ。
        /// </summary>
        EffectParameter halfPatchGridSize;

        /// <summary>
        /// TwoOverPatchGridSize パラメータ。
        /// </summary>
        EffectParameter twoOverPatchGridSize;

        /// <summary>
        /// HeightMap パラメータ。
        /// </summary>
        EffectParameter heightMap;

        float patchGridSize;

        // I/F
        public Matrix World
        {
            // NOTE
            // インスタンシングでは頂点ストリームから変換行列を得るため未使用となります。
            get { return Matrix.Identity; }
            set { }
        }

        // I/F
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

        /// <summary>
        /// インスタンスを生成します。
        /// backingEffect の共有を発生させたくない場合は、呼び出し元で Effect の複製を設定してください。
        /// なお、backingEffect で設定する Effect の Dispose() は呼び出し元で管理してください。
        /// </summary>
        /// <param name="backingEffect">インスタンシングに対応した Effect。</param>
        public CDLODTerrainEffect(Effect backingEffect)
        {
            if (backingEffect == null) throw new ArgumentNullException("backingEffect");
            this.backingEffect = backingEffect;

            InitializeParameters();
        }

        public void Apply()
        {
            backingEffect.CurrentTechnique.Passes[0].Apply();
        }

        /// <summary>
        /// プロパティからのアクセスに使用する EffectParameter の取得と初期化を行います。
        /// </summary>
        void InitializeParameters()
        {
            view = backingEffect.Parameters["View"];
            projection = backingEffect.Parameters["Projection"];
            // View の設定時に [M41, M42, M43] を EyePosition へ設定します。
            // このため、専用のプロパティによるアクセスを提供しません。
            eyePosition = backingEffect.Parameters["EyePosition"];

            ambientLightColor = backingEffect.Parameters["AmbientLightColor"];
            lightDirection = backingEffect.Parameters["LightDirection"];
            diffuseLightColor = backingEffect.Parameters["DiffuseLightColor"];

            terrainOffset = backingEffect.Parameters["TerrainOffset"];
            terrainScale = backingEffect.Parameters["TerrainScale"];

            samplerWorldToTextureScale = backingEffect.Parameters["SamplerWorldToTextureScale"];
            heightMapSize = backingEffect.Parameters["HeightMapSize"];
            twoHeightMapSize = backingEffect.Parameters["TwoHeightMapSize"];
            heightMapTexelSize = backingEffect.Parameters["HeightMapTexelSize"];
            twoHeightMapTexelSize = backingEffect.Parameters["TwoHeightMapTexelSize"];

            halfPatchGridSize = backingEffect.Parameters["HalfPatchGridSize"];
            twoOverPatchGridSize = backingEffect.Parameters["TwoOverPatchGridSize"];

            heightMap = backingEffect.Parameters["HeightMap"];
        }
    }
}
