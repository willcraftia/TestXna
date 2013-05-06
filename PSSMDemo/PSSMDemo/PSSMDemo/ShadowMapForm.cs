#region Using

using System;

#endregion

namespace PSSMDemo
{
    /// <summary>
    /// シャドウ マップの形式を表します。
    /// </summary>
    public enum ShadowMapForm
    {
        /// <summary>
        /// 基礎的なシャドウ マップ。
        /// </summary>
        Basic,

        /// <summary>
        /// 分散シャドウ マップ。
        /// </summary>
        Variance
    }
}
