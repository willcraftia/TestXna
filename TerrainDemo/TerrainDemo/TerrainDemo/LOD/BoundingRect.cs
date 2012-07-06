#region Using

using System;
using Microsoft.Xna.Framework;

#endregion

namespace TerrainDemo.LOD
{
    public struct BoundingRect
    {
		public Vector2 Point1 { get; internal set; }
		public Vector2 Point2 { get; internal set; }
		public Vector2 Point3 { get; internal set; }
		public Vector2 Point4 { get; internal set; }
		public int Count { get { return 4; } }

		public Vector2 Axis1 { get; internal set; }
		public Vector2 Axis2 { get; internal set; }

		public int A1Min, A1Max, A2Min, A2Max;

		/// <summary>
		/// Get the point at the supplied index.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public Vector2 this[int index]
		{
			get
			{
				switch (index)
				{
					case 0:
						return Point1;
					case 1:
						return Point2;
					case 2:
						return Point3;
					case 3:
						return Point4;
					default:
						throw new IndexOutOfRangeException("Index Out of Range: Index must be in the range of 0 to 3.");
				}
			}
		}


        private BoundingRect(Vector2 minimalPoint, Vector2 maximalPoint)
			: this()
		{
			Point1 = minimalPoint;
			Point3 = maximalPoint;
			Point2 = new Vector2(Point3.X, Point1.Y);
			Point4 = new Vector2(Point1.X, Point3.Y);
		}

        public static BoundingRect FromPoints(Vector2 minimalPoint, Vector2 maximalPoint)
		{
            var shape = new BoundingRect(minimalPoint, maximalPoint);
			shape.BuildAxis();
			return shape;
		}

		public Vector2 GetAxis(int index)
		{
			switch (index)
			{
				case 0: return Axis1;
				case 1: return Axis2;
				default: throw new ArgumentException("Index must be in the range of 0 to 3");
			}
		}

		public bool Intersects(ViewClipShape shape)
		{
			return Intersection.Intersects(ref this, ref shape);
		}

		void BuildAxis()
		{
			//Only generating 2 axis since the other axis are parallel.
			Axis1 = Point1 - Point4;
			Axis2 = Point1 - Point2;

			SetScalars();
		}

		void SetScalars()
		{
			Intersection.Project(Axis1, ref this, out A1Min, out A1Max);
			Intersection.Project(Axis2, ref this, out A2Min, out A2Max);	
		}
    }
}
