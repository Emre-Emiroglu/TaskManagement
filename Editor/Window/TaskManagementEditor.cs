using TaskManagement.Editor.Controllers;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Window
{
    /// <summary>
    /// Main editor window for managing projects and tasks inside Unity.
    /// Provides a full UI for creating, editing, sorting, and deleting tasks.
    /// </summary>
    public sealed class TaskManagementEditor : EditorWindow
    {
        #region ReadonlyFields
        private readonly ProjectController _projectController = new();
        private readonly TaskController _taskController = new();
        #endregion
        
        #region Fields
        private Vector2 _scrollPos = Vector2.zero;
        #endregion
        
        #region Core
        [MenuItem("Tools/Task Management Editor")]
        private static void ShowWindow()
        {
            TaskManagementEditor window = GetWindow<TaskManagementEditor>();
            
            window.titleContent = new GUIContent("Task Management Editor");
            window.minSize = new Vector2(960, 540);
            window.maxSize = new Vector2(1920, 1080);
            
            window.Show();
        }
        private void OnEnable() => _projectController.LoadProjects();
        #endregion
        
        #region Executes
        private void OnGUI()
        {
            if (_projectController.DrawProjectCreationPrompt())
                return;

            _projectController.DrawProjectSelector();
            
            _projectController.DrawProjectEditor();
            
            _taskController.DrawSortToolbar();
            
            _taskController.DrawTaskList(_projectController, ref _scrollPos);
            
            if (!_taskController.IsEditingTask)
                _taskController.DrawNewTaskSection(_projectController);
            else
                _taskController.DrawTaskEditSection();
        }
        #endregion
    }
}
