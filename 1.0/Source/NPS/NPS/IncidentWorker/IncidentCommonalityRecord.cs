using System;
using System.Xml;
using Verse;
using RimWorld;

namespace TKKN_NPS
{
	public class TKKN_IncidentCommonalityRecord
	{
		public IncidentDef incident;

		public float commonality;

		public void LoadDataFromXmlCustom(XmlNode xmlRoot)
		{
			DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, "incident", xmlRoot.Name);
			this.commonality = (float)ParseHelper.FromString(xmlRoot.FirstChild.Value, typeof(float));
		}
	}
}
