// See https://aka.ms/new-console-template for more information
// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Security;
using LibGit2Sharp;
using LibGit2Sharp.Handlers;
using RedirectFilesUtilities;
using static RedirectFilesUtilities.GhostRepository;
using static System.Net.WebRequestMethods;
using File = System.IO.File;

namespace RedirectFilesUtilities
{
	public class Program
	{
		private const string defaultRepoUrl = "https://github.com/jishimwe/PlayMusic.git";
		private const string defaultDestPath = @"C:\Users\test\Documents\GhostRepo\PlayMusic";
		private const string defaultFileRelativePath = @"app/src/main/AndroidManifest.xml";
		private const string defaultFileRealPath = @"C:\Users\test\Documents\GhostRepo\PlayMusic\app\src\main\AndroidManifest.xml";

		private const string defaultTxtReader = @"C:\Program Files\Sublime Text 3\sublime_text.exe";

		public static void Main(string[] args)
		{
			//HashSet<string> localFiles = new HashSet<string>();
			//Dictionary<string, string> localFiles = new Dictionary<string, string>();

			if (args.Length <= 2)
			{
				Console.WriteLine("No arguments. Launching tests instances");
				DefaultExec();
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
					if (!CommitFile(args))
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

		private static bool PushCommit(string[] args)
		{
			string dir = "", username = "", tokenFile = "";
			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-d":
						dir = args[i + 1];
						break;
					case "-t":
						tokenFile = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					default:
						PrintUsagePush();
						return false;
				}
			}

			if (InvalidArguments(dir, username, tokenFile))
			{
				PrintUsagePush();
				return false;
			}

			Repository repository = new(dir);

			var opt = new PushOptions
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = username, Password = GetToken(tokenFile) }
			};

			// TODO: Careful with hardcoded values
			Remote remote = repository.Network.Remotes["origin"];
			repository.Network.Push(remote, @"refs/heads/master", opt);

			return true;
		}

		public static bool CommitFileOrDirectory(string[] args)
		{
			string filepath = "", rootPath = "", username = "", message = "", email = "";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-f":
						filepath = args[i + 1];
						break;
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						email = args[i + 1];
						break;
					case "-m":
						message = args[i + 1];
						break;
					default:
						PrintUsageCommit();
						return false;
				}
			}

			if (InvalidArguments(filepath, rootPath, username))
			{
				PrintUsageCommit();
				return false;
			}

			if (Directory.Exists(filepath))
			{
				CommitDirectory(filepath, rootPath, username, email, message);
			}			else
			{
				CommitFile(filepath, rootPath, username, email, message);
			}			
			return false;
		}

		private static bool CommitFile(string[] args)
		{
			string filepath = "", rootPath = "", username ="", message = "", email = "";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-f":
						filepath = args[i + 1];
						break;
					case "-d":
						rootPath = args[i + 1];
						break;
					case "-u":
						username = args[i + 1];
						break;
					case "-e":
						email = args[i + 1];
						break;
					case "-m":
						message = args[i + 1];
						break;
					default:
						PrintUsageCommit();
						return false;
				}
			}

			if (InvalidArguments(filepath, rootPath, username))
			{
				PrintUsageCommit();
				return false;
			}

			Repository repository = new(rootPath);
			repository.Index.Add(filepath);
			repository.Index.Write();

			RemoveFromStaging(repository); // Removing unavailable files from staging
			Commands.Stage(repository, filepath);

			Signature author = new(username, email, DateTimeOffset.Now);
			Commit commit = repository.Commit(message, author, author);

			return true;
		}

		// TODO: Implement recursive directory commit
		private static bool CommitDirectory(string dirpath, string rootPath, string username, string email, string message)
		{
			Repository repository = new Repository(rootPath);
			RemoveFromStaging(repository);
			throw new NotImplementedException();
		}

		private static bool CommitDirectoryHelper(string dirpath, Repository repository)
		{
			DirectoryInfo di = new DirectoryInfo(dirpath);
			FileInfo[] files = null;

			try
			{
				files = di.GetFiles();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			foreach (FileInfo fi in files)
			{
				// String operation
				repository.Index.Add(fi.Name);
			}

		}

		private static bool CommitFile(string filepath, string rootPath, string username, string email, string message)
		{
			Repository repository = new(rootPath);
			repository.Index.Add(filepath);
			repository.Index.Write();

			RemoveFromStaging(repository);
			Commands.Stage(repository, filepath);

			Signature author = new(username, email, DateTimeOffset.Now);
			Commit commit = repository.Commit(message, author, author);

			return true;
		}

		private static bool PushDirectory(string[] args)
		{
			throw new NotImplementedException();
		}

		private static bool FetchRemote(string filepath, string repoPath)
		{
			string msg = "update file" + filepath;
			using Repository repository = new Repository(repoPath);
			Remote remote = repository.Network.Remotes["origin"];
			var refSpecs = remote.FetchRefSpecs.Select(x => x.Specification);
			Commands.Fetch(repository, remote.Name, refSpecs, null, msg);

			CheckoutOptions co = new CheckoutOptions();
			co.CheckoutModifiers = CheckoutModifiers.Force;
			repository.CheckoutPaths("master", new List<string>() {filepath}, co);

			string t = GetToken(@"C:\Users\test\Documents\TFEToken");

			PullOptions po = new PullOptions();
			po.FetchOptions = new FetchOptions();
			po.FetchOptions.CredentialsProvider = new CredentialsHandler(
				(_url, _username, _types) =>
					new UsernamePasswordCredentials()
					{
						Username = "jishimwe",
						Password = t
					});

			Signature signature = new Signature(new Identity("jishimwe", "jeanpaulishimwe@gmail.com"), DateTimeOffset.Now);

			Commands.Pull(repository, signature, po);

			return true;
		}

		private static bool OpenFileFromRedirect(string[] args)
		{
			string redirPath = "", rootPath = "";

			for (int i = 1; i < args.Length; i += 2)
			{
				switch (args[i])
				{
					case "-r":
						redirPath = args[i + 1];
						break;
					case "-d":
						rootPath = args[i + 1];
						break;
					default:
						PrintUsageOpenFile();
						return false;
				}
			}
			
			if (InvalidArguments(redirPath, rootPath))
			{
				PrintUsageOpenFile();
				return false;
			}

			Repository repository = new(rootPath);
			using StreamReader sr = new(redirPath);
			try
			{
				string? path = sr.ReadLine();
				if (path == null) return false;
				string realPath = Path.Combine(rootPath, path);
				if (File.Exists(realPath))
				{
					FetchRemote(path, rootPath);
					OpenFile(realPath);
					return true;
				}

				List<string> ls = new() { path };
				// TODO: Remove hardcoded "master" value
				repository.CheckoutPaths("master", ls, new CheckoutOptions());
				
				OpenFile(realPath);
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			return true;
		}

		private static bool InvalidArguments(params string[] args)
		{
			bool res = false;
			foreach(string a in args)
			{
				res |= a == "";
				if (res) return res;
			}
			return res;
		}

		private static void PrintUsageOpenFile()
		{
			Console.WriteLine("Opening a file from a redirect file");
			Console.WriteLine("-o         : flag to launch the opening a file" +
							  "-r <path>  : the path to a redirect file" +
							  "-d <path>  : the path to the root of real project");
			Console.WriteLine();
		}

		private static void PrintUsageCommit()
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

		private static void PrintUsagePush()
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

		private static string GetToken(string? filepath)
		{
			if (filepath == null || !File.Exists(filepath))
				return "";
			string s = File.ReadAllText(filepath);
			return s;
		}

		// Obsolete?
		private static string ReadRedirFile(string? filepath = null)
		{
			if (filepath == null)
			{
				Console.WriteLine("Enter the path of a redir file");
				filepath = Console.ReadLine();
			}
			
			if (filepath == null)
				return "no file";
			using StreamReader sr = new(filepath);
			string read = sr.ReadToEnd();
			return read ?? "no data";
		}

		private static void OpenFile(string filepath)
		{
			if (!File.Exists(filepath))
			{
				Console.WriteLine("The file at " + filepath + " doesn't exist");
				return;
			}
			Console.WriteLine("Opening file . . . " + filepath);

			ProcessStartInfo psi = new ProcessStartInfo()
			{
				FileName = filepath,
				UseShellExecute = true
			};
			Process.Start(psi);
		}

		private static void RemoveFromStaging(Repository repository)
		{
			foreach (var item in repository.RetrieveStatus(new StatusOptions()))
			{
				if (item.State == FileStatus.ModifiedInIndex)
					continue;

				Commands.Unstage(repository, item.FilePath);
				Console.WriteLine(item.FilePath + "\t" + item.State + "\t removed from staging");
			}
			Console.WriteLine();
		}

		private static void DefaultExec()
		{

			Console.WriteLine("Testing with arguments :");
			Console.WriteLine("Repository: " + defaultRepoUrl);
			Console.WriteLine("Destination Folder: " + defaultDestPath);

			GhostRepository ghostRepository = new(defaultRepoUrl, defaultDestPath);
			Console.WriteLine("Repository cloned");

			ghostRepository.CheckoutFile(defaultFileRelativePath);
			Console.WriteLine(defaultFileRelativePath + " checked out?");

			ghostRepository.CommitToRepository(defaultFileRealPath, defaultFileRelativePath);
			ghostRepository.PushFile();
			Console.WriteLine(defaultFileRelativePath + " pushed out?");

			ghostRepository.PrintGitStatus();
		}
	}
}
