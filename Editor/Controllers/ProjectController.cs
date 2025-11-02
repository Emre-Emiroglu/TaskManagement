using System.Collections.Generic;
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
        public const string TaskManagementProjectsPath = "Assets/TaskManagement/Projects/";
        #endregion

        #region Fields
        private List<ProjectData> _projects = new();
        private int _selectedProjectIndex = -1;
        private string _projectName = NewProjectDefaultName;
        #endregion

        #region Getters
        public ProjectData ProjectData => _projects[_selectedProjectIndex];
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
            
            EditorGUILayout.HelpBox(NoProjectFoundMessage, MessageType.Info);
            
            _projectName = EditorGUILayout.TextField(CreateNewProjectNameLabel, _projectName);

            if (GUILayout.Button(CreateNewProjectButtonText, GUILayout.Width(128)))
                CreateNewProject(_projectName);
                
            return true;
        }
        public void DrawProjectSelector()
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
        public void DrawProjectEditor()
        {
            if (_selectedProjectIndex < 0 || _selectedProjectIndex >= _projects.Count)
                return;

            ProjectData project = _projects[_selectedProjectIndex];
            
            EditorGUILayout.Space(4);
            
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField(EditProjectLabel, EditorStyles.boldLabel);

            string newName = EditorGUILayout.TextField(EditProjectNameLabel, project.ProjectName);
            
            if (newName != project.ProjectName)
            {
                project.ProjectName = newName;
                
                EditorUtility.SetDirty(project);
                
                AssetDatabase.SaveAssets();
            }

            EditorGUILayout.EndVertical();
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
        #endregion
    }
}