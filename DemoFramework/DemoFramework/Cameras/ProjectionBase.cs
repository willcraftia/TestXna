#region Using

using System;

#endregion

namespace Willcraftia.Xna.Framework.Cameras
{
    /// <summary>
    /// Projection 実装の基底クラスです。
    /// </summary>
    public abstract class ProjectionBase : Projection
    {
        /// <summary>
        /// Matrix プロパティの値が有効であるかどうかを示す値を取得します。
        /// Matrix プロパティの再計算を必要とするプロパティが変更されると、
        /// このプロパティが true に設定されます。
        /// このプロパティが true の場合にのみ、
        /// Update メソッドは Matrix プロパティの再計算を行います。
        /// サブクラスで MatrixDirty プロパティによる制御が不要な場合は
        /// MatrixDirty プロパティが常に true であるように実装します。
        /// </summary>
        public bool MatrixDirty { get; protected set; }

        /// <summary>
        /// 近くのビュー プレーンとの距離のデフォルト。
        /// </summary>
        public const float DefaultNearPlaneDistance = 0.1f;

        /// <summary>
        /// 遠くのビュー プレーンとの距離のデフォルト。
        /// </summary>
        public const float DefaultFarPlaneDistance = 1000.0f;

        /// <summary>
        /// 近くのビュー プレーンとの距離。
        /// </summary>
        float nearPlaneDistance = DefaultNearPlaneDistance;

        /// <summary>
        /// 遠くのビュー プレーンとの距離。
        /// </summary>
        float farPlaneDistance = DefaultFarPlaneDistance;

        /// <summary>
        /// 近くのビュー プレーンとの距離を取得または設定します。
        /// </summary>
        public float NearPlaneDistance
        {
            get { return nearPlaneDistance; }
            set
            {
                if (nearPlaneDistance == value) return;

                nearPlaneDistance = value;
                MatrixDirty = true;
            }
        }

        /// <summary>
        /// 遠くのビュー プレーンとの距離を取得または設定します。
        /// </summary>
        public float FarPlaneDistance
        {
            get { return farPlaneDistance; }
            set
            {
                if (farPlaneDistance == value) return;

                farPlaneDistance = value;
                MatrixDirty = true;
            }
        }

        /// <summary>
        /// インスタンスを生成します。
        /// </summary>
        protected ProjectionBase()
        {
            MatrixDirty = true;
        }

        /// <summary>
        /// MatrixDirty プロパティが true の場合は、
        /// UpdateOverride() メソッドを呼び出して Matrix プロパティを更新します。
        /// MatrixDirty プロパティが false の場合は何もしません。
        /// </summary>
        public sealed override void Update()
        {
            if (MatrixDirty) UpdateOverride();
        }

        /// <summary>
        /// MatrixDirty プロパティが true の場合に Update メソッドから呼び出されます。
        /// </summary>
        protected abstract void UpdateOverride();
    }
}
