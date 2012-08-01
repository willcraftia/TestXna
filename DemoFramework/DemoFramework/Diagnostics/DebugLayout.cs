#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace Willcraftia.Xna.Framework.Diagnostics
{
    /// <summary>
    /// 簡易なレイアウト計算を行う構造体です。
    /// </summary>
    public struct DebugLayout
    {
        /// <summary>
        /// コンテナ領域。
        /// </summary>
        public Rectangle ContainerBounds;

        /// <summary>
        /// 領域の幅。
        /// </summary>
        public int Width;

        /// <summary>
        /// 領域の高さ。
        /// </summary>
        public int Height;

        /// <summary>
        /// コンテナ領域に対する水平方向の余白。
        /// </summary>
        public int HorizontalMargin;

        /// <summary>
        /// コンテナ領域に対する垂直方向の余白。
        /// </summary>
        public int VerticalMargin;

        /// <summary>
        /// コンテナ領域に対する水平方向の配置方法。
        /// </summary>
        public DebugHorizontalAlignment HorizontalAlignment;

        /// <summary>
        /// コンテナ領域に対する垂直方向の配置方法。
        /// </summary>
        public DebugVerticalAlignment VerticalAlignment;

        /// <summary>
        /// Arrage メソッドの呼び出しで得られる領域。
        /// </summary>
        public Rectangle ArrangedBounds;

        /// <summary>
        /// 設定されたフィールドの情報に基づき、ArrangedBounds フィールドを計算します。
        /// </summary>
        public void Arrange()
        {
            switch (HorizontalAlignment)
            {
                case DebugHorizontalAlignment.Left:
                    {
                        ArrangedBounds.X = ContainerBounds.X + HorizontalMargin;
                        break;
                    }
                case DebugHorizontalAlignment.Right:
                    {
                        ArrangedBounds.X = ContainerBounds.X + ContainerBounds.Width - HorizontalMargin - Width;
                        break;
                    }
                case DebugHorizontalAlignment.Center:
                default:
                    {
                        ArrangedBounds.X = ContainerBounds.X + (ContainerBounds.Width - Width) / 2;
                        break;
                    }
            }

            switch (VerticalAlignment)
            {
                case DebugVerticalAlignment.Top:
                    {
                        ArrangedBounds.Y = ContainerBounds.Y + VerticalMargin;
                        break;
                    }
                case DebugVerticalAlignment.Bottom:
                    {
                        ArrangedBounds.Y = ContainerBounds.Y + ContainerBounds.Height - VerticalMargin - Height;
                        break;
                    }
                case DebugVerticalAlignment.Center:
                default:
                    {
                        ArrangedBounds.Y = ContainerBounds.Y + (ContainerBounds.Height - Height) / 2;
                        break;
                    }
            }

            ArrangedBounds.Width = Width;
            ArrangedBounds.Height = Height;
        }
    }
}
