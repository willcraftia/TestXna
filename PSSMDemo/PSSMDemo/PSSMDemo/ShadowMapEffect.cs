#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace PSSMDemo
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
            ViewProjection = (1 << 1)
        }

        #endregion

        Effect sourceEffect;

        Matrix world;

        Matrix view;

        Matrix projection;

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
        /// ビュー行列。
        /// </summary>
        public Matrix View
        {
            get { return view; }
            set
            {
                view = value;

                dirtyFlags |= DirtyFlags.ViewProjection;
            }
        }

        /// <summary>
        /// 射影行列。
        /// </summary>
        public Matrix Projection
        {
            get { return projection; }
            set
            {
                projection = value;

                dirtyFlags |= DirtyFlags.ViewProjection;
            }
        }

        /// <summary>
        /// 使用するシャドウ マップの種類。
        /// </summary>
        public ShadowMapForm Form { get; set; }

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
            view = Matrix.Identity;
            projection = Matrix.Identity;

            Form = ShadowMapForm.Basic;

            dirtyFlags = DirtyFlags.World | DirtyFlags.ViewProjection;
        }

        public void Apply()
        {
            if ((dirtyFlags & DirtyFlags.World) != 0)
            {
                worldParameter.SetValue(world);

                dirtyFlags &= ~DirtyFlags.World;
            }

            if ((dirtyFlags & DirtyFlags.ViewProjection) != 0)
            {
                Matrix viewProjection;
                Matrix.Multiply(ref view, ref projection, out viewProjection);

                lightViewProjectionParameter.SetValue(viewProjection);

                dirtyFlags &= ~DirtyFlags.ViewProjection;
            }

            if (Form == ShadowMapForm.Variance)
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
