using System;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace ExamplePlugin
{
	[ApiVersion(2, 1)]
	public class ExamplePlugin : TerrariaPlugin
	{
		/// <summary>
		/// Gets the author(s) of this plugin
		/// </summary>
		public override string Author => "Pryaxis & TShock Contributors";

		/// <summary>
		/// Gets the description of this plugin.
		/// A short, one lined description that tells people what your plugin does.
		/// </summary>
		public override string Description => "An example plugin";

		/// <summary>
		/// Gets the name of this plugin.
		/// </summary>
		public override string Name => "Example Plugin";

		/// <summary>
		/// Gets the version of this plugin.
		/// </summary>
		public override Version Version => new Version(1, 0, 0, 0);

		/// <summary>
		/// Initializes a new instance of the TestPlugin class.
		/// This is where you set the plugin's order and perform other constructor logic
		/// </summary>
		public ExamplePlugin(Main game) : base(game) { }

		/// <summary>
		/// Handles plugin initialization.
		/// Fired when the server is started and the plugin is being loaded.
		/// You may register hooks, perform loading procedures etc here.
		/// </summary>
		public override void Initialize()
		{
			Console.WriteLine("Initializing plugin...");

			// All hooks provided by TSAPI are a part of the _ServerApi_ namespace.
			// This example will show you how to use the ServerChat hook which is
			// fired whenever a client sends a message to the server.
			// In order to register the hook you need to pass in the class that
			// is registering the hook and it's callback function (OnServerChat)
			// By passing a reference to the `OnServerChat` method you are able to
			// execute code whenever a message is sent to the server.
			ServerApi.Hooks.ServerChat.Register(this, OnServerChat);

			// This is an example of subscribing to TShock's TogglePvP event.
			// This event is a part of the `GetDataHandlers` class.
			// All events located within this class are _packet implementation_ hooks.
			// These hooks will come in handy when dealing with packets
			// because they provide the packet's full structure, saving you from
			// reading the packet data yourself.
			GetDataHandlers.TogglePvp += OnTogglePvp;
		}

		/// <summary>
		/// Will fire when unloading your plugin.
		/// You should deregister hooks and free all unmanaged resources here.
		/// </summary>
		protected override void DisposeUnmanaged()
		{
			Console.WriteLine("Removing hooks...");

			// Deregister hooks here
			ServerApi.Hooks.ServerChat.Deregister(this, OnServerChat);
			GetDataHandlers.TogglePvp -= OnTogglePvp;
		}

		// This is the ServerChat's callback function; this function is called
		// whenever the ServerChat hook is fired, which is upon receiving a message
		// from the client.
		// This example acts as a debug and outputs the message to the console.
		void OnServerChat(ServerChatEventArgs args)
		{
			Console.WriteLine($"DEBUG: {args.Text}");
		}

		// This is the TogglePvp handler. This function is called whenever the server
		// receives packet #30 (TogglePvP)
		void OnTogglePvp(object sender, GetDataHandlers.TogglePvpEventArgs args)
		{
			Console.WriteLine("{0} has just {1} their Pvp Status",
				TShock.Players[args.PlayerId].Name,
				args.Pvp ? "enabled" : "disabled");
		}
	}
}
