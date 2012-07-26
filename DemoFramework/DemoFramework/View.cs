#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework
{
    /// <summary>
    /// View 行列を管理するクラスです。
    /// </summary>
    public abstract class View
    {
        /// <summary>
        /// View 行列。
        /// </summary>
        public Matrix Matrix = Matrix.Identity;

        /// <summary>
        /// Matrix プロパティを更新します。
        /// </summary>
        public abstract void Update();

        public static void GetEyePosition(ref Matrix view, out Vector3 result)
        {
            Matrix inverse;
            Matrix.Invert(ref view, out inverse);
            result = inverse.Translation;
        }

        public static Vector3 GetEyePosition(Matrix view)
        {
            Vector3 result;
            GetEyePosition(ref view, out result);
            return result;
        }
    }
}
