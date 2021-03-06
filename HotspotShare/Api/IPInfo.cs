﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;

namespace HotspotShare.Api
{
	/// <summary>
	/// This class allows you to retrieve the IP Address and Host Name for a specific machine on the local network when you only know it's MAC Address.
	/// </summary>
	public class IPInfo
	{
		public IPInfo(string macAddress, string ipAddress)
		{
			this.MacAddress = macAddress;
			this.IPAddress = ipAddress;
		}

		public string MacAddress { get; private set; }
		public string IPAddress { get; private set; }
		private string hostName = null;
		public string HostName
		{
			get
			{
				if (hostName == null)
				{
					if (this.IPAddress != null)
						try
						{
							hostName = Dns.GetHostEntry(this.IPAddress).HostName;
						}
						catch (Exception)
						{
						}
				}
				return hostName;
			}
		}

		#region "Static Methods"

		/// <summary>
		/// Retrieves the IPInfo for the machine on the local network with the specified MAC Address.
		/// </summary>
		/// <param name="macAddress">The MAC Address of the IPInfo to retrieve.</param>
		/// <returns></returns>
		public static IPInfo GetIPInfo(string macAddress)
		{
			var ipinfo = (from ip in IPInfo.GetIPInfo()
						  where ip.MacAddress.ToLowerInvariant() == macAddress.ToLowerInvariant()
						  select ip).FirstOrDefault();

			return ipinfo;
		}

		/// <summary>
		/// Retrieves the IPInfo for All machines on the local network.
		/// </summary>
		/// <returns></returns>
		public static List<IPInfo> GetIPInfo()
		{
			try
			{
				var list = new List<IPInfo>();

				var splitted = GetARPResult().Split(new char[] { '\n', '\r' });
				foreach (var arp in splitted)
				{
					// Parse out all the MAC / IP Address combinations
					if (!string.IsNullOrEmpty(arp))
					{
						var pieces = (from piece in arp.Split(new char[] { ' ', '\t' })
									  where !string.IsNullOrEmpty(piece)
									  select piece).ToArray();
						if (pieces.Length == 3)
						{
							list.Add(new IPInfo(pieces[1], pieces[0]));
						}
					}
				}

				// Return list of IPInfo objects containing MAC / IP Address combinations
				return list;
			}
			catch (Exception ex)
			{
				throw new Exception("IPInfo: Error Parsing 'arp -a' results", ex);
			}
		}

		/// <summary>
		/// This runs the "arp" utility in Windows to retrieve all the MAC / IP Address entries.
		/// </summary>
		/// <returns></returns>
		private static string GetARPResult()
		{
			Process p = null;
			string output = string.Empty;

			try
			{
				p = Process.Start(new ProcessStartInfo("arp", "-a")
				{
					CreateNoWindow = true,
					UseShellExecute = false,
					RedirectStandardOutput = true
				});

				output = p.StandardOutput.ReadToEnd();

				p.Close();
			}
			catch (Exception ex)
			{
				throw new Exception("IPInfo: Error Retrieving 'arp -a' Results", ex);
			}
			finally
			{
				if (p != null)
				{
					p.Close();
				}
			}

			return output;
		}

		#endregion
	}
}
