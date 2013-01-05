using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace KerbinClock
{
	public class CelestialEvents
	{
		private const double sKerbinEve = 14687876.8123152;
		private const double sKerbinDuna = 19645697.3538803;
		private const double sKerbinJool = 10090902.3595427;
	
		private const double dKerbinEve = 12009600;
		private const double dKerbinDuna = 5011200;
		private const double dKerbinJool = 4147200;

		private const double aKerbinEve = 15897600;
		private const double aKerbinDuna = 11318400;
		private const double aKerbinJool = 28771200;

		public List<CelestialEvent> getEvents(double UT)
		{
			// Returns 3 next events? I dunno lol
			// HOW DO I CODED
			List<CelestialEvent> eventlist = new List<CelestialEvent>();
			eventlist.Add (new CelestialEvent(UT,dKerbinEve,sKerbinEve,"Launch: Kerbin - Eve"));
			eventlist.Add (new CelestialEvent(UT,dKerbinDuna,sKerbinDuna,"Launch: Kerbin - Duna"));
			eventlist.Add (new CelestialEvent(UT,dKerbinJool,sKerbinJool,"Launch: Kerbin - Jool"));
			eventlist.Add (new CelestialEvent(UT,aKerbinEve,sKerbinEve,"Arrival: Kerbin - Eve"));
			eventlist.Add (new CelestialEvent(UT,aKerbinDuna,sKerbinDuna,"Arrival: Kerbin - Duna"));
			eventlist.Add (new CelestialEvent(UT,aKerbinJool,sKerbinJool,"Arrival: Kerbin - Jool"));

			List<CelestialEvent> sortedevents = eventlist.OrderBy (o=>o.When).ToList ();
			return sortedevents;

		}


	

		public class CelestialEvent
		{
			public double When = 0;
			public string What = "";
			public double Initial = 0;
			public double Synodic = 0;

			public CelestialEvent(double UT, double initial, double synodic, string what)
			{
				When = (initial - UT) % synodic;
				if (When < 0) When += synodic;

				What = what;
				Initial = initial;
				Synodic = synodic;
			}

			public string ToString(double UT)
			{
				When = (Initial - UT) % Synodic;
				if (When < 0) When += Synodic;
				double days = When / 60 / 60 / 24;
				return days.ToString ("0") + " days to " + What;
			}

		}
	}
}


