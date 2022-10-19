// See https://aka.ms/new-console-template for more information
// See https://aka.ms/new-console-template for more information

using LibGit2Sharp;
using RedirectFilesUtilities;
using static RedirectFilesUtilities.GhostRepository;
using static System.Net.WebRequestMethods;

namespace RedirectFilesUtilities
{
    public class Program
    {
        private const string defaultRepoUrl = "https://github.com/jishimwe/PlayMusic.git";
		private const string defaultDestPath = @"C:\Users\ishim\Documents\GhostRepo\PlayMusic";
        private const string defaultFileRelativePath = @"app/src/main/AndroidManifest.xml";
        private const string defaultFileRealPath = @"C:\Users\ishim\Documents\GhostRepo\PlayMusic\app\src\main\AndroidManifest.xml";

		static void Main(string[] args)
        {
            if (args.Length <= 2)
            {
                Console.WriteLine("No arguments. Launching tests instances");
                DefaultExec();
                return;
            }

            string repoUrl = "", destPath = "", redirUrl = "", redirPath = "";

            for (int i = 0; i < args.Length; i += 2)
            {
                string s = args[i];
                switch (s)
                {
                    case "-s":
                        repoUrl = args[i + 1];
                        break;

                    case "-d":
                        destPath = args[i + 1];
                        break;

                    case "-r":
                        redirUrl = args[i + 1];
                        break;

                    case "-t":
                        redirPath = args[i + 1];
                        break;

                    default:
                        Console.WriteLine(args[i] + " is not a valid argument");
                        PrintUsage();
                        Console.WriteLine("Terminating process");
                        break;
                }
            }

            if (repoUrl == "" || destPath == "" || redirPath == "" || redirUrl == "")
            {
                Console.WriteLine("Invalid arguments");
                PrintUsage();
				Console.WriteLine("Terminating process");
                return;
			}

            GhostRepository ghostRepository = new GhostRepository(repoUrl, destPath);
            GhostRepository redirRepository = new GhostRepository(redirUrl, redirPath, true);

            // TODO: Ask the user what to do
            // open file from redir
            // commit changes
            // push changes
            // ask for token

            //GhostRepository ghostRepository = new GhostRepository(args[0], args[1]);
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

        private static void PrintUsage()
        {
            Console.WriteLine("Launch the program in a terminal with the following arguments:" +
				"\n -s <Real repository> : The url for the project repository" +
				"\n -d <Destination path>: The path where to put the cloned project repository" +
				"\n -r <Redir repository>: The url for the redir repository" +
				"\n -t <Redir path>      : The path where to put the cloned redir repository");
        }

        private static string OpenRedirFile()
        {
            Console.WriteLine("Enter the path of a redir file");
            string filepath = Console.ReadLine();
            if (filepath == null)
                return "no file";
            using (StreamReader sr = new StreamReader(filepath))
            {
                string read = sr.ReadToEnd();
                return read ?? "no data";
            }
        }
    }
}
