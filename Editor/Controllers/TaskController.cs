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
    public sealed class TaskController
    {
        #region Constants
        private const string SortByLabel = "Sort by:";
        private const string NoTasksMessage = "No tasks in this project yet.";
        private const string InvalidDateFormatInfoMessage = "Invalid date format";
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
        private const string TaskRemainingTimeLabel = "Remaining Time:";
        private const string TaskEditButtonText = "Edit";
        private const string TaskDeleteButtonText = "Delete";
        private const string NewTaskLabel = "Add new task";
        private const string TaskCreatedDateFormat = "yyyy-MM-dd";
        private const string TaskDueDateTodayButtonText = "Today";
        private const string TaskDueDateAddOneDayButtonText = "+1";
        private const string TaskDueDateRemoveOneDayButtonText = "-1";
        private const string CreateNewTaskButtonText = "Add";
        private const string NewTaskTitleEmptyDisplayDialogTitle = "ERROR!";
        private const string NewTaskTitleEmptyDisplayDialogMessage = "Task title cannot be empty.";
        private const string NewTaskTitleEmptyDisplayDialogOk = "Okey";
        private const string DuplicateTaskDialogTitle = "Duplicate Task";
        private const string DuplicateTaskDialogMessage = "A task with this title already exists in this project.";
        private const string DuplicateTaskDialogOk = "OK";
        private const string TaskManagementTasksPath = "/Tasks";
        private const string EditTaskLabel = "Edit";
        private const string EditTaskSaveChangesButtonText = "Save Changes";
        private const string EditTaskCancelEditButtonText = "Cancel";
        private const string UpArrowIcon = "↑ ";
        private const string DownArrowIcon = "↓ ";
        private const string TaskIcon = "📝 ";
        private const string EditIcon = "✎ ";
        private const string DeleteIcon = "✖ ";
        private const string AddIcon = "✚ ";
        private const string SaveIcon = "💾 ";
        private const string CancelIcon = "↩ ";
        private const string CalendarIcon = "📅 ";
        private const string UserIcon = "👤 ";
        private const string CategoryIcon = "🏷️ ";
        private const string PriorityIcon = "⚡ ";
        private const string StatusIcon = "⭐ ";
        private const string DescriptionIcon = "📄 ";
        private const string SortIcon = "🔄 ";
        private const string NoTasksIcon = "📭 ";
        private const string ErrorIcon = "⚠️ ";
        private const string IdIcon = "🔑 ";
        private const string TimeIcon = "⏱️ ";
        #endregion
        
        #region StaticReadonlyFields
        private static readonly Color TaskStatusToDoBackgroundColor = new(0.8f, 0.7f, 0.2f, 0.2f);
        private static readonly Color TaskStatusInProgressBackgroundColor = new(0.2f, 0.5f, 0.8f, 0.2f);
        private static readonly Color TaskStatusDoneBackgroundColor = new(0.2f, 0.8f, 0.3f, 0.2f);
        private static readonly Color TaskStatusDefaultBackgroundColor = new(0.3f, 0.3f, 0.3f, 0.2f);
        private static readonly Color TaskPriorityLowPriorityColor = new(0.3f, 0.6f, 0.9f, 0.7f);
        private static readonly Color TaskPriorityNormalPriorityColor = new(0.9f, 0.6f, 0.2f, 0.7f);
        private static readonly Color TaskPriorityHighPriorityColor = new(0.9f, 0.3f, 0.3f, 0.7f);
        private static readonly Color TaskPriorityDefaultBackgroundColor = new(0.5f, 0.5f, 0.5f, 0.7f);
        private static readonly Color EditButtonColor = new(0.3f, 0.6f, 0.9f, 1f);
        private static readonly Color DeleteButtonColor = new(0.9f, 0.3f, 0.3f, 1f);
        private static readonly Color AddButtonColor = new(0.3f, 0.8f, 0.3f, 1f);
        #endregion
        
        #region Fields
        private SortOption _currentSortOption = SortOption.None;
        private bool _sortAscending = true;
        private TaskData _selectedTaskForEdit;
        private string _editTitle = string.Empty;
        private string _editDescription = string.Empty;
        private TaskStatus _editStatus = TaskStatus.ToDo;
        private TaskPriority _editPriority = TaskPriority.Low;
        private string _editCategory = string.Empty;
        private string _editAssignee = string.Empty;
        private string _editDueDate = string.Empty;
        private string _taskId = string.Empty;
        private string _taskTitle = string.Empty;
        private string _taskDescription = string.Empty;
        private TaskStatus _taskStatus = TaskStatus.ToDo;
        private TaskPriority _taskPriority = TaskPriority.Low;
        private string _taskCategory = string.Empty;
        private string _taskAssignee = string.Empty;
        private string _taskCreatedDate = string.Empty;
        private string _taskDueDate = string.Empty;
        private string _taskRemainingTime = string.Empty;
        #endregion
        
        #region Getters
        public bool IsEditingTask { get; private set; }
        #endregion
        
        #region Executes
        public void DrawSortToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            EditorGUILayout.LabelField($"{SortIcon}{SortByLabel}", GUILayout.Width(80));

            _currentSortOption = (SortOption)EditorGUILayout.EnumPopup(_currentSortOption, GUILayout.Width(128));

            GUILayout.FlexibleSpace();

            if (GUILayout.Button(_sortAscending ? UpArrowIcon : DownArrowIcon, EditorStyles.toolbarButton,
                    GUILayout.Width(32)))
                _sortAscending = !_sortAscending;

            EditorGUILayout.EndHorizontal();
        }
        public void DrawTaskList(ProjectController projectController, ref Vector2 scrollPos)
        {
            ProjectData project = projectController.ProjectData;
            
            if (!project)
                return;
            
            if (project.Tasks.Count == 0)
            {
                EditorGUILayout.HelpBox($"{NoTasksIcon} {NoTasksMessage}", MessageType.Info);
                return;
            }

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            List<TaskData> sortedTasks = project.Tasks.ToList();

            sortedTasks = _currentSortOption switch
            {
                SortOption.Assignee => _sortAscending
                    ? sortedTasks.OrderBy(t => t.Assignee).ToList()
                    : sortedTasks.OrderByDescending(t => t.Assignee).ToList(),
                SortOption.Priority => _sortAscending
                    ? sortedTasks.OrderBy(t => t.Priority).ToList()
                    : sortedTasks.OrderByDescending(t => t.Priority).ToList(),
                SortOption.Status => _sortAscending
                    ? sortedTasks.OrderBy(t => t.Status).ToList()
                    : sortedTasks.OrderByDescending(t => t.Status).ToList(),
                SortOption.RemainingTime => _sortAscending
                    ? sortedTasks.OrderBy(t => ParseRemainingDays(t.RemainingTime)).ToList()
                    : sortedTasks.OrderByDescending(t => ParseRemainingDays(t.RemainingTime)).ToList(),
                _ => sortedTasks
            };

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

            foreach (TaskData task in sortedTasks)
            {
                string currentRemaining = CalculateRemainingTime(task.DueDate);
                
                if (task.RemainingTime != currentRemaining)
                {
                    task.RemainingTime = currentRemaining;
                    EditorUtility.SetDirty(task);
                }

                Color originalColor = GUI.backgroundColor;
                Color backgroundColor = GetTaskBackgroundColor(task.Status);
                Color priorityColor = GetPriorityColor(task.Priority);
        
                GUI.backgroundColor = backgroundColor;
                EditorGUILayout.BeginVertical(TasksVerticalStyle);
                GUI.backgroundColor = originalColor;

                Rect lastRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.Height(4));
                EditorGUI.DrawRect(lastRect, priorityColor);
                
                EditorGUILayout.LabelField($"{IdIcon} {TaskIdLabel}", task.Id);
                EditorGUILayout.LabelField($"{TaskIcon} {TaskTitleLabel}", task.Title);
                EditorGUILayout.LabelField($"{DescriptionIcon} {TaskDescriptionLabel}", task.Description);
                EditorGUILayout.LabelField($"{StatusIcon} {TaskStatusLabel}", task.Status.ToString());
                EditorGUILayout.LabelField($"{PriorityIcon} {TaskPriorityLabel}", task.Priority.ToString());
                EditorGUILayout.LabelField($"{CategoryIcon} {TaskCategoryLabel}", task.Category);
                EditorGUILayout.LabelField($"{UserIcon} {TaskAssigneeLabel}", task.Assignee);
                EditorGUILayout.LabelField($"{CalendarIcon} {TaskCreatedDateLabel}", task.CreatedDate);
                EditorGUILayout.LabelField($"{CalendarIcon} {TaskDueDateLabel}", task.DueDate);
                EditorGUILayout.LabelField($"{TimeIcon} {TaskRemainingTimeLabel}", task.RemainingTime);

                EditorGUILayout.BeginHorizontal();
                
                GUI.backgroundColor = EditButtonColor;
                if (GUILayout.Button($"{EditIcon}{TaskEditButtonText}", GUILayout.Width(96)))
                {
                    Selection.activeObject = task;
                    _selectedTaskForEdit = task;
                    IsEditingTask = true;
                    _editTitle = task.Title;
                    _editDescription = task.Description;
                    _editStatus = task.Status;
                    _editPriority = task.Priority;
                    _editCategory = task.Category;
                    _editAssignee = task.Assignee;
                    _editDueDate = task.DueDate;
                }

                GUI.backgroundColor = DeleteButtonColor;
                if (GUILayout.Button($"{DeleteIcon}{TaskDeleteButtonText}", GUILayout.Width(96)))
                {
                    project.Tasks.Remove(task);
                    AssetDatabase.DeleteAsset(AssetDatabase.GetAssetPath(task));
                    EditorUtility.SetDirty(project);
                    AssetDatabase.SaveAssets();
                    GUIUtility.ExitGUI();
                }

                GUI.backgroundColor = originalColor;
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
            }

            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
        }
        private static int ParseRemainingDays(string remaining)
        {
            if (string.IsNullOrEmpty(remaining))
                return int.MaxValue;

            if (remaining.Contains("overdue"))
            {
                string number = new string(remaining.Where(char.IsDigit).ToArray());
                
                return -int.Parse(number);
            }

            if (!remaining.Contains("day"))
                return 0;
            {
                string number = new string(remaining.Where(char.IsDigit).ToArray());
                
                return int.TryParse(number, out int value) ? value : 0;
            }
        }
        private static string CalculateRemainingTime(string dueDate)
        {
            if (string.IsNullOrEmpty(dueDate))
                return string.Empty;

            if (!DateTime.TryParse(dueDate, out DateTime due))
                return InvalidDateFormatInfoMessage;

            DateTime now = DateTime.Now.Date;
            int remaining = (due - now).Days;

            return remaining switch
            {
                > 0 => $"{remaining} day{(remaining > 1 ? "s" : "")} left",
                0 => "Due today",
                _ => $"{Mathf.Abs(remaining)} day{(Mathf.Abs(remaining) > 1 ? "s" : "")} overdue"
            };
        }
        private static Color GetTaskBackgroundColor(TaskStatus status)
        {
            return status switch
            {
                TaskStatus.ToDo => TaskStatusToDoBackgroundColor,
                TaskStatus.InProgress => TaskStatusInProgressBackgroundColor,
                TaskStatus.Done => TaskStatusDoneBackgroundColor,
                _ => TaskStatusDefaultBackgroundColor
            };
        }
        private static Color GetPriorityColor(TaskPriority priority)
        {
            return priority switch
            {
                TaskPriority.Low => TaskPriorityLowPriorityColor,
                TaskPriority.Normal => TaskPriorityNormalPriorityColor,
                TaskPriority.High => TaskPriorityHighPriorityColor,
                _ => TaskPriorityDefaultBackgroundColor
            };
        }
        public void DrawNewTaskSection(ProjectController projectController)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"{AddIcon}{NewTaskLabel}", EditorStyles.boldLabel);

            _taskTitle = EditorGUILayout.TextField($"{TaskIcon} {TaskTitleLabel}", _taskTitle);
            _taskDescription = EditorGUILayout.TextField($"{DescriptionIcon} {TaskDescriptionLabel}", _taskDescription);
            _taskStatus = (TaskStatus)EditorGUILayout.EnumPopup($"{StatusIcon} {TaskStatusLabel}", _taskStatus);
            _taskPriority =
                (TaskPriority)EditorGUILayout.EnumPopup($"{PriorityIcon} {TaskPriorityLabel}", _taskPriority);
            _taskCategory = EditorGUILayout.TextField($"{CategoryIcon} {TaskCategoryLabel}", _taskCategory);
            _taskAssignee = EditorGUILayout.TextField($"{UserIcon} {TaskAssigneeLabel}", _taskAssignee);
            
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{CalendarIcon} {TaskCreatedDateLabel}", GUILayout.Width(128));
            _taskCreatedDate = DateTime.Now.ToString(TaskCreatedDateFormat);
            EditorGUILayout.LabelField(_taskCreatedDate);
            EditorGUILayout.EndHorizontal();
                
            EditorGUILayout.BeginHorizontal();
            _taskDueDate = EditorGUILayout.TextField($"{CalendarIcon} {TaskDueDateLabel}", _taskDueDate);
            
            if (GUILayout.Button(TaskDueDateTodayButtonText, GUILayout.Width(64)))
                _taskDueDate = DateTime.Now.ToString(TaskCreatedDateFormat);
            if (GUILayout.Button(TaskDueDateAddOneDayButtonText, GUILayout.Width(32)))
                _taskDueDate = AdjustDate(_taskDueDate, 1);
            if (GUILayout.Button(TaskDueDateRemoveOneDayButtonText, GUILayout.Width(32)))
                _taskDueDate = AdjustDate(_taskDueDate, -1);
            EditorGUILayout.EndHorizontal();
            
            string remaining = CalculateRemainingTime(_taskDueDate);
            if (!string.IsNullOrEmpty(remaining))
            {
                _taskRemainingTime = $"{TimeIcon} {TaskRemainingTimeLabel} {remaining}";
                EditorGUILayout.HelpBox(_taskRemainingTime, MessageType.None);
            }

            Color originalColor = GUI.backgroundColor;
            GUI.backgroundColor = AddButtonColor;
            if (GUILayout.Button($"{AddIcon}{CreateNewTaskButtonText}", GUILayout.Width(128)))
            {
                if (string.IsNullOrWhiteSpace(_taskTitle))
                {
                    EditorUtility.DisplayDialog($"{ErrorIcon}{NewTaskTitleEmptyDisplayDialogTitle}",
                        NewTaskTitleEmptyDisplayDialogMessage, NewTaskTitleEmptyDisplayDialogOk);
                    return;
                }
            
                _taskId = GUID.Generate().ToString();
                CreateNewTask(projectController, _taskId, _taskTitle, _taskDescription, _taskStatus, _taskPriority,
                    _taskCategory, _taskAssignee, _taskCreatedDate, _taskDueDate, _taskRemainingTime);
                ResetTaskFields();
            }
            GUI.backgroundColor = originalColor;

            EditorGUILayout.EndVertical();
        }
        private static string AdjustDate(string date, int days)
        {
            if (!DateTime.TryParse(date, out DateTime target))
                target = DateTime.Now;

            target = target.AddDays(days);
            
            return target.ToString(TaskCreatedDateFormat);
        }
        private static void CreateNewTask(ProjectController projectController, string id, string taskTitle,
            string description, TaskStatus status, TaskPriority priority, string category, string assignee,
            string createdDate, string dueDate, string remainingTime)
        {
            ProjectData project = projectController.ProjectData;
            
            if (!project)
                return;

            if (project.Tasks.Exists(t => t.Title == taskTitle))
            {
                EditorUtility.DisplayDialog($"{ErrorIcon}{DuplicateTaskDialogTitle}", DuplicateTaskDialogMessage,
                    DuplicateTaskDialogOk);
                
                return;
            }

            string path =
                $"{ProjectController.TaskManagementProjectsPath}{project.ProjectName}{TaskManagementTasksPath}";
            
            TaskData task = EditorAssetUtility.CreateOrLoadAsset<TaskData>(path, taskTitle);
            
            task.Id = id;
            task.Title = taskTitle;
            task.Description = description;
            task.Status = status;
            task.Priority = priority;
            task.Category = category;
            task.Assignee = assignee;

            if (string.IsNullOrEmpty(createdDate))
                createdDate = DateTime.Now.ToString(TaskCreatedDateFormat);
            
            task.CreatedDate = createdDate;
            
            task.DueDate = dueDate;

            task.RemainingTime = remainingTime;
            
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
            _taskRemainingTime = string.Empty;
        }
        public void DrawTaskEditSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField($"{EditIcon}{EditTaskLabel}", EditorStyles.boldLabel);

            _editTitle = EditorGUILayout.TextField($"{TaskIcon} {TaskTitleLabel}", _editTitle);
            _editDescription = EditorGUILayout.TextField($"{DescriptionIcon} {TaskDescriptionLabel}", _editDescription);
            _editStatus = (TaskStatus)EditorGUILayout.EnumPopup($"{StatusIcon} {TaskStatusLabel}", _editStatus);
            _editPriority =
                (TaskPriority)EditorGUILayout.EnumPopup($"{PriorityIcon} {TaskPriorityLabel}", _editPriority);
            _editCategory = EditorGUILayout.TextField($"{CategoryIcon} {TaskCategoryLabel}", _editCategory);
            _editAssignee = EditorGUILayout.TextField($"{UserIcon} {TaskAssigneeLabel}", _editAssignee);
            
            EditorGUILayout.BeginHorizontal();
            _editDueDate = EditorGUILayout.TextField($"{CalendarIcon} {TaskDueDateLabel}", _editDueDate);
            
            if (GUILayout.Button(TaskDueDateTodayButtonText, GUILayout.Width(64)))
                _editDueDate = DateTime.Now.ToString(TaskCreatedDateFormat);
            if (GUILayout.Button(TaskDueDateAddOneDayButtonText, GUILayout.Width(32)))
                _editDueDate = AdjustDate(_editDueDate, 1);
            if (GUILayout.Button(TaskDueDateRemoveOneDayButtonText, GUILayout.Width(32)))
                _editDueDate = AdjustDate(_editDueDate, -1);
            EditorGUILayout.EndHorizontal();

            string remaining = CalculateRemainingTime(_editDueDate);
            EditorGUILayout.HelpBox($"{TimeIcon} {TaskRemainingTimeLabel} {remaining}", MessageType.None);

            EditorGUILayout.BeginHorizontal();
            
            Color originalColor = GUI.backgroundColor;
            
            GUI.backgroundColor = AddButtonColor;
            if (GUILayout.Button($"{SaveIcon}{EditTaskSaveChangesButtonText}", GUILayout.Width(128)))
                ApplyTaskEdits();
        
            GUI.backgroundColor = DeleteButtonColor;
            if (GUILayout.Button($"{CancelIcon}{EditTaskCancelEditButtonText}", GUILayout.Width(96)))
            {
                IsEditingTask = false;
                _selectedTaskForEdit = null;
            }
            
            GUI.backgroundColor = originalColor;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.EndVertical();
        }
        private void ApplyTaskEdits()
        {
            if (!_selectedTaskForEdit)
                return;

            _selectedTaskForEdit.Title = _editTitle;
            _selectedTaskForEdit.Description = _editDescription;
            _selectedTaskForEdit.Status = _editStatus;
            _selectedTaskForEdit.Priority = _editPriority;
            _selectedTaskForEdit.Category = _editCategory;
            _selectedTaskForEdit.Assignee = _editAssignee;
            _selectedTaskForEdit.DueDate = _editDueDate;
            _selectedTaskForEdit.RemainingTime = CalculateRemainingTime(_editDueDate);

            EditorUtility.SetDirty(_selectedTaskForEdit);
            
            AssetDatabase.SaveAssets();

            IsEditingTask = false;
            _selectedTaskForEdit = null;
        }
        #endregion
    }
}