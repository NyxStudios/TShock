using Newtonsoft.Json;
using System;
using System.IO;

namespace TShockAPI.Configuration
{
	/// <summary>
	/// Implements <see cref="IConfigFile{TSettings}"/> to provide a generic config file containing some settings
	/// </summary>
	/// <typeparam name="TSettings"></typeparam>
	public class ConfigFile<TSettings> : IConfigFile<TSettings> where TSettings : new()
	{
		/// <summary>
		/// Settings managed by this config file
		/// </summary>
		public virtual TSettings Settings { get; set; } = new TSettings();

		/// <summary>
		/// Action invoked when the config file is read
		/// </summary>
		public static Action<ConfigFile<TSettings>> OnConfigRead;

		/// <summary>
		/// Reads json-formatted settings from a given path.
		/// If the given path does not exist <paramref name="incompleteSettings"/> is set to <see langword="true"/>
		/// and a default <typeparamref name="TSettings"/> object is returned
		/// </summary>
		/// <param name="path">The path to the file containing the settings</param>
		/// <param name="incompleteSettings">
		/// Whether the config object has any new fields in it, meaning that the config file should be
		/// overwritten.
		/// </param>
		/// <returns>Settings object</returns>
		public virtual TSettings Read(string path, out bool incompleteSettings)
		{
			if (!File.Exists(path))
			{
				incompleteSettings = true;
				return default;
			}
			using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				return Read(fs, out incompleteSettings);
			}
		}

		/// <summary>
		/// Reads json-formatted settings from a given stream
		/// </summary>
		/// <param name="stream">stream</param>
		/// <param name="incompleteSettings">
		/// Whether the config object has any new fields in it, meaning that the config file has to be
		/// overwritten.
		/// </param>
		/// <returns>Settings object</returns>
		public virtual TSettings Read(Stream stream, out bool incompleteSettings)
		{
			using (var sr = new StreamReader(stream))
			{
				return ConvertJson(sr.ReadToEnd(), out incompleteSettings);
			}
		}

		/// <summary>
		/// Converts a json-formatted string into the settings object used by this configuration. Invokes the <see cref="OnConfigRead"/> hook
		/// </summary>
		/// <param name="json">Json string to parse</param>
		/// <param name="incompleteSettings">Whether or not the json string contained an incomplete set of settings</param>
		/// <returns>Settings object</returns>
		public virtual TSettings ConvertJson(string json, out bool incompleteSettings)
		{
			var settings = FileTools.LoadConfigAndCheckForChanges<TSettings>(json, out incompleteSettings);

			Settings = settings;
			OnConfigRead?.Invoke(this);

			return settings;
		}

		/// <summary>
		/// Writes the configuration to a given path
		/// </summary>
		/// <param name="path">The file path the configuration file will be written to</param>
		public virtual void Write(string path)
		{
			using (var fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.Write))
			{
				Write(fs);
			}
		}

		/// <summary>
		/// Writes the configuration to a stream
		/// </summary>
		/// <param name="stream">stream</param>
		public virtual void Write(Stream stream)
		{
			var str = JsonConvert.SerializeObject(this, Formatting.Indented);
			using (var sw = new StreamWriter(stream))
			{
				sw.Write(str);
			}
		}
	}
}
