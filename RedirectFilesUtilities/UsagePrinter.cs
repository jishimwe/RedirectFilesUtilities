using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RedirectFilesUtilities
{
	public static class UsagePrinter
	{
		public static void PrintUsageOpenFile()
		{
			Console.WriteLine("Opening a file from a redirect file\n");
			Console.WriteLine("-open			: flag to launch the opening a file\n" +
			                  "\t-r <path>	: the path to a redirect file\n" +
			                  "\t-d <path>	: the path to the root of real project\n" +
			                  //"\t-e <mail>	: the git user email address\n" +
							  //"\t-t <path>	: flag to give a token with a path to a file containing the token\n" +
							  //"\t-u <user>	: the git user username\n" +
			                  "\t-b <branch>	: the name of the branch\n");
			Console.WriteLine();
		}

		public static void PrintUsageCommit()
		{
			Console.WriteLine("Commiting changes");
			Console.WriteLine("-commit			: flag to launch a commit\n" +
			                  "\t-f <path>	: path to the file to commit OR path to the directory to commit\n" +
			                  "\t-d <path>	: path to the root of real project\n" +
			                  "\t-m <message>	: a message for the commit\n" +
			                  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n");
			Console.WriteLine();
		}

		public static void PrintUsagePush()
		{
			Console.WriteLine("Pushing changes");
			Console.WriteLine("-push			: flag to launch a push\n" +
			                  "\t-d <path>	: path to the root of real project\n" +
			                  "\t-t <path>	: flag to give a token with a path to a file containing the token\n" +
			                  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n");
			Console.WriteLine();
		}

		public static void PrintUsageUpdate()
		{
			Console.WriteLine("Update repository");
			Console.WriteLine("-update			: flag to launch an update for the repo\n" + // TODO: will it only update the local files?
							  "\t-d <path>	: path to the root of real project\n" +
							  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n" +
							  "\t-t <path>	: flag to give a token with a path to a file containing the token\n");
			Console.WriteLine();
		}

		private static void PrintUsageToken()
		{
			Console.WriteLine("Giving a git token");
			Console.WriteLine("-t <path> : flag to give a token with a path to a file containing the token");
		}

		public static bool InvalidArguments(params string[] args)
		{
			bool res = false;
			foreach (string a in args)
			{
				res |= a == "";
				if (res) return res;
			}
			return res;
		}

		public static void PrintUsageMerge()
		{
			Console.WriteLine("Merge Options");
			Console.WriteLine("-merge <options>	: flag to indicate the merge strategy \n" +
			                  "\t\t0 : create a union of local and remote changes \n" +
							  "\t\t1 : accept local changes \n" +
			                  "\t\t2 : accept remote changes \n" +
							  "\t-d <path>	: path to the root of real project\n" +
							  "\t-u <user>	: the git user username\n" +
			                  "\t-e <mail>	: the git user email address\n");
			Console.WriteLine();
		}

		public static void PrintUsageForcePush()
		{
			Console.WriteLine("Force a push of a commit");
			Console.WriteLine("-forcePush			: flag to launch a force push\n" +
			                  "\t-d <path>	: path to the root of real project\n" +
			                  "\t-t <path>	: flag to give a token with a path to a file containing the token\n" +
							  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n");
		}

		public static void OpenConflictResolutionTool()
		{
			Console.WriteLine("Opening a conflict resolution tool to resolve a conflict" +
			                  "-conflictTool			: flag to open a conflict resolution tool\n" +
			                  "\t-t <path>	: path to the conflict resolution tool\n" +
							  "\t-d <path>	: path to the root of real project\n");
		}

		public static void PrintUsageConflicts(ConflictCollection conflicts)
		{
			Console.WriteLine("Some Conflicts need resolution");
			Console.WriteLine("[" + string.Join(", -- " , conflicts) + "]\t"  + conflicts.Names);
		}
	}
}
