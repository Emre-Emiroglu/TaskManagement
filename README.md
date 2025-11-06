<img src="icon.jpg" width="512" height="512" alt="TaskManagement Logo">

# TaskManagement
TaskManagement is a ScriptableObject-based task management tool designed to run entirely inside the Unity Editor. TaskManagement helps developers plan projects, organize tasks, and track progress directly within Unity. 100% editor-side, with no runtime dependencies. The package stores projects and tasks as ScriptableObject assets under Assets/TaskManagement/Projects/.

## Features
* Create / rename / delete projects from the Editor UI.
* Create / edit / delete tasks (ScriptableObject assets) grouped under projects.
* Task fields: Title, Description, Status, Priority, Category, Assignee, Due Date.
* Remaining time calculation for tasks (human readable).
* Sort tasks by Assignee, Priority, Status, Due date (Remaining time).
* Small, focused, modular controllers (`ProjectController`, `TaskController`) and Editor window (`TaskManagementEditor`).
* Unit tests (EditMode) included for core behaviors (create/delete project, create/edit/delete tasks, sorting).

## Getting Started
Install via UPM with git URL

`https://github.com/Emre-Emiroglu/TaskManagement.git`

Clone the repository
```bash
git clone https://github.com/Emre-Emiroglu/TaskManagement.git
```
This project is developed using Unity version 6000.2.6f2.

## Usage
* After importing the package / cloning, open Unity and let scripts compile.
* Open the editor window from the menu: Tools → Task Management Editor
* If there are no projects yet, you'll see a creation prompt. Enter a project name and click Create new project.
* With a project selected:
  * Use the Add New Task section to create tasks.
  * Click Edit on a task to modify fields.
  * Click Delete to remove a task.
  * Use the sort toolbar to change ordering (Assignee, Priority, Status, Remaining time).

## Acknowledgments
Special thanks to the Unity community for their invaluable resources and tools.

For more information, visit the GitHub repository.
