using System.Collections.Generic;
using UnityEngine;

namespace TaskManagement.Editor.Data
{
    [CreateAssetMenu(fileName = "ProjectData", menuName = "TaskManagement/ProjectData", order = 1)]
    public sealed class ProjectData : ScriptableObject
    {
        #region Fields
        [Header("Project Data Fields")]
        [SerializeField] private string projectName;
        [SerializeField] private List<TaskData> tasks;
        #endregion

        #region Getters
        public string ProjectName
        {
            get => projectName;
            set => projectName = value;
        }
        public List<TaskData> Tasks
        {
            get => tasks;
            set => tasks = value;
        }
        #endregion
    }
}