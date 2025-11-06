using System;
using System.Collections.Generic;
using System.Linq;
using TaskManagement.Editor.Data;
using TaskManagement.Editor.Enums;
using TaskManagement.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Controllers
{
    /// <summary>
    /// Controls all task-related functionality for the Task Management system.
    /// Handles creation, editing, deletion, sorting, and rendering of tasks within Unity Editor UI.
    /// </summary>
    public sealed class TaskController
    {
        #region Constants
        private const string DateFormat = "yyyy-MM-dd";
        private const string AddIcon = "✚";
        private const string EditIcon = "✎";
        #endregion

        #region Static Colors
        private static readonly Color EditColor = new(0.3f, 0.6f, 0.9f, 1f);
        private static readonly Color DeleteColor = new(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color AddColor = new(0.3f, 0.8f, 0.3f, 1f);
        #endregion

        #region Fields
        private SortOption _sortOption = SortOption.None;
        private bool _sortAscending = true;
        private TaskData _selectedTask;
        private string _newTitle = string.Empty;
        private string _newDescription = string.Empty;
        private TaskStatus _newStatus = TaskStatus.ToDo;
        private TaskPriority _newPriority = TaskPriority.Low;
        private string _newCategory = string.Empty;
        private string _newAssignee = string.Empty;
        private string _newDueDate = string.Empty;
        private string _editTitle;
        private string _editDescription;
        private TaskStatus _editStatus;
        private TaskPriority _editPriority;
        private string _editCategory;
        private string _editAssignee;
        private string _editDueDate;
        #endregion

        #region Getters
        /// <summary>
        /// Indicates whether a task is currently being edited.
        /// </summary>
        public bool IsEditingTask { get; private set; }
        #endregion

        #region Executes
        /// <summary>
        /// Draws the toolbar for sorting tasks by different criteria.
        /// </summary>
        public void DrawSortToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            
            EditorGUILayout.LabelField("🔄 Sort by:", GUILayout.Width(80));
            
            _sortOption = (SortOption)EditorGUILayout.EnumPopup(_sortOption, GUILayout.Width(128));

            GUILayout.FlexibleSpace();
            
            if (GUILayout.Button(_sortAscending ? "↑" : "↓", EditorStyles.toolbarButton, GUILayout.Width(32)))
                _sortAscending = !_sortAscending;

            EditorGUILayout.EndHorizontal();
        }
        
        /// <summary>
        /// Draws the scrollable list of tasks for the current project.
        /// </summary>
        /// <param name="projectController">The project controller containing the current project.</param>
        /// <param name="scroll">Reference to the scroll position for maintaining the UI state.</param>
        public void DrawTaskList(ProjectController projectController, ref Vector2 scroll)
        {
            ProjectData project = projectController.ProjectData;
            
            if (!project)
                return;

            if (project.Tasks.Count == 0)
            {
                EditorGUILayout.HelpBox("📭 No tasks in this project yet.", MessageType.Info);
                
                return;
            }

            List<TaskData> sortedTasks = SortTasks(project.Tasks);
            
            scroll = EditorGUILayout.BeginScrollView(scroll);
            
            foreach (TaskData task in sortedTasks)
                DrawTaskItem(task, project);
            
            EditorGUILayout.EndScrollView();
        }
        
        /// <summary>
        /// Draws the section for creating a new task.
        /// </summary>
        /// <param name="projectController">The controller managing the current project.</param>
        public void DrawNewTaskSection(ProjectController projectController)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"{AddIcon} Add New Task", EditorStyles.boldLabel);

            DrawTaskFields(ref _newTitle, ref _newDescription, ref _newStatus, ref _newPriority, ref _newCategory,
                ref _newAssignee, ref _newDueDate);

            DrawColoredButton($"{AddIcon} Add", AddColor, () =>
            {
                if (string.IsNullOrWhiteSpace(_newTitle))
                {
                    EditorUtility.DisplayDialog("Error", "Task title cannot be empty.", "OK");
                    
                    return;
                }

                if (projectController.ProjectData.Tasks.Exists(t => t.Title == _newTitle))
                {
                    EditorUtility.DisplayDialog("Duplicate Task", "A task with this title already exists.", "OK");
                    
                    return;
                }

                CreateTask(projectController);
                
                ResetNewTaskFields();
                
            });
            
            EditorGUILayout.EndVertical();
        }
        
        /// <summary>
        /// Draws the section for editing an existing task.
        /// </summary>
        public void DrawTaskEditSection()
        {
            if (!_selectedTask)
                return;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"{EditIcon} Edit Task", EditorStyles.boldLabel);

            DrawTaskFields(ref _editTitle, ref _editDescription, ref _editStatus, ref _editPriority, ref _editCategory,
                ref _editAssignee, ref _editDueDate);

            EditorGUILayout.BeginHorizontal();
            
            DrawColoredButton("💾 Save Changes", AddColor, ApplyTaskEdits);
            DrawColoredButton("↩ Cancel", DeleteColor, CancelEdit);
            
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        private void DrawTaskItem(TaskData task, ProjectData project)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"📝 {task.Title}", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Description:", task.Description);
            EditorGUILayout.LabelField("Status:", task.Status.ToString());
            EditorGUILayout.LabelField("Priority:", task.Priority.ToString());
            EditorGUILayout.LabelField("Category:", task.Category);
            EditorGUILayout.LabelField("Assignee:", task.Assignee);
            EditorGUILayout.LabelField("Due:", task.DueDate);
            EditorGUILayout.LabelField("Remaining:", CalculateRemainingTime(task.DueDate));

            EditorGUILayout.BeginHorizontal();
            
            DrawColoredButton($"{EditIcon} Edit", EditColor, () => BeginEdit(task));
            DrawColoredButton($"✖ Delete", DeleteColor, () => DeleteTask(project, task));
            
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        private static void DrawTaskFields(ref string title, ref string desc, ref TaskStatus status,
            ref TaskPriority priority, ref string category, ref string assignee, ref string due)
        {
            title = EditorGUILayout.TextField("Title", title);
            desc = EditorGUILayout.TextField("Description", desc);
            status = (TaskStatus)EditorGUILayout.EnumPopup("Status", status);
            priority = (TaskPriority)EditorGUILayout.EnumPopup("Priority", priority);
            category = EditorGUILayout.TextField("Category", category);
            assignee = EditorGUILayout.TextField("Assignee", assignee);

            EditorGUILayout.BeginHorizontal();
            
            due = EditorGUILayout.TextField("Due Date", due);
            
            if (GUILayout.Button("Today", GUILayout.Width(64)))
                due = DateTime.Now.ToString(DateFormat);
            if (GUILayout.Button("+1", GUILayout.Width(32)))
                due = AdjustDate(due, 1);
            if (GUILayout.Button("-1", GUILayout.Width(32)))
                due = AdjustDate(due, -1);
            
            EditorGUILayout.EndHorizontal();

            string remaining = CalculateRemainingTime(due);
            
            if (!string.IsNullOrEmpty(remaining))
                EditorGUILayout.HelpBox($"⏱️ {remaining}", MessageType.None);
        }
        private static void DrawColoredButton(string label, Color color, Action onClick)
        {
            Color original = GUI.backgroundColor;
            
            GUI.backgroundColor = color;
            
            if (GUILayout.Button(label, GUILayout.Width(128)))
                onClick?.Invoke();
            
            GUI.backgroundColor = original;
        }
        private void CreateTask(ProjectController controller)
        {
            ProjectData project = controller.ProjectData;
            
            string path = $"{ProjectController.TaskManagementProjectsPath}{project.ProjectName}/Tasks";
            
            TaskData task = EditorAssetUtility.CreateOrLoadAsset<TaskData>(path, _newTitle);

            task.Title = _newTitle;
            task.Description = _newDescription;
            task.Status = _newStatus;
            task.Priority = _newPriority;
            task.Category = _newCategory;
            task.Assignee = _newAssignee;
            task.DueDate = _newDueDate;

            project.Tasks.Add(task);
            
            EditorUtility.SetDirty(project);
            
            AssetDatabase.SaveAssets();
        }
        private static void DeleteTask(ProjectData project, TaskData task)
        {
            project.Tasks.Remove(task);
            
            AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(task));
            
            EditorUtility.SetDirty(project);
            
            AssetDatabase.SaveAssets();
        }
        private void BeginEdit(TaskData task)
        {
            _selectedTask = task;
            _editTitle = task.Title;
            _editDescription = task.Description;
            _editStatus = task.Status;
            _editPriority = task.Priority;
            _editCategory = task.Category;
            _editAssignee = task.Assignee;
            _editDueDate = task.DueDate;

            IsEditingTask = true;
        }
        private void ApplyTaskEdits()
        {
            if (!_selectedTask)
                return;

            _selectedTask.Title = _editTitle;
            _selectedTask.Description = _editDescription;
            _selectedTask.Status = _editStatus;
            _selectedTask.Priority = _editPriority;
            _selectedTask.Category = _editCategory;
            _selectedTask.Assignee = _editAssignee;
            _selectedTask.DueDate = _editDueDate;

            EditorUtility.SetDirty(_selectedTask);
            
            AssetDatabase.SaveAssets();
            
            CancelEdit();
        }
        private void CancelEdit()
        {
            _selectedTask = null;
            
            IsEditingTask = false;
        }
        private void ResetNewTaskFields()
        {
            _newTitle = string.Empty;
            _newDescription = string.Empty;
            _newCategory = string.Empty;
            _newAssignee = string.Empty;
            _newDueDate = string.Empty;
            _newPriority = TaskPriority.Low;
            _newStatus = TaskStatus.ToDo;
        }
        private static string AdjustDate(string date, int days)
        {
            if (!DateTime.TryParse(date, out DateTime parsed))
                parsed = DateTime.Now;
            
            return parsed.AddDays(days).ToString(DateFormat);
        }
        private static string CalculateRemainingTime(string dueDate)
        {
            if (string.IsNullOrEmpty(dueDate))
                return string.Empty;
            if (!DateTime.TryParse(dueDate, out DateTime due))
                return "Invalid date format";

            int days = (due - DateTime.Now.Date).Days;
            
            return days switch
            {
                > 0 => $"{days} day{(days > 1 ? "s" : "")} left",
                0 => "Due today",
                _ => $"{Math.Abs(days)} day{(Math.Abs(days) > 1 ? "s" : "")} overdue"
            };
        }
        private List<TaskData> SortTasks(List<TaskData> tasks)
        {
            IEnumerable<TaskData> sorted = _sortOption switch
            {
                SortOption.Assignee => _sortAscending
                    ? tasks.OrderBy(t => t.Assignee)
                    : tasks.OrderByDescending(t => t.Assignee),
                SortOption.Priority => _sortAscending
                    ? tasks.OrderBy(t => t.Priority)
                    : tasks.OrderByDescending(t => t.Priority),
                SortOption.Status => _sortAscending
                    ? tasks.OrderBy(t => t.Status)
                    : tasks.OrderByDescending(t => t.Status),
                SortOption.RemainingTime => _sortAscending
                    ? tasks.OrderBy(t => t.DueDate)
                    : tasks.OrderByDescending(t => t.DueDate),
                _ => tasks
            };
            
            return sorted.ToList();
        }
        #endregion
    }
}
