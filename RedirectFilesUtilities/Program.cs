// See https://aka.ms/new-console-template for more information
// See https://aka.ms/new-console-template for more information

using LibGit2Sharp;
using RedirectFilesUtilities;
using static RedirectFilesUtilities.GhostRepository;

namespace RedirectFilesUtilities
{
    public class Program
    {
        static void Main(string[] args)
        {
            GhostRepository ghostRepository = new GhostRepository(args[0], args[1]);
        }
    }
}
