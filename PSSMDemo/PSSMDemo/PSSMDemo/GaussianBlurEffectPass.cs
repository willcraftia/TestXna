#region Using

using System;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// ガウシアン ブラーの適用方向です。
    /// </summary>
    public enum GaussianBlurEffectPass
    {
        /// <summary>
        /// 水平方向へのブラーを適用します。
        /// </summary>
        Horizon,

        /// <summary>
        /// 垂直方向のブラーを適用します。
        /// </summary>
        Vertical
    }
}
