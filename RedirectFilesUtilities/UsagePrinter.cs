using LibGit2Sharp;

namespace RedirectFilesUtilities
{
	public static class UsagePrinter
	{
		public static void PrintUsageOpenFile()
		{
			Console.WriteLine("Opening a file from a redirect file");
			Console.WriteLine("-open		: flag to launch the opening a file\n" +
			                  "\t-f <path>	: the path to a redirect file\n" +
			                  /*"\t-d <path>	: the path to the root of real project\n" +
			                  "\t-b <branch>	: the name of the branch\n" +*/
			                  "\t-conf		: path to the config file containing the git information for the redirect and real projects\n");
			Console.WriteLine();
		}

		public static void PrintUsageCommit()
		{
			Console.WriteLine("Commiting changes");
			Console.WriteLine("-commit		: flag to launch a commit\n" +
			                  "\t-f <path>	: path to the file to commit OR path to the directory to commit\n" +
			                  "\t-m <message>	: a message for the commit\n" +
							  /*"\t-d <path>	: path to the root of real project\n" +
			                  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n" +*/
							  "\t-conf		: path to the config file containing the git information for the redirect and real projects\n");
			Console.WriteLine();
		}

		public static void PrintUsagePush()
		{
			Console.WriteLine("Pushing changes");
			Console.WriteLine("-push	: flag to launch a push\n" +
			                  /*"\t-d <path>	: path to the root of real project\n" +
			                  "\t-t <path>	: flag to give a token with a path to a file containing the token\n" +
			                  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n" +*/
							  "\"-conf	: path to the config file containing the git information for the redirect and real projects\n");
			Console.WriteLine();
		}

		public static void PrintUsageUpdate()
		{
			Console.WriteLine("Update repository");
			Console.WriteLine("-update	: flag to launch an update for the repo\n" + // TODO: will it only update the local files?
							  /*"\t-d <path>	: path to the root of real project\n" +
							  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n" +
							  "\t-t <path>	: flag to give a token with a path to a file containing the token\n" +*/
							  "\t-conf	: path to the config file containing the git information for the redirect and real projects\n");
			Console.WriteLine();
		}

		private static void PrintUsageToken()
		{
			Console.WriteLine("Giving a git token");
			Console.WriteLine("-t <path> : flag to give a token with a path to a file containing the token");
		}

		public static void PrintUsageMerge()
		{
			Console.WriteLine("Merge Options");
			Console.WriteLine("-merge <options>	: flag to indicate the merge strategy \n" +
			                  "-mo : " +
			                  "\t\t0 : create a union of local and remote changes \n" +
							  "\t\t1 : accept local changes \n" +
			                  "\t\t2 : accept remote changes \n" +
							  /*"\t-d <path>	: path to the root of real project\n" +
							  "\t-u <user>	: the git user username\n" +
			                  "\t-e <mail>	: the git user email address\n" +*/
			                  "\t-conf : path to the config file containing the git information for the redirect and real projects\n");
			Console.WriteLine();
		}

		public static void PrintUsageForcePush()
		{
			Console.WriteLine("Force a push of a commit");
			Console.WriteLine("-forcePush	: flag to launch a force push\n" +
			                  /*"\t-d <path>	: path to the root of real project\n" +
			                  "\t-t <path>	: flag to give a token with a path to a file containing the token\n" +
							  "\t-u <user>	: the git user username\n" +
							  "\t-e <mail>	: the git user email address\n" +*/
			                  "\t-conf		: path to the config file containing the git information for the redirect and real projects\n");
			Console.WriteLine();

		}

		public static void PrintOpenConflictResolutionTool()
		{
			Console.WriteLine("Opening a conflict resolution tool to resolve a conflict" +
			                  "-conflictTool: flag to open a conflict resolution tool\n" +
			                  "\t-t <path>	: path to the conflict resolution tool\n" +
							  "\t-d <path>	: path to the root of real project\n");
			Console.WriteLine();
		}

		public static void PrintUsageAddFile()
		{
			Console.WriteLine("Add a file to the staging area of the repository (File still need to be committed and eventually pushed");
			Console.WriteLine("-add			: flag to add a file to the repository\n" +
							  "\t-f <path>	: (relative) path to the file to add\n" +
							  "\t-d <path>	: path to the root of real project\n" +
							  "\t-rd <path>	: path to the root of redirect project\n");
			Console.WriteLine();
		}

		public static void PrintUsageConflicts(ConflictCollection conflicts)
		{
			//Console.WriteLine("Some Conflicts need resolution");
			foreach (var con in conflicts)
			{
				Console.WriteLine("[" + string.Join(", -- ", con) + "]\t" + con.Ancestor.Path);
			}
		}

		public static void PrintUsageCompiler()
		{
			Console.Write("To launch this tool with a compiler from YAFL, You need the following arguments :\n" +
			              "-comp : flag to indicate that the compiler launched this tool\n" +
			              "-conf : path to the config file containing the git information for the redirect and real projects\n" +
			              "-file : path to the real file\n");
			Console.WriteLine();
		}

		public static void PrintUsageWriteConfigFile()
		{
			Console.WriteLine("-config     : flag to launch the creation of a configuration file\r\n" +
			                  "-conf <path>: the path to the file that will contains the configuration information for a redirect project\r\n" +
			                  "-d <path>\t: the path to the root of real project\r\n" +
			                  "-dUrl <url> : url to the real project remote repository\r\n" +
			                  "-r <path>\t: the path to the root of the redirect project\r\n" +
			                  "-rUrl <url> : url to the redirect project remote repository\r\n" +
			                  "-u <user>\t: the git user username\r\n" +
			                  "-e <mail>\t: the git user email address\r\n" +
			                  "-t <path>\t: flag to give a token with a path to a file containing the token\r\n" +
			                  "-b <branch> : the name of the branch\r\n" +
			                  "-rm <remote>: the name of the remote\r\n" +
			                  "-rf <refSpecs>: the name of the RefSpecs\r\n" +
			                  "-ut <path>  : path to the Redirect File Utilities executable");
		}
	}
}
