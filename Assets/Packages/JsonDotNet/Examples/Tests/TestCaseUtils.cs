using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Assets.DustinHorne.JsonDotNetUnity.TestCases.TestModels;
using UnityEngine;
using Random = System.Random;

namespace Assets.DustinHorne.JsonDotNetUnity.TestCases
{
	public static class TestCaseUtils
	{
		private static Random _rnd = new Random();

		public static SampleBase GetSampleBase()
		{
			var sb = new SampleBase();
			sb.TextValue = Guid.NewGuid().ToString();
			sb.NumberValue = _rnd.Next();

			var x = _rnd.Next();
			var y = _rnd.Next();
			var z = _rnd.Next();

			sb.VectorValue = new Vector3((float) x, (float) y, (float) z);

			return sb;
		}

		public static SampleChild GetSampleChid()
		{
			var sc = new SampleChild();
			sc.TextValue = Guid.NewGuid().ToString();
			sc.NumberValue = _rnd.Next();

			var x = _rnd.Next();
			var y = _rnd.Next();
			var z = _rnd.Next();

			sc.VectorValue = new Vector3((float)x, (float)y, (float)z);
			sc.ObjectDictionary = new Dictionary<int, SimpleClassObject>();
			for (var i = 0; i < 4; i++)
			{
				var dobj = GetSimpleClassObject();
				sc.ObjectDictionary.Add(i, dobj);
			}

			sc.ObjectList = new List<SimpleClassObject>();
			for (var j = 0; j < 4; j++)
			{
				var lobj = GetSimpleClassObject();
				sc.ObjectList.Add(lobj);
			}

			return sc;
		}

		public static SimpleClassObject GetSimpleClassObject()
		{
			var sc = new SimpleClassObject();
			sc.TextValue = Guid.NewGuid().ToString();
			sc.NumberValue = _rnd.Next();

			var x = _rnd.Next();
			var y = _rnd.Next();
			var z = _rnd.Next();

			sc.VectorValue = new Vector3((float)x, (float)y, (float)z);

			return sc;
		}
	}
}
