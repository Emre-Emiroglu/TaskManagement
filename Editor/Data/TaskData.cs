using System;
using TaskManagement.Editor.Enums;
using UnityEngine;

namespace TaskManagement.Editor.Data
{
    [CreateAssetMenu(fileName = "TaskData", menuName = "TaskManagement/TaskData", order = 1)]
    public sealed class TaskData : ScriptableObject
    {
        #region Fields
        [Header("Task Data Fields")]
        [SerializeField] private string id;
        [SerializeField] private string title;
        [TextArea(2, 16)] [SerializeField] private string description;
        [SerializeField] private TaskStatus status = TaskStatus.ToDo;
        [SerializeField] private TaskPriority priority = TaskPriority.Normal;
        [SerializeField] private string category;
        [SerializeField] private string assignee;
        [SerializeField] private string createdDate;
        [SerializeField] private string dueDate;
        [SerializeField] private bool isArchived;
        #endregion
        
        #region Getters
        public string Id => id;
        public string Title => title;
        public string Description => description;
        public TaskStatus Status => status;
        public TaskPriority Priority => priority;
        public string Category => category;
        public string Assignee => assignee;
        public string CreatedDate => createdDate;
        public string DueDate => dueDate;
        public bool IsArchived => isArchived;
        #endregion
        
        #region Editor Helpers
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(createdDate))
                createdDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm");
        }
#endif
        #endregion
    }
}