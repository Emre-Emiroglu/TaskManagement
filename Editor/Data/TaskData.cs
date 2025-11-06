using TaskManagement.Editor.Enums;
using UnityEngine;

namespace TaskManagement.Editor.Data
{
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

        public string DueDate
        {
            get => dueDate;
            set => dueDate = value;
        }
        #endregion
    }
}