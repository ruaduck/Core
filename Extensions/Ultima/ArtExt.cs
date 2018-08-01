﻿#region Header
//   Vorspire    _,-'/-'/  ArtExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2018  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

// Optional: Uncomment if you have Ultima.dll installed
//#define UltimaSDK

#region References
#if UltimaSDK
using Server.Items;
using Server;

using System.Drawing;
#else
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using Server;
using Server.Items;

using VitaNex;
#endif
#endregion

namespace Ultima
{
	public static class ArtExtUtility
	{
		public const int TileWxH = 44, TileHalfWxH = TileWxH / 2;

		public static readonly Size TileSize = new Size(TileWxH, TileWxH);

#if !UltimaSDK
		private static readonly MethodInfo _UltimaArtGetStatic;
		private static readonly MethodInfo _UltimaArtMeasure;

		static ArtExtUtility()
		{
			try
			{
				var a = Assembly.LoadFrom("Ultima.dll");
				var t = a.GetType("Ultima.Art");

				if (t != null)
				{
					_UltimaArtGetStatic = t.GetMethods(BindingFlags.Static | BindingFlags.Public)
										   .FirstOrDefault(
											   m => m.Name == "GetStatic" && m.ReturnType.IsEqual<Bitmap>() && m.GetParameters().Length == 3);

					_UltimaArtMeasure = t.GetMethod("Measure", BindingFlags.Static | BindingFlags.Public);
				}
			}
			catch (Exception e)
			{
				VitaNexCore.ToConsole("UltimaArt could not load Ultima.dll or a member of Ultima.Art:");
				VitaNexCore.ToConsole(e);
			}
		}

		private static Bitmap ExternalGetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			var param = new object[] {index, false, checkMaxID};

			var img = (Bitmap)_UltimaArtGetStatic.Invoke(null, param);

			patched = (bool)param[1];

			return img;
		}

		// ReSharper disable UnusedParameter.Local
		private static Bitmap InternalGetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			patched = false;
			return null;
		}
		// ReSharper restore UnusedParameter.Local

		public static Bitmap GetStatic(int index, bool checkMaxID = true)
		{
			bool patched;
			return GetStatic(index, out patched, checkMaxID);
		}

		public static Bitmap GetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			if (_UltimaArtGetStatic != null)
			{
				return ExternalGetStatic(index, out patched, checkMaxID);
			}

			return InternalGetStatic(index, out patched, checkMaxID);
		}

		private static void ExternalMeasure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			var param = new object[] {img, 0, 0, 0, 0};

			_UltimaArtMeasure.Invoke(null, param);

			xMin = (int)param[1];
			yMin = (int)param[2];
			xMax = (int)param[3];
			yMax = (int)param[4];
		}

		private static void InternalMeasure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			xMin = yMin = 0;

			if (img != null)
			{
				xMax = img.Width;
				yMax = img.Height;
			}
			else
			{
				xMax = yMax = TileWxH;
			}
		}

		public static void Measure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			if (_UltimaArtMeasure != null)
			{
				ExternalMeasure(img, out xMin, out yMin, out xMax, out yMax);
			}
			else
			{
				InternalMeasure(img, out xMin, out yMin, out xMax, out yMax);
			}
		}
#else
		public static Bitmap GetStatic(int index, bool checkMaxID = true)
		{
			bool patched;
			return GetStatic(index, out patched, checkMaxID);
		}

		public static Bitmap GetStatic(int index, out bool patched, bool checkMaxID = true)
		{
			return Art.GetStatic(index, out patched, checkMaxID);
		}

		public static void Measure(Bitmap img, out int xMin, out int yMin, out int xMax, out int yMax)
		{
			Art.Measure(img, out xMin, out yMin, out xMax, out yMax);
		}
#endif

		public static Rectangle2D GetStaticBounds(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return new Rectangle2D(0, 0, TileWxH, TileWxH);
			}

			int xMin, xMax, yMin, yMax;
			Measure(img, out xMin, out yMin, out xMax, out yMax);

			return new Rectangle2D(new Point2D(xMin, yMin), new Point2D(xMax, yMax));
		}

		public static Rectangle2D GetStaticBounds(this Item item)
		{
			if (item == null)
			{
				return new Rectangle2D(0, 0, TileWxH, TileWxH);
			}

			return GetStaticBounds(item.ItemID);
		}

		public static Point GetImageOffset(this Item item)
		{
			if (item == null)
			{
				return Point.Empty;
			}

			return GetImageOffset(item.ItemID);
		}

		public static Point GetImageOffset(int id)
		{
			var p = Point.Empty;
			var b = GetImageSize(id);

			if (b.Width > TileWxH)
			{
				p.X -= (b.Width - TileWxH) / 2;
			}
			else if (b.Width < TileWxH)
			{
				p.X += (TileWxH - b.Width) / 2;
			}

			if (b.Height > TileWxH)
			{
				p.Y -= b.Height - TileWxH;
			}
			else if (b.Height < TileWxH)
			{
				p.Y += TileWxH - b.Height;
			}

			return p;
		}

		public static int GetImageWidth(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return TileWxH;
			}

			return img.Width;
		}

		public static int GetImageWidth(this Item item)
		{
			if (item == null)
			{
				return TileWxH;
			}

			if (item is BaseMulti)
			{
				return GetImageWidth((BaseMulti)item);
			}

			return GetImageWidth(item.ItemID);
		}

		public static int GetImageHeight(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return TileWxH;
			}

			return img.Height;
		}

		public static int GetImageHeight(this Item item)
		{
			if (item == null)
			{
				return TileWxH;
			}

			if (item is BaseMulti)
			{
				return GetImageHeight((BaseMulti)item);
			}

			return GetImageHeight(item.ItemID);
		}

		public static Size GetImageSize(int id)
		{
			var img = GetStatic(id);

			if (img == null)
			{
				return TileSize;
			}

			return new Size(img.Width, img.Height);
		}

		public static Size GetImageSize(this Item item)
		{
			if (item == null)
			{
				return TileSize;
			}

			if (item is BaseMulti)
			{
				return GetImageSize((BaseMulti)item);
			}

			return GetImageSize(item.ItemID);
		}

		public static Point GetImageOffset(this BaseMulti m)
		{
			if (m == null)
			{
				return Point.Empty;
			}

			return GetImageOffset(m.Components);
		}

		public static Point GetImageOffset(this Server.MultiComponentList mcl)
		{
			if (mcl == null)
			{
				return Point.Empty;
			}

			Point o, p = Point.Empty;

			foreach (var t in OrderByRender(mcl))
			{
				o = GetImageOffset(t.m_ItemID);

				o.X += (t.m_OffsetX * TileHalfWxH) - (t.m_OffsetY * TileHalfWxH);
				o.Y += (t.m_OffsetY * TileHalfWxH) + (t.m_OffsetX * TileHalfWxH);
				o.Y -= t.m_OffsetZ * 4;

				p.X = Math.Min(p.X, o.X);
				p.Y = Math.Min(p.Y, o.Y);
			}

			return p;
		}

		public static int GetImageWidth(this BaseMulti m)
		{
			if (m == null)
			{
				return 0;
			}

			return GetImageWidth(m.Components);
		}

		public static int GetImageWidth(this Server.MultiComponentList mcl)
		{
			if (mcl == null)
			{
				return 0;
			}

			Point o;
			int x1 = 0, x2 = 0, w;

			foreach (var t in OrderByRender(mcl))
			{
				o = GetImageOffset(t.m_ItemID);
				w = GetImageWidth(t.m_ItemID);

				o.X += (t.m_OffsetX * TileHalfWxH) - (t.m_OffsetY * TileHalfWxH);

				x1 = Math.Min(x1, o.X);
				x2 = Math.Max(x2, o.X + w);
			}

			return Math.Max(0, x2 - x1);
		}

		public static int GetImageHeight(this BaseMulti m)
		{
			if (m == null)
			{
				return 0;
			}

			return GetImageHeight(m.Components);
		}

		public static int GetImageHeight(this Server.MultiComponentList mcl)
		{
			if (mcl == null)
			{
				return 0;
			}

			Point o;
			int y1 = 0, y2 = 0, h;

			foreach (var t in OrderByRender(mcl))
			{
				o = GetImageOffset(t.m_ItemID);
				h = GetImageHeight(t.m_ItemID);

				o.Y += (t.m_OffsetY * TileHalfWxH) + (t.m_OffsetX * TileHalfWxH);
				o.Y -= t.m_OffsetZ * 4;

				y1 = Math.Min(y1, o.Y);
				y2 = Math.Max(y2, o.Y + h);
			}

			return Math.Max(0, y2 - y1);
		}

		public static Size GetImageSize(this BaseMulti m)
		{
			if (m == null)
			{
				return Size.Empty;
			}

			return GetImageSize(m.Components);
		}

		public static Size GetImageSize(this Server.MultiComponentList mcl)
		{
			if (mcl == null)
			{
				return Size.Empty;
			}

			Point o;
			Size s;
			int x1 = 0, y1 = 0, x2 = 0, y2 = 0;

			foreach (var t in OrderByRender(mcl))
			{
				o = GetImageOffset(t.m_ItemID);
				s = GetImageSize(t.m_ItemID);

				o.X += (t.m_OffsetX * TileHalfWxH) - (t.m_OffsetY * TileHalfWxH);
				o.Y += (t.m_OffsetY * TileHalfWxH) + (t.m_OffsetX * TileHalfWxH);
				o.Y -= t.m_OffsetZ * 4;

				x1 = Math.Min(x1, o.X);
				y1 = Math.Min(y1, o.Y);

				x2 = Math.Max(x2, o.X + s.Width);
				y2 = Math.Max(y2, o.Y + s.Height);
			}

			return new Size(Math.Max(0, x2 - x1), Math.Max(0, y2 - y1));
		}

		public static IEnumerable<MultiTileEntry> OrderByRender(this BaseMulti m)
		{
			if (m == null)
			{
				return Enumerable.Empty<MultiTileEntry>();
			}

			return OrderByRender(m.Components);
		}

		public static IEnumerable<MultiTileEntry> OrderByRender(this Server.MultiComponentList mcl)
		{
			if (mcl == null)
			{
				yield break;
			}

			foreach (var e in mcl.List //
								 .OrderBy(o => ((o.m_OffsetX * mcl.Height) + o.m_OffsetY) * 2)
								 .ThenBy(zt => zt.m_OffsetZ)
								 .ThenByDescending(zt => ((ulong)zt.m_Flags & (ulong)Server.TileFlag.Surface) != 0)
								 .ThenByDescending(zt => ((ulong)zt.m_Flags & (ulong)Server.TileFlag.Wall) != 0)
								 .ThenBy(zt => ((ulong)zt.m_Flags & (long)Server.TileFlag.Roof) != 0)
								 .ThenBy(zt => Server.TileData.ItemTable[zt.m_ItemID].CalcHeight))
			{
				yield return e;
			}
		}

		public static void EnumerateByRender(this BaseMulti m, Action<Point, MultiTileEntry> action)
		{
			if (m != null && action != null)
			{
				EnumerateByRender(m.Components, action);
			}
		}

		public static void EnumerateByRender(this Server.MultiComponentList mcl, Action<Point, MultiTileEntry> action)
		{
			if (mcl == null || action == null)
			{
				return;
			}

			Point o;

			foreach (var t in mcl.OrderByRender())
			{
				o = GetImageOffset(t.m_ItemID);

				o.X += (t.m_OffsetX * TileHalfWxH) - (t.m_OffsetY * TileHalfWxH);
				o.Y += (t.m_OffsetY * TileHalfWxH) + (t.m_OffsetX * TileHalfWxH);
				o.Y -= t.m_OffsetZ * 4;

				action(o, t);
			}
		}
	}
}