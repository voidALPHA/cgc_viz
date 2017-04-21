using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.DustinHorne.JsonDotNetUnity.TestCases.TestModels
{
	public class SampleChild : SampleBase
	{
		public List<SimpleClassObject> ObjectList { get; set; }

		public Dictionary<int, SimpleClassObject> ObjectDictionary { get; set; } 
	}
}