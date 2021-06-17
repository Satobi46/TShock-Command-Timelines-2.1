using System;
using System.Collections.Generic;
using Terraria;
using TerrariaApi.Server;
using TShockAPI;

namespace CommandTimelines
{
	[ApiVersion(2, 1)]
	public partial class CommandTimelines : TerrariaPlugin
	{
		public override string Author => "GameRoom & Enerdy";

		public override string Description => "Reads command macros.";

		public override string Name => "Command Timelines";

		public List<Timeline> Running { get; }

		public override Version Version => System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;

		public CommandTimelines(Main game) : base(game)
		{
			Running = new List<Timeline>();
		}

		protected override async void Dispose(bool disposing)
		{
			if (disposing)
			{
				foreach (Timeline tl in Running)
					await tl.Stop();
			}
		}

		public override void Initialize()
		{
			Commands.ChatCommands.Add(new Command(Permissions.Show, doTimeline, "timeline", "tl")
			{
				HelpText = String.Format("Commands: {0}timeline start <file> [params...], {0}timeline stop <file>, {0}timeline show",
				Commands.Specifier)
			});
		}
	}
}