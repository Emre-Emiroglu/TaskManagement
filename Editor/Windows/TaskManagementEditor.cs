﻿using System.Collections.Generic;
using System.Linq;
using TaskManagement.Editor.Data;
using TaskManagement.Editor.Enums;
using TaskManagement.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Windows
{
    public sealed class TaskManagementEditor : EditorWindow
    {
        #region Constants
        private const string MenuItemName = "Tools/Open Task Management Editor";
        private const string TitleContent = "Task Management Editor";
        private const int MinXSize = 512;
        private const int MinYSize = 512;
        private const int MaxXSize = 1024;
        private const int MaxYSize = 1024;
        private const string ProjectDataFilter = "t:ProjectData";
        private const string NoProjectFoundMessage = "No projects found yet.";
        private const string NewProjectDefaultName = "NewProject";
        private const string CreateNewProjectNameLabel = "Project Name:";
        private const string CreateNewProjectButtonText = "Create new project.";
        private const string SelectProjectLabel = "Active Project:";
        private const string TaskManagementProjectsPath = "Assets/TaskManagement/Projects/";
        private const string NoTasksMessage = "No tasks in this project yet.";
        private const string TasksVerticalStyle = "box";
        private const string TaskIdLabel = "Id:";
        private const string TaskTitleLabel = "Title:";
        private const string TaskDescriptionLabel = "Description:";
        private const string TaskStatusLabel = "Status:";
        private const string TaskPriorityLabel = "Priority:";
        private const string TaskCategoryLabel = "Category:";
        private const string TaskAssigneeLabel = "Assignee:";
        private const string TaskCreatedDateLabel = "Created Date:";
        private const string TaskDueDateLabel = "Due Date:";
        private const string TaskIsArchivedLabel = "Is Archived:";
        private const string TaskEditButtonText = "Edit";
        private const string TaskDeleteButtonText = "Delete";
        private const string NewTaskLabel = "Add new task.";
        private const string CreateNewTaskButtonText = "Create new task.";
        private const string NewTaskTitleEmptyDisplayDialogTitle = "ERROR!";
        private const string NewTaskTitleEmptyDisplayDialogMessage = "Task title cannot be empty.";
        private const string NewTaskTitleEmptyDisplayDialogOk = "Okey";
        private const string DuplicateTaskDialogTitle = "Duplicate Task";
        private const string DuplicateTaskDialogMessage = "A task with this title already exists in this project.";
        private const string DuplicateTaskDialogOk = "OK";
        private const string TaskManagementTasksPath = "/Tasks";
        #endregion
        
        #region Fields
        private List<ProjectData> _projects = new();
        private int _selectedProjectIndex = -1;
        private Vector2 _scrollPos = Vector2.zero;
        private string _projectName = NewProjectDefaultName;
        private string _taskId = string.Empty;
        private string _taskTitle = string.Empty;
        private string _taskDescription = string.Empty;
        private TaskStatus _taskStatus = TaskStatus.ToDo;
        private TaskPriority _taskPriority = TaskPriority.Low;
        private string _taskCategory = string.Empty;
        private string _taskAssignee = string.Empty;
        private string _taskCreatedDate = string.Empty;
        private string _taskDueDate = string.Empty;
        private bool _taskIsArchived;
        #endregion
        
        #region Core
        [MenuItem(MenuItemName)]
        private static void ShowWindow()
        {
            TaskManagementEditor window = GetWindow<TaskManagementEditor>();
            
            window.titleContent = new GUIContent(TitleContent);
            window.minSize = new Vector2(MinXSize, MinYSize);
            window.maxSize = new Vector2(MaxXSize, MaxYSize);
            
            window.Show();
        }
        private void OnEnable() => LoadProjects();
        #endregion
        
        #region Executes
        private void OnGUI()
        {
            if (DrawProjectCreationPrompt())
                return;

            DrawProjectSelector();
            
            EditorGUILayout.Space(8);
            
            DrawTaskList();
            
            EditorGUILayout.Space(16);
            
            DrawNewTaskSection();
        }
        private void LoadProjects()
        {
            string[] guids = AssetDatabase.FindAssets(ProjectDataFilter);
            
            _projects = new List<ProjectData>();
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                ProjectData project = AssetDatabase.LoadAssetAtPath<ProjectData>(path);
                
                if (project != null)
                    _projects.Add(project);
            }
        }
        private bool DrawProjectCreationPrompt()
        {
            if (_projects.Count > 0)
                return false;
            
            EditorGUILayout.HelpBox(NoProjectFoundMessage, MessageType.Info);
            
            _projectName = EditorGUILayout.TextField(CreateNewProjectNameLabel, _projectName);

            if (GUILayout.Button(CreateNewProjectButtonText, GUILayout.Width(128)))
                CreateNewProject(_projectName);
                
            return true;
        }
        private void DrawProjectSelector()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            string[] names = _projects.ConvertAll(p => p.ProjectName).ToArray();
            
            _selectedProjectIndex = Mathf.Clamp(_selectedProjectIndex, 0, _projects.Count - 1);
            _selectedProjectIndex = EditorGUILayout.Popup(SelectProjectLabel, _selectedProjectIndex, names);

            if (GUILayout.Button(CreateNewProjectButtonText, EditorStyles.toolbarButton, GUILayout.Width(128)))
            {
                _projectName = NewProjectDefaultName;
                
                CreateNewProject(_projectName);
            }
            
            EditorGUILayout.EndHorizontal();
        }
        private void DrawTaskList()
        {
            ProjectData project = _projects[_selectedProjectIndex];
            
            if (project.Tasks.Count == 0)
            {
                EditorGUILayout.HelpBox(NoTasksMessage, MessageType.Info);
                return;
            }

            _scrollPos = EditorGUILayout.BeginScrollView(_scrollPos);
            
            foreach (TaskData task in project.Tasks.ToList())
            {
                EditorGUILayout.BeginVertical(TasksVerticalStyle);
                
                EditorGUILayout.LabelField(TaskIdLabel, task.Id);
                EditorGUILayout.LabelField(TaskTitleLabel, task.Title);
                EditorGUILayout.LabelField(TaskDescriptionLabel, task.Description);
                EditorGUILayout.LabelField(TaskStatusLabel, task.Status.ToString());
                EditorGUILayout.LabelField(TaskPriorityLabel, task.Priority.ToString());
                EditorGUILayout.LabelField(TaskCategoryLabel, task.Category);
                EditorGUILayout.LabelField(TaskAssigneeLabel, task.Assignee);
                EditorGUILayout.LabelField(TaskCreatedDateLabel, task.CreatedDate);
                EditorGUILayout.LabelField(TaskDueDateLabel, task.DueDate);
                EditorGUILayout.LabelField(TaskIsArchivedLabel, task.IsArchived.ToString());

                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button(TaskEditButtonText, GUILayout.Width(96)))
                    Selection.activeObject = task;

                if (GUILayout.Button(TaskDeleteButtonText, GUILayout.Width(64)))
                {
                    project.Tasks.Remove(task);
                    
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(task));
                    
                    EditorUtility.SetDirty(project);
                    AssetDatabase.SaveAssets();
                    
                    GUIUtility.ExitGUI();
                }
                
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(4);
            }

            EditorGUILayout.EndScrollView();
        }
        private void DrawNewTaskSection()
        {
            EditorGUILayout.LabelField(NewTaskLabel, EditorStyles.boldLabel);

            _taskTitle = EditorGUILayout.TextField(TaskTitleLabel, _taskTitle);
            _taskDescription = EditorGUILayout.TextField(TaskDescriptionLabel, _taskDescription);
            _taskStatus = (TaskStatus)EditorGUILayout.EnumPopup(TaskStatusLabel, _taskStatus);
            _taskPriority = (TaskPriority)EditorGUILayout.EnumPopup(TaskPriorityLabel, _taskPriority);
            _taskCategory = EditorGUILayout.TextField(TaskCategoryLabel, _taskCategory);
            _taskAssignee = EditorGUILayout.TextField(TaskAssigneeLabel, _taskAssignee);
            _taskCreatedDate = EditorGUILayout.TextField(TaskCreatedDateLabel, _taskCreatedDate);
            _taskDueDate = EditorGUILayout.TextField(TaskDueDateLabel, _taskDueDate);
            _taskIsArchived = EditorGUILayout.Toggle(TaskIsArchivedLabel, _taskIsArchived);

            if (!GUILayout.Button(CreateNewTaskButtonText, GUILayout.Width(128)))
                return;
            
            if (string.IsNullOrWhiteSpace(_taskTitle))
            {
                EditorUtility.DisplayDialog(NewTaskTitleEmptyDisplayDialogTitle, NewTaskTitleEmptyDisplayDialogMessage,
                    NewTaskTitleEmptyDisplayDialogOk);
                return;
            }

            _taskId = GUID.Generate().ToString();
            
            CreateNewTask(_taskId, _taskTitle, _taskDescription, _taskStatus, _taskPriority, _taskCategory,
                _taskAssignee, _taskCreatedDate, _taskDueDate, _taskIsArchived);
            
            ResetTaskFields();
        }
        private void CreateNewProject(string projectName)
        {
            string path = $"{TaskManagementProjectsPath}{projectName}";
            
            ProjectData project = EditorAssetUtility.CreateOrLoadAsset<ProjectData>(path, projectName);
            
            project.ProjectName = projectName;
            project.Tasks = new List<TaskData>();
            
            _projects.Add(project);
            _selectedProjectIndex = _projects.Count - 1;
            
            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssets();
        }
        private void CreateNewTask(string id, string taskTitle, string description, TaskStatus status,
            TaskPriority priority, string category, string assignee, string createdDate, string dueDate,
            bool isArchived)
        {
            ProjectData project = _projects[_selectedProjectIndex];

            if (project.Tasks.Exists(t => t.Title == taskTitle))
            {
                EditorUtility.DisplayDialog(DuplicateTaskDialogTitle, DuplicateTaskDialogMessage,
                    DuplicateTaskDialogOk);
                
                return;
            }
            
            string path = $"{TaskManagementProjectsPath}{project.ProjectName}{TaskManagementTasksPath}";
            TaskData task = EditorAssetUtility.CreateOrLoadAsset<TaskData>(path, taskTitle);
            
            task.Id = id;
            task.Title = taskTitle;
            task.Description = description;
            task.Status = status;
            task.Priority = priority;
            task.Category = category;
            task.Assignee = assignee;

            if (string.IsNullOrEmpty(createdDate))
                createdDate = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm");
            
            task.CreatedDate = createdDate;
            
            task.DueDate = dueDate;
            task.IsArchived = isArchived;
            
            project.Tasks.Add(task);
            
            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssets();
        }
        private void ResetTaskFields()
        {
            _taskTitle = string.Empty;
            _taskDescription = string.Empty;
            _taskStatus = TaskStatus.ToDo;
            _taskPriority = TaskPriority.Low;
            _taskCategory = string.Empty;
            _taskAssignee = string.Empty;
            _taskCreatedDate = string.Empty;
            _taskDueDate = string.Empty;
            _taskIsArchived = false;
        }
        #endregion
    }
}
