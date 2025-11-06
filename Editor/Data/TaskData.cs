using TaskManagement.Editor.Enums;
using UnityEngine;

namespace TaskManagement.Editor.Data
{
    /// <summary>
    /// Represents a single task within a project.
    /// Contains metadata such as title, description, priority, status, and due date.
    /// </summary>
    [CreateAssetMenu(fileName = "TaskData", menuName = "TaskManagement/TaskData", order = 1)]
    public sealed class TaskData : ScriptableObject
    {
        #region Fields
        [Header("Task Data Fields")]
        [SerializeField] private string title;
        [TextArea(2, 16)] [SerializeField] private string description;
        [SerializeField] private TaskStatus status;
        [SerializeField] private TaskPriority priority;
        [SerializeField] private string category;
        [SerializeField] private string assignee;
        [SerializeField] private string dueDate;
        #endregion
        
        #region Properities
        /// <summary>
        /// Gets or sets the task title.
        /// </summary>
        public string Title
        {
            get => title;
            set => title = value;
        }
        
        /// <summary>
        /// Gets or sets the task description.
        /// </summary>
        public string Description
        {
            get => description;
            set => description = value;
        }
        
        /// <summary>
        /// Gets or sets the current status of the task.
        /// </summary>
        public TaskStatus Status
        {
            get => status;
            set => status = value;
        }
        
        /// <summary>
        /// Gets or sets the task priority level.
        /// </summary>
        public TaskPriority Priority
        {
            get => priority;
            set => priority = value;
        }
        
        /// <summary>
        /// Gets or sets the category or label for the task.
        /// </summary>
        public string Category
        {
            get => category;
            set => category = value;
        }
        
        /// <summary>
        /// Gets or sets the assignee responsible for the task.
        /// </summary>
        public string Assignee
        {
            get => assignee;
            set => assignee = value;
        }

        /// <summary>
        /// Gets or sets the due date for completing this task.
        /// </summary>
        public string DueDate
        {
            get => dueDate;
            set => dueDate = value;
        }
        #endregion
    }
}