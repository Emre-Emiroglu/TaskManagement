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
        [SerializeField] private TaskStatus status;
        [SerializeField] private TaskPriority priority;
        [SerializeField] private string category;
        [SerializeField] private string assignee;
        [SerializeField] private string createdDate;
        [SerializeField] private string dueDate;
        [SerializeField] private bool isArchived;
        #endregion
        
        #region Properities
        public string Id
        {
            get => id;
            set => id = value;
        }
        public string Title
        {
            get => title;
            set => title = value;
        }
        public string Description
        {
            get => description;
            set => description = value;
        }
        public TaskStatus Status
        {
            get => status;
            set => status = value;
        }
        public TaskPriority Priority
        {
            get => priority;
            set => priority = value;
        }
        public string Category
        {
            get => category;
            set => category = value;
        }
        public string Assignee
        {
            get => assignee;
            set => assignee = value;
        }
        public string CreatedDate
        {
            get => createdDate;
            set => createdDate = value;
        }
        public string DueDate
        {
            get => dueDate;
            set => dueDate = value;
        }
        public bool IsArchived
        {
            get => isArchived;
            set => isArchived = value;
        }
        #endregion
    }
}