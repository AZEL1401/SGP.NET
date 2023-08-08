using System.Diagnostics;
using System.Text;

using SGPdotNET.CoordinateSystem;
using SGPdotNET.Observation;
using SGPdotNET.TLE;

var provider = new LocalTleProvider(true, "C:\\Users\\Ali\\Desktop\\resource.txt");
var tles = provider.GetTles().Where(x => x.Value.Name.ToUpper() == "TERRA");
var satellites = tles.Select(pair => new Satellite(pair.Value)).ToList();

var gs = new GroundStation(new GeodeticCoordinate(35.7698135, 51.4447732, 1.5));

var resolution = new TimeSpan(0, 0, 0, 0, 10);
var duration = new TimeSpan(24, 0, 0);

Satellite tracking = satellites[0];
var dt = DateTime.UtcNow;
var dt2 = dt.Add(duration);

var sb = new StringBuilder();

if (dt < dt2)
{
	sb.AppendLine($"{tracking.Name}\t{dt:O}\t{resolution}\t{duration}\t{gs}{Environment.NewLine}");
}

var sw = new Stopwatch();

sw.Restart();
while (dt < dt2)
{
	var observation = gs.Observe(tracking, dt);
	sb.AppendLine($"{observation.Azimuth.Degrees:F4}\t{observation.Elevation.Degrees:F4}");
	dt = dt.Add(resolution);
}
sw.Stop();

sb.AppendLine($"{Environment.NewLine}{sw.Elapsed}");

File.AppendAllText("C:\\Users\\Ali\\Desktop\\terra.gtle", sb.ToString());