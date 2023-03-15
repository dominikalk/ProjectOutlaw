# Project Outlaw

This is a multiplayer social game developed at Cardiff University as part of a Group Project module.

## Contributors

- Dominik Alkhovik
- Tomos Jones
- Reuben Baker
- Saud Alharbi
- Jessica Davies
- Noura Alrashdi

## Git global setup

```
git config --global user.name "<Your Name>"
git config --global user.email "<Your Email>"
```

## Cloning the project

```
git clone https://github.com/dominikalk/ProjectOutlaw.git
cd ProjectOutlaw
```

This will only work if you have access to the repo.

## Let's try to create your first branch and push to the repo

Always checkout into master, pull the remote repo, and then checkout into a new branch so you always have the newest code available from other devs so there are fewer conflicts.

```
git checkout master
git pull origin master
git checkout -b <branch name>
# Write code
git add .
git commit -m '<commit message>'
git push origin <branch name>
# Create pull request and wait for it to be merged into master
```

The above flow is the one you should be using, commiting as much as possible. Below is a more detailed explanation.

### What does checkout mean?

Checkout means moving from one branch to another:

```
git checkout <branch name>
```

... would switch branch to the name specified.
Make sure all changes are staging (commited) if switching branch.

To create a new branch use the -b tag:

```
git checkout -b <branch name>
```

... would switch to a new branch of the name specified.

### Branch naming convention

We will preface the branch name with either **feature/** or **fix/** and then write the description of the branch as an non-capitalised non-spaced string
E.g. _feature/mapcolours_ or _fix/playerspeed_

### Tracking files and Staging changes

To track files run:

```
git add <file name>
```

... to track a specific file
or:

```
git add .
```

... to track all edited files

To stage changes run:

```
git commit -m '<commit message>'
```

### Pushing and pulling

To push to the remote repo:

```
git push origin <branch name>
```

To pull from the remote repo run:

```
git pull origin <branch name>
```

### Repository Permissions

It will not be possible to push straight into master as this could cause bugs to be merged easily. Instead you must create a pull request, fill out the description according to the template and wait for it to be peer assessed. This will mean we keep the code clean and make sure it stays up to standards.

### Extra Help

If you need extra help with git either ask the people in our group or there are plenty of tutorial videos online.

## Coding Standards

- The top of each file contains a brief description of its function. Please use the correct file for your tickets.
  Some of these descriptions show the structure of the dictionaries within the file; please follow these and naming convensions

- Please take care in naming variables and subroutines. There are other people working on the same repo. Please preface all subroutines with a tripple quote describing what it does including its parameters and what it returns.

- Please comment more as well, describing what more complex actions do, no need to go overboard though. Writing down your initials in the comments could help to find who did what too.

## Project Management

We are using Trello to manage tasks in the form of tickets. There are also columns which we can drag these into.
Tickets will be assigned to people individually so don't start working on one if someone else is working on it.
Tickets will be continuously added as it is always better to split larger problems into smaller parts. Each ticket will ideally have an intuitive name and a detailed descriptions stating where the code should be and what it should do.

- **Brainstorm** - Anyone can create tickets in here of ideas they have
- **Backlog** - Where we put tickets that we might implement if we have time but aren't totally necessary
- **On Hold** - Tickets that cannot be completed yet because they are waiting for others to be completed
- **Main Tickets** - Here are the important tickets we need to complete for our game to function
- **In Progress** - Tickets that are currently being worked on
- **In Review** - When you create a PR on your task
- **Complete** - Sweet, sweet victory
- **Milestones** - These aren't to be touched, they are there to make it easier to create tickets which will be completed

To keep the tickets more organised the type of ticket will be grouped by colour:

- **Purple** - Dev based tickets
- **Green** - Story based tickets
- **Blue** - Presentation based tickets

Communication is key so make sure to message people for help or information as much as you need, and the same goes for helping others.

## Let's try this out then shall we?

At the top of the file is a list of contributors. But your name isn't there yet. That is because you need to add yourself. Pull master, checkout to a new branch called _feature/--yourname--readme_, make the changes, commit them, push to the remote repo and create a pull request. This may seem like a long process but you will get used to them and it will make dev in general so much quicker.

Remember:

```
git checkout master
git pull origin master
git checkout -b <branch name>
# Write code
git add .
git commit -m '<commit message>'
git push origin <branch name>
# Create pull request and wait for it to be merged into master
```
