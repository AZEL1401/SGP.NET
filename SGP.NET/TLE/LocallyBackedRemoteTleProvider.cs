﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Threading;

namespace SGPdotNET.TLE
{
    /// <inheritdoc cref="RemoteTleProvider" />
    /// <summary>
    ///     Provides a class to retrieve TLEs from a remote network resource
    /// </summary>
    public class LocallyBackedRemoteTleProvider : RemoteTleProvider
    {
        private readonly string _localFilename;

        /// <inheritdoc />
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sources">The sources that should be queried</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        /// <param name="localFilename">The file in which the TLEs will be locally cached</param>
        public LocallyBackedRemoteTleProvider(IEnumerable<Url> sources, bool threeLine, string localFilename) : this(sources, threeLine,
            TimeSpan.FromDays(1), localFilename)
        {
        }

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="sources">The sources that should be queried</param>
        /// <param name="threeLine">True if the TLEs contain a third, preceding name line (3le format)</param>
        /// <param name="maxAge">The maximum time to keep TLEs cached before updating them from the remote</param>
        /// <param name="localFilename">The file in which the TLEs will be locally cached</param>
        public LocallyBackedRemoteTleProvider(IEnumerable<Url> sources, bool threeLine, TimeSpan maxAge, string localFilename) : base(sources, threeLine, maxAge)
        {
            _localFilename = localFilename;
        }

        internal override Dictionary<int, Tle> FetchNewTles()
        {
            if (File.Exists(_localFilename))
                using (var sr = new StreamReader(_localFilename))
                {
                    var dateLine = sr.ReadLine();

                    if (DateTime.TryParse(dateLine, CultureInfo.InvariantCulture, DateTimeStyles.AdjustToUniversal, out var date) && DateTime.UtcNow - date < MaxAge)
                    {
                        LastRefresh = date;
                        var restOfFile = sr.ReadToEnd()
                            .Replace("\r\n", "\n") // normalize line endings
                            .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); // split into lines

                        var elementSets = Tle.ParseElements(restOfFile, true);

                        return elementSets.ToDictionary(elementSet => (int)elementSet.NoradNumber);
                    }
                }

            var tles = base.FetchNewTles();
            WriteOutNewTles(tles);

            return tles;
        }

        private void WriteOutNewTles(Dictionary<int, Tle> tles)
        {
            var sb = new StringBuilder();
            sb.AppendLine(DateTime.UtcNow.ToString("u"));
            foreach (var tle in tles)
            {
                sb.AppendLine(tle.Value.Name);
                sb.AppendLine(tle.Value.Line1);
                sb.AppendLine(tle.Value.Line2);
            }

            File.WriteAllText(_localFilename, sb.ToString(), Encoding.UTF8);
        }
    }
}