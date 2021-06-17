using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TShockAPI;

namespace CommandTimelines
{
	public partial class CommandTimelines
	{
		void doTimeline(CommandArgs e)
		{
			string error = String.Format(
				"Invalid syntax! Proper syntax: {0}timeline start <file> [params...] OR {0}timeline stop <file> OR {0}timeline show",
				Commands.Specifier);

			if (e.Parameters.Count == 0)
			{
				e.Player.SendErrorMessage(error);
				return;
			}

			string cmd = e.Parameters[0].ToLowerInvariant();
			e.Parameters.RemoveAt(0);

			switch (cmd)
			{
				case "run":
				case "start":
					doStart(e);
					return;

				case "stop":
					doStop(e);
					return;

				case "list":
				case "show":
					doShow(e);
					return;

				case "lint":
				case "verify":
					doLint(e);
					return;

				default:
					e.Player.SendErrorMessage(error);
					return;
			}
		}

		async void doLint(CommandArgs e)
		{
			if (e.Parameters.Count == 0)
			{
				e.Player.SendErrorMessage($"Invalid syntax! Proper syntax: {Commands.Specifier}timeline lint <file>");
				return;
			}

			string path = e.Parameters[0];
			if (!Permissions.CanUseTimeline(e.Player, path))
			{
				e.Player.SendErrorMessage("You don't have access to this timeline.");
				return;
			}

			string name = Path.GetFileNameWithoutExtension(path);
			string fullPath = Path.Combine(TShock.SavePath, path.EndsWith(".txt") ? path : $"{path}.txt");
			if (!File.Exists(fullPath))
			{
				e.Player.SendErrorMessage($"'{name}' doesn't exist.");
				return;
			}

			string data;
			try
			{
				data = File.ReadAllText(fullPath);
			}
			catch (Exception ex)
			{
				e.Player.SendErrorMessage($"Timeline loading failed: {ex.Message}");
				return;
			}

			e.Parameters.RemoveAt(0);
			Timeline timeline = new Timeline(Path.GetFileNameWithoutExtension(path), data);

			try
			{
				// Consider working on an actual lint method that logs all errors found instead of using
				// the "return-on-first-exception" behaviour from doStart
				e.Player.SendInfoMessage("Checking timeline for errors...");
				await Task.Run(() => timeline.Process(timeline.Raw));
				e.Player.SendSuccessMessage("No errors detected.");

			}
			catch (EmptyTimelineException)
			{
				e.Player.SendErrorMessage($"'{name}' must contain at least one command.");
			}
			catch (MissingParameterException ex)
			{
				e.Player.SendInfoMessage($"'{name}' syntax: {Commands.Specifier}timeline start {name} {ex.Message}");
			}
			catch (CannotRunException ex)
			{
				e.Player.SendErrorMessage(
					$"Error processing timeline '{name}' at line {ex.Line}: Cannot run {Commands.Specifier}{ex.Command} as the server.");
			}
			catch (TimelineException ex)
			{
				e.Player.SendErrorMessage($"Error processing timeline '{name}' at line {ex.Line}: {ex.Message}");
			}
		}

		void doShow(CommandArgs e)
		{
			string running = String.Join(", ", Running.Select(t => t.Name));
			if (String.IsNullOrEmpty(running))
				e.Player.SendInfoMessage("No timelines are currently running.");
			else
				e.Player.SendInfoMessage($"Currently running timelines: {running}.");
		}

		async void doStart(CommandArgs e)
		{
			if (e.Parameters.Count == 0)
			{
				e.Player.SendErrorMessage($"Invalid syntax! Proper syntax: {Commands.Specifier}timeline start <file> [params...]");
				return;
			}

			string filePath = e.Parameters[0];
			if (!e.Player.CanUseTimeline(filePath))
			{
				e.Player.SendErrorMessage("You don't have access to this timeline.");
				return;
			}

			string path = Path.Combine(TShock.SavePath, filePath.EndsWith(".txt") ? filePath : $"{filePath}.txt");
			if (!File.Exists(path))
			{
				e.Player.SendErrorMessage($"'{filePath}' doesn't exist.");
				return;
			}

			string name = Path.GetFileNameWithoutExtension(filePath);
			if (Running.Exists(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
			{
				e.Player.SendErrorMessage($"'{name}' is already running.");
				return;
			}

			string data;
			try
			{
				data = File.ReadAllText(path);
			}
			catch (Exception ex)
			{
				e.Player.SendErrorMessage($"Timeline loading failed: {ex.Message}");
				return;
			}

			// Remove the file name
			e.Parameters.RemoveAt(0);
			Timeline timeline = new Timeline(name, data, e.Parameters);

			try
			{
				// Process the timeline and handle all exceptions
				e.Player.SendInfoMessage("Processing timeline...");
				await timeline.Start();

				timeline.Finished += (o, a) => Running.Remove(timeline);
				Running.Add(timeline);

				e.Player.SendSuccessMessage($"{name} started.");
			}
			catch (EmptyTimelineException)
			{
				e.Player.SendErrorMessage($"'{name}' must contain at least one command.");
			}
			catch (MissingParameterException ex)
			{
				e.Player.SendInfoMessage($"'{name}' syntax: {Commands.Specifier}timeline start {name} {ex.Message}");
			}
			catch (CannotRunException ex)
			{
				e.Player.SendErrorMessage(
					$"Error processing timeline '{name}' at line {ex.Line}: Cannot run {Commands.Specifier}{ex.Command} as the server.");
			}
			catch (TimelineException ex)
			{
				e.Player.SendErrorMessage($"Error processing timeline '{name}' at line {ex.Line}: {ex.Message}");
			}
		}

		async void doStop(CommandArgs e)
		{
			if (e.Parameters.Count == 0)
			{
				e.Player.SendErrorMessage($"Invalid syntax! Proper syntax: {Commands.Specifier}timeline stop <name>");
				return;
			}

			string name = e.Parameters[0];
			if (!Running.Exists(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
				e.Player.SendErrorMessage($"'{name}' isn't running.");
			else
			{
				if (!e.Player.CanUseTimeline(Path.Combine(TShock.SavePath, name.EndsWith(".txt") ? name : $"{name}.txt")))
				{
					e.Player.SendErrorMessage("You don't have access to this timeline.");
					return;
				}

				Timeline timeline = Running.Find(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
				e.Player.SendInfoMessage($"Stopping {timeline.Name}...");
				await timeline.Stop();
				e.Player.SendSuccessMessage($"{timeline.Name} stopped.");
			}
		}
	}
}
