using System.Collections.Generic;
using System.Linq;
using TaskManagement.Editor.Data;
using TaskManagement.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Controllers
{
    /// <summary>
    /// Handles all project-related operations in the Task Management system.
    /// Responsible for creating, loading, selecting, editing, and deleting project assets in the Unity Editor.
    /// </summary>
    public sealed class ProjectController
    {
        #region Constants
        public const string TaskManagementProjectsPath = "Assets/TaskManagement/Projects/";
        private const string ProjectIcon = "📁";
        private const string CreateIcon = "✚";
        #endregion

        #region StaticReadonlyFields
        private static readonly Color DeleteProjectColor = new(0.9f, 0.3f, 0.3f, 0.8f);
        private static readonly Color DefaultEditProjectColor = new(0.2f, 0.2f, 0.2f, 0.6f);
        private static readonly Color ProjectNameBackgroundColor = new(0.25f, 0.25f, 0.25f, 0.6f);
        #endregion

        #region ReadonlyFields
        private readonly List<ProjectData> _projects = new();
        #endregion

        #region Fields
        private int _selectedProjectIndex = -1;
        private string _projectName = "NewProject";
        #endregion

        #region Getters
        /// <summary>
        /// Gets the currently selected <see cref="ProjectData"/> instance.
        /// Returns <c>null</c> if no project is selected or the index is invalid.
        /// </summary>
        public ProjectData ProjectData => _selectedProjectIndex >= 0 && _selectedProjectIndex < _projects.Count
            ? _projects[_selectedProjectIndex]
            : null;
        #endregion

        #region Executes
        /// <summary>
        /// Loads all existing <see cref="ProjectData"/> assets from the Unity project.
        /// This is typically called when the editor window is initialized or refreshed.
        /// </summary>
        public void LoadProjects()
        {
            _projects.Clear();

            string[] guids = AssetDatabase.FindAssets("t:ProjectData");
            
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                
                ProjectData project = AssetDatabase.LoadAssetAtPath<ProjectData>(path);

                if (project)
                    _projects.Add(project);
            }
        }
        private void DeleteProject(ProjectData project)
        {
            if (!project)
                return;

            string projectPath = AssetDatabase.GetAssetPath(project);
            string projectFolderPath = $"{TaskManagementProjectsPath}{project.ProjectName}";

            foreach (string taskPath in project.Tasks.ToList().Select(AssetDatabase.GetAssetPath)
                         .Where(taskPath => !string.IsNullOrEmpty(taskPath)))
                AssetDatabase.DeleteAsset(taskPath);

            if (!string.IsNullOrEmpty(projectPath))
                AssetDatabase.DeleteAsset(projectPath);

            if (System.IO.Directory.Exists(projectFolderPath))
                AssetDatabase.DeleteAsset(projectFolderPath);

            _projects.Remove(project);

            _selectedProjectIndex = Mathf.Clamp(_selectedProjectIndex - 1, 0, _projects.Count - 1);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            LoadProjects();
        }
        private void CreateNewProject(string projectName)
        {
            if (!ValidateProjectName(projectName))
                return;

            if (ProjectExists(projectName))
                return;

            string path = $"{TaskManagementProjectsPath}{projectName}";

            ProjectData project = EditorAssetUtility.CreateOrLoadAsset<ProjectData>(path, projectName);
            
            project.ProjectName = projectName;
            project.Tasks = new List<TaskData>();

            _projects.Add(project);
            
            _selectedProjectIndex = _projects.Count - 1;

            EditorUtility.SetDirty(project);
            AssetDatabase.SaveAssets();
            
            LoadProjects();
        }
        private static bool ValidateProjectName(string name)
        {
            if (!string.IsNullOrWhiteSpace(name))
                return true;

            EditorUtility.DisplayDialog("Invalid Project Name", "Project name cannot be empty.", "OK");
            
            return false;
        }
        private bool ProjectExists(string name)
        {
            if (!_projects.Any(p => p.ProjectName.Equals(name, System.StringComparison.OrdinalIgnoreCase)))
                return false;

            EditorUtility.DisplayDialog("Duplicate Project", $"A project named '{name}' already exists.", "OK");
            
            return true;
        }

        /// <summary>
        /// Draws the initial creation prompt when no projects exist.
        /// Allows users to create the first project.
        /// </summary>
        public bool DrawProjectCreationPrompt()
        {
            if (_projects.Count > 0)
                return false;

            EditorGUILayout.HelpBox("⚠️ No projects found yet.", MessageType.Info);

            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField("Project Name:", GUILayout.Width(96));
            
            _projectName = EditorGUILayout.TextField(_projectName);
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button($"{CreateIcon} Create new project", GUILayout.Width(128)))
                CreateNewProject(_projectName);

            return true;
        }

        /// <summary>
        /// Draws the toolbar section for selecting existing projects or creating a new one.
        /// </summary>
        public void DrawProjectSelector()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

            GUILayout.Label(ProjectIcon, GUILayout.Width(32));
            
            EditorGUILayout.LabelField("Active Project:", GUILayout.Width(96));

            if (_projects.Count > 0)
            {
                string[] names = _projects.Select(p => p.ProjectName).ToArray();
                
                _selectedProjectIndex = Mathf.Clamp(_selectedProjectIndex, 0, _projects.Count - 1);

                GUI.backgroundColor = ProjectNameBackgroundColor;
                
                _selectedProjectIndex = EditorGUILayout.Popup(_selectedProjectIndex, names);
                
                GUI.backgroundColor = DefaultEditProjectColor;
            }

            GUILayout.FlexibleSpace();
            
            _projectName = EditorGUILayout.TextField(_projectName);

            if (GUILayout.Button($"{CreateIcon} Create new project", EditorStyles.toolbarButton,
                    GUILayout.Width(128)))
                CreateNewProject(_projectName);

            EditorGUILayout.EndHorizontal();
        }

        /// <summary>
        /// Draws the project editor UI for the currently selected project.
        /// Allows the user to rename or delete the project.
        /// </summary>
        public void DrawProjectEditor()
        {
            if (!ProjectData)
                return;

            ProjectData project = ProjectData;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);

            EditorGUILayout.LabelField($"{ProjectIcon} Edit Project", EditorStyles.boldLabel);
            
            string newName = EditorGUILayout.TextField("Project Name:", project.ProjectName);

            if (newName != project.ProjectName)
            {
                project.ProjectName = newName;
                EditorUtility.SetDirty(project);
                AssetDatabase.SaveAssets();
            }

            GUILayout.FlexibleSpace();

            GUI.backgroundColor = DeleteProjectColor;
            if (GUILayout.Button($"✖ Delete Project", GUILayout.Width(128)))
            {
                bool confirm = EditorUtility.DisplayDialog("Delete Project",
                    $"Are you sure you want to delete the project '{project.ProjectName}'?\nThis will also delete all its tasks.",
                    "Yes, Delete", "Cancel");

                if (confirm)
                    DeleteProject(project);
            }
            GUI.backgroundColor = DefaultEditProjectColor;

            EditorGUILayout.EndVertical();
        }
        #endregion
    }
}
