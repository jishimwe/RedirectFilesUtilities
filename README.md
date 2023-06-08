# Remote Development and Compilation
___

**Author** : *Ishimwe Jean-Paul*

**Noma** : *6919-12-00*
___

## Redirect File Utilities


This project implements the necessary function for a Redirect Project.
A Redirect Project is composed of two repository :

- A repository containing a real project with real code
- A repository containint a *redirect* project with redirect files

Redirect files are files tha contains information on where to find the *real* file project (generaly on remote repository)


### Implemented functions

Here are the function already implemented and how to use them.

- Creating a configuration file:
A function that will let the user create a file that will contain the information regularly used when handling a *redirect* project. The file will be the same as the one created and needed by the Visual Studio extension, so it can be created here in a command line or within the extension with a graphical interface.
To use it, launch the program with the following arguments:
```
-config     : flag to launch the creation of a configuration file
-conf <path>: the path to the file that will contains 
              the configuration information for a redirect project
-d <path>	: the path to the root of real project
-dUrl <url> : url to the real project remote repository
-r <path>	: the path to the root of the redirect project
-rUrl <url> : url to the redirect project remote repository
-u <user>	: the git user username
-e <mail>	: the git user email address
-t <path>	: flag to give a token with a path to a file 
              containing the token
-b <branch> : the name of the branch
-rm <remote>: the name of the remote
-rf <refSpecs>: the name of the RefSpecs
-ut <path>  : path to the Redirect File Utilities executable
```

- Open File From Redirect: 
A function that will find the file indicated by a *redirect* file and download it from the remote repository. \
To use it, launch the program with the folowing arguments:
```
-open		: flag to launch the opening a file
-r <path>	: the path to a redirect file
-d <path>	: the path to the root of real project
-b <branch>	: the name of the branch
```

- Add a file to the repository:
This function will add a file to the staging area. The file will still need to be committed and pushed.
This function will also add a corresponding file to the *redirect* repository, this file will be commited and pushed
automatically to the *redirect* repository.
```
-add		: flag to launch the addition of a file to the repository
-d <path>	: the path to the root of real project
-f <path>	: path to the file to add
-r <path>	: the path to the root of the redirect project
-u <user>	: the git user username
-e <mail>	: the git user email address
-t <path>	: flag to give a token with a path to a file containing the token
```

- Commit File or Directory: 
A function that will commit the changes done to file \
To use it, launch the program with the folowing arguments:
```
-commit		: flag to launch a commit 
-f <path>	: path to the file Or directory to commit
-d <path>	: path to the root of real project
-m <message>	: a message for the commit
-u <user>	: the git user username
-e <mail>	: the git user email address
```

- Push Commit:
A function that will push the commits done by the previous \
To use it, launch the program with the folowing arguments:
```
-push		: flag to launch a push
-d <path>	: path to the root of real project
-u <user>	: the git user username
-e <mail>	: the git user email address
-t <path>	: flag to give a token with a path to a file containing the token
```

- Force Push Commit:
A function that will push the commits without any safeguards. Use only when you manually handled conflicts \
To use it, launch the program with the folowing arguments:
```
-forcePush	: flag to launch a **forced** push
-d <path>	: path to the root of real project
-u <user>	: the git user username
-e <mail>	: the git user email address
-t <path>	: flag to give a token with a path to a file containing the token
```

- Update repository:
A function that will update the local repository to include remote changes \
To use it, launch the program with the following arguments:
```
-update		: flag to launch an update for the repo
-d <path>	: path to the root of real project
-u <user>	: the git user username
-e <mail>	: the git user email address
-t <path>	: flag to give a token with a path to a file containing the token
```

- Merge conflicting commits:
```
-merge <options>: flag to indicate the merge strategy with the options being ->
	0 : create a union of local and remote changes
	1 : accept local changes
	2 : accept remote changes
	3 : <default> create a **merge formatted file**
-d <path>	: path to the root of real project
-u <user>	: the git user username
-e <mail>	: the git user email address
```

___
### Future functions


- [x] Commiting a directory
- [x] Pushing a directory
- [ ] Conflict solving
- [x] Merging
- [x] Add a file
- [ ] Remove a file