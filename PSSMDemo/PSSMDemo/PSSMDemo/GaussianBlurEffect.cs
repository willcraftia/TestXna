#region Using

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// ガウシアン ブラー エフェクト。
    /// </summary>
    public sealed class GaussianBlurEffect
    {
        #region DirtyFlags

        /// <summary>
        /// ダーティ フラグ。
        /// 変更された状態のみを最適用するために用います。
        /// </summary>
        [Flags]
        enum DirtyFlags
        {
            KernelSize = (1 << 0),
            KernelOffsets = (1 << 1),
            KernelWeights = (1 << 2)
        }

        #endregion

        /// <summary>
        /// 最大適用半径。
        /// </summary>
        public const int MaxRadius = 15;

        /// <summary>
        /// 最大カーネル サイズ。
        /// </summary>
        public const int MaxKernelSize = MaxRadius * 2 + 1;

        /// <summary>
        /// デフォルトの適用半径。
        /// </summary>
        public const int DefaultRadius = 7;

        /// <summary>
        /// デフォルトの適用量。
        /// </summary>
        public const float DefaultAmount = 2.0f;

        Effect sourceEffect;

        int kernelSize;

        Vector3[] horizontalKernels;

        Vector3[] verticalKernels;

        int radius;

        float amount;

        int width;

        int height;

        DirtyFlags dirtyFlags;

        EffectParameter kernelSizeParameter;

        EffectParameter kernelsParameter;

        public GaussianBlurEffectPass Pass { get; set; }

        /// <summary>
        /// 適用半径。
        /// </summary>
        public int Radius
        {
            get { return radius; }
            set
            {
                if (value < 1 || MaxRadius < value) throw new ArgumentOutOfRangeException("value");

                if (radius == value) return;

                radius = value;

                dirtyFlags |= DirtyFlags.KernelSize | DirtyFlags.KernelWeights;
            }
        }

        /// <summary>
        /// 適用量。
        /// </summary>
        public float Amount
        {
            get { return amount; }
            set
            {
                if (value < float.Epsilon) throw new ArgumentOutOfRangeException("value");

                if (amount == value) return;

                amount = value;

                dirtyFlags |= DirtyFlags.KernelWeights;
            }
        }

        /// <summary>
        /// ブラー対象テクスチャの幅。
        /// </summary>
        public int Width
        {
            get { return width; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                if (width == value) return;

                width = value;

                dirtyFlags |= DirtyFlags.KernelOffsets;
            }
        }

        /// <summary>
        /// ブラー対象テクスチャの高さ。
        /// </summary>
        public int Height
        {
            get { return height; }
            set
            {
                if (value < 1) throw new ArgumentOutOfRangeException("value");

                if (height == value) return;

                height = value;

                dirtyFlags |= DirtyFlags.KernelOffsets;
            }
        }

        /// <summary>
        /// エフェクト (GaussianBlur.fx) を指定してインスタンスを生成します。
        /// </summary>
        /// <param name="sourceEffect">エフェクト。</param>
        public GaussianBlurEffect(Effect sourceEffect)
        {
            if (sourceEffect == null) throw new ArgumentNullException("sourceEffect");

            this.sourceEffect = sourceEffect;

            kernelSizeParameter = sourceEffect.Parameters["KernelSize"];
            kernelsParameter = sourceEffect.Parameters["Kernels"];

            horizontalKernels = new Vector3[MaxKernelSize];
            verticalKernels = new Vector3[MaxKernelSize];

            radius = DefaultRadius;
            amount = DefaultAmount;
            width = 1;
            height = 1;

            dirtyFlags |= DirtyFlags.KernelSize | DirtyFlags.KernelOffsets | DirtyFlags.KernelWeights;
        }

        public void Apply()
        {
            SetKernelSize();
            SetKernelOffsets();
            SetKernelWeights();

            switch (Pass)
            {
                case GaussianBlurEffectPass.Horizon:
                    kernelsParameter.SetValue(horizontalKernels);
                    break;
                case GaussianBlurEffectPass.Vertical:
                    kernelsParameter.SetValue(verticalKernels);
                    break;
                default:
                    throw new InvalidOperationException("Unknown direction: " + Pass);
            }

            sourceEffect.CurrentTechnique.Passes[0].Apply();
        }

        void SetKernelSize()
        {
            if ((dirtyFlags & DirtyFlags.KernelSize) != 0)
            {
                kernelSize = radius * 2 + 1;
                kernelSizeParameter.SetValue(kernelSize);

                dirtyFlags &= ~DirtyFlags.KernelSize;
                dirtyFlags |= DirtyFlags.KernelOffsets | DirtyFlags.KernelWeights;
            }
        }

        void SetKernelOffsets()
        {
            if ((dirtyFlags & DirtyFlags.KernelOffsets) != 0)
            {
                var dx = 1.0f / (float) width;
                var dy = 1.0f / (float) height;

                horizontalKernels[0].X = 0.0f;
                horizontalKernels[0].Y = 0.0f;
                verticalKernels[0].X = 0.0f;
                verticalKernels[0].Y = 0.0f;

                for (int i = 0; i < kernelSize / 2; i++)
                {
                    int baseIndex = i * 2;
                    int left = baseIndex + 1;
                    int right = baseIndex + 2;

                    // XNA BloomPostprocess サンプルに従ってオフセットを決定。
                    float sampleOffset = i * 2 + 1.5f;
                    var offsetX = dx * sampleOffset;
                    var offsetY = dy * sampleOffset;

                    horizontalKernels[left].X = offsetX;
                    horizontalKernels[right].X = -offsetX;

                    verticalKernels[left].Y = offsetY;
                    verticalKernels[right].Y = -offsetY;
                }

                dirtyFlags &= ~DirtyFlags.KernelOffsets;
            }
        }

        void SetKernelWeights()
        {
            if ((dirtyFlags & DirtyFlags.KernelWeights) != 0)
            {
                var totalWeight = 0.0f;
                var sigma = (float) radius / amount;

                var weight = CalculateGaussian(sigma, 0);

                horizontalKernels[0].Z = weight;
                verticalKernels[0].Z = weight;

                totalWeight += weight;

                for (int i = 0; i < kernelSize / 2; i++)
                {
                    int baseIndex = i * 2;
                    int left = baseIndex + 1;
                    int right = baseIndex + 2;

                    weight = CalculateGaussian(sigma, i + 1);
                    totalWeight += weight * 2;

                    horizontalKernels[left].Z = weight;
                    horizontalKernels[right].Z = weight;

                    verticalKernels[left].Z = weight;
                    verticalKernels[right].Z = weight;
                }

                // Normalize
                float inverseTotalWeights = 1.0f / totalWeight;
                for (int i = 0; i < kernelSize; i++)
                {
                    horizontalKernels[i].Z *= inverseTotalWeights;
                    verticalKernels[i].Z *= inverseTotalWeights;
                }

                dirtyFlags &= ~DirtyFlags.KernelWeights;
            }
        }

        public static float CalculateGaussian(float sigma, float n)
        {
            // 参考: sigmaRoot = (float) Math.Sqrt(2.0f * Math.PI * sigma * sigma)
            var twoSigmaSquare = 2.0f * sigma * sigma;
            var sigmaRoot = (float) Math.Sqrt(Math.PI * twoSigmaSquare);
            return (float) Math.Exp(-(n * n) / twoSigmaSquare) / sigmaRoot;
        }
    }
}
