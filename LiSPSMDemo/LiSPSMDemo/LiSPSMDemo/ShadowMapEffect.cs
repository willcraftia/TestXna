#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace LiSPSMDemo
{
    /// <summary>
    /// シャドウ マップ エフェクト。
    /// </summary>
    public sealed class ShadowMapEffect
    {
        #region DirtyFlags

        /// <summary>
        /// ダーティ フラグ。
        /// 変更された状態のみを最適用するために用います。
        /// </summary>
        [Flags]
        enum DirtyFlags
        {
            World = (1 << 0),
            LightViewProjection = (1 << 1)
        }

        #endregion

        Effect sourceEffect;

        Matrix world;

        Matrix lightViewProjection;

        DirtyFlags dirtyFlags;

        EffectParameter worldParameter;

        EffectParameter lightViewProjectionParameter;

        EffectTechnique basicTechnique;

        EffectTechnique varianceTechnique;

        /// <summary>
        /// ワールド行列。
        /// </summary>
        public Matrix World
        {
            get { return world; }
            set
            {
                if (world == value) return;

                world = value;

                dirtyFlags |= DirtyFlags.World;
            }
        }

        /// <summary>
        /// ライト空間行列 (ライトのビュー×射影行列)。
        /// </summary>
        public Matrix LightViewProjection
        {
            get { return lightViewProjection; }
            set
            {
                if (lightViewProjection == value) return;

                lightViewProjection = value;

                dirtyFlags |= DirtyFlags.LightViewProjection;
            }
        }

        /// <summary>
        /// 使用するシャドウ マップの種類。
        /// </summary>
        public ShadowMapEffectForm Form { get; set; }

        /// <summary>
        /// エフェクト (ShadowMap.fx) を指定してインスタンスを生成します。
        /// </summary>
        /// <param name="sourceEffect">エフェクト。</param>
        public ShadowMapEffect(Effect sourceEffect)
        {
            if (sourceEffect == null) throw new ArgumentNullException("sourceEffect");

            this.sourceEffect = sourceEffect;

            worldParameter = sourceEffect.Parameters["World"];
            lightViewProjectionParameter = sourceEffect.Parameters["LightViewProjection"];

            basicTechnique = sourceEffect.Techniques["Basic"];
            varianceTechnique = sourceEffect.Techniques["Variance"];

            world = Matrix.Identity;
            lightViewProjection = Matrix.Identity;

            Form = ShadowMapEffectForm.Basic;

            dirtyFlags = DirtyFlags.World | DirtyFlags.LightViewProjection;
        }

        public void Apply()
        {
            if ((dirtyFlags & DirtyFlags.World) != 0)
            {
                worldParameter.SetValue(world);

                dirtyFlags &= ~DirtyFlags.World;
            }

            if ((dirtyFlags & DirtyFlags.LightViewProjection) != 0)
            {
                lightViewProjectionParameter.SetValue(lightViewProjection);

                dirtyFlags &= ~DirtyFlags.LightViewProjection;
            }

            if (Form == ShadowMapEffectForm.Variance)
            {
                sourceEffect.CurrentTechnique = varianceTechnique;
            }
            else
            {
                sourceEffect.CurrentTechnique = basicTechnique;
            }

            sourceEffect.CurrentTechnique.Passes[0].Apply();
        }
    }
}
