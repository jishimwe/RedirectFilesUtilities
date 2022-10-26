using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using static RedirectFilesUtilities.UsagePrinter;
using static RedirectFilesUtilities.GitUtilities;
using static System.Net.WebRequestMethods;

namespace RedirectFilesUtilities
{
	public class Program
	{
		public static void Main(string[] args)
		{
			if (args.Length <= 2)
			{
				Console.WriteLine("No arguments");
				PrintUsageOpenFile();
				PrintUsageCommit();
				PrintUsagePush();
				return;
			}

			string actionType = args[0];
			switch (actionType)
			{
				case "-o": //opening a file
					if (!OpenFileFromRedirect(args))
					{
						Console.WriteLine("Failed to open the file");
						Environment.Exit(-1);
					}
					break;

				case "-c": //Commiting changes
					if (!CommitFileOrDirectory(args))
					{
						Console.WriteLine("Failed to commit");
						Environment.Exit(-1);
					}
					break;

				case "-p": //Pushing commit
					if (!PushCommit(args))
					{
						Console.WriteLine("Failed to push");
						Environment.Exit(-1);
					}
					break;
				default:
					PrintUsageOpenFile();
					PrintUsageCommit();
					PrintUsagePush();
					Environment.Exit(-1);
					break;
			}
		}
	}
}
