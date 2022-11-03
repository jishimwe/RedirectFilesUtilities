using LibGit2Sharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedirectFilesUtilities
{
	public static class UsagePrinter
	{
		public static void PrintUsageOpenFile()
		{
			Console.WriteLine("Opening a file from a redirect file");
			Console.WriteLine("-o         : flag to launch the opening a file" +
			                  "-r <path>  : the path to a redirect file" +
			                  "-d <path>  : the path to the root of real project");
			Console.WriteLine();
		}

		public static void PrintUsageCommit()
		{
			Console.WriteLine("Commiting changes");
			Console.WriteLine("-c               : flag to launch a commit" +
			                  "-f <path>        : path to the file to commit OR" +
			                  //"-pd <directory>  : path to the directory to commit" +
			                  "-d <path>        : path to the root of real project" +
			                  "-m <message>		: a message for the commit" +
			                  "-u <user>		: the git user username");
			Console.WriteLine();
		}

		public static void PrintUsagePush()
		{
			Console.WriteLine("Pushing changes");
			Console.WriteLine("-p           : flag to launch a push" +
			                  "-d <path>    : path to the root of real project" +
			                  "-t <path>    : flag to give a token with a path to a file containing the token" +
			                  "-u <user>    : the git user username");
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
			Console.WriteLine("-m <options>	: flag to indicate the merge strategy \n" +
			                  "0 : create a union of local and remote changes \n" +
							  "1 : accept local changes \n" +
			                  "2 : accept remote changes \n");
		}

		public static void PrintUsageConflicts(ConflictCollection conflicts)
		{
			Console.WriteLine("Some Conflicts need resolution");
			Console.WriteLine(conflicts.Names);
		}
	}
}
