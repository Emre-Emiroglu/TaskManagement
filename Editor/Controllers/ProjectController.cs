using System.Collections.Generic;
using System.Linq;
using TaskManagement.Editor.Data;
using TaskManagement.Editor.Utilities;
using UnityEditor;
using UnityEngine;

namespace TaskManagement.Editor.Controllers
{
    public sealed class ProjectController
    {
        #region Constants
        private const string NewProjectDefaultName = "NewProject";
        private const string ProjectDataFilter = "t:ProjectData";
        private const string NoProjectFoundMessage = "No projects found yet.";
        private const string CreateNewProjectNameLabel = "Project Name:";
        private const string CreateNewProjectButtonText = "Create new project.";
        private const string SelectProjectLabel = "Active Project:";
        private const string EditProjectLabel = "Edit Project";
        private const string EditProjectNameLabel = "Project Name:";
        private const string DeleteProjectButtonText = "Delete Project";
        private const string DeleteProjectDialogPrefix = "Are you sure you want to delete the project '";
        private const string DeleteProjectDialogSuffix = "'?\nThis will also delete all its tasks.";
        private const string DeleteProjectDialogOk = "Yes, Delete";
        private const string DeleteProjectDialogCancel = "Cancel";
        public const string TaskManagementProjectsPath = "Assets/TaskManagement/Projects/";
        private const string CreateProjectInvalidProjectNameDialogTittle = "Invalid Project Name";
        private const string CreateProjectInvalidProjectNameDialogMessage = "Project name cannot be empty.";
        private const string CreateProjectInvalidProjectNameDialogOk = "OK";
        private const string DuplicateProjectInvalidProjectNameDialogTittle = "Duplicate Project";
        private const string DuplicateProjectInvalidProjectNameDialogMessagePrefix = "A project named '";
        private const string DuplicateProjectInvalidProjectNameDialogMessageSuffix = "' already exists.";
        private const string DuplicateProjectInvalidProjectNameDialogOk = "OK";
        private const string CreateProjectIcon = "✚ ";
        private const string DeleteProjectIcon = "✖ ";
        private const string ProjectIcon = "📁 ";
        private const string NoProjectFoundIcon = "⚠️ ";
        #endregion
        
        #region StaticReadonlyFields
        private static readonly Color DeleteProjectColor = new(0.9f, 0.3f, 0.3f, 0.8f);
        private static readonly Color DefaultEditProjectColor = new(0.2f, 0.2f, 0.2f, 0.6f);
        private static readonly Color ProjectNameBackgroundColor = new(0.25f, 0.25f, 0.25f, 0.6f);
        #endregion

        #region Fields
        private List<ProjectData> _projects = new();
        private int _selectedProjectIndex = -1;
        private string _projectName = NewProjectDefaultName;
        #endregion

        #region Getters
        public ProjectData ProjectData => _selectedProjectIndex >= 0 && _selectedProjectIndex < _projects.Count
            ? _projects[_selectedProjectIndex]
            : null;
        #endregion

        #region Executes
        public void LoadProjects()
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
        public bool DrawProjectCreationPrompt()
        {
            if (_projects.Count > 0)
                return false;
            
            EditorGUILayout.HelpBox($"{NoProjectFoundIcon} {NoProjectFoundMessage}", MessageType.Info);
            
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(CreateNewProjectNameLabel, GUILayout.Width(96));
            
            _projectName = EditorGUILayout.TextField(_projectName);
            
            EditorGUILayout.EndHorizontal();

            if (GUILayout.Button($"{CreateProjectIcon}{CreateNewProjectButtonText}", GUILayout.Width(128)))
                CreateNewProject(_projectName);
            
            return true;
        }
        public void DrawProjectSelector()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
    
            GUILayout.Label($"{ProjectIcon}", GUILayout.Width(32));
            EditorGUILayout.LabelField(SelectProjectLabel, GUILayout.Width(96));
    
            string[] names = _projects.ConvertAll(p => p.ProjectName).ToArray();
            
            _selectedProjectIndex = Mathf.Clamp(_selectedProjectIndex, 0, _projects.Count - 1);
    
            EditorGUI.BeginChangeCheck();
            
            Color originalColor = GUI.backgroundColor;
            
            GUI.backgroundColor = ProjectNameBackgroundColor;
            
            _selectedProjectIndex = EditorGUILayout.Popup(_selectedProjectIndex, names);
            
            GUI.backgroundColor = originalColor;
    
            GUILayout.FlexibleSpace();
            
            _projectName = EditorGUILayout.TextField(_projectName);

            if (GUILayout.Button($"{CreateProjectIcon}{CreateNewProjectButtonText}", EditorStyles.toolbarButton,
                    GUILayout.Width(128)))
                CreateNewProject(_projectName);
    
            EditorGUILayout.EndHorizontal();
        }
        public void DrawProjectEditor()
        {
            if (_selectedProjectIndex < 0 || _selectedProjectIndex >= _projects.Count)
                return;

            ProjectData project = ProjectData;
    
            if (!project)
                return;
    
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
    
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.Label($"{ProjectIcon}", GUILayout.Width(32));
            
            EditorGUILayout.LabelField(EditProjectLabel, EditorStyles.boldLabel);
            
            EditorGUILayout.EndHorizontal();
    
            EditorGUILayout.BeginHorizontal();
            
            EditorGUILayout.LabelField(EditProjectNameLabel, GUILayout.Width(96));
            
            string newName = EditorGUILayout.TextField(project.ProjectName);
            
            EditorGUILayout.EndHorizontal();
    
            if (newName != project.ProjectName)
            {
                project.ProjectName = newName;
                
                EditorUtility.SetDirty(project);
                
                AssetDatabase.SaveAssets();
            }
    
            EditorGUILayout.BeginHorizontal();
            
            GUILayout.FlexibleSpace();
    
            GUI.backgroundColor = DeleteProjectColor;
            
            if (GUILayout.Button($"{DeleteProjectIcon}{DeleteProjectButtonText}", GUILayout.Width(128)))
            {
                bool confirm = EditorUtility.DisplayDialog(DeleteProjectButtonText,
                    $"{DeleteProjectDialogPrefix}{project.ProjectName}{DeleteProjectDialogSuffix}",
                    DeleteProjectDialogOk, DeleteProjectDialogCancel);

                if (confirm)
                    DeleteProject(project);
            }
            
            GUI.backgroundColor = DefaultEditProjectColor;
    
            EditorGUILayout.EndHorizontal();
            
            EditorGUILayout.EndVertical();
        }
        private void DeleteProject(ProjectData project)
        {
            if (!project)
                return;

            string projectPath = AssetDatabase.GetAssetPath(project);
            string projectFolderPath = TaskManagementProjectsPath + project.ProjectName;
            
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
        }
        private void CreateNewProject(string projectName)
        {
            if (string.IsNullOrWhiteSpace(projectName))
            {
                EditorUtility.DisplayDialog(CreateProjectInvalidProjectNameDialogTittle,
                    CreateProjectInvalidProjectNameDialogMessage, CreateProjectInvalidProjectNameDialogOk);
                
                return;
            }
            
            if (_projects.Any(p => p.ProjectName.Equals(projectName, System.StringComparison.OrdinalIgnoreCase)))
            {
                EditorUtility.DisplayDialog(DuplicateProjectInvalidProjectNameDialogTittle,
                    $"{DuplicateProjectInvalidProjectNameDialogMessagePrefix}{projectName}{DuplicateProjectInvalidProjectNameDialogMessageSuffix}",
                    DuplicateProjectInvalidProjectNameDialogOk);
                
                return;
            }
            
            string path = $"{TaskManagementProjectsPath}{projectName}";
            
            ProjectData project = EditorAssetUtility.CreateOrLoadAsset<ProjectData>(path, projectName);

            project.ProjectName = projectName;
            
            project.Tasks = new List<TaskData>();

            _projects.Add(project);
            
            _selectedProjectIndex = _projects.Count - 1;

            EditorUtility.SetDirty(project);
            
            AssetDatabase.SaveAssets();
        }
        #endregion
    }
}