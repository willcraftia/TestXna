#region Using

using System;

#endregion

namespace LiSPSMDemo
{
    /// <summary>
    /// シャドウ マップの種類。
    /// </summary>
    public enum ShadowMapEffectForm
    {
        /// <summary>
        /// 基礎的なシャドウ マップを用います。
        /// </summary>
        Basic,

        /// <summary>
        /// VSM を用います。
        /// </summary>
        Variance
    }
}
