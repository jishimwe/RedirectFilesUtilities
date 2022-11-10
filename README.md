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

- Open File From Redirect: 
A function that will find the file indicated by a redirect file and download it from the remote repository. \
To use it, launch the program with the folowing arguments:
```
-open		: flag to launch the opening a file
-r <path>	: the path to a redirect file
-d <path>	: the path to the root of real project
-b <branch>	: the name of the branch
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