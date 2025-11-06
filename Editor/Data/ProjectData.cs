using System.Collections.Generic;
using UnityEngine;

namespace TaskManagement.Editor.Data
{
    /// <summary>
    /// Represents a project within the Task Management system.
    /// Stores metadata about the project and its associated tasks.
    /// </summary>
    [CreateAssetMenu(fileName = "ProjectData", menuName = "TaskManagement/ProjectData", order = 1)]
    public sealed class ProjectData : ScriptableObject
    {
        #region Fields
        [Header("Project Data Fields")]
        [SerializeField] private string projectName;
        [SerializeField] private List<TaskData> tasks;
        #endregion

        #region Properities
        /// <summary>
        /// Gets or sets the display name of this project.
        /// </summary>
        public string ProjectName
        {
            get => projectName;
            set => projectName = value;
        }
        
        /// <summary>
        /// Gets or sets the list of tasks that belong to this project.
        /// </summary>
        public List<TaskData> Tasks
        {
            get => tasks;
            set => tasks = value;
        }
        #endregion
    }
}