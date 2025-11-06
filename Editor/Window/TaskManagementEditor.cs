using TaskManagement.Editor.Controllers;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Window
{
    public sealed class TaskManagementEditor : EditorWindow
    {
        #region Constants
        private const string MenuItemName = "Tools/Task Management Editor";
        private const string TitleContent = "Task Management Editor";
        private const int MinXSize = 960;
        private const int MinYSize = 540;
        private const int MaxXSize = 1920;
        private const int MaxYSize = 1080;
        #endregion
        
        #region ReadonlyFields
        private readonly ProjectController _projectController = new();
        private readonly TaskController _taskController = new();
        #endregion
        
        #region Fields
        private Vector2 _scrollPos = Vector2.zero;
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
