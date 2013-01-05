using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KerbinClock;

/*
    KerbinClock - Provides ingame UT clock and event notifications
    Copyright (C) 2012  Matti Eiden

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.


	Special thanks to Khyperia for HyperEdit source code which served
	as a base for this plugin
*/


public class KerbinClockPart : PartModule
{
	public override void OnAwake()
	{
		if (KerbinClockBehavior.GameObjectInstance == null)
			KerbinClockBehavior.GameObjectInstance = GameObject.Find("KerbinClockBehavior") ?? new GameObject("KerbinClockBehavior", typeof(KerbinClockBehavior));
	}
}

public class KerbinClockBehavior : MonoBehaviour
{
	public static GameObject GameObjectInstance;
	
	private static Core _core;
	private static bool _draw = true;
	
	public void Awake ()
	{
		DontDestroyOnLoad (this);
		CancelInvoke();
		InvokeRepeating ("ClockUpdate",0.2F,0.2F);

	}
	public void ClockUpdate ()
	{
		if (IsEnabled && _draw) {
			_core.ClockUpdate ();
		}
	}
	public void OnGUI()
	{
		if (IsEnabled && _draw)
			Window.DrawAll();
	}
	
	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.F11))
			_draw = !_draw;
	}
	
	public void FixedUpdate()
	{
		if (!IsEnabled)
			return;
		if (_core == null)
			_core = new Core();
		_core.OnFixedUpdate();

	}
	
	private static bool IsEnabled
	{
		get { return FlightGlobals.fetch != null && FlightGlobals.ActiveVessel != null; }
	}
}

namespace KerbinClock
{
	public class Core : Window
	{
		//private readonly Panel[] _editors;
		//private bool _minimized = true;
		private Label UT = new Label(""); 
		//private Label KT = new Label("");
		private Label ET = new Label("");
		private TextArea TextEvents = new TextArea("");
		private CelestialEvents CEvents = new CelestialEvents();
		private List<CelestialEvents.CelestialEvent> CEventList = new List<CelestialEvents.CelestialEvent>();

		private bool expanded = false;
		public Core()
			: base("KerbinClock (F11)")
		{
			WindowRect = new Rect(130, 1, 320, 100);
			//_editors = AppDomain.CurrentDomain.GetAssemblies().SelectMany(a => a.GetTypes())
			//	.Where(t => t.BaseType == typeof(Panel)).Select(t => (Panel)Activator.CreateInstance(t))
			//		.ToArray();
			//foreach (var editor in _editors)
			//	editor.IsRendered = false;
			Contents = new List<IWindowContent> { UT, ET, new Button("Show events",expandView)};
			// FlipMinimized();

		}
		/*
		private void FlipMinimized()
		{
			_minimized = !_minimized;
			if (_minimized)
				Contents = new List<IWindowContent> { new Button("Restore", FlipMinimized) };
			else

		}*/
		
		protected override GUILayoutOption[] WindowOptions
		{
			get
			{ // ExpandWidth?
				return new [] {GUILayout.Width(200), GUILayout.Height(60)};
				//return _minimized ? new[] { GUILayout.Width(80), GUILayout.Height(50) } : new[] { GUILayout.Width(180), GUILayout.ExpandHeight(true) };
			}
		}
		public void ClockUpdate ()
		{	
			//Debug.Log ("ClockUpdate");
			double dUT = Planetarium.GetUniversalTime ();
			//int iUT = Convert.ToInt32 (Math.Floor (dUT));
		
			double dHours = dUT / 60.0 / 60.0;

			double dKYears = Math.Floor (dHours / 2556.5402) + 1;
			double dKDays = Math.Floor (dHours % 2556.5402 / 6.0) + 1; // Kerbin year is 2556.5402 hours
			double dKHours = Math.Floor (dHours % 6.0);

			double dEYears = Math.Floor (dHours / 8766.1527121) + 1;
			double dEDays = Math.Floor (dHours % 8766.1527121 / 24.0) + 1; // Earth year is 8766.1527121 hours
			double dEHours = Math.Floor (dHours % 24.0);

			//int iKYears = Convert.ToInt32 (Math.Floor (dHours / 2556.5402)) + 1;
			//int iKDays = Convert.ToInt32 (Math.Floor (dHours / 6.0) % 2556.5402);
			//int iKHours = Convert.ToInt32 (Math.Floor (dHours % 6.0));
		
			//int iEYears = Convert.ToInt32 (Math.Floor (dHours / 8766.1527121)) + 1;
			//int iEDays = Convert.ToInt32 (Math.Floor (dHours / 24.0) % 8766.1527121);
			//int iEHours = Convert.ToInt32 (Math.Floor (dHours % 24.0));
		
			double dMinutes = Math.Floor ((dUT / 60.0) % 60.0);
			double dSeconds = Math.Floor (dUT % 60.0);
		
		
			UT.Text = "UT: " + Math.Floor (dUT).ToString ("0,0");
			//KT.Text = "KT: Year " + dKYears.ToString ("0") + ", Day " + dKDays.ToString ("0") + ", " + 
			//	dKHours.ToString ("00") + ":" + dMinutes.ToString ("00") + ":" + dSeconds.ToString ("00");
			ET.Text = "ET: Year " + dEYears.ToString ("0") + ", Day " + dEDays.ToString ("0") + ", " + 
				dEHours.ToString ("00") + ":" + dMinutes.ToString ("00") + ":" + dSeconds.ToString ("00");


			// We should update the times anyway so we know when to launch popup notifications..
			// There are 6 events in the list. Should I create a for loop? Jesus this would be easy in python..
			// Okay hang on I think I got this
			TextEvents.Text = "";
			List<string> temp = new List<string>();

			foreach (CelestialEvents.CelestialEvent CEvent in CEventList) // Loop through List with foreach
			{
				temp.Add (CEvent.ToString (dUT));
			}
			TextEvents.Text = string.Join ("\n",temp.ToArray ());

		}
		private void expandView ()
		{
			if (expanded) {
				expanded = false;
				Contents = new List<IWindowContent> { UT, ET, new Button("Show events",expandView) };

			} 
			else {
				expanded = true;

				// First we update the events
				CEventList = CEvents.getEvents (Planetarium.GetUniversalTime());

				// Then we update the clocks
				ClockUpdate ();
				//CelestialEvents events = new CelestialEvents();
				//List<CelestialEvents.CelestialEvent> SortedEvents = events.getNextEvent (Planetarium.GetUniversalTime());


				//string firsteventWhen = Math.Floor (SortedEvents[0].When).ToString ("0,0");
				//string firsteventWhat = SortedEvents[0].What;
				//Label firstevent = new Label(firsteventWhen + ": " + firsteventWhat);

				Contents = new List<IWindowContent> { UT, ET, TextEvents, new Button("Hide events",expandView) };
			}
		}

		public void OnFixedUpdate ()
		{ 

			/*
			foreach (var window in _editors) {
				test.Text = "F12 to hide/show - UTC: " + Planetarium.GetUniversalTime ().ToString ();
				window.Update ();
			}*/
		}
		/*
		public static Orbit CreateOrbit(double inc, double e, double sma, double lan, double w, double mEp, double epoch, CelestialBody body)
		{
			if (Math.Sign(e - 1) == Math.Sign(sma))
				sma = -sma;
			while (mEp < 0)
				mEp += Math.PI * 2;
			while (mEp > Math.PI * 2)
				mEp -= Math.PI * 2;
			return new Orbit(inc, e, sma, lan, w, mEp, epoch, body);
		} */
	}
	/*
	public static class Extentions
	{
		public static void DirectSet(this Orbit orbit, Orbit newOrbit, double time)
		{
			newOrbit.UpdateFromUT(time);
			orbit.inclination = newOrbit.inclination;
			orbit.eccentricity = newOrbit.eccentricity;
			orbit.semiMajorAxis = newOrbit.semiMajorAxis;
			orbit.LAN = newOrbit.LAN;
			orbit.argumentOfPeriapsis = newOrbit.argumentOfPeriapsis;
			orbit.meanAnomalyAtEpoch = newOrbit.meanAnomalyAtEpoch;
			orbit.epoch = newOrbit.epoch;
			orbit.referenceBody = newOrbit.referenceBody;
			orbit.UpdateFromUT(Planetarium.GetUniversalTime());
		}
		
		public static void IndirectSet(this Orbit orbit, Orbit newOrbit, double time)
		{
			newOrbit.UpdateFromUT(time);
			orbit.UpdateFromOrbitAtUT(newOrbit, time, newOrbit.referenceBody);
		}
		
		public static void Teleport(this Krakensbane krakensbane, Vector3d offset)
		{
			foreach (var vessel in FlightGlobals.Vessels.Where(v => v.packed == false && v != FlightGlobals.ActiveVessel))
				vessel.GoOnRails();
			krakensbane.setOffset(offset);
		}
	}*/
	/*
	public static class UnitParser
	{
		static readonly Converter[] Converters = new[]
		{
			new Converter("km", "m", 1000),
			new Converter("mm", "m", 1000000),
			new Converter("gm", "m", 1000000000),
			new Converter("h", "s", 3600)
		};
		
		public static bool Parse(string s, string desired, out double d)
		{
			if (string.IsNullOrEmpty(s))
			{
				d = 0;
				return false;
			}
			var indexOfUnit = s.LastIndexOfAny("0123456789".ToCharArray()) + 1;
			if (double.TryParse(s.Substring(0, indexOfUnit), out d) == false)
				return false;
			if (string.IsNullOrEmpty(desired))
				return true;
			var unit = s.Substring(indexOfUnit).Replace(" ", "").ToLower();
			if (string.IsNullOrEmpty(unit))
				return true;
			while (unit != desired)
			{
				var converter = Converters.FirstOrDefault(c => unit.Contains(c.From));
				if (converter == null)
					return false;
				var index = unit.IndexOf(converter.From, StringComparison.Ordinal);
				unit = unit.Remove(index, converter.From.Length).Insert(index, converter.To);
				d *= converter.Multiplier;
			}
			return true;
		}
		*/
		/*
		private class Converter
		{
			public string From { get; private set; }
			public string To { get; private set; }
			public double Multiplier { get; private set; }
			
			public Converter(string from, string to, double multiplier)
			{
				From = @from;
				To = to;
				Multiplier = multiplier;
			}
		} 
	}*/
}