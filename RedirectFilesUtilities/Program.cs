// See https://aka.ms/new-console-template for more information
// See https://aka.ms/new-console-template for more information

using System.Collections;
using System.Runtime.CompilerServices;
using LibGit2Sharp;
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

		static void Main(string[] args)
		{
			//HashSet<string> localFiles = new HashSet<string>();
			Dictionary<string, string> localFiles = new Dictionary<string, string>();

			if (args.Length <= 2)
            {
                Console.WriteLine("No arguments. Launching tests instances");
                DefaultExec();
                return;
            }

            string repoUrl = "", destPath = "", redirUrl = "", redirPath = "", tokenPath = "";
            string actionType = args[0];
            switch (actionType)
            {
                case "-o": //opening a file
	                if (!OpenFileFromRedirect(args))
	                {
                        Console.WriteLine("Failed to open the file");
	                }
	                break;

                case "-c": //Commiting changes
	                if (!CommitFiles(args))
	                {
                        Console.WriteLine("Failed to commit");
	                }
	                break;

                case "-p": //Pushing commit
	                if (!PushCommit(args))
	                {
                        Console.WriteLine("Failed to push");
	                }
	                break;
            }

            GhostRepository ghostRepository = new GhostRepository(repoUrl, destPath);
            GhostRepository redirRepository = new GhostRepository(redirUrl, redirPath, true);

            // TODO: Ask the user what to do
            // open file from redir
            // commit changes
            // push changes
            // ask for token
		}

		private static bool PushCommit(string[] args)
		{
			//throw new NotImplementedException();
            string dir = "", username = "", tokenFile = "";
			for (int i = 1; i < args.Length; i += 2)
			{
				string s = args[i];
				switch (s)
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

			if (dir == "" || tokenFile == "")
			{
				PrintUsagePush();
				return false;
			}

			Repository repository = new Repository(dir);

			var opt = new PushOptions
			{
				CredentialsProvider = (_url, _user, _cred) =>
					new UsernamePasswordCredentials { Username = username, Password = GetToken(tokenFile) }
			};

			// TODO: Careful with hardcoded elements
			Remote remote = repository.Network.Remotes["origin"];
			repository.Network.Push(remote, @"refs/heads/master", opt);

			return true;
		}

		private static bool CommitFiles(string[] args)
		{
			string filepath, rootPath, username;

			for (int i = 1; i < args.Length; i += 2)
			{
				string s = args[i];
				switch (s)
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
					default:
						PrintUsageCommit();
						return false;
				}
			}

			return true;
		}

		private static bool OpenFileFromRedirect(string[] args)
		{
			string redirPath, rootPath;
            throw new NotImplementedException();
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
							  "-u <user>    : the git user username");
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

        private static string GetToken(string filepath)
        {
	        if (filepath == null || !File.Exists(filepath))
		        return "";
	        string s = File.ReadAllText(filepath);
	        return s;
        }

		private static string ReadRedirFile(string? filepath = null)
        {
	        if (filepath == null)
	        {
		        Console.WriteLine("Enter the path of a redir file");
		        filepath = Console.ReadLine();
			}
            
            if (filepath == null)
                return "no file";
            using (StreamReader sr = new StreamReader(filepath))
            {
                string read = sr.ReadToEnd();
                return read ?? "no data";
            }
        }

        private static void OpenFile(string filepath)
        {
	        if (!File.Exists(filepath))
	        {
                Console.WriteLine("The file at " + filepath + " doesn't exist");
		        return;
	        }
            Console.WriteLine("Opening file . . . " + filepath);
	        System.Diagnostics.Process.Start(filepath);
		}

        private static void DefaultExec()
        {

	        Console.WriteLine("Testing with arguments :");
	        Console.WriteLine("Repository: " + defaultRepoUrl);
	        Console.WriteLine("Destination Folder: " + defaultDestPath);

	        GhostRepository ghostRepository = new GhostRepository(defaultRepoUrl, defaultDestPath);
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
